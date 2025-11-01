/// FxSpec-based tests for FxSpec.Core types.
/// This is dogfooding - using FxSpec to test itself!
module FxSpec.Core.Tests.TypesSpecs

open System
open FxSpec.Core
open FxSpec.Matchers
open FxSpec.Core.Tests.FxSpecMatchers

/// Specs for TestResult type and its helper functions
[<Tests>]
let testResultSpecs =
    spec {
        yield describe "TestResult" [
            context "isPass" [
                it "returns true for Pass" (fun () ->
                    let result = TestResult.Pass
                    expect (TestResult.isPass result) |> to' beTrue
                )

                it "returns false for Fail" (fun () ->
                    let result = TestResult.Fail None
                    expect (TestResult.isPass result) |> to' beFalse
                )

                it "returns false for Skipped" (fun () ->
                    let result = TestResult.Skipped "reason"
                    expect (TestResult.isPass result) |> to' beFalse
                )
            ]

            context "isFail" [
                it "returns false for Pass" (fun () ->
                    let result = TestResult.Pass
                    expect (TestResult.isFail result) |> to' beFalse
                )

                it "returns true for Fail" (fun () ->
                    let result = TestResult.Fail None
                    expect (TestResult.isFail result) |> to' beTrue
                )

                it "returns false for Skipped" (fun () ->
                    let result = TestResult.Skipped "reason"
                    expect (TestResult.isFail result) |> to' beFalse
                )
            ]

            context "isSkipped" [
                it "returns false for Pass" (fun () ->
                    let result = TestResult.Pass
                    expect (TestResult.isSkipped result) |> to' beFalse
                )

                it "returns false for Fail" (fun () ->
                    let result = TestResult.Fail None
                    expect (TestResult.isSkipped result) |> to' beFalse
                )

                it "returns true for Skipped" (fun () ->
                    let result = TestResult.Skipped "reason"
                    expect (TestResult.isSkipped result) |> to' beTrue
                )
            ]
        ]
    }

/// Specs for TestNode type and its helper functions
[<Tests>]
let testNodeSpecs =
    spec {
        yield describe "TestNode" [
            context "description" [
                it "returns the description for Example nodes" (fun () ->
                    let example = Example("test example", fun () -> TestResult.Pass)
                    expect (TestNode.description example) |> to' (equal "test example")
                )

                it "returns the description for Group nodes" (fun () ->
                    let group = Group("test group", [])
                    expect (TestNode.description group) |> to' (equal "test group")
                )
            ]

            context "countExamples" [
                it "counts a single Example as 1" (fun () ->
                    let example = Example("test", fun () -> TestResult.Pass)
                    expect (TestNode.countExamples example) |> to' (equal 1)
                )

                it "counts examples in a Group" (fun () ->
                    let example1 = Example("test 1", fun () -> TestResult.Pass)
                    let example2 = Example("test 2", fun () -> TestResult.Pass)
                    let group = Group("group", [example1; example2])
                    expect (TestNode.countExamples group) |> to' (equal 2)
                )

                it "counts examples recursively in nested groups" (fun () ->
                    let example1 = Example("test 1", fun () -> TestResult.Pass)
                    let example2 = Example("test 2", fun () -> TestResult.Pass)
                    let innerGroup = Group("inner", [example1])
                    let outerGroup = Group("outer", [innerGroup; example2])
                    expect (TestNode.countExamples outerGroup) |> to' (equal 2)
                )
            ]

            context "countGroups" [
                it "counts an Example as 0 groups" (fun () ->
                    let example = Example("test", fun () -> TestResult.Pass)
                    expect (TestNode.countGroups example) |> to' (equal 0)
                )

                it "counts a single Group as 1" (fun () ->
                    let example = Example("test", fun () -> TestResult.Pass)
                    let group = Group("group", [example])
                    expect (TestNode.countGroups group) |> to' (equal 1)
                )

                it "counts nested groups correctly" (fun () ->
                    let example = Example("test", fun () -> TestResult.Pass)
                    let innerGroup = Group("inner", [example])
                    let outerGroup = Group("outer", [innerGroup; example])
                    expect (TestNode.countGroups outerGroup) |> to' (equal 2)
                )
            ]
        ]
    }

/// Specs for TestResultNode type and its helper functions
[<Tests>]
let testResultNodeSpecs =
    spec {
        yield describe "TestResultNode" [
            context "collectResults" [
                it "collects all results from a group" (fun () ->
                    let result1 = ExampleResult("test 1", TestResult.Pass, TimeSpan.Zero)
                    let result2 = ExampleResult("test 2", TestResult.Fail None, TimeSpan.Zero)
                    let groupResult = GroupResult("group", [result1; result2])

                    let collected = TestResultNode.collectResults groupResult
                    expect collected |> to' (haveLength 2)
                )
            ]

            context "countPassed" [
                it "counts passed tests correctly" (fun () ->
                    let result1 = ExampleResult("test 1", TestResult.Pass, TimeSpan.Zero)
                    let result2 = ExampleResult("test 2", TestResult.Pass, TimeSpan.Zero)
                    let result3 = ExampleResult("test 3", TestResult.Fail None, TimeSpan.Zero)
                    let groupResult = GroupResult("group", [result1; result2; result3])

                    expect (TestResultNode.countPassed groupResult) |> to' (equal 2)
                )
            ]

            context "countFailed" [
                it "counts failed tests correctly" (fun () ->
                    let result1 = ExampleResult("test 1", TestResult.Pass, TimeSpan.Zero)
                    let result2 = ExampleResult("test 2", TestResult.Fail None, TimeSpan.Zero)
                    let result3 = ExampleResult("test 3", TestResult.Fail None, TimeSpan.Zero)
                    let groupResult = GroupResult("group", [result1; result2; result3])

                    expect (TestResultNode.countFailed groupResult) |> to' (equal 2)
                )
            ]

            context "countSkipped" [
                it "counts skipped tests correctly" (fun () ->
                    let result1 = ExampleResult("test 1", TestResult.Pass, TimeSpan.Zero)
                    let result2 = ExampleResult("test 2", TestResult.Skipped "reason", TimeSpan.Zero)
                    let groupResult = GroupResult("group", [result1; result2])

                    expect (TestResultNode.countSkipped groupResult) |> to' (equal 1)
                )
            ]
        ]
    }

