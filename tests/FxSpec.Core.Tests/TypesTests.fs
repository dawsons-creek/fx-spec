module FxSpec.Core.Tests.TypesTests

open FxSpec.Core

// Test helper to verify test results
let assertPass result =
    match result with
    | Pass -> ()
    | Fail ex -> failwith $"Expected Pass but got Fail: {ex}"
    | Skipped reason -> failwith $"Expected Pass but got Skipped: {reason}"

let assertFail result =
    match result with
    | Fail _ -> ()
    | Pass -> failwith "Expected Fail but got Pass"
    | Skipped reason -> failwith $"Expected Fail but got Skipped: {reason}"

// Test TestResult helpers
let testResultIsPass() =
    let result = Pass
    if not (TestResult.isPass result) then
        failwith "isPass should return true for Pass"
    if TestResult.isFail result then
        failwith "isFail should return false for Pass"
    if TestResult.isSkipped result then
        failwith "isSkipped should return false for Pass"

let testResultIsFail() =
    let result = Fail None
    if TestResult.isPass result then
        failwith "isPass should return false for Fail"
    if not (TestResult.isFail result) then
        failwith "isFail should return true for Fail"
    if TestResult.isSkipped result then
        failwith "isSkipped should return false for Fail"

let testResultIsSkipped() =
    let result = Skipped "test reason"
    if TestResult.isPass result then
        failwith "isPass should return false for Skipped"
    if TestResult.isFail result then
        failwith "isFail should return false for Skipped"
    if not (TestResult.isSkipped result) then
        failwith "isSkipped should return true for Skipped"

// Test TestNode helpers
let testNodeDescription() =
    let example = Example("test example", fun () -> Pass)
    let group = Group("test group", [])
    
    if TestNode.description example <> "test example" then
        failwith "Example description should match"
    if TestNode.description group <> "test group" then
        failwith "Group description should match"

let testNodeCountExamples() =
    let example1 = Example("test 1", fun () -> Pass)
    let example2 = Example("test 2", fun () -> Pass)
    let group = Group("group", [example1; example2])
    
    if TestNode.countExamples example1 <> 1 then
        failwith "Single example should count as 1"
    if TestNode.countExamples group <> 2 then
        failwith "Group with 2 examples should count as 2"

let testNodeCountGroups() =
    let example = Example("test", fun () -> Pass)
    let innerGroup = Group("inner", [example])
    let outerGroup = Group("outer", [innerGroup; example])
    
    if TestNode.countGroups example <> 0 then
        failwith "Example should have 0 groups"
    if TestNode.countGroups innerGroup <> 1 then
        failwith "Single group should count as 1"
    if TestNode.countGroups outerGroup <> 2 then
        failwith "Nested groups should count correctly"

// Test TestResultNode helpers
let testResultNodeCollectResults() =
    let result1 = ExampleResult("test 1", Pass, System.TimeSpan.Zero)
    let result2 = ExampleResult("test 2", Fail None, System.TimeSpan.Zero)
    let groupResult = GroupResult("group", [result1; result2])
    
    let collected = TestResultNode.collectResults groupResult
    if List.length collected <> 2 then
        failwith "Should collect 2 results"

let testResultNodeCountPassed() =
    let result1 = ExampleResult("test 1", Pass, System.TimeSpan.Zero)
    let result2 = ExampleResult("test 2", Pass, System.TimeSpan.Zero)
    let result3 = ExampleResult("test 3", Fail None, System.TimeSpan.Zero)
    let groupResult = GroupResult("group", [result1; result2; result3])
    
    if TestResultNode.countPassed groupResult <> 2 then
        failwith "Should count 2 passed tests"

let testResultNodeCountFailed() =
    let result1 = ExampleResult("test 1", Pass, System.TimeSpan.Zero)
    let result2 = ExampleResult("test 2", Fail None, System.TimeSpan.Zero)
    let result3 = ExampleResult("test 3", Fail None, System.TimeSpan.Zero)
    let groupResult = GroupResult("group", [result1; result2; result3])
    
    if TestResultNode.countFailed groupResult <> 2 then
        failwith "Should count 2 failed tests"

let testResultNodeCountSkipped() =
    let result1 = ExampleResult("test 1", Pass, System.TimeSpan.Zero)
    let result2 = ExampleResult("test 2", Skipped "reason", System.TimeSpan.Zero)
    let groupResult = GroupResult("group", [result1; result2])
    
    if TestResultNode.countSkipped groupResult <> 1 then
        failwith "Should count 1 skipped test"

// Run all tests
let runAllTests() =
    printfn "Running Types tests..."
    
    testResultIsPass()
    printfn "  ✓ TestResult.isPass"
    
    testResultIsFail()
    printfn "  ✓ TestResult.isFail"
    
    testResultIsSkipped()
    printfn "  ✓ TestResult.isSkipped"
    
    testNodeDescription()
    printfn "  ✓ TestNode.description"
    
    testNodeCountExamples()
    printfn "  ✓ TestNode.countExamples"
    
    testNodeCountGroups()
    printfn "  ✓ TestNode.countGroups"
    
    testResultNodeCollectResults()
    printfn "  ✓ TestResultNode.collectResults"
    
    testResultNodeCountPassed()
    printfn "  ✓ TestResultNode.countPassed"
    
    testResultNodeCountFailed()
    printfn "  ✓ TestResultNode.countFailed"
    
    testResultNodeCountSkipped()
    printfn "  ✓ TestResultNode.countSkipped"
    
    printfn "\nAll Types tests passed! ✓"

