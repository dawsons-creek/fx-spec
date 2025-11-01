namespace FxSpec.Matchers

open System
open System.Text.RegularExpressions

/// Matchers for string assertions.
[<AutoOpen>]
module StringMatchers =

    /// Private helper to handle null checks consistently across string matchers.
    let private nullSafeStringMatcher
        (nullMessage: string)
        (expected: obj option)
        (predicate: string -> bool)
        (failureMessage: string -> string)
        : Matcher<string> =
        fun actual ->
            if isNull actual then
                Fail (nullMessage, expected, Some null)
            elif predicate actual then
                Pass
            else
                Fail (failureMessage actual, expected, Some (box actual))

    /// Matches if the string starts with the expected prefix.
    /// Usage: expect "hello world" |> to' (startWith "hello")
    let startWith (expected: string) : Matcher<string> =
        nullSafeStringMatcher
            "Expected string to start with prefix, but found null"
            (Some (box expected))
            (fun actual -> actual.StartsWith(expected))
            (fun actual -> sprintf "Expected string to start with '%s', but found '%s'" expected actual)

    /// Matches if the string ends with the expected suffix.
    /// Usage: expect "hello world" |> to' (endWith "world")
    let endWith (expected: string) : Matcher<string> =
        nullSafeStringMatcher
            "Expected string to end with suffix, but found null"
            (Some (box expected))
            (fun actual -> actual.EndsWith(expected))
            (fun actual -> sprintf "Expected string to end with '%s', but found '%s'" expected actual)

    /// Matches if the string contains the expected substring.
    /// Usage: expect "hello world" |> to' (containSubstring "lo wo")
    let containSubstring (expected: string) : Matcher<string> =
        nullSafeStringMatcher
            "Expected string to contain substring, but found null"
            (Some (box expected))
            (fun actual -> actual.Contains(expected))
            (fun actual -> sprintf "Expected string to contain '%s', but found '%s'" expected actual)

    /// Matches if the string matches the regular expression pattern.
    /// Usage: expect "hello123" |> to' (matchRegex "hello\\d+")
    let matchRegex (pattern: string) : Matcher<string> =
        let regex = Regex(pattern)
        nullSafeStringMatcher
            "Expected string to match regex, but found null"
            (Some (box pattern))
            (fun actual -> regex.IsMatch(actual))
            (fun actual -> sprintf "Expected string to match pattern '%s', but found '%s'" pattern actual)

    /// Matches if the string is empty.
    /// Usage: expect "" |> to' beEmptyString
    let beEmptyString : Matcher<string> =
        nullSafeStringMatcher
            "Expected empty string, but found null"
            (Some (box ""))
            String.IsNullOrEmpty
            (fun actual -> sprintf "Expected empty string, but found '%s'" actual)

    /// Matches if the string is null or empty.
    /// Usage: expect "" |> to' beNullOrEmpty
    let beNullOrEmpty : Matcher<string> = function
        | null | "" -> Pass
        | actual -> Fail (sprintf "Expected null or empty string, but found '%s'" actual,
                         Some (box "null or empty"), Some (box actual))

    /// Matches if the string is null, empty, or whitespace.
    /// Usage: expect "   " |> to' beNullOrWhitespace
    let beNullOrWhitespace : Matcher<string> = function
        | actual when String.IsNullOrWhiteSpace(actual) -> Pass
        | actual -> Fail (sprintf "Expected null, empty, or whitespace string, but found '%s'" actual,
                         Some (box "null, empty, or whitespace"), Some (box actual))

    /// Matches if the string has the expected length.
    /// Usage: expect "hello" |> to' (haveStringLength 5)
    let haveStringLength (expected: int) : Matcher<string> =
        fun actual ->
            if isNull actual then
                Fail (sprintf "Expected string of length %d, but found null" expected,
                     Some (box expected), Some null)
            else
                let actualLength = actual.Length
                if actualLength = expected then
                    Pass
                else
                    Fail (sprintf "Expected string of length %d, but found length %d ('%s')" expected actualLength actual,
                         Some (box expected), Some (box actualLength))

    /// Matches if the string equals the expected value (case-insensitive).
    /// Usage: expect "HELLO" |> to' (equalIgnoreCase "hello")
    let equalIgnoreCase (expected: string) : Matcher<string> = function
        | null when isNull expected -> Pass
        | null -> Fail (sprintf "Expected '%s', but found null" expected,
                       Some (box expected), Some null)
        | actual when isNull expected -> Fail (sprintf "Expected null, but found '%s'" actual,
                                              Some null, Some (box actual))
        | actual when String.Equals(actual, expected, StringComparison.OrdinalIgnoreCase) -> Pass
        | actual -> Fail (sprintf "Expected '%s' (case-insensitive), but found '%s'" expected actual,
                         Some (box expected), Some (box actual))

    /// Matches if the string contains only letters.
    /// Usage: expect "hello" |> to' beAlphabetic
    let beAlphabetic : Matcher<string> = function
        | null -> Fail ("Expected alphabetic string, but found null",
                       Some (box "alphabetic"), Some null)
        | "" -> Fail ("Expected alphabetic string, but found empty string",
                     Some (box "alphabetic"), Some (box ""))
        | actual when actual |> Seq.forall Char.IsLetter -> Pass
        | actual -> Fail (sprintf "Expected alphabetic string, but found '%s'" actual,
                         Some (box "alphabetic"), Some (box actual))

    /// Matches if the string contains only digits.
    /// Usage: expect "12345" |> to' beNumeric
    let beNumeric : Matcher<string> = function
        | null -> Fail ("Expected numeric string, but found null",
                       Some (box "numeric"), Some null)
        | "" -> Fail ("Expected numeric string, but found empty string",
                     Some (box "numeric"), Some (box ""))
        | actual when actual |> Seq.forall Char.IsDigit -> Pass
        | actual -> Fail (sprintf "Expected numeric string, but found '%s'" actual,
                         Some (box "numeric"), Some (box actual))
