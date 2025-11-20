namespace FX.Spec.Core

open System

/// Represents the outcome of a single test execution.
type TestResult =
    /// The test passed successfully.
    | Pass
    /// The test failed with an optional exception.
    | Fail of exn option
    /// The test was skipped with a reason.
    | Skipped of reason: string

/// A function that, when executed, produces a TestResult asynchronously.
/// This thunk allows the runner to defer execution until needed.
type TestExecution = unit -> Async<TestResult>

/// Metadata associated with an example or group.
type TestMetadata =
    { Tags: string list
      Traits: Map<string, string> }

module TestMetadata =
    /// The empty metadata value.
    let empty = { Tags = []; Traits = Map.empty }

    let private normalizeTag (tag: string) =
        if String.IsNullOrWhiteSpace tag then
            None
        else
            Some(tag.Trim().ToLowerInvariant())

    /// Creates metadata containing the supplied tags.
    let ofTags tags =
        let normalized = tags |> List.choose normalizeTag |> List.distinct
        { empty with Tags = normalized }

    /// Adds a tag if it does not already exist.
    let addTag tag metadata =
        match normalizeTag tag with
        | None -> metadata
        | Some normalized ->
            if metadata.Tags |> List.contains normalized then
                metadata
            else
                { metadata with
                    Tags = metadata.Tags @ [ normalized ] }

    /// Adds many tags, preserving original order and de-duplicating.
    let addTags tags metadata =
        tags |> List.fold (fun acc tag -> addTag tag acc) metadata

    /// Removes a tag, if present.
    let removeTag tag metadata =
        match normalizeTag tag with
        | None -> metadata
        | Some normalized ->
            { metadata with
                Tags = metadata.Tags |> List.filter ((<>) normalized) }

    /// Clears all tags from the metadata.
    let clearTags metadata = { metadata with Tags = [] }

    /// Sets (or replaces) a trait value.
    let setTrait key value metadata =
        { metadata with
            Traits = metadata.Traits |> Map.add key value }

    /// Attempts to retrieve a trait.
    let tryGetTrait key metadata = metadata.Traits |> Map.tryFind key

    /// Checks whether the metadata is tagged with a specific value.
    let hasTag tag metadata =
        match normalizeTag tag with
        | None -> false
        | Some normalized -> metadata.Tags |> List.contains normalized

    /// Returns true when all specified tags are present.
    let hasAllTags tags metadata =
        tags
        |> List.choose normalizeTag
        |> List.forall (fun normalized -> metadata.Tags |> List.contains normalized)

    /// Returns true when any of the specified tags are present.
    let hasAnyTag tags metadata =
        tags
        |> List.choose normalizeTag
        |> List.exists (fun normalized -> metadata.Tags |> List.contains normalized)

/// Represents hooks that can be attached to a group.
type GroupHooks =
    { BeforeAll: (unit -> Async<unit>) list
      BeforeEach: (unit -> Async<unit>) list
      AfterEach: (unit -> Async<unit>) list
      AfterAll: (unit -> Async<unit>) list }

/// Helper module for GroupHooks.
module GroupHooks =
    /// Creates an empty set of hooks.
    let empty =
        { BeforeAll = []
          BeforeEach = []
          AfterEach = []
          AfterAll = [] }

    /// Adds a beforeAll hook.
    let addBeforeAll hook hooks =
        { hooks with
            BeforeAll = hooks.BeforeAll @ [ hook ] }

    /// Adds a beforeEach hook.
    let addBeforeEach hook hooks =
        { hooks with
            BeforeEach = hooks.BeforeEach @ [ hook ] }

    /// Adds an afterEach hook.
    let addAfterEach hook hooks =
        { hooks with
            AfterEach = hooks.AfterEach @ [ hook ] }

    /// Adds an afterAll hook.
    let addAfterAll hook hooks =
        { hooks with
            AfterAll = hooks.AfterAll @ [ hook ] }

/// Represents a node in the test suite tree.
/// This is the core data structure that the DSL builds.
type TestNode =
    /// A leaf node representing an individual test case.
    /// Contains a description, the test execution function, and metadata.
    | Example of description: string * test: TestExecution * metadata: TestMetadata
    /// An internal node representing a group of tests.
    /// Contains a description, hooks, child nodes, and metadata.
    | Group of description: string * hooks: GroupHooks * tests: TestNode list * metadata: TestMetadata
    /// A focused test example (fit). When any focused tests exist, only focused tests run.
    | FocusedExample of description: string * test: TestExecution * metadata: TestMetadata
    /// A focused group (fdescribe). When any focused groups exist, only tests in focused groups run.
    | FocusedGroup of description: string * hooks: GroupHooks * tests: TestNode list * metadata: TestMetadata
    /// Hook nodes that get processed and attached to their parent group.
    | BeforeAllHook of (unit -> Async<unit>)
    | BeforeEachHook of (unit -> Async<unit>)
    | AfterEachHook of (unit -> Async<unit>)
    | AfterAllHook of (unit -> Async<unit>)

/// Represents the result of executing a test node.
/// This mirrors the TestNode structure but includes execution results.
type TestResultNode =
    /// Result of executing an individual test example.
    | ExampleResult of description: string * result: TestResult * duration: TimeSpan * metadata: TestMetadata

    /// Result of executing a group of tests.
    | GroupResult of description: string * results: TestResultNode list * metadata: TestMetadata

/// Custom attribute to mark test suites for discovery.
/// Apply this to let-bound values of type TestNode or TestNode list.
[<AttributeUsage(AttributeTargets.Property ||| AttributeTargets.Field)>]
type TestsAttribute() =
    inherit Attribute()

/// Helper module for working with TestNode.
module TestNode =
    /// Gets the description of a test node.
    let description =
        function
        | Example(desc, _, _)
        | Group(desc, _, _, _)
        | FocusedExample(desc, _, _)
        | FocusedGroup(desc, _, _, _) -> desc
        | BeforeAllHook _ -> "beforeAll hook"
        | BeforeEachHook _ -> "beforeEach hook"
        | AfterEachHook _ -> "afterEach hook"
        | AfterAllHook _ -> "afterAll hook"

    /// Retrieves the metadata associated with the node.
    let metadata =
        function
        | Example(_, _, meta)
        | Group(_, _, _, meta)
        | FocusedExample(_, _, meta)
        | FocusedGroup(_, _, _, meta) -> meta
        | BeforeAllHook _
        | BeforeEachHook _
        | AfterEachHook _
        | AfterAllHook _ -> TestMetadata.empty

    /// Replaces the metadata associated with the node.
    let withMetadata metadata node =
        match node with
        | Example(desc, test, _) -> Example(desc, test, metadata)
        | Group(desc, hooks, children, _) -> Group(desc, hooks, children, metadata)
        | FocusedExample(desc, test, _) -> FocusedExample(desc, test, metadata)
        | FocusedGroup(desc, hooks, children, _) -> FocusedGroup(desc, hooks, children, metadata)
        | BeforeAllHook hook -> BeforeAllHook hook
        | BeforeEachHook hook -> BeforeEachHook hook
        | AfterEachHook hook -> AfterEachHook hook
        | AfterAllHook hook -> AfterAllHook hook

    /// Adds tags to the node metadata.
    let addTags tags node =
        let updatedMeta = node |> metadata |> TestMetadata.addTags tags
        withMetadata updatedMeta node

    /// Adds a single tag to the node metadata.
    let addTag tag node = addTags [ tag ] node

    /// Checks if the node metadata already contains the specified tag.
    let containsTag tag node =
        node |> metadata |> TestMetadata.hasTag tag

    /// Checks if the node metadata contains any of the specified tags.
    let containsAnyTag tags node =
        node |> metadata |> TestMetadata.hasAnyTag tags

    /// Sets a trait value in the node metadata.
    let setTrait key value node =
        let updatedMeta = node |> metadata |> TestMetadata.setTrait key value
        withMetadata updatedMeta node

    /// Recursively counts the number of examples in a test tree.
    let rec countExamples =
        function
        | Example _ -> 1
        | FocusedExample _ -> 1
        | Group(_, _, children, _) -> children |> List.sumBy countExamples
        | FocusedGroup(_, _, children, _) -> children |> List.sumBy countExamples
        | BeforeAllHook _
        | BeforeEachHook _
        | AfterEachHook _
        | AfterAllHook _ -> 0

    /// Recursively counts the number of groups in a test tree.
    let rec countGroups =
        function
        | Example _
        | FocusedExample _ -> 0
        | Group(_, _, children, _) -> 1 + (children |> List.sumBy countGroups)
        | FocusedGroup(_, _, children, _) -> 1 + (children |> List.sumBy countGroups)
        | BeforeAllHook _
        | BeforeEachHook _
        | AfterEachHook _
        | AfterAllHook _ -> 0

    /// Checks if a test tree contains any focused tests.
    let rec hasFocused =
        function
        | FocusedExample _ -> true
        | FocusedGroup _ -> true
        | Group(_, _, children, _) -> children |> List.exists hasFocused
        | Example _ -> false
        | BeforeAllHook _
        | BeforeEachHook _
        | AfterEachHook _
        | AfterAllHook _ -> false

    /// Filters a test tree to only include focused tests.
    /// If no focused tests exist, returns the original tree.
    let rec filterFocused (nodes: TestNode list) : TestNode list =
        if nodes |> List.exists hasFocused then
            nodes
            |> List.choose (fun node ->
                match node with
                | FocusedExample(desc, test, meta) -> Some(Example(desc, test, meta))
                | FocusedGroup(desc, hooks, children, meta) -> Some(Group(desc, hooks, filterFocused children, meta))
                | Group(desc, hooks, children, meta) ->
                    let filtered = filterFocused children

                    if List.isEmpty filtered then
                        None
                    else
                        Some(Group(desc, hooks, filtered, meta))
                | Example _ -> None
                | BeforeAllHook _
                | BeforeEachHook _
                | AfterEachHook _
                | AfterAllHook _ -> None)
        else
            nodes

    /// Filters nodes so only those whose metadata contains all specified tags remain.
    let filterByTags tags (nodes: TestNode list) : TestNode list =
        match tags with
        | [] -> nodes
        | _ ->
            let rec appendDistinct existing additional =
                match additional with
                | [] -> existing
                | head :: tail ->
                    let updated =
                        if List.contains head existing then
                            existing
                        else
                            existing @ [ head ]

                    appendDistinct updated tail

            let rec traverse inheritedTags node =
                let nodeMeta = metadata node
                let combinedTags = appendDistinct inheritedTags nodeMeta.Tags
                let combinedMeta = { nodeMeta with Tags = combinedTags }
                let satisfies = TestMetadata.hasAllTags tags combinedMeta

                match node with
                | Group(desc, hooks, children, meta) ->
                    let filteredChildren = children |> List.choose (traverse combinedTags)

                    if satisfies || not (List.isEmpty filteredChildren) then
                        Some(Group(desc, hooks, filteredChildren, meta))
                    else
                        None
                | FocusedGroup(desc, hooks, children, meta) ->
                    let filteredChildren = children |> List.choose (traverse combinedTags)

                    if satisfies || not (List.isEmpty filteredChildren) then
                        Some(FocusedGroup(desc, hooks, filteredChildren, meta))
                    else
                        None
                | Example(desc, test, meta) -> if satisfies then Some(Example(desc, test, meta)) else None
                | FocusedExample(desc, test, meta) ->
                    if satisfies then
                        Some(FocusedExample(desc, test, meta))
                    else
                        None
                | BeforeAllHook _
                | BeforeEachHook _
                | AfterEachHook _
                | AfterAllHook _ -> None

            nodes |> List.choose (traverse [])

/// Helper module for working with TestResult.
module TestResult =
    /// Checks if a test result is a pass.
    let isPass =
        function
        | Pass -> true
        | _ -> false

    /// Checks if a test result is a failure.
    let isFail =
        function
        | Fail _ -> true
        | _ -> false

    /// Checks if a test result is skipped.
    let isSkipped =
        function
        | Skipped _ -> true
        | _ -> false

/// Helper module for working with TestResultNode.
module TestResultNode =
    /// Gets the description of a test result node.
    let description =
        function
        | ExampleResult(desc, _, _, _)
        | GroupResult(desc, _, _) -> desc

    /// Gets the metadata associated with the result node.
    let metadata =
        function
        | ExampleResult(_, _, _, meta)
        | GroupResult(_, _, meta) -> meta

    /// Recursively collects all test results from a result tree.
    let rec collectResults node =
        match node with
        | ExampleResult(_, result, _, _) -> [ result ]
        | GroupResult(_, children, _) -> children |> List.collect collectResults

    /// Counts the number of passed tests in a result tree.
    let countPassed node =
        collectResults node |> List.filter TestResult.isPass |> List.length

    /// Counts the number of failed tests in a result tree.
    let countFailed node =
        collectResults node |> List.filter TestResult.isFail |> List.length

    /// Counts the number of skipped tests in a result tree.
    let countSkipped node =
        collectResults node |> List.filter TestResult.isSkipped |> List.length

    /// Gets the total number of tests in a result tree.
    let countTotal node = collectResults node |> List.length
