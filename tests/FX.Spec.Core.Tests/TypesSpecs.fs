/// FxSpec-based tests for FX.Spec.Core types.
/// This is dogfooding - using FX.Spec to test itself!
module FX.Spec.Core.Tests.TypesSpecs

open System
open FX.Spec.Core
open FX.Spec.Matchers
open FX.Spec.Core.Tests.FXSpecMatchers

/// Specs for TestResult type and its helper functions
[<Tests>]
let testResultSpecs =
    describe
        "TestResult"
        [ context
              "isPass"
              [ it "returns true for Pass" (fun () ->
                    let result = TestResult.Pass
                    expectBool(TestResult.isPass result).toBeTrue ())

                it "returns false for Fail" (fun () ->
                    let result = TestResult.Fail None
                    expectBool(TestResult.isPass result).toBeFalse ())

                it "returns false for Skipped" (fun () ->
                    let result = TestResult.Skipped "reason"
                    expectBool(TestResult.isPass result).toBeFalse ()) ]

          context
              "isFail"
              [ it "returns false for Pass" (fun () ->
                    let result = TestResult.Pass
                    expectBool(TestResult.isFail result).toBeFalse ())

                it "returns true for Fail" (fun () ->
                    let result = TestResult.Fail None
                    expectBool(TestResult.isFail result).toBeTrue ())

                it "returns false for Skipped" (fun () ->
                    let result = TestResult.Skipped "reason"
                    expectBool(TestResult.isFail result).toBeFalse ()) ]

          context
              "isSkipped"
              [ it "returns false for Pass" (fun () ->
                    let result = TestResult.Pass
                    expectBool(TestResult.isSkipped result).toBeFalse ())

                it "returns false for Fail" (fun () ->
                    let result = TestResult.Fail None
                    expectBool(TestResult.isSkipped result).toBeFalse ())

                it "returns true for Skipped" (fun () ->
                    let result = TestResult.Skipped "reason"
                    expectBool(TestResult.isSkipped result).toBeTrue ()) ] ]

/// Specs for TestNode type and its helper functions
[<Tests>]
let testNodeSpecs =
    describe
        "TestNode"
        [ context
              "description"
              [ it "returns the description for Example nodes" (fun () ->
                    let example =
                        Example("test example", (fun () -> TestResult.Pass), TestMetadata.empty)

                    expect(TestNode.description example).toEqual ("test example"))

                it "returns the description for Group nodes" (fun () ->
                    let group = Group("test group", GroupHooks.empty, [], TestMetadata.empty)
                    expect(TestNode.description group).toEqual ("test group")) ]

          context
              "countExamples"
              [ it "counts a single Example as 1" (fun () ->
                    let example = Example("test", (fun () -> TestResult.Pass), TestMetadata.empty)
                    expect(TestNode.countExamples example).toEqual (1))

                it "counts examples in a Group" (fun () ->
                    let example1 = Example("test 1", (fun () -> TestResult.Pass), TestMetadata.empty)
                    let example2 = Example("test 2", (fun () -> TestResult.Pass), TestMetadata.empty)

                    let group =
                        Group("group", GroupHooks.empty, [ example1; example2 ], TestMetadata.empty)

                    expect(TestNode.countExamples group).toEqual (2))

                it "counts examples recursively in nested groups" (fun () ->
                    let example1 = Example("test 1", (fun () -> TestResult.Pass), TestMetadata.empty)
                    let example2 = Example("test 2", (fun () -> TestResult.Pass), TestMetadata.empty)
                    let innerGroup = Group("inner", GroupHooks.empty, [ example1 ], TestMetadata.empty)

                    let outerGroup =
                        Group("outer", GroupHooks.empty, [ innerGroup; example2 ], TestMetadata.empty)

                    expect(TestNode.countExamples outerGroup).toEqual (2)) ]

          context
              "countGroups"
              [ it "counts an Example as 0 groups" (fun () ->
                    let example = Example("test", (fun () -> TestResult.Pass), TestMetadata.empty)
                    expect(TestNode.countGroups example).toEqual (0))

                it "counts a single Group as 1" (fun () ->
                    let example = Example("test", (fun () -> TestResult.Pass), TestMetadata.empty)
                    let group = Group("group", GroupHooks.empty, [ example ], TestMetadata.empty)
                    expect(TestNode.countGroups group).toEqual (1))

                it "counts nested groups correctly" (fun () ->
                    let example = Example("test", (fun () -> TestResult.Pass), TestMetadata.empty)
                    let innerGroup = Group("inner", GroupHooks.empty, [ example ], TestMetadata.empty)

                    let outerGroup =
                        Group("outer", GroupHooks.empty, [ innerGroup; example ], TestMetadata.empty)

                    expect(TestNode.countGroups outerGroup).toEqual (2)) ] ]

/// Specs for TestResultNode type and its helper functions
[<Tests>]
let testResultNodeSpecs =
    describe
        "TestResultNode"
        [ context
              "collectResults"
              [ it "collects all results from a group" (fun () ->
                    let result1 =
                        ExampleResult("test 1", TestResult.Pass, TimeSpan.Zero, TestMetadata.empty)

                    let result2 =
                        ExampleResult("test 2", TestResult.Fail None, TimeSpan.Zero, TestMetadata.empty)

                    let groupResult = GroupResult("group", [ result1; result2 ], TestMetadata.empty)

                    let collected = TestResultNode.collectResults groupResult
                    expectSeq(collected).toHaveLength (2)) ]

          context
              "countPassed"
              [ it "counts passed tests correctly" (fun () ->
                    let result1 =
                        ExampleResult("test 1", TestResult.Pass, TimeSpan.Zero, TestMetadata.empty)

                    let result2 =
                        ExampleResult("test 2", TestResult.Pass, TimeSpan.Zero, TestMetadata.empty)

                    let result3 =
                        ExampleResult("test 3", TestResult.Fail None, TimeSpan.Zero, TestMetadata.empty)

                    let groupResult =
                        GroupResult("group", [ result1; result2; result3 ], TestMetadata.empty)

                    expect(TestResultNode.countPassed groupResult).toEqual (2)) ]

          context
              "countFailed"
              [ it "counts failed tests correctly" (fun () ->
                    let result1 =
                        ExampleResult("test 1", TestResult.Pass, TimeSpan.Zero, TestMetadata.empty)

                    let result2 =
                        ExampleResult("test 2", TestResult.Fail None, TimeSpan.Zero, TestMetadata.empty)

                    let result3 =
                        ExampleResult("test 3", TestResult.Fail None, TimeSpan.Zero, TestMetadata.empty)

                    let groupResult =
                        GroupResult("group", [ result1; result2; result3 ], TestMetadata.empty)

                    expect(TestResultNode.countFailed groupResult).toEqual (2)) ]

          context
              "countSkipped"
              [ it "counts skipped tests correctly" (fun () ->
                    let result1 =
                        ExampleResult("test 1", TestResult.Pass, TimeSpan.Zero, TestMetadata.empty)

                    let result2 =
                        ExampleResult("test 2", TestResult.Skipped "reason", TimeSpan.Zero, TestMetadata.empty)

                    let groupResult = GroupResult("group", [ result1; result2 ], TestMetadata.empty)

                    expect(TestResultNode.countSkipped groupResult).toEqual (1)) ] ]
