namespace FxSpec.Runner

open System

/// Command-line interface for the FxSpec test runner.
module Program =
    
    /// Output format options.
    type OutputFormat =
        | Documentation  // Rich Spectre.Console output (default)
        | Simple         // Simple text output

    /// Command-line options.
    type Options = {
        AssemblyPath: string option
        Filter: string option
        Verbose: bool
        Format: OutputFormat
        Seed: int option
    }

    /// Default options.
    let defaultOptions = {
        AssemblyPath = None
        Filter = None
        Verbose = false
        Format = Documentation
        Seed = None
    }
    
    /// Parses command-line arguments.
    let rec parseArgs (args: string list) (options: Options) : Options =
        match args with
        | [] -> options
        | "--filter" :: filter :: rest ->
            parseArgs rest { options with Filter = Some filter }
        | "-f" :: filter :: rest ->
            parseArgs rest { options with Filter = Some filter }
        | "--format" :: "simple" :: rest ->
            parseArgs rest { options with Format = Simple }
        | "--format" :: "documentation" :: rest ->
            parseArgs rest { options with Format = Documentation }
        | "--format" :: "doc" :: rest ->
            parseArgs rest { options with Format = Documentation }
        | "--verbose" :: rest ->
            parseArgs rest { options with Verbose = true }
        | "-v" :: rest ->
            parseArgs rest { options with Verbose = true }
        | "--seed" :: seedStr :: rest ->
            match Int32.TryParse(seedStr) with
            | true, seedVal -> parseArgs rest { options with Seed = Some seedVal }
            | _ ->
                printfn "Warning: Invalid seed value '%s'. Ignoring seed." seedStr
                parseArgs rest options
        | "--help" :: _ | "-h" :: _ ->
            printfn "FxSpec Test Runner"
            printfn ""
            printfn "Usage: fxspec <assembly-path> [options]"
            printfn ""
            printfn "Options:"
            printfn "  --filter, -f <pattern>     Filter tests by description"
            printfn "  --format <format>          Output format: documentation (default), simple"
            printfn "  --verbose, -v              Verbose output"
            printfn "  --seed <int>               Seed for randomizing test order"
            printfn "  --help, -h                 Show this help"
            printfn ""
            printfn "Examples:"
            printfn "  fxspec MyTests.dll"
            printfn "  fxspec MyTests.dll --filter \"Calculator\""
            printfn "  fxspec MyTests.dll --format simple"
            printfn "  fxspec MyTests.dll -f \"User\" -v"
            printfn "  fxspec MyTests.dll --seed 12345"
            exit 0
        | path :: rest when not (path.StartsWith("-")) ->
            parseArgs rest { options with AssemblyPath = Some path }
        | unknown :: rest ->
            printfn "Warning: Unknown option '%s'" unknown
            parseArgs rest options
    
    /// Main entry point.
    [<EntryPoint>]
    let main argv =
        try
            printfn "FxSpec Test Runner"
            printfn "=================="
            printfn ""
            
            // Parse arguments
            let options = parseArgs (Array.toList argv) defaultOptions
            
            // Validate assembly path
            match options.AssemblyPath with
            | None ->
                printfn "Error: No assembly path provided"
                printfn "Usage: fxspec <assembly-path> [options]"
                printfn "Run 'fxspec --help' for more information"
                1
            | Some assemblyPath ->
                // Discover tests
                let tests = Discovery.discoverTestsFromFile assemblyPath
                
                if List.isEmpty tests then
                    printfn "No tests found in assembly"
                    0
                else
                    // Apply filter if provided
                    let filteredTests =
                        match options.Filter with
                        | Some filter ->
                            printfn "Filtering tests by: %s" filter
                            let filtered = Discovery.filterTests filter tests
                            let count = Discovery.countExamples filtered
                            printfn "Running %d filtered examples" count
                            printfn ""
                            filtered
                        | None ->
                            printfn ""
                            tests
                    
                    if List.isEmpty filteredTests then
                        printfn "No tests match the filter"
                        0
                    else
                        // Determine seed for randomization
                        let seed = 
                            match options.Seed with
                            | Some s -> s
                            | None -> 
                                let rand = System.Random()
                                rand.Next()

                        printfn "Running tests with seed: %d" seed
                        printfn ""

                        // Shuffle tests if a seed is provided
                        let shuffledTests = Shuffle.shuffle seed filteredTests

                        // Execute tests
                        let (results, summary) = Executor.executeWithSummary shuffledTests

                        // Print results based on format
                        match options.Format with
                        | Documentation ->
                            DocumentationFormatter.render results
                        | Simple ->
                            SimpleFormatter.printResults results summary
                            ExecutionSummary.exitCode summary
        with ex ->
            printfn "Fatal error: %s" ex.Message
            if argv |> Array.contains "--verbose" || argv |> Array.contains "-v" then
                printfn "Stack trace: %s" ex.StackTrace
            1

