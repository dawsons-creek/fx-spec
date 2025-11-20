/// BDD specifications for hook error handling
module FX.Spec.Core.Tests.HookErrorHandlingSpecs

open System
open FX.Spec.Core
open FX.Spec.Matchers
open FX.Spec.Runner

[<Tests>]
let hookErrorHandlingSpecs =
    describe
        "Hook Error Handling"
        [

          context
              "beforeAll failures"
              [ it "reports beforeAll failure as a failed test result" (fun () ->
                    let testNode =
                        describe
                            "test suite"
                            [ beforeAll (fun () -> failwith "beforeAll failed!")
                              it "should not run" (fun () -> ()) ]

                    let results = Executor.executeTests [ testNode ] |> Async.RunSynchronously
                    let allResults = results |> List.collect TestResultNode.collectResults

                    // Should have at least one failure
                    let failures = allResults |> List.filter TestResult.isFail
                    expectBool(failures.Length > 0).toBeTrue ())

                it "continues running other test groups after beforeAll failure" (fun () ->
                    let mutable group2Ran = false

                    let testNodes =
                        [ describe
                              "group 1 with failing beforeAll"
                              [ beforeAll (fun () -> failwith "beforeAll failed!")
                                it "test 1" (fun () -> ()) ]
                          describe "group 2 should still run" [ it "test 2" (fun () -> group2Ran <- true) ] ]

                    let _ = Executor.executeTests testNodes |> Async.RunSynchronously

                    // Group 2 should have run despite group 1's beforeAll failure
                    expectBool(group2Ran).toBeTrue ())

                it "provides clear error message for beforeAll failures" (fun () ->
                    let testNode =
                        describe
                            "test suite"
                            [ beforeAll (fun () -> failwith "Database connection failed")
                              it "should not run" (fun () -> ()) ]

                    let results = Executor.executeTests [ testNode ] |> Async.RunSynchronously
                    let allResults = results |> List.collect TestResultNode.collectResults

                    let failures = allResults |> List.filter TestResult.isFail

                    match failures with
                    | TestResult.Fail(Some ex) :: _ ->
                        // Error message should mention the hook type and the actual error
                        expectBool(
                            ex.Message.Contains("beforeAll")
                            || ex.Message.Contains("Database connection failed")
                        )
                            .toBeTrue ()
                    | TestResult.Fail None :: _ -> failwith "Expected failure with exception details"
                    | _ -> failwith "Expected at least one failure") ]

          context
              "afterAll failures"
              [ it "reports afterAll failure as a failed test result" (fun () ->
                    let testNode =
                        describe
                            "test suite"
                            [ it "test runs" (fun () -> ())
                              afterAll (fun () -> failwith "afterAll cleanup failed!") ]

                    let results = Executor.executeTests [ testNode ] |> Async.RunSynchronously
                    let allResults = results |> List.collect TestResultNode.collectResults

                    // Should have at least one failure for afterAll
                    let failures = allResults |> List.filter TestResult.isFail
                    expectBool(failures.Length > 0).toBeTrue ())

                it "still runs tests even if afterAll will fail" (fun () ->
                    let mutable testRan = false

                    let testNode =
                        describe
                            "test suite"
                            [ it "test should run" (fun () -> testRan <- true)
                              afterAll (fun () -> failwith "afterAll cleanup failed!") ]

                    let _ = Executor.executeTests [ testNode ] |> Async.RunSynchronously

                    expectBool(testRan).toBeTrue ()) ]

          context
              "beforeEach failures"
              [ it "reports beforeEach failure without aborting entire suite" (fun () ->
                    let mutable test1Attempted = false
                    let mutable test2Attempted = false

                    let testNode =
                        describe
                            "test suite"
                            [ beforeEach (fun () ->
                                  test1Attempted <- true // Track that we entered beforeEach
                                  failwith "beforeEach setup failed!")
                              it "test 1" (fun () -> ())
                              it "test 2" (fun () -> test2Attempted <- true) ]

                    let results = Executor.executeTests [ testNode ] |> Async.RunSynchronously
                    let allResults = results |> List.collect TestResultNode.collectResults

                    // Both tests should fail (beforeEach runs for each and fails)
                    let failures = allResults |> List.filter TestResult.isFail
                    expectInt(failures.Length).toEqual (2)

                    // beforeEach should have been attempted (and failed) for test1
                    expectBool(test1Attempted).toBeTrue ()
                // test2's body should NOT have run (because beforeEach failed)
                // but test2 itself should have been attempted (its beforeEach failed too)
                ) ]

          context
              "afterEach failures"
              [ it "reports afterEach failure but doesn't abort other tests" (fun () ->
                    let mutable test1Passed = false
                    let mutable test2Passed = false

                    let testNode =
                        describe
                            "test suite"
                            [ afterEach (fun () -> failwith "afterEach cleanup failed!")
                              it "test 1" (fun () -> test1Passed <- true)
                              it "test 2" (fun () -> test2Passed <- true) ]

                    let _ = Executor.executeTests [ testNode ] |> Async.RunSynchronously

                    // Both tests should have run
                    expectBool(test1Passed).toBeTrue ()
                    expectBool(test2Passed).toBeTrue ()) ]

          context
              "error message quality"
              [ it "includes hook type in error messages" (fun () ->
                    let testNode =
                        describe "suite" [ beforeAll (fun () -> failwith "Setup error"); it "test" (fun () -> ()) ]

                    let results = Executor.executeTests [ testNode ] |> Async.RunSynchronously
                    let allResults = results |> List.collect TestResultNode.collectResults

                    let failure = allResults |> List.tryFind TestResult.isFail

                    match failure with
                    | Some(TestResult.Fail(Some ex)) ->
                        let msg: string = ex.Message
                        expectStr(msg).toContain ("beforeAll")
                    | Some(TestResult.Fail None) -> failwith "Expected failure with exception details"
                    | _ -> failwith "Expected a failure") ] ]
