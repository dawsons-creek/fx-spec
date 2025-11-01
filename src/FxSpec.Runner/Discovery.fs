namespace FxSpec.Runner

open System
open System.Reflection
open FxSpec.Core

/// Module for discovering tests in assemblies using reflection.
module Discovery =

    /// Checks if a member has the [<Tests>] attribute.
    let private hasTestsAttribute (member': MemberInfo) : bool =
        member'.GetCustomAttributes(typeof<TestsAttribute>, false).Length > 0

    /// Checks if a property type is TestNode list.
    let private isTestNodeListProperty (prop: PropertyInfo) : bool =
        prop.PropertyType = typeof<TestNode list>

    /// Checks if a field type is TestNode list.
    let private isTestNodeListField (field: FieldInfo) : bool =
        field.FieldType = typeof<TestNode list>

    /// Extracts test nodes from a property value.
    let private extractPropertyValue (prop: PropertyInfo) : TestNode array =
        try
            let value = prop.GetValue(null) :?> TestNode list
            value |> List.toArray
        with ex ->
            printfn "Warning: Could not extract property value from %s: %s" prop.Name ex.Message
            [||]

    /// Extracts test nodes from a field value.
    let private extractFieldValue (field: FieldInfo) : TestNode array =
        try
            let value = field.GetValue(null) :?> TestNode list
            value |> List.toArray
        with ex ->
            printfn "Warning: Could not extract field value from %s: %s" field.Name ex.Message
            [||]

    /// Discovers test nodes from properties in a type.
    let private discoverFromProperties (typ: Type) : TestNode array =
        let bindingFlags = BindingFlags.Public ||| BindingFlags.Static
        typ.GetProperties(bindingFlags)
        |> Array.filter hasTestsAttribute
        |> Array.filter isTestNodeListProperty
        |> Array.collect extractPropertyValue

    /// Discovers test nodes from fields in a type.
    let private discoverFromFields (typ: Type) : TestNode array =
        let bindingFlags = BindingFlags.Public ||| BindingFlags.Static
        typ.GetFields(bindingFlags)
        |> Array.filter hasTestsAttribute
        |> Array.filter isTestNodeListField
        |> Array.collect extractFieldValue

    /// Discovers test nodes from a single type.
    let private discoverFromType (typ: Type) : TestNode array =
        try
            let fromProperties = discoverFromProperties typ
            let fromFields = discoverFromFields typ
            Array.append fromProperties fromFields
        with ex ->
            printfn "Warning: Could not inspect type %s: %s" typ.FullName ex.Message
            [||]

    /// Discovers all test suites in the given assembly.
    /// Looks for static let-bound values of type TestNode list marked with [<Tests>].
    /// Applies focused filtering if any focused tests are found.
    let discoverTests (assembly: Assembly) : TestNode list =
        try
            let allTests =
                assembly.GetTypes()
                |> Array.collect discoverFromType
                |> Array.toList

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
                | Group (desc, hooks, children) | FocusedGroup (desc, hooks, children) ->
                    // Check if group description matches
                    let descMatches = desc.ToLowerInvariant().Contains(filterLower)

                    // If group description matches, include all children
                    // Otherwise, recursively filter children
                    if descMatches then
                        Some (Group (desc, hooks, children))
                    else
                        let filteredChildren = filterTests filter children
                        // Include group if any children match
                        if not (List.isEmpty filteredChildren) then
                            Some (Group (desc, hooks, filteredChildren))
                        else
                            None
                | BeforeAllHook _ | BeforeEachHook _ | AfterEachHook _ | AfterAllHook _ ->
                    // Skip hook nodes during filtering
                    None
            )
    
    /// Counts total examples in a list of test nodes.
    let countExamples (nodes: TestNode list) : int =
        nodes |> List.sumBy TestNode.countExamples
    
    /// Counts total groups in a list of test nodes.
    let countGroups (nodes: TestNode list) : int =
        nodes |> List.sumBy TestNode.countGroups

