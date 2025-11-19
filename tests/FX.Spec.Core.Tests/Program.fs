// Test runner for FxSpec.Core.Tests
// This runs both old-style tests (for validation) and new FxSpec-based tests (dogfooding!)
open System
open FxSpec.Core
open FxSpec.Core.Tests

/// Simple test executor that runs a spec tree and reports results
let rec executeNode (node: TestNode) : TestResultNode =
    match node with
    | Example (desc, testFn) | FocusedExample (desc, testFn) ->
        let sw = System.Diagnostics.Stopwatch.StartNew()
        let result = testFn()
        sw.Stop()
        ExampleResult (desc, result, sw.Elapsed)
    | Group (desc, _, children) | FocusedGroup (desc, _, children) ->
        let results = children |> List.map executeNode
        GroupResult (desc, results)
    | BeforeAllHook _ | BeforeEachHook _ | AfterEachHook _ | AfterAllHook _ ->
        // Hooks are not executed in this simple runner
        GroupResult ("hook", [])

/// Simple formatter that prints test results
let rec printResults indent (node: TestResultNode) =
    match node with
    | ExampleResult (desc, result, duration) ->
        let symbol =
            match result with
            | Pass -> "✓"
            | Fail _ -> "✗"
            | Skipped _ -> "⊘"
        let color =
            match result with
            | Pass -> ConsoleColor.Green
            | Fail _ -> ConsoleColor.Red
            | Skipped _ -> ConsoleColor.Yellow

        Console.ForegroundColor <- color
        printfn "%s%s %s (%.2fms)" indent symbol desc (duration.TotalMilliseconds)
        Console.ResetColor()

        match result with
        | Fail (Some ex) ->
            Console.ForegroundColor <- ConsoleColor.Red
            printfn "%s  Error: %s" indent ex.Message
            Console.ResetColor()
        | Fail None ->
            Console.ForegroundColor <- ConsoleColor.Red
            printfn "%s  Error: Test failed with no exception" indent
            Console.ResetColor()
        | Skipped reason ->
            Console.ForegroundColor <- ConsoleColor.Yellow
            printfn "%s  Reason: %s" indent reason
            Console.ResetColor()
        | Pass -> ()
    | GroupResult (desc, results) ->
        printfn "%s%s" indent desc
        results |> List.iter (printResults (indent + "  "))

/// Run a spec and print results
let runSpec name (nodes: TestNode list) =
    printfn ""
    printfn "==================================="
    printfn "%s" name
    printfn "==================================="
    printfn ""

    let results = nodes |> List.map executeNode
    results |> List.iter (printResults "")

    let totalResults = results |> List.collect TestResultNode.collectResults
    let passed = results |> List.sumBy TestResultNode.countPassed
    let failed = results |> List.sumBy TestResultNode.countFailed
    let skipped = results |> List.sumBy TestResultNode.countSkipped
    let total = List.length totalResults

    printfn ""
    if failed > 0 then
        Console.ForegroundColor <- ConsoleColor.Red
        printfn "%d examples, %d failures, %d skipped" total failed skipped
        Console.ResetColor()
    else
        Console.ForegroundColor <- ConsoleColor.Green
        printfn "%d examples, %d failures, %d skipped" total failed skipped
        Console.ResetColor()

    failed = 0

[<EntryPoint>]
let main argv =
    printfn "╔═══════════════════════════════════════════════════════════╗"
    printfn "║           FxSpec.Core Test Suite                          ║"
    printfn "║           Testing FxSpec with FxSpec! 🎯                  ║"
    printfn "║           (Legacy runner - use FxSpec.Runner instead)     ║"
    printfn "╚═══════════════════════════════════════════════════════════╝"

    try
        // Run FxSpec-based tests using simple runner
        let success1 = runSpec "TypesSpecs - Testing FxSpec Types" [TypesSpecs.testResultSpecs]
        let success2 = runSpec "TypesSpecs - Testing TestNode" [TypesSpecs.testNodeSpecs]
        let success3 = runSpec "TypesSpecs - Testing TestResultNode" [TypesSpecs.testResultNodeSpecs]
        let success4 = runSpec "SpecBuilderSpecs - Basic Functionality" [SpecBuilderSpecs.specBuilderSpecs]
        let success5 = runSpec "SpecBuilderSpecs - Example Execution" [SpecBuilderSpecs.exampleExecutionSpecs]
        let success6 = runSpec "SpecBuilderSpecs - Complex Nesting" [SpecBuilderSpecs.complexNestingSpecs]
        let success7 = runSpec "AsyncSupportSpecs - Async Test Support" [AsyncSupportSpecs.asyncSupportSpecs]
        
        printfn ""
        printfn "╔═══════════════════════════════════════════════════════════╗"
        if success1 && success2 && success3 && success4 && success5 && success6 && success7 then
            Console.ForegroundColor <- ConsoleColor.Green
            printfn "║                 All tests passed! ✓                       ║"
            Console.ResetColor()
            printfn "║                                                           ║"
            printfn "║  🎉 FxSpec successfully tests itself using FxSpec! 🎉    ║"
            printfn "║                                                           ║"
            printfn "║  💡 Tip: Use ./run-tests.sh for beautiful output!        ║"
            printfn "╚═══════════════════════════════════════════════════════════╝"
            0 // Success
        else
            Console.ForegroundColor <- ConsoleColor.Red
            printfn "║                 Some tests failed! ✗                      ║"
            Console.ResetColor()
            printfn "╚═══════════════════════════════════════════════════════════╝"
            1 // Failure
    with ex ->
        printfn ""
        printfn "╔═══════════════════════════════════════════════════════════╗"
        Console.ForegroundColor <- ConsoleColor.Red
        printfn "║                 Test execution failed! ✗                  ║"
        Console.ResetColor()
        printfn "╚═══════════════════════════════════════════════════════════╝"
        printfn ""
        printfn "Error: %s" ex.Message
        printfn ""
        printfn "Stack trace:"
        printfn "%s" ex.StackTrace
        1 // Failure
