namespace FxSpec.Matchers

open System
open System.Text.RegularExpressions

/// Matchers for string assertions.
[<AutoOpen>]
module StringMatchers =
    
    /// Matches if the string starts with the expected prefix.
    /// Usage: expect "hello world" |> to' (startWith "hello")
    let startWith (expected: string) : Matcher<string> =
        fun actual ->
            if isNull actual then
                let msg = "Expected string to start with prefix, but found null"
                Fail (msg, Some (box expected), Some null)
            elif actual.StartsWith(expected) then
                Pass
            else
                let msg = sprintf "Expected string to start with '%s', but found '%s'" expected actual
                Fail (msg, Some (box expected), Some (box actual))
    
    /// Matches if the string ends with the expected suffix.
    /// Usage: expect "hello world" |> to' (endWith "world")
    let endWith (expected: string) : Matcher<string> =
        fun actual ->
            if isNull actual then
                let msg = "Expected string to end with suffix, but found null"
                Fail (msg, Some (box expected), Some null)
            elif actual.EndsWith(expected) then
                Pass
            else
                let msg = sprintf "Expected string to end with '%s', but found '%s'" expected actual
                Fail (msg, Some (box expected), Some (box actual))
    
    /// Matches if the string contains the expected substring.
    /// Usage: expect "hello world" |> to' (containSubstring "lo wo")
    let containSubstring (expected: string) : Matcher<string> =
        fun actual ->
            if isNull actual then
                let msg = "Expected string to contain substring, but found null"
                Fail (msg, Some (box expected), Some null)
            elif actual.Contains(expected) then
                Pass
            else
                let msg = sprintf "Expected string to contain '%s', but found '%s'" expected actual
                Fail (msg, Some (box expected), Some (box actual))
    
    /// Matches if the string matches the regular expression pattern.
    /// Usage: expect "hello123" |> to' (matchRegex "hello\\d+")
    let matchRegex (pattern: string) : Matcher<string> =
        fun actual ->
            if isNull actual then
                let msg = "Expected string to match regex, but found null"
                Fail (msg, Some (box pattern), Some null)
            else
                let regex = Regex(pattern)
                if regex.IsMatch(actual) then
                    Pass
                else
                    let msg = sprintf "Expected string to match pattern '%s', but found '%s'" pattern actual
                    Fail (msg, Some (box pattern), Some (box actual))
    
    /// Matches if the string is empty.
    /// Usage: expect "" |> to' beEmptyString
    let beEmptyString : Matcher<string> =
        fun actual ->
            if isNull actual then
                let msg = "Expected empty string, but found null"
                Fail (msg, Some (box ""), Some null)
            elif String.IsNullOrEmpty(actual) then
                Pass
            else
                let msg = sprintf "Expected empty string, but found '%s'" actual
                Fail (msg, Some (box ""), Some (box actual))
    
    /// Matches if the string is null or empty.
    /// Usage: expect "" |> to' beNullOrEmpty
    let beNullOrEmpty : Matcher<string> =
        fun actual ->
            if String.IsNullOrEmpty(actual) then
                Pass
            else
                let msg = sprintf "Expected null or empty string, but found '%s'" actual
                Fail (msg, Some (box "null or empty"), Some (box actual))
    
    /// Matches if the string is null, empty, or whitespace.
    /// Usage: expect "   " |> to' beNullOrWhitespace
    let beNullOrWhitespace : Matcher<string> =
        fun actual ->
            if String.IsNullOrWhiteSpace(actual) then
                Pass
            else
                let msg = sprintf "Expected null, empty, or whitespace string, but found '%s'" actual
                Fail (msg, Some (box "null, empty, or whitespace"), Some (box actual))
    
    /// Matches if the string has the expected length.
    /// Usage: expect "hello" |> to' (haveStringLength 5)
    let haveStringLength (expected: int) : Matcher<string> =
        fun actual ->
            if isNull actual then
                let msg = sprintf "Expected string of length %d, but found null" expected
                Fail (msg, Some (box expected), Some null)
            else
                let actualLength = actual.Length
                if actualLength = expected then
                    Pass
                else
                    let msg = sprintf "Expected string of length %d, but found length %d ('%s')" expected actualLength actual
                    Fail (msg, Some (box expected), Some (box actualLength))
    
    /// Matches if the string equals the expected value (case-insensitive).
    /// Usage: expect "HELLO" |> to' (equalIgnoreCase "hello")
    let equalIgnoreCase (expected: string) : Matcher<string> =
        fun actual ->
            if isNull actual && isNull expected then
                Pass
            elif isNull actual || isNull expected then
                let msg = sprintf "Expected '%s', but found '%s'" expected actual
                Fail (msg, Some (box expected), Some (box actual))
            elif String.Equals(actual, expected, StringComparison.OrdinalIgnoreCase) then
                Pass
            else
                let msg = sprintf "Expected '%s' (case-insensitive), but found '%s'" expected actual
                Fail (msg, Some (box expected), Some (box actual))
    
    /// Matches if the string contains only letters.
    /// Usage: expect "hello" |> to' beAlphabetic
    let beAlphabetic : Matcher<string> =
        fun actual ->
            if isNull actual then
                let msg = "Expected alphabetic string, but found null"
                Fail (msg, Some (box "alphabetic"), Some null)
            elif String.IsNullOrEmpty(actual) then
                let msg = "Expected alphabetic string, but found empty string"
                Fail (msg, Some (box "alphabetic"), Some (box ""))
            elif actual |> Seq.forall Char.IsLetter then
                Pass
            else
                let msg = sprintf "Expected alphabetic string, but found '%s'" actual
                Fail (msg, Some (box "alphabetic"), Some (box actual))
    
    /// Matches if the string contains only digits.
    /// Usage: expect "12345" |> to' beNumeric
    let beNumeric : Matcher<string> =
        fun actual ->
            if isNull actual then
                let msg = "Expected numeric string, but found null"
                Fail (msg, Some (box "numeric"), Some null)
            elif String.IsNullOrEmpty(actual) then
                let msg = "Expected numeric string, but found empty string"
                Fail (msg, Some (box "numeric"), Some (box ""))
            elif actual |> Seq.forall Char.IsDigit then
                Pass
            else
                let msg = sprintf "Expected numeric string, but found '%s'" actual
                Fail (msg, Some (box "numeric"), Some (box actual))

