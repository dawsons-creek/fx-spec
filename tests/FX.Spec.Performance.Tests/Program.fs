open System
open FX.Spec.Core
open FX.Spec.Performance.Tests

/// Simple test executor tailored for the performance DSL specs.
/// Mirrors the lightweight runner used in other fx-spec test projects.
module InternalRunner =

    let rec executeNode (node: TestNode) : TestResultNode =
        match node with
        | Example(desc, testFn, meta)
        | FocusedExample(desc, testFn, meta) ->
            let sw = Diagnostics.Stopwatch.StartNew()
            let result = testFn ()
            sw.Stop()
            ExampleResult(desc, result, sw.Elapsed, meta)

        | Group(desc, _, children, meta)
        | FocusedGroup(desc, _, children, meta) ->
            let results = children |> List.map executeNode
            GroupResult(desc, results, meta)

        | BeforeAllHook _
        | BeforeEachHook _
        | AfterEachHook _
        | AfterAllHook _ ->
            // Hooks are ignored in this lightweight runner; they are handled by the main runner.
            GroupResult("hook", [], TestMetadata.empty)

    let rec printResults indent (node: TestResultNode) =
        match node with
        | ExampleResult(desc, result, duration, _) ->
            let (symbol, color) =
                match result with
                | Pass -> ("✓", ConsoleColor.Green)
                | Fail _ -> ("✗", ConsoleColor.Red)
                | Skipped _ -> ("⊘", ConsoleColor.Yellow)

            Console.ForegroundColor <- color
            printfn "%s%s %s (%.2fms)" indent symbol desc (duration.TotalMilliseconds)
            Console.ResetColor()

            match result with
            | Fail(Some ex) ->
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

        | GroupResult(desc, children, _) ->
            printfn "%s%s" indent desc
            children |> List.iter (printResults (indent + "  "))

    let summarize (nodes: TestResultNode list) =
        let results = nodes |> List.collect TestResultNode.collectResults
        let total = results.Length
        let passed = results |> List.filter TestResult.isPass |> List.length
        let failed = results |> List.filter TestResult.isFail |> List.length
        let skipped = results |> List.filter TestResult.isSkipped |> List.length
        total, passed, failed, skipped

    let runSpec name (nodes: TestNode list) =
        printfn ""
        printfn "═════════════════════════════════════════════════════════════"
        printfn "%s" name
        printfn "═════════════════════════════════════════════════════════════"
        printfn ""

        let results = nodes |> List.map executeNode
        results |> List.iter (printResults "")

        let total, _, failed, skipped = summarize results

        printfn ""

        if failed > 0 then
            Console.ForegroundColor <- ConsoleColor.Red
        else
            Console.ForegroundColor <- ConsoleColor.Green

        printfn "%d examples, %d failures, %d skipped" total failed skipped
        Console.ResetColor()

        failed = 0

[<EntryPoint>]
let main _argv =
    printfn "╔═══════════════════════════════════════════════════════════╗"
    printfn "║        FX.Spec Performance DSL – Self Test Suite          ║"
    printfn "║        Exercising perf DSL scaffolding via fx-spec        ║"
    printfn "╚═══════════════════════════════════════════════════════════╝"

    try
        let success =
            InternalRunner.runSpec "Performance DSL scaffolding" [ PerformanceDslSpecs.performanceDslSpecs ]

        if success then
            printfn ""
            Console.ForegroundColor <- ConsoleColor.Green
            printfn "All performance DSL scaffolding specs passed! ✓"
            Console.ResetColor()
            0
        else
            printfn ""
            Console.ForegroundColor <- ConsoleColor.Red
            printfn "Some performance DSL scaffolding specs failed. ✗"
            Console.ResetColor()
            1
    with ex ->
        printfn ""
        Console.ForegroundColor <- ConsoleColor.Red
        printfn "Test execution encountered a fatal error: %s" ex.Message
        Console.ResetColor()
        printfn "%s" ex.StackTrace
        1
