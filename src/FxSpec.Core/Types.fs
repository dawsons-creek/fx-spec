namespace FxSpec.Core

/// Represents the outcome of a single test execution.
type TestResult =
    /// The test passed successfully.
    | Pass
    /// The test failed with an optional exception.
    | Fail of exn option
    /// The test was skipped with a reason.
    | Skipped of reason: string

/// A function that, when executed, produces a TestResult.
/// This is a thunk that delays test execution until the runner is ready.
type TestExecution = unit -> TestResult

/// Represents a node in the test suite tree.
/// This is the core data structure that the DSL builds.
type TestNode =
    /// A leaf node representing an individual test case.
    /// Contains a description and the test execution function.
    | Example of description: string * test: TestExecution

    /// An internal node representing a group of tests.
    /// Contains a description and a list of child nodes (which can be Groups or Examples).
    | Group of description: string * tests: TestNode list

    /// A focused test example (fit). When any focused tests exist, only focused tests run.
    | FocusedExample of description: string * test: TestExecution

    /// A focused group (fdescribe). When any focused groups exist, only tests in focused groups run.
    | FocusedGroup of description: string * tests: TestNode list

/// Represents the result of executing a test node.
/// This mirrors the TestNode structure but includes execution results.
type TestResultNode =
    /// Result of executing an individual test example.
    | ExampleResult of description: string * result: TestResult * duration: System.TimeSpan
    
    /// Result of executing a group of tests.
    | GroupResult of description: string * results: TestResultNode list

/// Custom attribute to mark test suites for discovery.
/// Apply this to let-bound values of type TestNode or TestNode list.
[<System.AttributeUsage(System.AttributeTargets.Property ||| System.AttributeTargets.Field)>]
type TestsAttribute() =
    inherit System.Attribute()

/// Helper module for working with TestResult.
module TestResult =
    /// Checks if a test result is a pass.
    let isPass = function
        | Pass -> true
        | _ -> false
    
    /// Checks if a test result is a failure.
    let isFail = function
        | Fail _ -> true
        | _ -> false
    
    /// Checks if a test result is skipped.
    let isSkipped = function
        | Skipped _ -> true
        | _ -> false

/// Helper module for working with TestNode.
module TestNode =
    /// Gets the description of a test node.
    let description = function
        | Example (desc, _) -> desc
        | Group (desc, _) -> desc
        | FocusedExample (desc, _) -> desc
        | FocusedGroup (desc, _) -> desc

    /// Recursively counts the number of examples in a test tree.
    let rec countExamples = function
        | Example _ -> 1
        | FocusedExample _ -> 1
        | Group (_, children) ->
            children |> List.sumBy countExamples
        | FocusedGroup (_, children) ->
            children |> List.sumBy countExamples

    /// Recursively counts the number of groups in a test tree.
    let rec countGroups = function
        | Example _ -> 0
        | FocusedExample _ -> 0
        | Group (_, children) ->
            1 + (children |> List.sumBy countGroups)
        | FocusedGroup (_, children) ->
            1 + (children |> List.sumBy countGroups)

    /// Checks if a test tree contains any focused tests.
    let rec hasFocused = function
        | FocusedExample _ -> true
        | FocusedGroup _ -> true
        | Group (_, children) -> children |> List.exists hasFocused
        | Example _ -> false

    /// Filters a test tree to only include focused tests.
    /// If no focused tests exist, returns the original tree.
    let rec filterFocused (nodes: TestNode list) : TestNode list =
        if nodes |> List.exists hasFocused then
            nodes |> List.choose (fun node ->
                match node with
                | FocusedExample (desc, test) -> Some (Example (desc, test))
                | FocusedGroup (desc, children) -> Some (Group (desc, filterFocused children))
                | Group (desc, children) ->
                    let filtered = filterFocused children
                    if List.isEmpty filtered then None
                    else Some (Group (desc, filtered))
                | Example _ -> None
            )
        else
            nodes

/// Helper module for working with TestResultNode.
module TestResultNode =
    /// Gets the description of a test result node.
    let description = function
        | ExampleResult (desc, _, _) -> desc
        | GroupResult (desc, _) -> desc
    
    /// Recursively collects all test results from a result tree.
    let rec collectResults node =
        match node with
        | ExampleResult (_, result, _) -> [result]
        | GroupResult (_, children) ->
            children |> List.collect collectResults
    
    /// Counts the number of passed tests in a result tree.
    let countPassed node =
        collectResults node
        |> List.filter TestResult.isPass
        |> List.length
    
    /// Counts the number of failed tests in a result tree.
    let countFailed node =
        collectResults node
        |> List.filter TestResult.isFail
        |> List.length
    
    /// Counts the number of skipped tests in a result tree.
    let countSkipped node =
        collectResults node
        |> List.filter TestResult.isSkipped
        |> List.length
    
    /// Gets the total number of tests in a result tree.
    let countTotal node =
        collectResults node |> List.length

