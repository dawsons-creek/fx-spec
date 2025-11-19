/// Formats exception stack traces with clickable file links
module FX.Spec.Runner.StackTraceFormatter

open System
open System.Text.RegularExpressions
open Spectre.Console

/// Represents a parsed stack frame
type StackFrame = {
    MethodName: string
    FileName: string option
    LineNumber: int option
}

/// Maximum number of stack frames to show
let private maxStackFramesToShow = 10

/// Filters out framework and test runner frames from stack trace
let private isRelevantFrame (line: string) =
    not (line.Contains("FX.Spec.Runner") ||
         line.Contains("FX.Spec.Core.SpecHelpers") ||
         line.Contains("System.Reflection") ||
         line.Contains("System.Runtime") ||
         line.Contains("Microsoft.FSharp.Core") ||
         line.Contains("System.Threading"))

/// Parses a .NET stack trace line
/// Typical format: "   at Namespace.Class.Method() in /path/to/file.fs:line 42"
let private parseStackTraceLine (line: string) : StackFrame option =
    let line = line.Trim()
    
    if not (line.StartsWith("at ")) then
        None
    else
        // Remove "at " prefix
        let content = line.Substring(3)
        
        // Try to match pattern: "Method() in /path/file.fs:line 123"
        let pattern = @"^(.+?)\s+in\s+(.+?):line\s+(\d+)$"
        let m = Regex.Match(content, pattern)
        
        if m.Success then
            Some {
                MethodName = m.Groups.[1].Value
                FileName = Some m.Groups.[2].Value
                LineNumber = Some (Int32.Parse(m.Groups.[3].Value))
            }
        else
            // No file info, just method name
            Some {
                MethodName = content
                FileName = None
                LineNumber = None
            }

/// Creates a clickable file link for VS Code
let private createFileLink (filePath: string) (lineNumber: int) : string =
    // VS Code URL scheme: vscode://file/path:line:column
    // Also support file:// for other editors
    sprintf "vscode://file%s:%d:0" filePath lineNumber

/// Formats a single stack frame with clickable link
let private formatStackFrame (indent: string) (frame: StackFrame) : unit =
    match frame.FileName, frame.LineNumber with
    | Some file, Some line ->
        // Create clickable link
        let link = createFileLink file line
        let fileName = System.IO.Path.GetFileName(file)
        let directory = System.IO.Path.GetDirectoryName(file)
        
        // Format: "  at MethodName"
        AnsiConsole.Markup(sprintf "%s[dim]at[/] [yellow]%s[/]" indent (Markup.Escape(frame.MethodName)))
        AnsiConsole.WriteLine()
        
        // Format: "     in file.fs:42 (full/path)" (clickable)
        AnsiConsole.Markup(sprintf "%s   [dim]in[/] " indent)
        AnsiConsole.Markup(sprintf "[link=%s][cyan underline]%s:%d[/][/]" link (Markup.Escape(fileName)) line)
        
        // Show abbreviated directory path
        if not (String.IsNullOrEmpty(directory)) then
            let dirName = System.IO.Path.GetFileName(directory)
            if not (String.IsNullOrEmpty(dirName)) then
                AnsiConsole.Markup(sprintf " [dim](%s)[/]" (Markup.Escape(dirName)))
        
        AnsiConsole.WriteLine()
    | Some file, None ->
        let fileName = System.IO.Path.GetFileName(file)
        AnsiConsole.Markup(sprintf "%s[dim]at[/] [yellow]%s[/]" indent (Markup.Escape(frame.MethodName)))
        AnsiConsole.WriteLine()
        AnsiConsole.MarkupLine(sprintf "%s   [dim]in[/] [cyan]%s[/]" indent (Markup.Escape(fileName)))
    | None, _ ->
        AnsiConsole.MarkupLine(sprintf "%s[dim]at[/] [yellow]%s[/]" indent (Markup.Escape(frame.MethodName)))

/// Formats the full stack trace with clickable links
let formatStackTrace (indent: int) (ex: Exception) : unit =
    if String.IsNullOrWhiteSpace(ex.StackTrace) then
        ()
    else
        let indentStr = String(' ', indent * 2)
        
        // Parse and filter stack trace lines
        let frames =
            ex.StackTrace.Split('\n')
            |> Array.filter isRelevantFrame
            |> Array.choose parseStackTraceLine
            |> Array.truncate maxStackFramesToShow
        
        if frames.Length > 0 then
            AnsiConsole.WriteLine()
            AnsiConsole.MarkupLine(sprintf "%s[dim]Stack trace:[/]" indentStr)
            
            frames |> Array.iter (formatStackFrame (indentStr + "  "))
            
            // Show if there are more frames
            if ex.StackTrace.Split('\n').Length > maxStackFramesToShow then
                AnsiConsole.MarkupLine(sprintf "%s  [dim]... (%d more frames)[/]" 
                    indentStr 
                    (ex.StackTrace.Split('\n').Length - maxStackFramesToShow))

/// Creates a formatted error panel for regular exceptions with stack trace
let createExceptionPanel (indent: int) (ex: Exception) : unit =
    let indentStr = String(' ', indent * 2)
    
    // Show exception type and message
    let exType = ex.GetType().Name
    AnsiConsole.MarkupLine(sprintf "%s[red bold]%s:[/] [red]%s[/]" 
        indentStr 
        (Markup.Escape(exType))
        (Markup.Escape(ex.Message)))
    
    // Show stack trace with clickable links
    formatStackTrace indent ex
