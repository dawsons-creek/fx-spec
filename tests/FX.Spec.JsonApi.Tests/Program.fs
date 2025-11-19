module FX.Spec.JsonApi.Tests.Program

open System
open FX.Spec.Core
open FX.Spec.JsonApi.Tests

let rec private executeNode (node: TestNode) : TestResultNode =
    match node with
    | Example(description, testFn, metadata)
    | FocusedExample(description, testFn, metadata) ->
        let stopwatch = Diagnostics.Stopwatch.StartNew()
        let result =
            try
                testFn ()
            finally
                stopwatch.Stop()

        ExampleResult(description, result, stopwatch.Elapsed, metadata)

    | Group(description, _, children, metadata)
    | FocusedGroup(description, _, children, metadata) ->
        let results = children |> List.map executeNode
        GroupResult(description, results, metadata)

    | BeforeAllHook _
    | BeforeEachHook _
    | AfterEachHook _
    | AfterAllHook _ ->
        GroupResult("hook", [], TestMetadata.empty)

let rec private printResults (indent: string) (node: TestResultNode) =
    match node with
    | ExampleResult(description, result, duration, _) ->
        let symbol, colour =
            match result with
            | Pass -> "✓", ConsoleColor.Green
            | Fail _ -> "✗", ConsoleColor.Red
            | Skipped _ -> "⊘", ConsoleColor.Yellow

        Console.ForegroundColor <- colour
        printfn "%s%s %s (%.2fms)" indent symbol description duration.TotalMilliseconds
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

    | GroupResult(description, results, _) ->
        printfn "%s%s" indent description
        let childIndent = indent + "  "
        results |> List.iter (printResults childIndent)

let private runSpec (name: string) (nodes: TestNode list) =
    printfn ""
    printfn "═══════════════════════════════════════════════════════════"
    printfn "%s" name
    printfn "═══════════════════════════════════════════════════════════"
    printfn ""

    let results = nodes |> List.map executeNode
    results |> List.iter (printResults "")

    let passed = results |> List.sumBy TestResultNode.countPassed
    let failed = results |> List.sumBy TestResultNode.countFailed
    let skipped = results |> List.sumBy TestResultNode.countSkipped
    let total = results |> List.collect TestResultNode.collectResults |> List.length

    printfn ""
    let summaryColour = if failed > 0 then ConsoleColor.Red else ConsoleColor.Green
    Console.ForegroundColor <- summaryColour
    printfn "%d examples, %d failures, %d skipped" total failed skipped
    Console.ResetColor()
    failed = 0

[<EntryPoint>]
let main _ =
    let banner lines =
        printfn "╔═══════════════════════════════════════════════════════════╗"
        lines |> List.iter (fun line -> printfn "║ %-57s ║" line)
        printfn "╚═══════════════════════════════════════════════════════════╝"

    banner
        [ "FX.Spec.JsonApi Test Suite"
          "JSON:API helper specifications"
          "Powered by FX.Spec 🧪" ]

    try
        let suites =
            [ "JsonApiHelpers", JsonApiHelpersSpecs.specs ]

        let results =
            suites
            |> List.map (fun (name, nodes) -> name, runSpec name [ nodes ])

        let allPassed = results |> List.forall snd

        printfn ""
        if allPassed then
            banner [ "All specs passing! 🎉" ]
            0
        else
            banner [ "Some specs failed. ❌" ]
            1
    with ex ->
        printfn ""
        banner [ "Spec execution failed. ❌" ]
        printfn "Error: %s" ex.Message
        printfn ""
        printfn "Stack trace:"
        printfn "%s" ex.StackTrace
        1
