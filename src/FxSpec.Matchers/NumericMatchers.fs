namespace FxSpec.Matchers

open System

/// Matchers for numeric comparisons.
[<AutoOpen>]
module NumericMatchers =
    
    /// Matches if the actual value is greater than the expected value.
    /// Usage: expect 10 |> to' (beGreaterThan 5)
    let beGreaterThan (expected: 'a when 'a : comparison) : Matcher<'a> =
        fun actual ->
            if actual > expected then
                Pass
            else
                let msg = sprintf "Expected value to be greater than %A, but found %A" expected actual
                Fail (msg, Some (box expected), Some (box actual))
    
    /// Matches if the actual value is greater than or equal to the expected value.
    /// Usage: expect 10 |> to' (beGreaterThanOrEqual 10)
    let beGreaterThanOrEqual (expected: 'a when 'a : comparison) : Matcher<'a> =
        fun actual ->
            if actual >= expected then
                Pass
            else
                let msg = sprintf "Expected value to be greater than or equal to %A, but found %A" expected actual
                Fail (msg, Some (box expected), Some (box actual))
    
    /// Matches if the actual value is less than the expected value.
    /// Usage: expect 5 |> to' (beLessThan 10)
    let beLessThan (expected: 'a when 'a : comparison) : Matcher<'a> =
        fun actual ->
            if actual < expected then
                Pass
            else
                let msg = sprintf "Expected value to be less than %A, but found %A" expected actual
                Fail (msg, Some (box expected), Some (box actual))
    
    /// Matches if the actual value is less than or equal to the expected value.
    /// Usage: expect 5 |> to' (beLessThanOrEqual 5)
    let beLessThanOrEqual (expected: 'a when 'a : comparison) : Matcher<'a> =
        fun actual ->
            if actual <= expected then
                Pass
            else
                let msg = sprintf "Expected value to be less than or equal to %A, but found %A" expected actual
                Fail (msg, Some (box expected), Some (box actual))
    
    /// Matches if the actual value is between min and max (inclusive).
    /// Usage: expect 5 |> to' (beBetween 1 10)
    let beBetween (min: 'a when 'a : comparison) (max: 'a when 'a : comparison) : Matcher<'a> =
        if min > max then
            invalidArg (nameof min) "Minimum value must be less than or equal to maximum value"
        fun actual ->
            if actual >= min && actual <= max then
                Pass
            else
                let msg = sprintf "Expected value to be between %A and %A, but found %A" min max actual
                Fail (msg, Some (box (min, max)), Some (box actual))
    
    /// Matches if the actual floating-point value is close to the expected value within a tolerance.
    /// Usage: expect 3.14159 |> to' (beCloseTo 3.14 0.01)
    let beCloseTo (expected: float) (tolerance: float) : Matcher<float> =
        if tolerance < 0.0 then
            invalidArg (nameof tolerance) "Tolerance must be non-negative"
        if Double.IsNaN(tolerance) || Double.IsNaN(expected) then
            invalidArg (nameof expected) "Expected and tolerance must be valid numbers (not NaN)"
        if Double.IsInfinity(tolerance) || Double.IsInfinity(expected) then
            invalidArg (nameof expected) "Expected and tolerance must be finite numbers (not infinity)"
        fun actual ->
            let diff = abs (actual - expected)
            if diff <= tolerance then
                Pass
            else
                let msg = sprintf "Expected value to be close to %f (±%f), but found %f (diff: %f)" expected tolerance actual diff
                Fail (msg, Some (box expected), Some (box actual))
    
    /// Matches if the actual value is positive (> 0).
    /// Usage: expect 5 |> to' bePositive
    let inline bePositive< ^a when ^a : comparison and ^a : (static member Zero : ^a)> : Matcher< ^a> =
        fun actual ->
            let zero = LanguagePrimitives.GenericZero< ^a>
            if actual > zero then
                Pass
            else
                let msg = sprintf "Expected positive value, but found %A" actual
                Fail (msg, Some (box "positive"), Some (box actual))

    /// Matches if the actual value is negative (< 0).
    /// Usage: expect -5 |> to' beNegative
    let inline beNegative< ^a when ^a : comparison and ^a : (static member Zero : ^a)> : Matcher< ^a> =
        fun actual ->
            let zero = LanguagePrimitives.GenericZero< ^a>
            if actual < zero then
                Pass
            else
                let msg = sprintf "Expected negative value, but found %A" actual
                Fail (msg, Some (box "negative"), Some (box actual))

    /// Matches if the actual value is zero.
    /// Usage: expect 0 |> to' beZero
    let inline beZero< ^a when ^a : equality and ^a : (static member Zero : ^a)> : Matcher< ^a> =
        fun actual ->
            let zero = LanguagePrimitives.GenericZero< ^a>
            if actual = zero then
                Pass
            else
                let msg = sprintf "Expected zero, but found %A" actual
                Fail (msg, Some (box zero), Some (box actual))
    
    /// Matches if the actual value is even.
    /// Usage: expect 4 |> to' beEven
    let beEven : Matcher<int> =
        fun actual ->
            if actual % 2 = 0 then
                Pass
            else
                let msg = sprintf "Expected even number, but found %d" actual
                Fail (msg, Some (box "even"), Some (box actual))
    
    /// Matches if the actual value is odd.
    /// Usage: expect 5 |> to' beOdd
    let beOdd : Matcher<int> =
        fun actual ->
            if actual % 2 <> 0 then
                Pass
            else
                let msg = sprintf "Expected odd number, but found %d" actual
                Fail (msg, Some (box "odd"), Some (box actual))
    
    /// Matches if the actual value is divisible by the expected divisor.
    /// Usage: expect 15 |> to' (beDivisibleBy 5)
    let beDivisibleBy (divisor: int) : Matcher<int> =
        if divisor = 0 then
            invalidArg (nameof divisor) "Divisor cannot be zero"
        fun actual ->
            if actual % divisor = 0 then
                Pass
            else
                let msg = sprintf "Expected %d to be divisible by %d, but it is not" actual divisor
                Fail (msg, Some (box divisor), Some (box actual))

