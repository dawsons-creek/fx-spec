/// Tests for Phase 5 features: pending, focused tests, and hooks
module FxSpec.Core.Tests.Phase5Specs

open FxSpec.Core
open FxSpec.Matchers
open FxSpec.Core.Tests.FxSpecMatchers

/// Specs for pending/skipped tests
[<Tests>]
let pendingSpecs =
    spec {
        yield describe "Pending tests" [
            context "xit" [
                it "creates a skipped test" (fun () ->
                    let nodes = spec {
                        yield xit "pending test" (fun () -> failwith "Should not run")
                    }
                    
                    expect nodes |> should (haveLength 1)
                    
                    // Execute the test
                    let result = match List.head nodes with
                                 | Example (_, test) -> test()
                                 | _ -> failwith "Expected Example"
                    
                    expect (TestResult.isSkipped result) |> should beTrue
                )
            ]
            
            context "pending" [
                it "is an alias for xit" (fun () ->
                    let nodes = spec {
                        yield pending "pending test" (fun () -> failwith "Should not run")
                    }
                    
                    expect nodes |> should (haveLength 1)
                    
                    // Execute the test
                    let result = match List.head nodes with
                                 | Example (_, test) -> test()
                                 | _ -> failwith "Expected Example"
                    
                    expect (TestResult.isSkipped result) |> should beTrue
                )
            ]
        ]
    }

/// Specs for focused tests
[<Tests>]
let focusedSpecs =
    spec {
        yield describe "Focused tests" [
            context "fit" [
                it "creates a focused example" (fun () ->
                    let nodes = spec {
                        yield fit "focused test" (fun () -> ())
                    }
                    
                    expect nodes |> should (haveLength 1)
                    
                    match List.head nodes with
                    | FocusedExample (desc, _) -> 
                        expect desc |> should (equal "focused test")
                    | _ -> failwith "Expected FocusedExample"
                )
            ]
            
            context "fdescribe" [
                it "creates a focused group" (fun () ->
                    let nodes = spec {
                        yield fdescribe "focused group" [
                            it "test 1" (fun () -> ())
                            it "test 2" (fun () -> ())
                        ]
                    }
                    
                    expect nodes |> should (haveLength 1)
                    
                    match List.head nodes with
                    | FocusedGroup (desc, _, children) ->
                        expect desc |> should (equal "focused group")
                        expect children |> should (haveLength 2)
                    | _ -> failwith "Expected FocusedGroup"
                )
            ]

            context "fcontext" [
                it "is an alias for fdescribe" (fun () ->
                    let nodes = spec {
                        yield fcontext "focused context" [
                            it "test" (fun () -> ())
                        ]
                    }

                    expect nodes |> should (haveLength 1)

                    match List.head nodes with
                    | FocusedGroup (desc, _, _) ->
                        expect desc |> should (equal "focused context")
                    | _ -> failwith "Expected FocusedGroup"
                )
            ]
        ]
    }

/// Specs for focused filtering
[<Tests>]
let focusedFilteringSpecs =
    spec {
        yield describe "Focused filtering" [
            context "hasFocused" [
                it "returns true for FocusedExample" (fun () ->
                    let node = FocusedExample ("test", fun () -> TestResult.Pass)
                    expect (TestNode.hasFocused node) |> should beTrue
                )

                it "returns true for FocusedGroup" (fun () ->
                    let node = FocusedGroup ("group", GroupHooks.empty, [])
                    expect (TestNode.hasFocused node) |> should beTrue
                )

                it "returns false for regular Example" (fun () ->
                    let node = Example ("test", fun () -> TestResult.Pass)
                    expect (TestNode.hasFocused node) |> should beFalse
                )

                it "returns true for Group containing focused tests" (fun () ->
                    let node = Group ("group", GroupHooks.empty, [
                        FocusedExample ("focused", fun () -> TestResult.Pass)
                        Example ("regular", fun () -> TestResult.Pass)
                    ])
                    expect (TestNode.hasFocused node) |> should beTrue
                )
            ]

            context "filterFocused" [
                it "returns original tree when no focused tests exist" (fun () ->
                    let nodes = [
                        Example ("test 1", fun () -> TestResult.Pass)
                        Example ("test 2", fun () -> TestResult.Pass)
                    ]
                    let filtered = TestNode.filterFocused nodes
                    expect filtered |> should (haveLength 2)
                )

                it "filters to only focused tests when they exist" (fun () ->
                    let nodes = [
                        FocusedExample ("focused", fun () -> TestResult.Pass)
                        Example ("regular", fun () -> TestResult.Pass)
                    ]
                    let filtered = TestNode.filterFocused nodes
                    expect filtered |> should (haveLength 1)
                )
            ]
        ]
    }

