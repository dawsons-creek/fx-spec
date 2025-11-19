/// Tests for Phase 5 features: pending, focused tests, and hooks
module FxSpec.Core.Tests.Phase5Specs

open FxSpec.Core
open FxSpec.Matchers
open FxSpec.Core.Tests.FxSpecMatchers

/// Specs for pending/skipped tests
[<Tests>]
let pendingSpecs =
    describe "Pending tests" [
        context "xit" [
            it "creates a skipped test" (fun () ->
                let nodes = [
                    xit "pending test" (fun () -> failwith "Should not run")
                ]
                
                expect(List.length nodes).toEqual(1)
                
                // Execute the test
                let result = match List.head nodes with
                             | Example (_, test) -> test()
                             | _ -> failwith "Expected Example"
                
                expectBool(TestResult.isSkipped result).toBeTrue()
            )
        ]
        
        context "pending" [
            it "is an alias for xit" (fun () ->
                let nodes = [
                    pending "pending test" (fun () -> failwith "Should not run")
                ]
                
                expect(List.length nodes).toEqual(1)
                
                // Execute the test
                let result = match List.head nodes with
                             | Example (_, test) -> test()
                             | _ -> failwith "Expected Example"
                
                expectBool(TestResult.isSkipped result).toBeTrue()
            )
        ]
    ]

/// Specs for focused tests
[<Tests>]
let focusedSpecs =
    describe "Focused tests" [
        context "fit" [
            it "creates a focused example" (fun () ->
                let nodes = [
                    fit "focused test" (fun () -> ())
                ]
                
                expect(List.length nodes).toEqual(1)
                
                match List.head nodes with
                | FocusedExample (desc, _) -> 
                    expect(desc).toEqual("focused test")
                | _ -> failwith "Expected FocusedExample"
            )
        ]
        
        context "fdescribe" [
            it "creates a focused group" (fun () ->
                let nodes = [
                    fdescribe "focused group" [
                        it "test 1" (fun () -> ())
                        it "test 2" (fun () -> ())
                    ]
                ]
                
                expect(List.length nodes).toEqual(1)
                
                match List.head nodes with
                | FocusedGroup (desc, _, children) ->
                    expect(desc).toEqual("focused group")
                    expect(List.length children).toEqual(2)
                | _ -> failwith "Expected FocusedGroup"
            )
        ]

        context "fcontext" [
            it "is an alias for fdescribe" (fun () ->
                let nodes = [
                    fcontext "focused context" [
                        it "test" (fun () -> ())
                    ]
                ]

                expect(List.length nodes).toEqual(1)

                match List.head nodes with
                | FocusedGroup (desc, _, _) ->
                    expect(desc).toEqual("focused context")
                | _ -> failwith "Expected FocusedGroup"
            )
        ]
    ]

/// Specs for focused filtering
[<Tests>]
let focusedFilteringSpecs =
    describe "Focused filtering" [
        context "hasFocused" [
            it "returns true for FocusedExample" (fun () ->
                let node = FocusedExample ("test", fun () -> TestResult.Pass)
                expectBool(TestNode.hasFocused node).toBeTrue()
            )

            it "returns true for FocusedGroup" (fun () ->
                let node = FocusedGroup ("group", GroupHooks.empty, [])
                expectBool(TestNode.hasFocused node).toBeTrue()
            )

            it "returns false for regular Example" (fun () ->
                let node = Example ("test", fun () -> TestResult.Pass)
                expectBool(TestNode.hasFocused node).toBeFalse()
            )

            it "returns true for Group containing focused tests" (fun () ->
                let node = Group ("group", GroupHooks.empty, [
                    FocusedExample ("focused", fun () -> TestResult.Pass)
                    Example ("regular", fun () -> TestResult.Pass)
                ])
                expectBool(TestNode.hasFocused node).toBeTrue()
            )
        ]

        context "filterFocused" [
            it "returns original tree when no focused tests exist" (fun () ->
                let nodes = [
                    Example ("test 1", fun () -> TestResult.Pass)
                    Example ("test 2", fun () -> TestResult.Pass)
                ]
                let filtered = TestNode.filterFocused nodes
                expect(List.length filtered).toEqual(2)
            )

            it "filters to only focused tests when they exist" (fun () ->
                let nodes = [
                    FocusedExample ("focused", fun () -> TestResult.Pass)
                    Example ("regular", fun () -> TestResult.Pass)
                ]
                let filtered = TestNode.filterFocused nodes
                expect(List.length filtered).toEqual(1)
            )
        ]
    ]

