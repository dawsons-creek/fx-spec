/// Tests for Phase 6 features: hooks and state management
module FxSpec.Core.Tests.HooksSpecs

open System
open FxSpec.Core
open FxSpec.Matchers
open FxSpec.Runner.Executor

/// Specs for beforeEach/afterEach hooks
[<Tests>]
let beforeEachAfterEachSpecs =
    spec {
        yield describe "beforeEach and afterEach hooks" [
            context "beforeEach" [
                it "runs before each test in the group" (fun () ->
                    let mutable setupCount = 0
                    let mutable test1Ran = false
                    let mutable test2Ran = false

                    let nodes = spec {
                        yield describe "with beforeEach" [
                            beforeEach (fun () -> setupCount <- setupCount + 1)

                            it "test 1" (fun () ->
                                test1Ran <- true
                                expect setupCount |> should (equal 1)
                            )

                            it "test 2" (fun () ->
                                test2Ran <- true
                                expect setupCount |> should (equal 2)
                            )
                        ]
                    }

                    // Execute the tests
                    nodes |> List.iter (executeNode >> ignore)

                    expect test1Ran |> should beTrue
                    expect test2Ran |> should beTrue
                    expect setupCount |> should (equal 2)
                )

                it "runs in outer-to-inner order for nested groups" (fun () ->
                    let mutable executionOrder = []

                    let nodes = spec {
                        yield describe "outer" [
                            beforeEach (fun () -> executionOrder <- executionOrder @ ["outer"])

                            context "inner" [
                                beforeEach (fun () -> executionOrder <- executionOrder @ ["inner"])

                                it "test" (fun () ->
                                    expect executionOrder |> should (equal ["outer"; "inner"])
                                )
                            ]
                        ]
                    }

                    // Execute and verify order
                    nodes |> List.iter (executeNode >> ignore)
                    expect executionOrder |> should (equal ["outer"; "inner"])
                )
            ]

            context "afterEach" [
                it "runs after each test in the group" (fun () ->
                    let mutable teardownCount = 0
                    let mutable test1Completed = false
                    let mutable test2Completed = false

                    let nodes = spec {
                        yield describe "with afterEach" [
                            afterEach (fun () -> teardownCount <- teardownCount + 1)

                            it "test 1" (fun () ->
                                test1Completed <- true
                            )

                            it "test 2" (fun () ->
                                test2Completed <- true
                            )
                        ]
                    }

                    // Execute the tests
                    nodes |> List.iter (executeNode >> ignore)
                    expect test1Completed |> should beTrue
                    expect test2Completed |> should beTrue
                    expect teardownCount |> should (equal 2)
                )

                it "runs in inner-to-outer order for nested groups" (fun () ->
                    let mutable executionOrder = []

                    let nodes = spec {
                        yield describe "outer" [
                            afterEach (fun () -> executionOrder <- executionOrder @ ["outer"])

                            context "inner" [
                                afterEach (fun () -> executionOrder <- executionOrder @ ["inner"])

                                it "test" (fun () -> ())
                            ]
                        ]
                    }

                    // Execute and verify order
                    nodes |> List.iter (executeNode >> ignore)
                    expect executionOrder |> should (equal ["inner"; "outer"])
                )

                it "runs even when test fails" (fun () ->
                    let mutable teardownRan = false

                    let nodes = spec {
                        yield describe "with failing test" [
                            afterEach (fun () -> teardownRan <- true)

                            it "failing test" (fun () ->
                                failwith "test error"
                            )
                        ]
                    }

                    // Execute the test (it should fail but afterEach should still run)
                    nodes |> List.iter (executeNode >> ignore)
                    expect teardownRan |> should beTrue
                )
            ]

            context "combined beforeEach and afterEach" [
                it "runs in correct order" (fun () ->
                    let mutable executionOrder = []

                    let nodes = spec {
                        yield describe "with both hooks" [
                            beforeEach (fun () -> executionOrder <- executionOrder @ ["before"])
                            afterEach (fun () -> executionOrder <- executionOrder @ ["after"])

                            it "test" (fun () ->
                                executionOrder <- executionOrder @ ["test"]
                            )
                        ]
                    }

                    // Execute and verify order
                    nodes |> List.iter (executeNode >> ignore)
                    expect executionOrder |> should (equal ["before"; "test"; "after"])
                )
            ]
        ]
    }

/// Specs for beforeAll/afterAll hooks
[<Tests>]
let beforeAllAfterAllSpecs =
    spec {
        yield describe "beforeAll and afterAll hooks" [
            context "beforeAll" [
                it "runs once before all tests in the group" (fun () ->
                    let mutable setupCount = 0
                    let mutable test1Ran = false
                    let mutable test2Ran = false

                    let nodes = spec {
                        yield describe "with beforeAll" [
                            beforeAll (fun () -> setupCount <- setupCount + 1)

                            it "test 1" (fun () ->
                                test1Ran <- true
                                expect setupCount |> should (equal 1)
                            )

                            it "test 2" (fun () ->
                                test2Ran <- true
                                expect setupCount |> should (equal 1)  // Still 1, not 2
                            )
                        ]
                    }

                    // Execute the tests
                    nodes |> List.iter (executeNode >> ignore)
                    expect test1Ran |> should beTrue
                    expect test2Ran |> should beTrue
                    expect setupCount |> should (equal 1)  // Only ran once
                )
            ]

            context "afterAll" [
                it "runs once after all tests in the group" (fun () ->
                    let mutable teardownCount = 0
                    let mutable test1Completed = false
                    let mutable test2Completed = false

                    let nodes = spec {
                        yield describe "with afterAll" [
                            afterAll (fun () -> teardownCount <- teardownCount + 1)

                            it "test 1" (fun () ->
                                test1Completed <- true
                                expect teardownCount |> should (equal 0)  // Not run yet
                            )

                            it "test 2" (fun () ->
                                test2Completed <- true
                                expect teardownCount |> should (equal 0)  // Still not run
                            )
                        ]
                    }

                    // Execute the tests
                    nodes |> List.iter (executeNode >> ignore)
                    expect test1Completed |> should beTrue
                    expect test2Completed |> should beTrue
                    expect teardownCount |> should (equal 1)  // Ran once after all tests
                )

                it "runs even when tests fail" (fun () ->
                    let mutable teardownRan = false

                    let nodes = spec {
                        yield describe "with failing tests" [
                            afterAll (fun () -> teardownRan <- true)

                            it "failing test 1" (fun () ->
                                failwith "error 1"
                            )

                            it "failing test 2" (fun () ->
                                failwith "error 2"
                            )
                        ]
                    }

                    // Execute the tests (they should fail but afterAll should still run)
                    nodes |> List.iter (executeNode >> ignore)
                    expect teardownRan |> should beTrue
                )
            ]

            context "combined beforeAll and afterAll" [
                it "runs in correct order" (fun () ->
                    let mutable executionOrder = []

                    let nodes = spec {
                        yield describe "with both hooks" [
                            beforeAll (fun () -> executionOrder <- executionOrder @ ["beforeAll"])
                            afterAll (fun () -> executionOrder <- executionOrder @ ["afterAll"])

                            it "test 1" (fun () ->
                                executionOrder <- executionOrder @ ["test1"]
                            )

                            it "test 2" (fun () ->
                                executionOrder <- executionOrder @ ["test2"]
                            )
                        ]
                    }

                    // Execute and verify order
                    nodes |> List.iter (executeNode >> ignore)
                    expect executionOrder |> should (equal ["beforeAll"; "test1"; "test2"; "afterAll"])
                )
            ]

            context "mixed with beforeEach/afterEach" [
                it "runs all hooks in correct order" (fun () ->
                    let mutable executionOrder = []

                    let nodes = spec {
                        yield describe "with all hooks" [
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
                    }

                    // Execute and verify order
                    nodes |> List.iter (executeNode >> ignore)
                    let expected = [
                        "beforeAll"
                        "beforeEach"; "test1"; "afterEach"
                        "beforeEach"; "test2"; "afterEach"
                        "afterAll"
                    ]
                    expect executionOrder |> should (equal expected)
                )
            ]
        ]
    }

