/// Custom matchers for testing FxSpec's own types.
/// These matchers allow us to dogfood FxSpec by using it to test itself.
module FxSpec.Core.Tests.FxSpecMatchers

open FxSpec.Core
open FxSpec.Matchers

/// Matches if the TestResult is Pass.
/// Usage: expect result |> to' bePass
let bePass : Matcher<TestResult> =
    fun actual ->
        match actual with
        | TestResult.Pass -> MatchResult.Pass
        | TestResult.Fail ex ->
            let msg = sprintf "Expected Pass, but got Fail: %A" ex
            MatchResult.Fail (msg, Some (box "Pass"), Some (box actual))
        | TestResult.Skipped reason ->
            let msg = sprintf "Expected Pass, but got Skipped: %s" reason
            MatchResult.Fail (msg, Some (box "Pass"), Some (box actual))

/// Matches if the TestResult is Fail.
/// Usage: expect result |> to' beFail
let beFail : Matcher<TestResult> =
    fun actual ->
        match actual with
        | TestResult.Fail _ -> MatchResult.Pass
        | TestResult.Pass ->
            let msg = "Expected Fail, but got Pass"
            MatchResult.Fail (msg, Some (box "Fail"), Some (box "Pass"))
        | TestResult.Skipped reason ->
            let msg = sprintf "Expected Fail, but got Skipped: %s" reason
            MatchResult.Fail (msg, Some (box "Fail"), Some (box actual))

/// Matches if the TestResult is Fail with a specific exception message.
/// Usage: expect result |> to' (beFailWith "error message")
let beFailWith (expectedMessage: string) : Matcher<TestResult> =
    fun actual ->
        match actual with
        | TestResult.Fail (Some ex) when ex.Message = expectedMessage -> MatchResult.Pass
        | TestResult.Fail (Some ex) ->
            let msg = sprintf "Expected Fail with message '%s', but got message '%s'" expectedMessage ex.Message
            MatchResult.Fail (msg, Some (box expectedMessage), Some (box ex.Message))
        | TestResult.Fail None ->
            let msg = sprintf "Expected Fail with message '%s', but got Fail with no exception" expectedMessage
            MatchResult.Fail (msg, Some (box expectedMessage), Some null)
        | TestResult.Pass ->
            let msg = sprintf "Expected Fail with message '%s', but got Pass" expectedMessage
            MatchResult.Fail (msg, Some (box expectedMessage), Some (box "Pass"))
        | TestResult.Skipped reason ->
            let msg = sprintf "Expected Fail with message '%s', but got Skipped: %s" expectedMessage reason
            MatchResult.Fail (msg, Some (box expectedMessage), Some (box reason))

/// Matches if the TestResult is Skipped.
/// Usage: expect result |> to' beSkipped
let beSkipped : Matcher<TestResult> =
    fun actual ->
        match actual with
        | TestResult.Skipped _ -> MatchResult.Pass
        | TestResult.Pass ->
            let msg = "Expected Skipped, but got Pass"
            MatchResult.Fail (msg, Some (box "Skipped"), Some (box "Pass"))
        | TestResult.Fail ex ->
            let msg = sprintf "Expected Skipped, but got Fail: %A" ex
            MatchResult.Fail (msg, Some (box "Skipped"), Some (box actual))

/// Matches if the TestResult is Skipped with a specific reason.
/// Usage: expect result |> to' (beSkippedWith "reason")
let beSkippedWith (expectedReason: string) : Matcher<TestResult> =
    fun actual ->
        match actual with
        | TestResult.Skipped reason when reason = expectedReason -> MatchResult.Pass
        | TestResult.Skipped reason ->
            let msg = sprintf "Expected Skipped with reason '%s', but got reason '%s'" expectedReason reason
            MatchResult.Fail (msg, Some (box expectedReason), Some (box reason))
        | TestResult.Pass ->
            let msg = sprintf "Expected Skipped with reason '%s', but got Pass" expectedReason
            MatchResult.Fail (msg, Some (box expectedReason), Some (box "Pass"))
        | TestResult.Fail ex ->
            let msg = sprintf "Expected Skipped with reason '%s', but got Fail: %A" expectedReason ex
            MatchResult.Fail (msg, Some (box expectedReason), Some (box actual))

/// Matches if the TestNode is an Example with the expected description.
/// Usage: expect node |> to' (beExample "test description")
let beExample (expectedDesc: string) : Matcher<TestNode> =
    fun actual ->
        match actual with
        | Example (desc, _) | FocusedExample (desc, _) when desc = expectedDesc -> MatchResult.Pass
        | Example (desc, _) | FocusedExample (desc, _) ->
            let msg = sprintf "Expected Example with description '%s', but got '%s'" expectedDesc desc
            MatchResult.Fail (msg, Some (box expectedDesc), Some (box desc))
        | Group (desc, _, _) | FocusedGroup (desc, _, _) ->
            let msg = sprintf "Expected Example with description '%s', but got Group '%s'" expectedDesc desc
            MatchResult.Fail (msg, Some (box "Example"), Some (box "Group"))
        | BeforeAllHook _ | BeforeEachHook _ | AfterEachHook _ | AfterAllHook _ ->
            let msg = sprintf "Expected Example with description '%s', but got Hook" expectedDesc
            MatchResult.Fail (msg, Some (box "Example"), Some (box "Hook"))

/// Matches if the TestNode is a Group with the expected description.
/// Usage: expect node |> to' (beGroup "group description")
let beGroup (expectedDesc: string) : Matcher<TestNode> =
    fun actual ->
        match actual with
        | Group (desc, _, _) | FocusedGroup (desc, _, _) when desc = expectedDesc -> MatchResult.Pass
        | Group (desc, _, _) | FocusedGroup (desc, _, _) ->
            let msg = sprintf "Expected Group with description '%s', but got '%s'" expectedDesc desc
            MatchResult.Fail (msg, Some (box expectedDesc), Some (box desc))
        | Example (desc, _) | FocusedExample (desc, _) ->
            let msg = sprintf "Expected Group with description '%s', but got Example '%s'" expectedDesc desc
            MatchResult.Fail (msg, Some (box "Group"), Some (box "Example"))
        | BeforeAllHook _ | BeforeEachHook _ | AfterEachHook _ | AfterAllHook _ ->
            let msg = sprintf "Expected Group with description '%s', but got Hook" expectedDesc
            MatchResult.Fail (msg, Some (box "Group"), Some (box "Hook"))

/// Matches if the TestNode is a Group with the expected number of children.
/// Usage: expect node |> to' (beGroupWithChildren 3)
let beGroupWithChildren (expectedCount: int) : Matcher<TestNode> =
    fun actual ->
        match actual with
        | Group (_, _, children) | FocusedGroup (_, _, children) ->
            let actualCount = List.length children
            if actualCount = expectedCount then
                MatchResult.Pass
            else
                let msg = sprintf "Expected Group with %d children, but got %d children" expectedCount actualCount
                MatchResult.Fail (msg, Some (box expectedCount), Some (box actualCount))
        | Example (desc, _) | FocusedExample (desc, _) ->
            let msg = sprintf "Expected Group with %d children, but got Example '%s'" expectedCount desc
            MatchResult.Fail (msg, Some (box "Group"), Some (box "Example"))
        | BeforeAllHook _ | BeforeEachHook _ | AfterEachHook _ | AfterAllHook _ ->
            let msg = "Expected Group, but got Hook"
            MatchResult.Fail (msg, Some (box "Group"), Some (box "Hook"))

/// Matches if the TestNode has the expected number of examples (recursively).
/// Usage: expect node |> to' (haveExampleCount 5)
let haveExampleCount (expectedCount: int) : Matcher<TestNode> =
    fun actual ->
        let actualCount = TestNode.countExamples actual
        if actualCount = expectedCount then
            MatchResult.Pass
        else
            let msg = sprintf "Expected %d examples, but found %d" expectedCount actualCount
            MatchResult.Fail (msg, Some (box expectedCount), Some (box actualCount))

/// Matches if the TestNode has the expected number of groups (recursively).
/// Usage: expect node |> to' (haveGroupCount 3)
let haveGroupCount (expectedCount: int) : Matcher<TestNode> =
    fun actual ->
        let actualCount = TestNode.countGroups actual
        if actualCount = expectedCount then
            MatchResult.Pass
        else
            let msg = sprintf "Expected %d groups, but found %d" expectedCount actualCount
            MatchResult.Fail (msg, Some (box expectedCount), Some (box actualCount))

