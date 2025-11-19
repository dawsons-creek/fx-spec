namespace FX.Spec.Runner

open System
open FX.Spec.Core

/// Simple console formatter for test results.
/// This is a basic implementation - Phase 4 will add Spectre.Console for beautiful output.
module SimpleFormatter =

    /// Maximum number of stack trace lines to display in error messages.
    let private maxStackTraceLines = 3

    /// Maximum number of collection items to show in error messages.
    let private maxCollectionItemsToShow = 10

    /// Formats a test result with a symbol.
    let formatResult =
        function
        | Pass -> "✓"
        | Fail _ -> "✗"
        | Skipped _ -> "⊘"

    /// Gets the color for a test result (using ANSI codes).
    let resultColor =
        function
        | Pass -> "\u001b[32m" // Green
        | Fail _ -> "\u001b[31m" // Red
        | Skipped _ -> "\u001b[33m" // Yellow

    /// ANSI reset code.
    let resetColor = "\u001b[0m"

    /// Formats a single test result node recursively.
    let rec formatResultNode (indent: int) (node: TestResultNode) : string list =
        let indentStr = String.replicate indent "  "

        match node with
        | ExampleResult(description, result, duration, _) ->
            let symbol = formatResult result
            let color = resultColor result
            let durationMs = duration.TotalMilliseconds

            let line =
                sprintf "%s%s%s %s%s (%.0fms)" indentStr color symbol resetColor description durationMs

            [ line ]

        | GroupResult(description, children, _) ->
            let header = sprintf "%s%s" indentStr description
            let childLines = children |> List.collect (formatResultNode (indent + 1))
            header :: childLines

    /// Formats a list of test result nodes.
    let formatResults (results: TestResultNode list) : string =
        let lines = results |> List.collect (formatResultNode 0)
        String.concat "\n" lines

    /// Checks if a stack trace line is relevant (not framework code).
    let private isRelevantStackLine (line: string) : bool =
        not (line.Contains("FX.Spec.Runner"))
        && not (line.Contains("FX.Spec.Core.SpecHelpers"))
        && not (line.Contains("System.Reflection"))
        && not (line.Contains("System.Runtime"))
        && not (line.Contains("Microsoft.FSharp.Core"))

    /// Formats a stack trace for display.
    let private formatStackTrace (ex: Exception) : string =
        if String.IsNullOrWhiteSpace(ex.StackTrace) then
            ""
        else
            let relevantLines =
                ex.StackTrace.Split('\n')
                |> Array.filter isRelevantStackLine
                |> Array.truncate maxStackTraceLines

            match relevantLines with
            | [||] -> ""
            | lines ->
                let formatted = lines |> Array.map (fun l -> "    " + l.Trim())
                "\n  Stack trace:\n" + String.concat "\n" formatted

    /// Formats failure details for a test result.
    let formatFailureDetails (result: TestResult) : string option =
        match result with
        | Fail(Some ex) ->
            let exType = ex.GetType().Name
            let msg = sprintf "  %s: %s" exType ex.Message
            let stackTrace = formatStackTrace ex
            Some(msg + stackTrace)
        | Fail None -> Some "  Error: Test failed without exception"
        | Skipped reason -> Some(sprintf "  Skipped: %s" reason)
        | Pass -> None

    /// Collects all failures from result nodes.
    let rec collectFailures (node: TestResultNode) : (string * TestResult) list =
        match node with
        | ExampleResult(description, result, _, _) ->
            match result with
            | Fail _
            | Skipped _ -> [ (description, result) ]
            | Pass -> []
        | GroupResult(_, children, _) -> children |> List.collect collectFailures

    /// Formats all failures with details.
    let formatFailures (results: TestResultNode list) : string =
        let failures = results |> List.collect collectFailures

        if List.isEmpty failures then
            ""
        else
            let header = sprintf "\n%sFailures:%s\n" (resultColor (Fail None)) resetColor

            let failureDetails =
                failures
                |> List.mapi (fun i (description, result) ->
                    let num = sprintf "%d) %s" (i + 1) description

                    match formatFailureDetails result with
                    | Some details -> num + "\n" + details
                    | None -> num)
                |> String.concat "\n\n"

            header + failureDetails

    /// Formats the execution summary.
    let formatSummary (summary: ExecutionSummary) : string =
        let color =
            if summary.FailedTests > 0 then
                resultColor (Fail None)
            else
                resultColor Pass

        let summaryText = ExecutionSummary.format summary
        sprintf "\n%s%s%s" color summaryText resetColor

    /// Formats complete test output.
    let formatOutput (results: TestResultNode list) (summary: ExecutionSummary) : string =
        let resultsOutput = formatResults results
        let failuresOutput = formatFailures results
        let summaryOutput = formatSummary summary

        resultsOutput + failuresOutput + summaryOutput

    /// Prints test results to console.
    let printResults (results: TestResultNode list) (summary: ExecutionSummary) : unit =
        let output = formatOutput results summary
        printfn "%s" output
