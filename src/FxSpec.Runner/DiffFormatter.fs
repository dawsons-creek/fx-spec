/// Formats expected vs actual diffs for better failure messages
module FxSpec.Runner.DiffFormatter

open System
open Spectre.Console

/// Maximum number of collection items to show in error messages
let private maxCollectionItemsToShow = 10

/// Formats a value for display in error messages
let rec formatValue (value: obj option) =
    match value with
    | None -> "[no value]"
    | Some null -> "null"
    | Some v ->
        match v with
        | :? string as s -> sprintf "\"%s\"" s
        | :? int as i -> string i
        | :? float as f -> string f
        | :? bool as b -> if b then "true" else "false"
        | :? System.Collections.IEnumerable as enumerable ->
            let items =
                seq { for item in enumerable -> formatValue (Some item) }
                |> Seq.truncate maxCollectionItemsToShow
                |> String.concat ", "
            sprintf "[%s]" items
        | _ -> v.ToString()

/// Creates a panel showing expected vs actual values
let createDiffPanel (expected: obj option) (actual: obj option) =
    let table = Table()
    table.Border <- TableBorder.Rounded
    table.BorderColor(Color.Grey) |> ignore

    // Add columns
    table.AddColumn(TableColumn("[bold]Expected[/]").Centered()) |> ignore
    table.AddColumn(TableColumn("[bold]Actual[/]").Centered()) |> ignore
    
    // Format values
    let expectedStr = formatValue expected
    let actualStr = formatValue actual
    
    // Add row with colored values
    table.AddRow(
        Markup(sprintf "[green]%s[/]" (Markup.Escape(expectedStr))),
        Markup(sprintf "[red]%s[/]" (Markup.Escape(actualStr)))
    ) |> ignore
    
    table

/// Creates a detailed failure panel with message and diff
let createFailurePanel (message: string) (expected: obj option) (actual: obj option) =
    let rows = Rows(
        Markup(sprintf "[yellow]%s[/]" (Markup.Escape(message))),
        Text(""),
        createDiffPanel expected actual
    )

    let panel = Panel(rows)
    panel.Header <- PanelHeader("[red bold]✗ Assertion Failed[/]")
    panel.Border <- BoxBorder.Rounded
    panel.BorderColor(Color.Red) |> ignore
    panel.Padding <- Padding(1, 1, 1, 1)

    panel

/// Renders a failure message with diff to the console
let renderFailure (message: string) (expected: obj option) (actual: obj option) =
    let panel = createFailurePanel message expected actual
    AnsiConsole.Write(panel)

/// Creates a simple text representation of a diff (for non-Spectre output)
let formatDiffText (message: string) (expected: obj option) (actual: obj option) =
    let lines = [
        ""
        message
        ""
        sprintf "Expected: %s" (formatValue expected)
        sprintf "Actual:   %s" (formatValue actual)
        ""
    ]
    String.concat Environment.NewLine lines

/// Compares two strings and highlights differences
let compareStrings (expected: string) (actual: string) =
    if expected = actual then
        None
    else
        let maxLen = max expected.Length actual.Length
        let firstDiff =
            seq { 0 .. maxLen - 1 }
            |> Seq.tryFindIndex (fun i ->
                let expChar = if i < expected.Length then Some expected.[i] else None
                let actChar = if i < actual.Length then Some actual.[i] else None
                expChar <> actChar)

        firstDiff |> Option.map (sprintf "First difference at position %d")

/// Creates a rich comparison for strings with character-level diff
let createStringDiffPanel (expected: string) (actual: string) =
    let table = Table()
    table.Border <- TableBorder.Rounded
    table.BorderColor(Color.Grey) |> ignore

    table.AddColumn(TableColumn("[bold]Expected[/]")) |> ignore
    table.AddColumn(TableColumn("[bold]Actual[/]")) |> ignore
    
    // Show the strings
    table.AddRow(
        Markup(sprintf "[green]\"%s\"[/]" (Markup.Escape(expected))),
        Markup(sprintf "[red]\"%s\"[/]" (Markup.Escape(actual)))
    ) |> ignore
    
    // Show lengths if different
    if expected.Length <> actual.Length then
        table.AddRow(
            Markup(sprintf "[dim]Length: %d[/]" expected.Length),
            Markup(sprintf "[dim]Length: %d[/]" actual.Length)
        ) |> ignore
    
    // Show first difference
    match compareStrings expected actual with
    | Some diff -> 
        table.AddRow(
            Markup(sprintf "[dim]%s[/]" (Markup.Escape(diff))),
            Text("")
        ) |> ignore
    | None -> ()
    
    table

/// Renders a string comparison with detailed diff
let renderStringDiff (message: string) (expected: string) (actual: string) =
    let rows = Rows(
        Markup(sprintf "[yellow]%s[/]" (Markup.Escape(message))),
        Text(""),
        createStringDiffPanel expected actual
    )

    let panel = Panel(rows)
    panel.Header <- PanelHeader("[red bold]✗ String Comparison Failed[/]")
    panel.Border <- BoxBorder.Rounded
    panel.BorderColor(Color.Red) |> ignore
    panel.Padding <- Padding(1, 1, 1, 1)

    AnsiConsole.Write(panel)

