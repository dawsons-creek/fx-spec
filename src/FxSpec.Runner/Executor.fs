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
    
    /// Executes a single test node recursively.
    /// Returns a TestResultNode with timing information.
    let rec executeNode (node: TestNode) : TestResultNode =
        match node with
        | Example (description, test) ->
            let sw = Stopwatch.StartNew()
            let result =
                try
                    test()
                with ex ->
                    // Any exception during test execution is a failure
                    Fail (Some ex)
            sw.Stop()
            ExampleResult (description, result, sw.Elapsed)

        | Group (description, children) ->
            // Execute all children and collect results
            let results = children |> List.map executeNode
            GroupResult (description, results)

        | FocusedExample (description, test) ->
            // Focused examples execute the same as regular examples
            let sw = Stopwatch.StartNew()
            let result =
                try
                    test()
                with ex ->
                    Fail (Some ex)
            sw.Stop()
            ExampleResult (description, result, sw.Elapsed)

        | FocusedGroup (description, children) ->
            // Focused groups execute the same as regular groups
            let results = children |> List.map executeNode
            GroupResult (description, results)
    
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

