module FxSpec.Core.Tests.SpecBuilderTests

open FxSpec.Core

// Test that spec builder creates correct structure
let testSimpleExample() =
    let nodes = spec {
        yield it "test example" (fun () -> ())
    }
    
    match nodes with
    | [Example(desc, _)] when desc = "test example" -> ()
    | _ -> failwith "Should create a single Example node"

let testSimpleDescribe() =
    let nodes = spec {
        yield describe "Test Group" [
            it "example 1" (fun () -> ())
            it "example 2" (fun () -> ())
        ]
    }
    
    match nodes with
    | [Group(desc, children)] when desc = "Test Group" && List.length children = 2 -> ()
    | _ -> failwith "Should create a Group with 2 children"

let testNestedDescribe() =
    let nodes = spec {
        yield describe "Outer" [
            describe "Inner" [
                it "nested test" (fun () -> ())
            ]
        ]
    }
    
    match nodes with
    | [Group("Outer", [Group("Inner", [Example("nested test", _)])])] -> ()
    | _ -> failwith "Should create nested groups correctly"

let testMultipleDescribe() =
    let nodes = spec {
        yield describe "Group 1" [
            it "test 1" (fun () -> ())
        ]
        yield describe "Group 2" [
            it "test 2" (fun () -> ())
        ]
    }
    
    match nodes with
    | [Group("Group 1", _); Group("Group 2", _)] -> ()
    | _ -> failwith "Should create multiple top-level groups"

let testContextAlias() =
    let nodes = spec {
        yield context "Test Context" [
            it "test" (fun () -> ())
        ]
    }
    
    match nodes with
    | [Group("Test Context", _)] -> ()
    | _ -> failwith "context should work as alias for describe"

let testExampleExecution() =
    let mutable executed = false
    let nodes = spec {
        yield it "test" (fun () -> executed <- true)
    }
    
    match nodes with
    | [Example(_, testFn)] ->
        let result = testFn()
        if not executed then
            failwith "Test function should have been executed"
        match result with
        | Pass -> ()
        | _ -> failwith "Successful test should return Pass"
    | _ -> failwith "Should create Example node"

let testExampleFailure() =
    let nodes = spec {
        yield it "failing test" (fun () -> failwith "intentional failure")
    }
    
    match nodes with
    | [Example(_, testFn)] ->
        let result = testFn()
        match result with
        | Fail (Some ex) when ex.Message = "intentional failure" -> ()
        | _ -> failwith "Failed test should return Fail with exception"
    | _ -> failwith "Should create Example node"

let testComplexNesting() =
    let nodes = spec {
        yield describe "Feature" [
            context "Scenario 1" [
                it "test 1" (fun () -> ())
                it "test 2" (fun () -> ())
            ]
            context "Scenario 2" [
                describe "Nested" [
                    it "test 3" (fun () -> ())
                ]
            ]
        ]
    }
    
    let exampleCount = nodes |> List.sumBy TestNode.countExamples
    let groupCount = nodes |> List.sumBy TestNode.countGroups
    
    if exampleCount <> 3 then
        failwith $"Should have 3 examples, got {exampleCount}"
    if groupCount <> 4 then
        failwith $"Should have 4 groups, got {groupCount}"

// Run all tests
let runAllTests() =
    printfn "Running SpecBuilder tests..."
    
    testSimpleExample()
    printfn "  ✓ Simple example creation"
    
    testSimpleDescribe()
    printfn "  ✓ Simple describe block"
    
    testNestedDescribe()
    printfn "  ✓ Nested describe blocks"
    
    testMultipleDescribe()
    printfn "  ✓ Multiple describe blocks"
    
    testContextAlias()
    printfn "  ✓ Context as alias for describe"
    
    testExampleExecution()
    printfn "  ✓ Example execution returns Pass"
    
    testExampleFailure()
    printfn "  ✓ Example failure returns Fail"
    
    testComplexNesting()
    printfn "  ✓ Complex nesting structure"
    
    printfn "\nAll SpecBuilder tests passed! ✓"

