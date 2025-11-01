namespace FxSpec.Matchers

/// A matcher is a function that takes an actual value and returns a MatchResult.
/// Matchers are composable and type-safe.
type Matcher<'a> = 'a -> MatchResult

/// Core assertion functions for the fluent API.
[<AutoOpen>]
module Assertions =
    
    /// The starting point for an assertion.
    /// This is an identity function that exists purely for readability.
    /// Usage: expect actual |> to' (equal expected)
    let expect (actual: 'a) : 'a = actual
    
    /// Applies a matcher to an actual value and throws if it fails.
    /// This is the core assertion engine.
    /// Usage: expect actual |> to' matcher
    let to' (matcher: Matcher<'a>) (actual: 'a) : unit =
        match matcher actual with
        | Pass -> ()
        | Fail (msg, exp, act) ->
            raise (AssertionException(msg, exp, act))
    
    /// Applies a negated matcher to an actual value.
    /// Usage: expect actual |> notTo' matcher
    let notTo' (matcher: Matcher<'a>) (actual: 'a) : unit =
        match matcher actual with
        | Pass ->
            raise (AssertionException("Expected not to match, but it did", None, Some (box actual)))
        | Fail _ -> ()

