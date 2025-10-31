namespace FxSpec.Matchers

/// Core matchers for common assertions.
[<AutoOpen>]
module CoreMatchers =
    
    /// Matches if the actual value equals the expected value.
    /// Uses F#'s structural equality.
    /// Usage: expect actual |> to' (equal expected)
    let equal (expected: 'a) : Matcher<'a> =
        fun actual ->
            if actual = expected then
                Pass
            else
                let msg = sprintf "Expected %A, but found %A" expected actual
                Fail (msg, Some (box expected), Some (box actual))
    
    /// Matches if the actual value is null.
    /// Usage: expect actual |> to' beNil
    let beNil<'a when 'a : null> : Matcher<'a> =
        fun actual ->
            if obj.ReferenceEquals(actual, null) then
                Pass
            else
                let msg = "Expected null, but found non-null value"
                Fail (msg, Some null, Some (box actual))
    
    /// Matches if the actual value is not null.
    /// Usage: expect actual |> to' notBeNil
    let notBeNil<'a when 'a : null> : Matcher<'a> =
        fun actual ->
            if not (obj.ReferenceEquals(actual, null)) then
                Pass
            else
                let msg = "Expected non-null value, but found null"
                Fail (msg, None, Some null)
    
    /// Matches if the actual Option is Some with the expected value.
    /// Usage: expect actual |> to' (beSome 42)
    let beSome (expected: 'a) : Matcher<'a option> =
        fun actual ->
            match actual with
            | Some value when value = expected ->
                Pass
            | Some value ->
                let msg = sprintf "Expected Some %A, but found Some %A" expected value
                Fail (msg, Some (box expected), Some (box value))
            | None ->
                let msg = sprintf "Expected Some %A, but found None" expected
                Fail (msg, Some (box expected), Some (box None))
    
    /// Matches if the actual Option is None.
    /// Usage: expect actual |> to' beNone
    let beNone<'a> : Matcher<'a option> =
        fun actual ->
            match actual with
            | None -> Pass
            | Some value ->
                let msg = sprintf "Expected None, but found Some %A" value
                Fail (msg, Some (box None), Some (box value))
    
    /// Matches if the actual Result is Ok with the expected value.
    /// Usage: expect actual |> to' (beOk 42)
    let beOk (expected: 'a) : Matcher<Result<'a, 'b>> =
        fun actual ->
            match actual with
            | Ok value when value = expected ->
                Pass
            | Ok value ->
                let msg = sprintf "Expected Ok %A, but found Ok %A" expected value
                Fail (msg, Some (box expected), Some (box value))
            | Error err ->
                let msg = sprintf "Expected Ok %A, but found Error %A" expected err
                Fail (msg, Some (box expected), Some (box err))
    
    /// Matches if the actual Result is Error with the expected value.
    /// Usage: expect actual |> to' (beError "failed")
    let beError (expected: 'b) : Matcher<Result<'a, 'b>> =
        fun actual ->
            match actual with
            | Error err when err = expected ->
                Pass
            | Error err ->
                let msg = sprintf "Expected Error %A, but found Error %A" expected err
                Fail (msg, Some (box expected), Some (box err))
            | Ok value ->
                let msg = sprintf "Expected Error %A, but found Ok %A" expected value
                Fail (msg, Some (box expected), Some (box value))
    
    /// Matches if the actual value is true.
    /// Usage: expect actual |> to' beTrue
    let beTrue : Matcher<bool> =
        fun actual ->
            if actual then
                Pass
            else
                let msg = "Expected true, but found false"
                Fail (msg, Some (box true), Some (box false))
    
    /// Matches if the actual value is false.
    /// Usage: expect actual |> to' beFalse
    let beFalse : Matcher<bool> =
        fun actual ->
            if not actual then
                Pass
            else
                let msg = "Expected false, but found true"
                Fail (msg, Some (box false), Some (box true))
    
    /// Matches if the actual value satisfies the predicate.
    /// Usage: expect actual |> to' (satisfy (fun x -> x > 0))
    let satisfy (predicate: 'a -> bool) (description: string) : Matcher<'a> =
        fun actual ->
            if predicate actual then
                Pass
            else
                let msg = sprintf "Expected value to satisfy '%s', but %A did not" description actual
                Fail (msg, None, Some (box actual))
    
    /// Matches if the actual value is the same reference as the expected value.
    /// Usage: expect actual |> to' (beSameAs expected)
    let beSameAs (expected: 'a) : Matcher<'a> =
        fun actual ->
            if obj.ReferenceEquals(actual, expected) then
                Pass
            else
                let msg = "Expected same reference, but found different references"
                Fail (msg, Some (box expected), Some (box actual))
    
    /// Matches if the actual value is of the specified type.
    /// Usage: expect actual |> to' (beOfType<string>)
    let beOfType<'T> : Matcher<obj> =
        fun actual ->
            if actual :? 'T then
                Pass
            else
                let expectedType = typeof<'T>.Name
                let actualType = if actual = null then "null" else actual.GetType().Name
                let msg = sprintf "Expected type %s, but found type %s" expectedType actualType
                Fail (msg, Some (box expectedType), Some (box actualType))

