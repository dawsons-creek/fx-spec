namespace FX.Spec.Matchers

/// Represents the outcome of a matcher evaluation.
/// This is the core type that enables type-safe, composable assertions.
type MatchResult =
    /// The matcher passed - the actual value met the expectation.
    | Pass
    
    /// The matcher failed - the actual value did not meet the expectation.
    /// Contains a descriptive message and optional expected/actual values for diffing.
    | Fail of message: string * expected: obj option * actual: obj option

/// Exception thrown when an assertion fails.
/// This is caught by the test runner to mark a test as failed.
type AssertionException(message: string, expected: obj option, actual: obj option) =
    inherit System.Exception(message)
    
    /// The expected value, if available.
    member _.Expected = expected
    
    /// The actual value, if available.
    member _.Actual = actual
    
    /// Creates an AssertionException from a MatchResult.Fail case.
    static member FromMatchResult(result: MatchResult) =
        match result with
        | Fail (msg, exp, act) -> AssertionException(msg, exp, act)
        | Pass -> invalidArg "result" "Cannot create AssertionException from Pass"

/// Helper module for working with MatchResult.
module MatchResult =
    /// Checks if a match result is a pass.
    let isPass = function
        | Pass -> true
        | Fail _ -> false
    
    /// Checks if a match result is a failure.
    let isFail = function
        | Fail _ -> true
        | Pass -> false
    
    /// Gets the failure message if the result is a failure.
    let tryGetMessage = function
        | Pass -> None
        | Fail (msg, _, _) -> Some msg
    
    /// Gets the expected value if the result is a failure.
    let tryGetExpected = function
        | Pass -> None
        | Fail (_, exp, _) -> exp
    
    /// Gets the actual value if the result is a failure.
    let tryGetActual = function
        | Pass -> None
        | Fail (_, _, act) -> act
    
    /// Combines two match results with AND logic.
    /// Both must pass for the result to be Pass.
    let combine result1 result2 =
        match result1, result2 with
        | Pass, Pass -> Pass
        | Fail (msg, exp, act), _ -> Fail (msg, exp, act)
        | _, Fail (msg, exp, act) -> Fail (msg, exp, act)
    
    /// Negates a match result.
    /// Pass becomes Fail and vice versa.
    let negate result =
        match result with
        | Pass -> Fail ("Expected not to match, but it did", None, None)
        | Fail _ -> Pass

