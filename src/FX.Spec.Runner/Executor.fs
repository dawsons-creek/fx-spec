namespace FxSpec.Runner

open System
open System.Diagnostics
open FxSpec.Core

/// Summary of test execution.
type ExecutionSummary = {
    TotalTests: int
    PassedTests: int
    FailedTests: int
    SkippedTests: int
    Duration: TimeSpan
}

/// Helper module for working with execution summaries.
module ExecutionSummary =

    /// Checks if all tests passed.
    let allPassed (summary: ExecutionSummary) : bool =
        summary.FailedTests = 0 && summary.SkippedTests = 0

    /// Checks if any tests failed.
    let anyFailed (summary: ExecutionSummary) : bool =
        summary.FailedTests > 0

    /// Gets the exit code based on the summary (0 = success, 1 = failure).
    let exitCode (summary: ExecutionSummary) : int =
        if anyFailed summary then 1 else 0

    /// Formats the summary as a string.
    let format (summary: ExecutionSummary) : string =
        let duration = summary.Duration.TotalSeconds
        sprintf "%d examples, %d failures, %d skipped (%.2fs)"
            summary.TotalTests
            summary.FailedTests
            summary.SkippedTests
            duration

/// Module for executing test nodes and producing results.
module Executor =

    /// Executes a single example with hooks.
    let executeExample (description: string) (test: TestExecution) (beforeEachHooks: (unit -> unit) list) (afterEachHooks: (unit -> unit) list) : TestResultNode =
        let sw = Stopwatch.StartNew()
        let result =
            try
                // Run beforeEach hooks (outer to inner)
                beforeEachHooks |> List.iter (fun hook -> hook())

                // Run the test
                let testResult = test()

                // Run afterEach hooks (inner to outer)
                afterEachHooks |> List.iter (fun hook -> hook())

                testResult
            with ex ->
                // Run afterEach hooks even on failure
                try
                    afterEachHooks |> List.iter (fun hook -> hook())
                with _ -> ()  // Ignore hook failures during cleanup

                // Any exception during test execution is a failure
                Fail (Some ex)
        sw.Stop()
        ExampleResult (description, result, sw.Elapsed)

    /// Executes a single test node recursively with accumulated hooks.
    /// Returns a TestResultNode with timing information.
    let rec executeNodeWithHooks (beforeEachHooks: (unit -> unit) list) (afterEachHooks: (unit -> unit) list) (node: TestNode) : TestResultNode =
        match node with
        | Example (description, test) ->
            executeExample description test beforeEachHooks afterEachHooks

        | Group (description, hooks, children) ->
            // Run beforeAll hooks, catching failures
            let beforeAllResults =
                hooks.BeforeAll
                |> List.map (fun hook ->
                    let sw = Stopwatch.StartNew()
                    try
                        hook()
                        sw.Stop()
                        None  // Success
                    with ex ->
                        sw.Stop()
                        let wrappedEx = System.Exception($"beforeAll hook failed: {ex.Message}", ex)
                        Some (ExampleResult ($"beforeAll hook", Fail (Some wrappedEx), sw.Elapsed))
                )
                |> List.choose id

            // Accumulate hooks for children
            let childBeforeEachHooks = beforeEachHooks @ hooks.BeforeEach
            let childAfterEachHooks = hooks.AfterEach @ afterEachHooks

            // Execute all children with accumulated hooks
            let childResults = children |> List.map (executeNodeWithHooks childBeforeEachHooks childAfterEachHooks)

            // Run afterAll hooks, catching failures
            let afterAllResults =
                hooks.AfterAll
                |> List.map (fun hook ->
                    let sw = Stopwatch.StartNew()
                    try
                        hook()
                        sw.Stop()
                        None  // Success
                    with ex ->
                        sw.Stop()
                        let wrappedEx = System.Exception($"afterAll hook failed: {ex.Message}", ex)
                        Some (ExampleResult ($"afterAll hook", Fail (Some wrappedEx), sw.Elapsed))
                )
                |> List.choose id

            // Combine all results: beforeAll failures + child results + afterAll failures
            GroupResult (description, beforeAllResults @ childResults @ afterAllResults)

        | FocusedExample (description, test) ->
            executeExample description test beforeEachHooks afterEachHooks

        | FocusedGroup (description, hooks, children) ->
            // Run beforeAll hooks, catching failures
            let beforeAllResults =
                hooks.BeforeAll
                |> List.map (fun hook ->
                    let sw = Stopwatch.StartNew()
                    try
                        hook()
                        sw.Stop()
                        None  // Success
                    with ex ->
                        sw.Stop()
                        let wrappedEx = System.Exception($"beforeAll hook failed: {ex.Message}", ex)
                        Some (ExampleResult ($"beforeAll hook", Fail (Some wrappedEx), sw.Elapsed))
                )
                |> List.choose id

            // Accumulate hooks for children
            let childBeforeEachHooks = beforeEachHooks @ hooks.BeforeEach
            let childAfterEachHooks = hooks.AfterEach @ afterEachHooks

            // Execute all children with accumulated hooks
            let childResults = children |> List.map (executeNodeWithHooks childBeforeEachHooks childAfterEachHooks)

            // Run afterAll hooks, catching failures
            let afterAllResults =
                hooks.AfterAll
                |> List.map (fun hook ->
                    let sw = Stopwatch.StartNew()
                    try
                        hook()
                        sw.Stop()
                        None  // Success
                    with ex ->
                        sw.Stop()
                        let wrappedEx = System.Exception($"afterAll hook failed: {ex.Message}", ex)
                        Some (ExampleResult ($"afterAll hook", Fail (Some wrappedEx), sw.Elapsed))
                )
                |> List.choose id

            // Combine all results: beforeAll failures + child results + afterAll failures
            GroupResult (description, beforeAllResults @ childResults @ afterAllResults)

        | BeforeAllHook _ | BeforeEachHook _ | AfterEachHook _ | AfterAllHook _ ->
            // Hook nodes should have been processed during group construction
            // If we encounter them here, just skip them
            GroupResult ("hook", [])

    /// Executes a single test node recursively.
    /// Returns a TestResultNode with timing information.
    let rec executeNode (node: TestNode) : TestResultNode =
        executeNodeWithHooks [] [] node
    
    /// Executes a list of test nodes.
    let executeTests (nodes: TestNode list) : TestResultNode list =
        nodes |> List.map executeNode
    
    /// Executes tests and returns summary statistics.
    let executeWithSummary (nodes: TestNode list) : TestResultNode list * ExecutionSummary =
        let sw = Stopwatch.StartNew()
        let results = executeTests nodes
        sw.Stop()

        let summary = {
            TotalTests = results |> List.sumBy TestResultNode.countTotal
            PassedTests = results |> List.sumBy TestResultNode.countPassed
            FailedTests = results |> List.sumBy TestResultNode.countFailed
            SkippedTests = results |> List.sumBy TestResultNode.countSkipped
            Duration = sw.Elapsed
        }

        (results, summary)

