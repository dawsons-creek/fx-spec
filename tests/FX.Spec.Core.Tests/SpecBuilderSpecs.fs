/// FxSpec-based tests for the SpecBuilder DSL.
/// This is dogfooding - using FX.Spec to test itself!
module FX.Spec.Core.Tests.SpecBuilderSpecs

open FX.Spec.Core
open FX.Spec.Matchers
open FX.Spec.Core.Tests.FXSpecMatchers

/// Specs for basic spec builder functionality
[<Tests>]
let specBuilderSpecs =
    describe
        "SpecBuilder"
        [ context
              "simple examples"
              [ it "creates a single Example node" (fun () ->
                    let nodes = [ it "test example" (fun () -> ()) ]

                    expect(List.length nodes).toEqual (1)
                    expectTestNode(List.head nodes).toBeExample ("test example")) ]

          context
              "simple describe blocks"
              [ it "creates a Group with children" (fun () ->
                    let nodes =
                        [ describe "Test Group" [ it "example 1" (fun () -> ()); it "example 2" (fun () -> ()) ] ]

                    expect(List.length nodes).toEqual (1)
                    expectTestNode(List.head nodes).toBeGroup ("Test Group")
                    expectTestNode(List.head nodes).toBeGroupWithChildren (2)) ]

          context
              "nested describe blocks"
              [ it "creates nested Group structures" (fun () ->
                    let nodes =
                        [ describe
                              "Outer"
                              [ describe "Inner" [ it "nested test" (fun () -> ()) ]
                                it "outer test" (fun () -> ()) ] ]

                    expect(List.length nodes).toEqual (1)

                    match List.head nodes with
                    | Group("Outer",
                            _,
                            [ Group("Inner", _, [ Example("nested test", _, _) ], _); Example("outer test", _, _) ],
                            _) -> ()
                    | _ -> failwith "Should create nested groups correctly") ]

          context
              "multiple describe blocks"
              [ it "creates multiple top-level groups" (fun () ->
                    let nodes =
                        [ describe "Group 1" [ it "test 1" (fun () -> ()) ]
                          describe "Group 2" [ it "test 2" (fun () -> ()) ] ]

                    expect(List.length nodes).toEqual (2)
                    expectTestNode(nodes.[0]).toBeGroup ("Group 1")
                    expectTestNode(nodes.[1]).toBeGroup ("Group 2")) ]

          context
              "context alias"
              [ it "works as an alias for describe" (fun () ->
                    let nodes = [ context "Test Context" [ it "test" (fun () -> ()) ] ]

                    expect(List.length nodes).toEqual (1)
                    expectTestNode(List.head nodes).toBeGroup ("Test Context")) ] ]

/// Specs for example execution
[<Tests>]
let exampleExecutionSpecs =
    describe
        "Example execution"
        [ context
              "successful tests"
              [ it "executes the test function" (fun () ->
                    let mutable executed = false
                    let nodes = [ it "test" (fun () -> executed <- true) ]

                    match nodes with
                    | [ Example(_, testFn, _) ] ->
                        let result = testFn () |> Async.RunSynchronously
                        expectBool(executed).toBeTrue ()
                        expectTestResult(result).toBePass ()
                    | _ -> failwith "Should create Example node")

                it "returns Pass for successful tests" (fun () ->
                    let nodes = [ it "test" (fun () -> ()) ]

                    match nodes with
                    | [ Example(_, testFn, _) ] ->
                        let result = testFn () |> Async.RunSynchronously
                        expectTestResult(result).toBePass ()
                    | _ -> failwith "Should create Example node") ]

          context
              "failing tests"
              [ it "returns Fail with exception for failed tests" (fun () ->
                    let nodes = [ it "failing test" (fun () -> failwith "intentional failure") ]

                    match nodes with
                    | [ Example(_, testFn, _) ] ->
                        let result = testFn () |> Async.RunSynchronously
                        expectTestResult(result).toBeFail ()
                        expectTestResult(result).toBeFailWith ("intentional failure")
                    | _ -> failwith "Should create Example node") ] ]

/// Specs for complex nesting scenarios
[<Tests>]
let complexNestingSpecs =
    describe
        "Complex nesting"
        [ it "handles deeply nested structures correctly" (fun () ->
              let nodes =
                  [ describe
                        "Feature"
                        [ context "Scenario 1" [ it "test 1" (fun () -> ()); it "test 2" (fun () -> ()) ]
                          context "Scenario 2" [ describe "Nested" [ it "test 3" (fun () -> ()) ] ] ] ]

              expect(List.length nodes).toEqual (1)

              let exampleCount = nodes |> List.sumBy TestNode.countExamples
              let groupCount = nodes |> List.sumBy TestNode.countGroups

              expectNum(exampleCount).toEqual (3)
              expectNum(groupCount).toEqual (4)) ]
