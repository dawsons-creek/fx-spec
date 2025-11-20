namespace FX.Spec.Runner

open System
open System.Diagnostics
open FX.Spec.Core

/// Summary of test execution.
type ExecutionSummary =
    { TotalTests: int
      PassedTests: int
      FailedTests: int
      SkippedTests: int
      Duration: TimeSpan }

/// Helper module for working with execution summaries.
module ExecutionSummary =

    /// Checks if all tests passed.
    let allPassed (summary: ExecutionSummary) : bool =
        summary.FailedTests = 0 && summary.SkippedTests = 0

    /// Checks if any tests failed.
    let anyFailed (summary: ExecutionSummary) : bool = summary.FailedTests > 0

    /// Gets the exit code based on the summary (0 = success, 1 = failure).
    let exitCode (summary: ExecutionSummary) : int = if anyFailed summary then 1 else 0

    /// Formats the summary as a string.
    let format (summary: ExecutionSummary) : string =
        let duration = summary.Duration.TotalSeconds

        sprintf
            "%d examples, %d failures, %d skipped (%.2fs)"
            summary.TotalTests
            summary.FailedTests
            summary.SkippedTests
            duration

/// Module for executing test nodes and producing results.
module Executor =

    let private runSequentially (items: 'a list) (f: 'a -> Async<'b>) : Async<'b list> =
        let rec loop acc remaining =
            async {
                match remaining with
                | [] -> return List.rev acc
                | x :: xs ->
                    let! result = f x
                    return! loop (result :: acc) xs
            }

        loop [] items

    /// Executes a single example with hooks.
    let executeExample
        (description: string)
        (test: TestExecution)
        (metadata: TestMetadata)
        (beforeEachHooks: (unit -> Async<unit>) list)
        (afterEachHooks: (unit -> Async<unit>) list)
        : Async<TestResultNode> =
        async {
            let sw = Stopwatch.StartNew()

            let! result =
                async {
                    let mutable runResult = None
                    let mutable runError = None

                    try
                        // Run beforeEach hooks (outer to inner)
                        for hook in beforeEachHooks do
                            do! hook ()

                        // Run the test
                        let! testResult = test ()
                        runResult <- Some testResult
                    with ex ->
                        runError <- Some ex

                    // Run afterEach hooks (inner to outer)
                    try
                        for hook in afterEachHooks do
                            do! hook ()
                    with ex ->
                        // If we didn't have an error yet, this is our error
                        if runError.IsNone then
                            runError <- Some ex
                        // If we already had an error, we ignore the cleanup error

                    match runError with
                    | Some ex -> return Fail(Some ex)
                    | None -> 
                        match runResult with
                        | Some res -> return res
                        | None -> return Fail(Some (Exception("Unknown error: result is missing but no exception captured")))
                }

            sw.Stop()
            return ExampleResult(description, result, sw.Elapsed, metadata)
        }

    /// Runs beforeAll hooks and returns any failures as TestResultNodes.
    let private runBeforeAllHooks (hooks: (unit -> Async<unit>) list) : Async<TestResultNode list> =
        let rec loop acc remaining =
            async {
                match remaining with
                | [] -> return List.rev acc
                | hook :: rest ->
                    let sw = Stopwatch.StartNew()

                    let! maybeFailure =
                        async {
                            try
                                do! hook ()
                                sw.Stop()
                                return None
                            with ex ->
                                sw.Stop()
                                let wrappedEx = System.Exception($"beforeAll hook failed: {ex.Message}", ex)

                                return
                                    Some(
                                        ExampleResult(
                                            "beforeAll hook",
                                            Fail(Some wrappedEx),
                                            sw.Elapsed,
                                            TestMetadata.empty
                                        )
                                    )
                        }

                    let acc' =
                        match maybeFailure with
                        | Some failure -> failure :: acc
                        | None -> acc

                    return! loop acc' rest
            }

        loop [] hooks

    /// Runs afterAll hooks and returns any failures as TestResultNodes.
    let private runAfterAllHooks (hooks: (unit -> Async<unit>) list) : Async<TestResultNode list> =
        let rec loop acc remaining =
            async {
                match remaining with
                | [] -> return List.rev acc
                | hook :: rest ->
                    let sw = Stopwatch.StartNew()

                    let! maybeFailure =
                        async {
                            try
                                do! hook ()
                                sw.Stop()
                                return None
                            with ex ->
                                sw.Stop()
                                let wrappedEx = System.Exception($"afterAll hook failed: {ex.Message}", ex)

                                return
                                    Some(
                                        ExampleResult(
                                            "afterAll hook",
                                            Fail(Some wrappedEx),
                                            sw.Elapsed,
                                            TestMetadata.empty
                                        )
                                    )
                        }

                    let acc' =
                        match maybeFailure with
                        | Some failure -> failure :: acc
                        | None -> acc

                    return! loop acc' rest
            }

        loop [] hooks

    /// Executes a single test node recursively with accumulated hooks.
    /// Returns a TestResultNode with timing information.
    let rec executeNodeWithHooks
        (beforeEachHooks: (unit -> Async<unit>) list)
        (afterEachHooks: (unit -> Async<unit>) list)
        (node: TestNode)
        : Async<TestResultNode> =
        async {
            match node with
            | Example(description, test, metadata) ->
                return! executeExample description test metadata beforeEachHooks afterEachHooks

            | Group(description, hooks, children, metadata) ->
                return! executeGroupNode description hooks children metadata beforeEachHooks afterEachHooks

            | FocusedExample(description, test, metadata) ->
                return! executeExample description test metadata beforeEachHooks afterEachHooks

            | FocusedGroup(description, hooks, children, metadata) ->
                return! executeGroupNode description hooks children metadata beforeEachHooks afterEachHooks

            | BeforeAllHook _
            | BeforeEachHook _
            | AfterEachHook _
            | AfterAllHook _ ->
                // Hook nodes should have been processed during group construction
                // If we encounter them here, just skip them
                return GroupResult("hook", [], TestMetadata.empty)
        }

    and private executeGroupNode
        (description: string)
        (hooks: GroupHooks)
        (children: TestNode list)
        (metadata: TestMetadata)
        (beforeEachHooks: (unit -> Async<unit>) list)
        (afterEachHooks: (unit -> Async<unit>) list)
        : Async<TestResultNode> =
        async {
            let! beforeAllResults = runBeforeAllHooks hooks.BeforeAll

            let childBeforeEachHooks = beforeEachHooks @ hooks.BeforeEach
            let childAfterEachHooks = hooks.AfterEach @ afterEachHooks

            let! childResults =
                runSequentially children (fun child ->
                    executeNodeWithHooks childBeforeEachHooks childAfterEachHooks child)

            let! afterAllResults = runAfterAllHooks hooks.AfterAll

            return GroupResult(description, beforeAllResults @ childResults @ afterAllResults, metadata)
        }

    /// Executes a single test node recursively.
    /// Returns a TestResultNode with timing information.
    let executeNode (node: TestNode) : Async<TestResultNode> = executeNodeWithHooks [] [] node

    /// Executes a list of test nodes.
    let executeTests (nodes: TestNode list) : Async<TestResultNode list> = runSequentially nodes executeNode

    /// Executes tests and returns summary statistics.
    let executeWithSummary (nodes: TestNode list) : Async<TestResultNode list * ExecutionSummary> =
        async {
            let sw = Stopwatch.StartNew()
            let! results = executeTests nodes
            sw.Stop()

            let summary =
                { TotalTests = results |> List.sumBy TestResultNode.countTotal
                  PassedTests = results |> List.sumBy TestResultNode.countPassed
                  FailedTests = results |> List.sumBy TestResultNode.countFailed
                  SkippedTests = results |> List.sumBy TestResultNode.countSkipped
                  Duration = sw.Elapsed }

            return results, summary
        }
