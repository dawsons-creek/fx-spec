/// Tests for Phase 6 features: hooks and state management
module FxSpec.Core.Tests.HooksSpecs

open System
open FxSpec.Core
open FxSpec.Matchers
open FxSpec.Runner.Executor

/// Specs for beforeEach/afterEach hooks
[<Tests>]
let beforeEachAfterEachSpecs =
    describe "beforeEach and afterEach hooks" [
        context "beforeEach" [
            it "runs before each test in the group" (fun () ->
                let mutable setupCount = 0
                let mutable test1Ran = false
                let mutable test2Ran = false

                let nodes = [
                    describe "with beforeEach" [
                        beforeEach (fun () -> setupCount <- setupCount + 1)

                        it "test 1" (fun () ->
                            test1Ran <- true
                            expect(setupCount).toEqual(1)
                        )

                        it "test 2" (fun () ->
                            test2Ran <- true
                            expect(setupCount).toEqual(2)
                        )
                    ]
                ]

                // Execute the tests
                nodes |> List.iter (executeNode >> ignore)

                expectBool(test1Ran).toBeTrue()
                expectBool(test2Ran).toBeTrue()
                expect(setupCount).toEqual(2)
            )

            it "runs in outer-to-inner order for nested groups" (fun () ->
                let mutable executionOrder = []

                let nodes = [
                    describe "outer" [
                        beforeEach (fun () -> executionOrder <- executionOrder @ ["outer"])

                        context "inner" [
                            beforeEach (fun () -> executionOrder <- executionOrder @ ["inner"])

                            it "test" (fun () ->
                                expect(executionOrder).toEqual(["outer"; "inner"])
                            )
                        ]
                    ]
                ]

                // Execute and verify order
                nodes |> List.iter (executeNode >> ignore)
                expect(executionOrder).toEqual(["outer"; "inner"])
            )
        ]

        context "afterEach" [
            it "runs after each test in the group" (fun () ->
                let mutable teardownCount = 0
                let mutable test1Completed = false
                let mutable test2Completed = false

                let nodes = [
                    describe "with afterEach" [
                        afterEach (fun () -> teardownCount <- teardownCount + 1)

                        it "test 1" (fun () ->
                            test1Completed <- true
                        )

                        it "test 2" (fun () ->
                            test2Completed <- true
                        )
                    ]
                ]

                // Execute the tests
                nodes |> List.iter (executeNode >> ignore)
                expectBool(test1Completed).toBeTrue()
                expectBool(test2Completed).toBeTrue()
                expect(teardownCount).toEqual(2)
            )

            it "runs in inner-to-outer order for nested groups" (fun () ->
                let mutable executionOrder = []

                let nodes = [
                    describe "outer" [
                        afterEach (fun () -> executionOrder <- executionOrder @ ["outer"])

                        context "inner" [
                            afterEach (fun () -> executionOrder <- executionOrder @ ["inner"])

                            it "test" (fun () -> ())
                        ]
                    ]
                ]

                // Execute and verify order
                nodes |> List.iter (executeNode >> ignore)
                expect(executionOrder).toEqual(["inner"; "outer"])
            )

            it "runs even when test fails" (fun () ->
                let mutable teardownRan = false

                let nodes = [
                    describe "with failing test" [
                        afterEach (fun () -> teardownRan <- true)

                        it "failing test" (fun () ->
                            failwith "test error"
                        )
                    ]
                ]

                // Execute the test (it should fail but afterEach should still run)
                nodes |> List.iter (executeNode >> ignore)
                expectBool(teardownRan).toBeTrue()
            )
        ]

        context "combined beforeEach and afterEach" [
            it "runs in correct order" (fun () ->
                let mutable executionOrder = []

                let nodes = [
                    describe "with both hooks" [
                        beforeEach (fun () -> executionOrder <- executionOrder @ ["before"])
                        afterEach (fun () -> executionOrder <- executionOrder @ ["after"])

                        it "test" (fun () ->
                            executionOrder <- executionOrder @ ["test"]
                        )
                    ]
                ]

                // Execute and verify order
                nodes |> List.iter (executeNode >> ignore)
                expect(executionOrder).toEqual(["before"; "test"; "after"])
            )
        ]
    ]

/// Specs for beforeAll/afterAll hooks
[<Tests>]
let beforeAllAfterAllSpecs =
    describe "beforeAll and afterAll hooks" [
        context "beforeAll" [
            it "runs once before all tests in the group" (fun () ->
                let mutable setupCount = 0
                let mutable test1Ran = false
                let mutable test2Ran = false

                let nodes = [
                    describe "with beforeAll" [
                        beforeAll (fun () -> setupCount <- setupCount + 1)

                        it "test 1" (fun () ->
                            test1Ran <- true
                            expect(setupCount).toEqual(1)
                        )

                        it "test 2" (fun () ->
                            test2Ran <- true
                            expect(setupCount).toEqual(1)  // Still 1, not 2
                        )
                    ]
                ]

                // Execute the tests
                nodes |> List.iter (executeNode >> ignore)
                expectBool(test1Ran).toBeTrue()
                expectBool(test2Ran).toBeTrue()
                expect(setupCount).toEqual(1)  // Only ran once
            )
        ]

        context "afterAll" [
            it "runs once after all tests in the group" (fun () ->
                let mutable teardownCount = 0
                let mutable test1Completed = false
                let mutable test2Completed = false

                let nodes = [
                    describe "with afterAll" [
                        afterAll (fun () -> teardownCount <- teardownCount + 1)

                        it "test 1" (fun () ->
                            test1Completed <- true
                            expect(teardownCount).toEqual(0)  // Not run yet
                        )

                        it "test 2" (fun () ->
                            test2Completed <- true
                            expect(teardownCount).toEqual(0)  // Still not run
                        )
                    ]
                ]

                // Execute the tests
                nodes |> List.iter (executeNode >> ignore)
                expectBool(test1Completed).toBeTrue()
                expectBool(test2Completed).toBeTrue()
                expect(teardownCount).toEqual(1)  // Ran once after all tests
            )

            it "runs even when tests fail" (fun () ->
                let mutable teardownRan = false

                let nodes = [
                    describe "with failing tests" [
                        afterAll (fun () -> teardownRan <- true)

                        it "failing test 1" (fun () ->
                            failwith "error 1"
                        )

                        it "failing test 2" (fun () ->
                            failwith "error 2"
                        )
                    ]
                ]

                // Execute the tests (they should fail but afterAll should still run)
                nodes |> List.iter (executeNode >> ignore)
                expectBool(teardownRan).toBeTrue()
            )
        ]

        context "combined beforeAll and afterAll" [
            it "runs in correct order" (fun () ->
                let mutable executionOrder = []

                let nodes = [
                    describe "with both hooks" [
                        beforeAll (fun () -> executionOrder <- executionOrder @ ["beforeAll"])
                        afterAll (fun () -> executionOrder <- executionOrder @ ["afterAll"])

                        it "test 1" (fun () ->
                            executionOrder <- executionOrder @ ["test1"]
                        )

                        it "test 2" (fun () ->
                            executionOrder <- executionOrder @ ["test2"]
                        )
                    ]
                ]

                // Execute and verify order
                nodes |> List.iter (executeNode >> ignore)
                expect(executionOrder).toEqual(["beforeAll"; "test1"; "test2"; "afterAll"])
            )
        ]

        context "mixed with beforeEach/afterEach" [
            it "runs all hooks in correct order" (fun () ->
                let mutable executionOrder = []

                let nodes = [
                    describe "with all hooks" [
                        beforeAll (fun () -> executionOrder <- executionOrder @ ["beforeAll"])
                        beforeEach (fun () -> executionOrder <- executionOrder @ ["beforeEach"])
                        afterEach (fun () -> executionOrder <- executionOrder @ ["afterEach"])
                        afterAll (fun () -> executionOrder <- executionOrder @ ["afterAll"])

                        it "test 1" (fun () ->
                            executionOrder <- executionOrder @ ["test1"]
                        )

                        it "test 2" (fun () ->
                            executionOrder <- executionOrder @ ["test2"]
                        )
                    ]
                ]

                // Execute and verify order
                nodes |> List.iter (executeNode >> ignore)
                let expected = [
                    "beforeAll"
                    "beforeEach"; "test1"; "afterEach"
                    "beforeEach"; "test2"; "afterEach"
                    "afterAll"
                ]
                expect(executionOrder).toEqual(expected)
            )
        ]
    ]

