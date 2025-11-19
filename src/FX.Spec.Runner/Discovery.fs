namespace FX.Spec.Runner

open System
open System.Reflection
open FX.Spec.Core

/// Module for discovering tests in assemblies using reflection.
module Discovery =

    /// Checks if a member has the [<Tests>] attribute.
    let private hasTestsAttribute (member': MemberInfo) : bool =
        member'.GetCustomAttributes(typeof<TestsAttribute>, false).Length > 0

    /// Checks if a property type is TestNode list.
    let private isTestNodeListProperty (prop: PropertyInfo) : bool =
        prop.PropertyType = typeof<TestNode list>

    /// Checks if a property type is a single TestNode.
    let private isTestNodeProperty (prop: PropertyInfo) : bool = prop.PropertyType = typeof<TestNode>

    /// Checks if a field type is TestNode list.
    let private isTestNodeListField (field: FieldInfo) : bool = field.FieldType = typeof<TestNode list>

    /// Checks if a field type is a single TestNode.
    let private isTestNodeField (field: FieldInfo) : bool = field.FieldType = typeof<TestNode>

    /// Extracts test nodes from a property value (TestNode list).
    let private extractPropertyValue (prop: PropertyInfo) : TestNode array =
        try
            let value = prop.GetValue(null) :?> TestNode list
            value |> List.toArray
        with ex ->
            printfn "Warning: Could not extract property value from %s: %s" prop.Name ex.Message
            [||]

    /// Extracts a single test node from a property value.
    let private extractSinglePropertyValue (prop: PropertyInfo) : TestNode array =
        try
            let value = prop.GetValue(null) :?> TestNode
            [| value |]
        with ex ->
            printfn "Warning: Could not extract property value from %s: %s" prop.Name ex.Message
            [||]

    /// Extracts test nodes from a field value (TestNode list).
    let private extractFieldValue (field: FieldInfo) : TestNode array =
        try
            let value = field.GetValue(null) :?> TestNode list
            value |> List.toArray
        with ex ->
            printfn "Warning: Could not extract field value from %s: %s" field.Name ex.Message
            [||]

    /// Extracts a single test node from a field value.
    let private extractSingleFieldValue (field: FieldInfo) : TestNode array =
        try
            let value = field.GetValue(null) :?> TestNode
            [| value |]
        with ex ->
            printfn "Warning: Could not extract field value from %s: %s" field.Name ex.Message
            [||]

    /// Discovers test nodes from properties in a type.
    let private discoverFromProperties (typ: Type) : TestNode array =
        let bindingFlags = BindingFlags.Public ||| BindingFlags.Static
        let props = typ.GetProperties(bindingFlags) |> Array.filter hasTestsAttribute

        let fromLists =
            props
            |> Array.filter isTestNodeListProperty
            |> Array.collect extractPropertyValue

        let fromSingle =
            props
            |> Array.filter isTestNodeProperty
            |> Array.collect extractSinglePropertyValue

        Array.append fromLists fromSingle

    /// Discovers test nodes from fields in a type.
    let private discoverFromFields (typ: Type) : TestNode array =
        let bindingFlags = BindingFlags.Public ||| BindingFlags.Static
        let fields = typ.GetFields(bindingFlags) |> Array.filter hasTestsAttribute

        let fromLists =
            fields |> Array.filter isTestNodeListField |> Array.collect extractFieldValue

        let fromSingle =
            fields |> Array.filter isTestNodeField |> Array.collect extractSingleFieldValue

        Array.append fromLists fromSingle

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
            let allTests = assembly.GetTypes() |> Array.collect discoverFromType |> Array.toList

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

    /// Filters a single test node by description (case-insensitive substring match).
    let rec private filterTestNode (filter: string) (filterLower: string) (node: TestNode) : TestNode option =
        match node with
        | Example(desc, test, meta)
        | FocusedExample(desc, test, meta) when desc.ToLowerInvariant().Contains(filterLower) ->
            Some(Example(desc, test, meta))
        | Group(desc, hooks, children, meta)
        | FocusedGroup(desc, hooks, children, meta) when desc.ToLowerInvariant().Contains(filterLower) ->
            Some(Group(desc, hooks, children, meta))
        | Group(desc, hooks, children, meta)
        | FocusedGroup(desc, hooks, children, meta) ->
            match filterTests filter children with
            | [] -> None
            | filteredChildren -> Some(Group(desc, hooks, filteredChildren, meta))
        | _ -> None

    /// Filters test nodes by description (case-insensitive substring match).
    and filterTests (filter: string) (nodes: TestNode list) : TestNode list =
        if String.IsNullOrWhiteSpace(filter) then
            nodes
        else
            let filterLower = filter.ToLowerInvariant()
            nodes |> List.choose (filterTestNode filter filterLower)

    /// Counts total examples in a list of test nodes.
    let countExamples (nodes: TestNode list) : int =
        nodes |> List.sumBy TestNode.countExamples

    /// Counts total groups in a list of test nodes.
    let countGroups (nodes: TestNode list) : int =
        nodes |> List.sumBy TestNode.countGroups
