/// FxSpec-based tests for the SpecBuilder DSL.
/// This is dogfooding - using FxSpec to test itself!
module FxSpec.Core.Tests.SpecBuilderSpecs

open FxSpec.Core
open FxSpec.Matchers
open FxSpec.Core.Tests.FxSpecMatchers

/// Specs for basic spec builder functionality
[<Tests>]
let specBuilderSpecs =
    spec {
        yield describe "SpecBuilder" [
            context "simple examples" [
                it "creates a single Example node" (fun () ->
                    let nodes = spec {
                        yield it "test example" (fun () -> ())
                    }

                    expect nodes |> should (haveLength 1)
                    expect (List.head nodes) |> should (beExample "test example")
                )
            ]

            context "simple describe blocks" [
                it "creates a Group with children" (fun () ->
                    let nodes = spec {
                        yield describe "Test Group" [
                            it "example 1" (fun () -> ())
                            it "example 2" (fun () -> ())
                        ]
                    }

                    expect nodes |> should (haveLength 1)
                    expect (List.head nodes) |> should (beGroup "Test Group")
                    expect (List.head nodes) |> should (beGroupWithChildren 2)
                )
            ]

            context "nested describe blocks" [
                it "creates nested Group structures" (fun () ->
                    let nodes = spec {
                        yield describe "Outer" [
                            describe "Inner" [
                                it "nested test" (fun () -> ())
                            ]
                        ]
                    }

                    expect nodes |> should (haveLength 1)

                    match List.head nodes with
                    | Group("Outer", _, [Group("Inner", _, [Example("nested test", _)])]) -> ()
                    | _ -> failwith "Should create nested groups correctly"
                )
            ]

            context "multiple describe blocks" [
                it "creates multiple top-level groups" (fun () ->
                    let nodes = spec {
                        yield describe "Group 1" [
                            it "test 1" (fun () -> ())
                        ]
                        yield describe "Group 2" [
                            it "test 2" (fun () -> ())
                        ]
                    }

                    expect nodes |> should (haveLength 2)
                    expect (nodes.[0]) |> should (beGroup "Group 1")
                    expect (nodes.[1]) |> should (beGroup "Group 2")
                )
            ]

            context "context alias" [
                it "works as an alias for describe" (fun () ->
                    let nodes = spec {
                        yield context "Test Context" [
                            it "test" (fun () -> ())
                        ]
                    }

                    expect nodes |> should (haveLength 1)
                    expect (List.head nodes) |> should (beGroup "Test Context")
                )
            ]
        ]
    }

/// Specs for example execution
[<Tests>]
let exampleExecutionSpecs =
    spec {
        yield describe "Example execution" [
            context "successful tests" [
                it "executes the test function" (fun () ->
                    let mutable executed = false
                    let nodes = spec {
                        yield it "test" (fun () -> executed <- true)
                    }

                    match nodes with
                    | [Example(_, testFn)] ->
                        let result = testFn()
                        expect executed |> should beTrue
                        expect result |> should bePass
                    | _ -> failwith "Should create Example node"
                )

                it "returns Pass for successful tests" (fun () ->
                    let nodes = spec {
                        yield it "test" (fun () -> ())
                    }

                    match nodes with
                    | [Example(_, testFn)] ->
                        let result = testFn()
                        expect result |> should bePass
                    | _ -> failwith "Should create Example node"
                )
            ]

            context "failing tests" [
                it "returns Fail with exception for failed tests" (fun () ->
                    let nodes = spec {
                        yield it "failing test" (fun () -> failwith "intentional failure")
                    }

                    match nodes with
                    | [Example(_, testFn)] ->
                        let result = testFn()
                        expect result |> should beFail
                        expect result |> should (beFailWith "intentional failure")
                    | _ -> failwith "Should create Example node"
                )
            ]
        ]
    }

/// Specs for complex nesting scenarios
[<Tests>]
let complexNestingSpecs =
    spec {
        yield describe "Complex nesting" [
            it "handles deeply nested structures correctly" (fun () ->
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

                expect nodes |> should (haveLength 1)

                let exampleCount = nodes |> List.sumBy TestNode.countExamples
                let groupCount = nodes |> List.sumBy TestNode.countGroups

                expect exampleCount |> should (equal 3)
                expect groupCount |> should (equal 4)
            )
        ]
    }

