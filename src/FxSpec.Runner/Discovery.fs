namespace FxSpec.Runner

open System
open System.Reflection
open FxSpec.Core

/// Module for discovering tests in assemblies using reflection.
module Discovery =
    
    /// Discovers all test suites in the given assembly.
    /// Looks for static let-bound values of type TestNode list marked with [<Tests>].
    /// Applies focused filtering if any focused tests are found.
    let discoverTests (assembly: Assembly) : TestNode list =
        try
            // Get all types in the assembly
            let types = assembly.GetTypes()

            // For each type, find properties/fields marked with [<Tests>]
            let allTests =
                types
                |> Array.collect (fun typ ->
                    try
                        // Get all public static properties and fields
                        let bindingFlags = BindingFlags.Public ||| BindingFlags.Static
                        let properties = typ.GetProperties(bindingFlags)
                        let fields = typ.GetFields(bindingFlags)

                        // Check properties
                        let testProperties =
                            properties
                            |> Array.filter (fun prop ->
                                // Check if it has the [<Tests>] attribute
                                prop.GetCustomAttributes(typeof<TestsAttribute>, false).Length > 0
                            )
                            |> Array.filter (fun prop ->
                                // Check if it returns TestNode list
                                prop.PropertyType = typeof<TestNode list>
                            )
                            |> Array.map (fun prop ->
                                // Get the value
                                prop.GetValue(null) :?> TestNode list
                            )
                            |> Array.collect (fun nodes -> nodes |> List.toArray)

                        // Check fields
                        let testFields =
                            fields
                            |> Array.filter (fun field ->
                                // Check if it has the [<Tests>] attribute
                                field.GetCustomAttributes(typeof<TestsAttribute>, false).Length > 0
                            )
                            |> Array.filter (fun field ->
                                // Check if it's TestNode list
                                field.FieldType = typeof<TestNode list>
                            )
                            |> Array.map (fun field ->
                                // Get the value
                                field.GetValue(null) :?> TestNode list
                            )
                            |> Array.collect (fun nodes -> nodes |> List.toArray)

                        Array.append testProperties testFields
                    with ex ->
                        // If we can't inspect a type, skip it
                        printfn "Warning: Could not inspect type %s: %s" typ.FullName ex.Message
                        [||]
                )
                |> Array.toList

            // Apply focused filtering if any focused tests exist
            TestNode.filterFocused allTests
        with ex ->
            printfn "Error discovering tests: %s" ex.Message
            []
    
    /// Loads an assembly from a file path and discovers tests.
    let discoverTestsFromFile (assemblyPath: string) : TestNode list =
        try
            if not (IO.File.Exists(assemblyPath)) then
                printfn "Error: Assembly file not found: %s" assemblyPath
                []
            else
                printfn "Loading assembly: %s" assemblyPath
                let assembly = Assembly.LoadFrom(assemblyPath)
                printfn "Discovering tests..."
                let tests = discoverTests assembly
                let exampleCount = tests |> List.sumBy TestNode.countExamples
                let groupCount = tests |> List.sumBy TestNode.countGroups
                printfn "Found %d examples in %d groups" exampleCount groupCount
                tests
        with ex ->
            printfn "Error loading assembly: %s" ex.Message
            printfn "Stack trace: %s" ex.StackTrace
            []
    
    /// Filters test nodes by description (case-insensitive substring match).
    let rec filterTests (filter: string) (nodes: TestNode list) : TestNode list =
        if String.IsNullOrWhiteSpace(filter) then
            nodes
        else
            let filterLower = filter.ToLowerInvariant()
            
            nodes
            |> List.choose (fun node ->
                match node with
                | Example (desc, test) | FocusedExample (desc, test) ->
                    if desc.ToLowerInvariant().Contains(filterLower) then
                        Some (Example (desc, test))
                    else
                        None
                | Group (desc, children) | FocusedGroup (desc, children) ->
                    // Check if group description matches
                    let descMatches = desc.ToLowerInvariant().Contains(filterLower)

                    // If group description matches, include all children
                    // Otherwise, recursively filter children
                    if descMatches then
                        Some (Group (desc, children))
                    else
                        let filteredChildren = filterTests filter children
                        // Include group if any children match
                        if not (List.isEmpty filteredChildren) then
                            Some (Group (desc, filteredChildren))
                        else
                            None
            )
    
    /// Counts total examples in a list of test nodes.
    let countExamples (nodes: TestNode list) : int =
        nodes |> List.sumBy TestNode.countExamples
    
    /// Counts total groups in a list of test nodes.
    let countGroups (nodes: TestNode list) : int =
        nodes |> List.sumBy TestNode.countGroups

