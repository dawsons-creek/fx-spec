/// Beautiful test output formatter using Spectre.Console
module FxSpec.Runner.DocumentationFormatter

open System
open FxSpec.Core
open Spectre.Console

/// Duration threshold (ms) for fast tests (shown in grey)
let private fastTestThresholdMs = 10.0

/// Duration threshold (ms) for medium tests (shown in yellow)
let private mediumTestThresholdMs = 100.0

/// Builds the full path to a test (e.g., "SpecBuilder > simple examples > creates a single Example node")
let rec buildTestPath (path: string list) (node: TestResultNode) : (string * TestResult * TimeSpan) list =
    match node with
    | ExampleResult (desc, result, duration) ->
        let fullPath = (path @ [desc]) |> String.concat " > "
        [(fullPath, result, duration)]
    | GroupResult (desc, children) ->
        let newPath = path @ [desc]
        children |> List.collect (buildTestPath newPath)

/// Gets a color for a test result
let getResultColor = function
    | TestResult.Pass -> Color.Green
    | TestResult.Fail _ -> Color.Red
    | TestResult.Skipped _ -> Color.Yellow

/// Gets a symbol for a test result
let getResultSymbol = function
    | TestResult.Pass -> "✓"
    | TestResult.Fail _ -> "✗"
    | TestResult.Skipped _ -> "⊘"

/// Formats duration with color based on speed
let formatDuration (duration: TimeSpan) =
    let ms = duration.TotalMilliseconds
    let color =
        if ms < fastTestThresholdMs then Color.Grey
        elif ms < mediumTestThresholdMs then Color.Yellow
        else Color.Red

    Markup(sprintf "[%s](%dms)[/]" (color.ToString().ToLower()) (int ms))

/// Renders a single test result line
let renderTestLine (indent: int) (desc: string) (result: TestResult) (duration: TimeSpan) =
    let indentStr = String(' ', indent * 2)
    let symbol = getResultSymbol result
    let color = getResultColor result
    
    let markup = Markup(sprintf "%s[%s]%s[/] %s " 
        indentStr 
        (color.ToString().ToLower())
        symbol
        (Markup.Escape(desc)))
    
    let grid = Grid()
    grid.AddColumn(GridColumn().NoWrap()) |> ignore
    grid.AddColumn(GridColumn().NoWrap()) |> ignore
    
    grid.AddRow(markup, formatDuration duration) |> ignore
    
    AnsiConsole.Write(grid)

/// Renders a group header
let renderGroupHeader (indent: int) (desc: string) =
    let indentStr = String(' ', indent * 2)
    AnsiConsole.MarkupLine(sprintf "%s[bold]%s[/]" indentStr (Markup.Escape(desc)))

/// Renders failure details with diff
let renderFailureDetails (indent: int) (fullPath: string) (result: TestResult) =
    match result with
    | TestResult.Fail (Some ex) ->
        let indentStr = String(' ', (indent + 1) * 2)
        
        // Show the full test path
        AnsiConsole.WriteLine()
        AnsiConsole.MarkupLine(sprintf "%s[dim]%s[/]" indentStr (Markup.Escape(fullPath)))
        AnsiConsole.WriteLine()
        
        // Check if it's an AssertionException with expected/actual values
        match ex with
        | :? FxSpec.Matchers.AssertionException as assertEx ->
            // Use DiffFormatter for rich diff display
            let message = assertEx.Message
            let expected = assertEx.Expected
            let actual = assertEx.Actual
            
            // Indent the panel
            for _ in 1 .. indent + 1 do
                AnsiConsole.Write("  ")
            
            DiffFormatter.renderFailure message expected actual
            AnsiConsole.WriteLine()
        | _ ->
            // Regular exception - just show the message
            AnsiConsole.MarkupLine(sprintf "%s[red]%s[/]" indentStr (Markup.Escape(ex.Message)))
            AnsiConsole.WriteLine()
    | TestResult.Fail None ->
        let indentStr = String(' ', (indent + 1) * 2)
        AnsiConsole.MarkupLine(sprintf "%s[red]Test failed with no exception[/]" indentStr)
        AnsiConsole.WriteLine()
    | TestResult.Skipped reason ->
        let indentStr = String(' ', (indent + 1) * 2)
        AnsiConsole.MarkupLine(sprintf "%s[yellow]Skipped: %s[/]" indentStr (Markup.Escape(reason)))
        AnsiConsole.WriteLine()
    | TestResult.Pass -> ()

/// Recursively renders a test result tree
let rec renderNode (indent: int) (pathSoFar: string list) (isLast: bool) (node: TestResultNode) =
    match node with
    | ExampleResult (desc, result, duration) ->
        let fullPath = (pathSoFar @ [desc]) |> String.concat " > "
        renderTestLine indent desc result duration
        AnsiConsole.WriteLine()

        // Show failure details if test failed
        if TestResult.isFail result then
            renderFailureDetails indent fullPath result

        // Show skip reason if test was skipped
        if TestResult.isSkipped result then
            match result with
            | TestResult.Skipped reason ->
                let indentStr = String(' ', (indent + 1) * 2)
                AnsiConsole.MarkupLine(sprintf "%s[dim]%s[/]" indentStr (Markup.Escape(reason)))
            | _ -> ()

    | GroupResult (desc, children) ->
        renderGroupHeader indent desc
        let newPath = pathSoFar @ [desc]
        let childCount = List.length children
        children |> List.iteri (fun i child -> 
            renderNode (indent + 1) newPath (i = childCount - 1) child)
        
        // Add blank line after group (unless it's the last node)
        if not isLast then
            AnsiConsole.WriteLine()

/// Creates a summary table with statistics
let createSummaryTable (passed: int) (failed: int) (skipped: int) (duration: TimeSpan) =
    let table = Table()
    table.Border <- TableBorder.Rounded
    table.BorderColor(if failed > 0 then Color.Red else Color.Green) |> ignore
    
    table.AddColumn(TableColumn("[bold]Total[/]").Centered()) |> ignore
    table.AddColumn(TableColumn("[bold green]Passed[/]").Centered()) |> ignore
    table.AddColumn(TableColumn("[bold red]Failed[/]").Centered()) |> ignore
    table.AddColumn(TableColumn("[bold yellow]Skipped[/]").Centered()) |> ignore
    table.AddColumn(TableColumn("[bold]Duration[/]").Centered()) |> ignore

    let total = passed + failed + skipped
    table.AddRow(
        Markup(sprintf "[bold]%d[/]" total),
        Markup(sprintf "[green]%d[/]" passed),
        Markup(sprintf "[red]%d[/]" failed),
        Markup(sprintf "[yellow]%d[/]" skipped),
        Markup(sprintf "[bold]%.2fs[/]" duration.TotalSeconds)
    ) |> ignore

    table

/// Renders the complete test results with beautiful formatting
let render (results: TestResultNode list) =
    AnsiConsole.WriteLine()
    
    // Render all test results
    let resultCount = List.length results
    results |> List.iteri (fun i node -> renderNode 0 [] (i = resultCount - 1) node)
    
    // Calculate statistics
    let allResults = results |> List.collect (buildTestPath [])
    let passed = allResults |> List.filter (fun (_, r, _) -> TestResult.isPass r) |> List.length
    let failed = allResults |> List.filter (fun (_, r, _) -> TestResult.isFail r) |> List.length
    let skipped = allResults |> List.filter (fun (_, r, _) -> TestResult.isSkipped r) |> List.length
    let totalDuration = allResults |> List.sumBy (fun (_, _, d) -> d.TotalSeconds) |> TimeSpan.FromSeconds
    
    // Render summary
    AnsiConsole.WriteLine()
    let summaryTable = createSummaryTable passed failed skipped totalDuration
    AnsiConsole.Write(summaryTable :> Rendering.IRenderable)
    AnsiConsole.WriteLine()
    
    // Return exit code
    if failed > 0 then 1 else 0

