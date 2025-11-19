/// Custom matchers for testing FX.Spec's own types.
/// These matchers allow us to dogfood FX.Spec by using it to test itself.
module FX.Spec.Core.Tests.FXSpecMatchers

open FX.Spec.Core
open FX.Spec.Matchers

/// Expectation wrapper for TestResult values.
type TestResultExpectation(actual: TestResult) =
    member _.Value = actual

    member _.toBePass() =
        match actual with
        | TestResult.Pass -> ()
        | TestResult.Fail ex ->
            let msg = sprintf "Expected Pass, but got Fail: %A" ex
            raise (AssertionException(msg, Some(box "Pass"), Some(box actual)))
        | TestResult.Skipped reason ->
            let msg = sprintf "Expected Pass, but got Skipped: %s" reason
            raise (AssertionException(msg, Some(box "Pass"), Some(box actual)))

    member _.toBeFail() =
        match actual with
        | TestResult.Fail _ -> ()
        | TestResult.Pass ->
            let msg = "Expected Fail, but got Pass"
            raise (AssertionException(msg, Some(box "Fail"), Some(box "Pass")))
        | TestResult.Skipped reason ->
            let msg = sprintf "Expected Fail, but got Skipped: %s" reason
            raise (AssertionException(msg, Some(box "Fail"), Some(box actual)))

    member _.toBeFailWith(expectedMessage: string) =
        match actual with
        | TestResult.Fail(Some ex) when ex.Message = expectedMessage -> ()
        | TestResult.Fail(Some ex) ->
            let msg =
                sprintf "Expected Fail with message '%s', but got message '%s'" expectedMessage ex.Message

            raise (AssertionException(msg, Some(box expectedMessage), Some(box ex.Message)))
        | TestResult.Fail None ->
            let msg =
                sprintf "Expected Fail with message '%s', but got Fail with no exception" expectedMessage

            raise (AssertionException(msg, Some(box expectedMessage), Some null))
        | TestResult.Pass ->
            let msg = sprintf "Expected Fail with message '%s', but got Pass" expectedMessage
            raise (AssertionException(msg, Some(box expectedMessage), Some(box "Pass")))
        | TestResult.Skipped reason ->
            let msg =
                sprintf "Expected Fail with message '%s', but got Skipped: %s" expectedMessage reason

            raise (AssertionException(msg, Some(box expectedMessage), Some(box reason)))

/// Expectation wrapper for TestNode values.
type TestNodeExpectation(actual: TestNode) =
    member _.Value = actual

    member _.toBeExample(expectedDesc: string) =
        match actual with
        | Example(desc, _, _)
        | FocusedExample(desc, _, _) when desc = expectedDesc -> ()
        | Example(desc, _, _)
        | FocusedExample(desc, _, _) ->
            let msg =
                sprintf "Expected Example with description '%s', but got '%s'" expectedDesc desc

            raise (AssertionException(msg, Some(box expectedDesc), Some(box desc)))
        | Group(desc, _, _, _)
        | FocusedGroup(desc, _, _, _) ->
            let msg =
                sprintf "Expected Example with description '%s', but got Group '%s'" expectedDesc desc

            raise (AssertionException(msg, Some(box "Example"), Some(box "Group")))
        | BeforeAllHook _
        | BeforeEachHook _
        | AfterEachHook _
        | AfterAllHook _ ->
            let msg =
                sprintf "Expected Example with description '%s', but got Hook" expectedDesc

            raise (AssertionException(msg, Some(box "Example"), Some(box "Hook")))

    member _.toBeGroup(expectedDesc: string) =
        match actual with
        | Group(desc, _, _, _)
        | FocusedGroup(desc, _, _, _) when desc = expectedDesc -> ()
        | Group(desc, _, _, _)
        | FocusedGroup(desc, _, _, _) ->
            let msg =
                sprintf "Expected Group with description '%s', but got '%s'" expectedDesc desc

            raise (AssertionException(msg, Some(box expectedDesc), Some(box desc)))
        | Example(desc, _, _)
        | FocusedExample(desc, _, _) ->
            let msg =
                sprintf "Expected Group with description '%s', but got Example '%s'" expectedDesc desc

            raise (AssertionException(msg, Some(box "Group"), Some(box "Example")))
        | BeforeAllHook _
        | BeforeEachHook _
        | AfterEachHook _
        | AfterAllHook _ ->
            let msg = sprintf "Expected Group with description '%s', but got Hook" expectedDesc
            raise (AssertionException(msg, Some(box "Group"), Some(box "Hook")))

    member _.toBeGroupWithChildren(expectedCount: int) =
        match actual with
        | Group(_, _, children, _)
        | FocusedGroup(_, _, children, _) ->
            let actualCount = List.length children

            if actualCount = expectedCount then
                ()
            else
                let msg =
                    sprintf "Expected Group with %d children, but got %d children" expectedCount actualCount

                raise (AssertionException(msg, Some(box expectedCount), Some(box actualCount)))
        | Example(desc, _, _)
        | FocusedExample(desc, _, _) ->
            let msg =
                sprintf "Expected Group with %d children, but got Example '%s'" expectedCount desc

            raise (AssertionException(msg, Some(box "Group"), Some(box "Example")))
        | BeforeAllHook _
        | BeforeEachHook _
        | AfterEachHook _
        | AfterAllHook _ ->
            let msg = "Expected Group, but got Hook"
            raise (AssertionException(msg, Some(box "Group"), Some(box "Hook")))

/// Custom expectation functions
[<AutoOpen>]
module TestExpectations =

    /// Creates an expectation wrapper for TestResult values.
    /// Usage: expectTestResult(result).toBePass()
    let expectTestResult (actual: TestResult) : TestResultExpectation = TestResultExpectation(actual)

    /// Creates an expectation wrapper for TestNode values.
    /// Usage: expectTestNode(node).toBeExample("description")
    let expectTestNode (actual: TestNode) : TestNodeExpectation = TestNodeExpectation(actual)
