namespace FxSpec.Matchers

/// Wrapper type that enables fluent method chaining for assertions.
/// Usage: expect(actual).toEqual(expected)
type Expectation<'a when 'a : equality>(actual: 'a) =
    member _.Value = actual
    
    /// Asserts that the actual value equals the expected value.
    /// Usage: expect(actual).toEqual(expected)
    member _.toEqual(expected: 'a) =
        if actual = expected then
            ()
        else
            let msg = sprintf "Expected %A, but found %A" expected actual
            raise (AssertionException(msg, Some (box expected), Some (box actual)))
    
    /// Asserts that the actual value is null.
    /// Usage: expect(actual).toBeNull()
    member _.toBeNull() =
        match box actual with
        | null -> ()
        | _ -> raise (AssertionException("Expected null, but found non-null value",
                                         Some null, Some (box actual)))
    
    /// Asserts that the actual value is not null.
    /// Usage: expect(actual).notToBeNull()
    member _.notToBeNull() =
        match box actual with
        | null -> raise (AssertionException("Expected non-null value, but found null",
                                            None, Some null))
        | _ -> ()
    
    /// Asserts that the value satisfies a predicate.
    /// Usage: expect(10).toSatisfy((fun x -> x > 5), "be greater than 5")
    member _.toSatisfy(predicate: 'a -> bool, description: string) =
        if predicate actual then ()
        else
            let msg = sprintf "Expected %A to %s, but it did not" actual description
            raise (AssertionException(msg, Some (box description), Some (box actual)))
    
    /// Asserts that the value is the same reference as expected.
    /// Usage: expect(obj1).toBeSameAs(obj2)
    member _.toBeSameAs(expected: 'a) =
        if System.Object.ReferenceEquals(box actual, box expected) then ()
        else
            let msg = sprintf "Expected %A and %A to be the same reference" expected actual
            raise (AssertionException(msg, Some (box expected), Some (box actual)))
    
    /// Asserts that the value is of the expected type.
    /// Usage: expect(value).toBeOfType<string>()
    member _.toBeOfType<'T>() =
        match box actual with
        | :? 'T -> ()
        | _ ->
            let actualType = actual.GetType()
            let expectedType = typeof<'T>
            let msg = sprintf "Expected type %s, but found type %s" expectedType.Name actualType.Name
            raise (AssertionException(msg, Some (box expectedType.Name), Some (box actualType.Name)))

/// Wrapper for Option expectations with specific assertions.
type OptionExpectation<'a when 'a : equality>(actual: 'a option) =
    member _.Value = actual
    
    /// Asserts that the Option is Some with the expected value.
    /// Usage: expect(actual).toBeSome(42)
    member _.toBeSome(expected: 'a) =
        match actual with
        | Some value when value = expected -> ()
        | Some value ->
            let msg = sprintf "Expected Some %A, but found Some %A" expected value
            raise (AssertionException(msg, Some (box expected), Some (box value)))
        | None ->
            let msg = sprintf "Expected Some %A, but found None" expected
            raise (AssertionException(msg, Some (box expected), Some (box None)))
    
    /// Asserts that the Option is None.
    /// Usage: expect(actual).toBeNone()
    member _.toBeNone() =
        match actual with
        | None -> ()
        | Some value -> raise (AssertionException(sprintf "Expected None, but found Some %A" value,
                                                  Some (box None), Some (box value)))

/// Wrapper for Result expectations with specific assertions.
type ResultExpectation<'a, 'b when 'a : equality and 'b : equality>(actual: Result<'a, 'b>) =
    member _.Value = actual
    
    /// Asserts that the Result is Ok with the expected value.
    /// Usage: expect(actual).toBeOk(42)
    member _.toBeOk(expected: 'a) =
        match actual with
        | Ok value when value = expected -> ()
        | Ok value ->
            let msg = sprintf "Expected Ok %A, but found Ok %A" expected value
            raise (AssertionException(msg, Some (box expected), Some (box value)))
        | Error err ->
            let msg = sprintf "Expected Ok %A, but found Error %A" expected err
            raise (AssertionException(msg, Some (box expected), Some (box err)))
    
    /// Asserts that the Result is Error with the expected value.
    /// Usage: expect(actual).toBeError("failed")
    member _.toBeError(expected: 'b) =
        match actual with
        | Error err when err = expected -> ()
        | Error err ->
            let msg = sprintf "Expected Error %A, but found Error %A" expected err
            raise (AssertionException(msg, Some (box expected), Some (box err)))
        | Ok value ->
            let msg = sprintf "Expected Error %A, but found Ok %A" expected value
            raise (AssertionException(msg, Some (box expected), Some (box value)))

/// Wrapper for boolean expectations.
type BoolExpectation(actual: bool) =
    member _.Value = actual
    
    /// Asserts that the value is true.
    /// Usage: expect(actual).toBeTrue()
    member _.toBeTrue() =
        if actual then ()
        else raise (AssertionException("Expected true, but found false",
                                       Some (box true), Some (box false)))
    
    /// Asserts that the value is false.
    /// Usage: expect(actual).toBeFalse()
    member _.toBeFalse() =
        if not actual then ()
        else raise (AssertionException("Expected false, but found true",
                                       Some (box false), Some (box true)))

/// Wrapper for numeric expectations with comparison assertions.
type NumericExpectation<'a when 'a : comparison>(actual: 'a) =
    member _.Value = actual
    
    /// Asserts that the value equals the expected value.
    member _.toEqual(expected: 'a) =
        if actual = expected then ()
        else
            let msg = sprintf "Expected %A, but found %A" expected actual
            raise (AssertionException(msg, Some (box expected), Some (box actual)))
    
    /// Asserts that the value is greater than the expected value.
    /// Usage: expect(actual).toBeGreaterThan(5)
    member _.toBeGreaterThan(expected: 'a) =
        if actual > expected then ()
        else
            let msg = sprintf "Expected %A to be greater than %A" actual expected
            raise (AssertionException(msg, Some (box expected), Some (box actual)))
    
    /// Asserts that the value is less than the expected value.
    /// Usage: expect(actual).toBeLessThan(10)
    member _.toBeLessThan(expected: 'a) =
        if actual < expected then ()
        else
            let msg = sprintf "Expected %A to be less than %A" actual expected
            raise (AssertionException(msg, Some (box expected), Some (box actual)))
    
    /// Asserts that the value is greater than or equal to the expected value.
    member _.toBeGreaterThanOrEqual(expected: 'a) =
        if actual >= expected then ()
        else
            let msg = sprintf "Expected %A to be greater than or equal to %A" actual expected
            raise (AssertionException(msg, Some (box expected), Some (box actual)))
    
    /// Asserts that the value is less than or equal to the expected value.
    member _.toBeLessThanOrEqual(expected: 'a) =
        if actual <= expected then ()
        else
            let msg = sprintf "Expected %A to be less than or equal to %A" actual expected
            raise (AssertionException(msg, Some (box expected), Some (box actual)))
    
    /// Asserts that the value is between min and max (inclusive).
    /// Usage: expectNum(5).toBeBetween(1, 10)
    member _.toBeBetween(min: 'a, max: 'a) =
        if actual >= min && actual <= max then ()
        else
            let msg = sprintf "Expected %A to be between %A and %A" actual min max
            raise (AssertionException(msg, Some (box (min, max)), Some (box actual)))

/// Wrapper for floating point expectations with closeness comparison.
type FloatExpectation(actual: float) =
    member _.Value = actual
    
    /// Asserts that the value is close to expected within tolerance.
    /// Usage: expectFloat(10.1).toBeCloseTo(10.0, 0.2)
    member _.toBeCloseTo(expected: float, tolerance: float) =
        let diff = abs (actual - expected)
        if diff <= tolerance then ()
        else
            let msg = sprintf "Expected %f to be close to %f (within %f), but difference was %f" actual expected tolerance diff
            raise (AssertionException(msg, Some (box expected), Some (box actual)))
    
    /// Asserts that the value is positive (> 0).
    member _.toBePositive() =
        if actual > 0.0 then ()
        else
            let msg = sprintf "Expected %f to be positive" actual
            raise (AssertionException(msg, Some (box "positive"), Some (box actual)))
    
    /// Asserts that the value is negative (< 0).
    member _.toBeNegative() =
        if actual < 0.0 then ()
        else
            let msg = sprintf "Expected %f to be negative" actual
            raise (AssertionException(msg, Some (box "negative"), Some (box actual)))
    
    /// Asserts that the value is zero.
    member _.toBeZero() =
        if actual = 0.0 then ()
        else
            let msg = sprintf "Expected %f to be zero" actual
            raise (AssertionException(msg, Some (box 0.0), Some (box actual)))

/// Wrapper for integer expectations with additional integer-specific assertions.
type IntExpectation(actual: int) =
    member _.Value = actual
    
    /// Asserts that the value equals the expected value.
    member _.toEqual(expected: int) =
        if actual = expected then ()
        else
            let msg = sprintf "Expected %d, but found %d" expected actual
            raise (AssertionException(msg, Some (box expected), Some (box actual)))
    
    /// Asserts that the value is even.
    /// Usage: expectInt(4).toBeEven()
    member _.toBeEven() =
        if actual % 2 = 0 then ()
        else
            let msg = sprintf "Expected %d to be even" actual
            raise (AssertionException(msg, Some (box "even"), Some (box actual)))
    
    /// Asserts that the value is odd.
    /// Usage: expectInt(5).toBeOdd()
    member _.toBeOdd() =
        if actual % 2 <> 0 then ()
        else
            let msg = sprintf "Expected %d to be odd" actual
            raise (AssertionException(msg, Some (box "odd"), Some (box actual)))
    
    /// Asserts that the value is divisible by the expected divisor.
    /// Usage: expectInt(10).toBeDivisibleBy(5)
    member _.toBeDivisibleBy(divisor: int) =
        if actual % divisor = 0 then ()
        else
            let msg = sprintf "Expected %d to be divisible by %d" actual divisor
            raise (AssertionException(msg, Some (box divisor), Some (box actual)))

/// Wrapper for collection expectations.
type CollectionExpectation<'a when 'a : equality>(actual: 'a seq) =
    member _.Value = actual
    
    /// Asserts that the collection contains the expected item.
    /// Usage: expect(list).toContain(42)
    member _.toContain(expected: 'a) =
        if Seq.contains expected actual then ()
        else
            let items = actual |> Seq.truncate 10 |> Seq.toList
            let msg = sprintf "Expected collection to contain %A, but it did not. Collection: %A" expected items
            raise (AssertionException(msg, Some (box expected), Some (box items)))
    
    /// Asserts that the collection is empty.
    /// Usage: expect(list).toBeEmpty()
    member _.toBeEmpty() =
        if Seq.isEmpty actual then ()
        else
            let count = Seq.length actual
            let msg = sprintf "Expected empty collection, but found %d items" count
            raise (AssertionException(msg, Some (box 0), Some (box count)))
    
    /// Asserts that the collection has the expected length.
    /// Usage: expect(list).toHaveLength(3)
    member _.toHaveLength(expected: int) =
        let actualLength = Seq.length actual
        if actualLength = expected then ()
        else
            let msg = sprintf "Expected collection to have length %d, but found length %d" expected actualLength
            raise (AssertionException(msg, Some (box expected), Some (box actualLength)))
    
    /// Asserts that the collection has at least the expected count.
    /// Usage: expectSeq(list).toHaveCountAtLeast(2)
    member _.toHaveCountAtLeast(expected: int) =
        let actualLength = Seq.length actual
        if actualLength >= expected then ()
        else
            let msg = sprintf "Expected collection to have at least %d items, but found %d" expected actualLength
            raise (AssertionException(msg, Some (box expected), Some (box actualLength)))
    
    /// Asserts that the collection has at most the expected count.
    /// Usage: expectSeq(list).toHaveCountAtMost(5)
    member _.toHaveCountAtMost(expected: int) =
        let actualLength = Seq.length actual
        if actualLength <= expected then ()
        else
            let msg = sprintf "Expected collection to have at most %d items, but found %d" expected actualLength
            raise (AssertionException(msg, Some (box expected), Some (box actualLength)))
    
    /// Asserts that all items in the collection satisfy a predicate.
    /// Usage: expectSeq(list).toAllSatisfy((fun x -> x > 0), "be positive")
    member _.toAllSatisfy(predicate: 'a -> bool, description: string) =
        if Seq.forall predicate actual then ()
        else
            let msg = sprintf "Expected all items to %s" description
            raise (AssertionException(msg, Some (box description), None))
    
    /// Asserts that at least one item in the collection satisfies a predicate.
    /// Usage: expectSeq(list).toAnySatisfy((fun x -> x > 0), "be positive")
    member _.toAnySatisfy(predicate: 'a -> bool, description: string) =
        if Seq.exists predicate actual then ()
        else
            let msg = sprintf "Expected at least one item to %s" description
            raise (AssertionException(msg, Some (box description), None))
    
    /// Asserts that the collection contains all expected items.
    /// Usage: expectSeq(list).toContainAll([1; 2; 3])
    member _.toContainAll(expected: 'a seq) =
        let missing = expected |> Seq.filter (fun item -> not (Seq.contains item actual))
        if Seq.isEmpty missing then ()
        else
            let missingList = missing |> Seq.toList
            let msg = sprintf "Expected collection to contain all items, but missing: %A" missingList
            raise (AssertionException(msg, Some (box expected), Some (box missingList)))
    
    /// Asserts that two sequences are equal (same items in same order).
    /// Usage: expectSeq(list1).toEqualSeq(list2)
    member _.toEqualSeq(expected: 'a seq) =
        if Seq.toList actual = Seq.toList expected then ()
        else
            let msg = sprintf "Expected sequences to be equal, but they differ"
            raise (AssertionException(msg, Some (box expected), Some (box actual)))
    
    /// Asserts that the sequence starts with the expected items.
    /// Usage: expectSeq([1; 2; 3; 4]).toStartWithSeq([1; 2])
    member _.toStartWithSeq(expected: 'a seq) =
        let expectedList = Seq.toList expected
        let actualStart = actual |> Seq.truncate (List.length expectedList) |> Seq.toList
        if actualStart = expectedList then ()
        else
            let msg = sprintf "Expected sequence to start with %A, but got %A" expectedList actualStart
            raise (AssertionException(msg, Some (box expected), Some (box actualStart)))
    
    /// Asserts that the sequence ends with the expected items.
    /// Usage: expectSeq([1; 2; 3; 4]).toEndWithSeq([3; 4])
    member _.toEndWithSeq(expected: 'a seq) =
        let expectedList = Seq.toList expected
        let actualList = Seq.toList actual
        let actualEnd = actualList |> List.skip (List.length actualList - List.length expectedList)
        if actualEnd = expectedList then ()
        else
            let msg = sprintf "Expected sequence to end with %A, but got %A" expectedList actualEnd
            raise (AssertionException(msg, Some (box expected), Some (box actualEnd)))

/// Wrapper for string expectations.
type StringExpectation(actual: string) =
    member _.Value = actual
    
    /// Asserts that the string equals the expected string.
    member _.toEqual(expected: string) =
        if actual = expected then ()
        else
            let msg = sprintf "Expected %A, but found %A" expected actual
            raise (AssertionException(msg, Some (box expected), Some (box actual)))
    
    /// Asserts that the string starts with the expected substring.
    /// Usage: expect(str).toStartWith("hello")
    member _.toStartWith(expected: string) =
        if actual.StartsWith(expected) then ()
        else
            let msg = sprintf "Expected string to start with %A, but found %A" expected actual
            raise (AssertionException(msg, Some (box expected), Some (box actual)))
    
    /// Asserts that the string ends with the expected substring.
    /// Usage: expect(str).toEndWith("world")
    member _.toEndWith(expected: string) =
        if actual.EndsWith(expected) then ()
        else
            let msg = sprintf "Expected string to end with %A, but found %A" expected actual
            raise (AssertionException(msg, Some (box expected), Some (box actual)))
    
    /// Asserts that the string contains the expected substring.
    /// Usage: expect(str).toContain("hello")
    member _.toContain(expected: string) =
        if actual.Contains(expected) then ()
        else
            let msg = sprintf "Expected string to contain %A, but found %A" expected actual
            raise (AssertionException(msg, Some (box expected), Some (box actual)))
    
    /// Asserts that the string is empty.
    /// Usage: expect(str).toBeEmpty()
    member _.toBeEmpty() =
        if System.String.IsNullOrEmpty(actual) then ()
        else
            let msg = sprintf "Expected empty string, but found %A" actual
            raise (AssertionException(msg, Some (box ""), Some (box actual)))
    
    /// Asserts that the string matches a regex pattern.
    /// Usage: expectStr("hello123").toMatchRegex("^hello\\d+$")
    member _.toMatchRegex(pattern: string) =
        if System.Text.RegularExpressions.Regex.IsMatch(actual, pattern) then ()
        else
            let msg = sprintf "Expected string to match regex %A, but found %A" pattern actual
            raise (AssertionException(msg, Some (box pattern), Some (box actual)))
    
    /// Asserts that the string is null or empty.
    /// Usage: expectStr("").toBeNullOrEmpty()
    member _.toBeNullOrEmpty() =
        if System.String.IsNullOrEmpty(actual) then ()
        else
            let msg = sprintf "Expected null or empty string, but found %A" actual
            raise (AssertionException(msg, Some (box ""), Some (box actual)))
    
    /// Asserts that the string is null or whitespace.
    /// Usage: expectStr("   ").toBeNullOrWhitespace()
    member _.toBeNullOrWhitespace() =
        if System.String.IsNullOrWhiteSpace(actual) then ()
        else
            let msg = sprintf "Expected null or whitespace string, but found %A" actual
            raise (AssertionException(msg, Some (box ""), Some (box actual)))
    
    /// Asserts that the string has the expected length.
    /// Usage: expectStr("hello").toHaveLength(5)
    member _.toHaveLength(expected: int) =
        let actualLength = if actual = null then 0 else actual.Length
        if actualLength = expected then ()
        else
            let msg = sprintf "Expected string to have length %d, but found length %d" expected actualLength
            raise (AssertionException(msg, Some (box expected), Some (box actualLength)))
    
    /// Asserts that the string equals another string (case-insensitive).
    /// Usage: expectStr("Hello").toEqualIgnoreCase("hello")
    member _.toEqualIgnoreCase(expected: string) =
        if System.String.Equals(actual, expected, System.StringComparison.OrdinalIgnoreCase) then ()
        else
            let msg = sprintf "Expected string to equal %A (ignoring case), but found %A" expected actual
            raise (AssertionException(msg, Some (box expected), Some (box actual)))
    
    /// Asserts that the string contains only alphabetic characters.
    /// Usage: expectStr("hello").toBeAlphabetic()
    member _.toBeAlphabetic() =
        if actual |> Seq.forall System.Char.IsLetter then ()
        else
            let msg = sprintf "Expected string to be alphabetic, but found %A" actual
            raise (AssertionException(msg, Some (box "alphabetic"), Some (box actual)))
    
    /// Asserts that the string contains only numeric characters.
    /// Usage: expectStr("12345").toBeNumeric()
    member _.toBeNumeric() =
        if actual |> Seq.forall System.Char.IsDigit then ()
        else
            let msg = sprintf "Expected string to be numeric, but found %A" actual
            raise (AssertionException(msg, Some (box "numeric"), Some (box actual)))

/// Core assertion functions for the fluent API.
[<AutoOpen>]
module Assertions =

    /// Creates an expectation wrapper for fluent assertions.
    /// Usage: expect(actual).toEqual(expected)
    /// Reads naturally as: "expect actual to equal expected"
    let expect (actual: 'a when 'a : equality) : Expectation<'a> = Expectation(actual)
    
    /// Creates an expectation wrapper for Option values.
    /// Usage: expectOption(Some 42).toBeSome(42)
    let expectOption (actual: 'a option when 'a : equality) : OptionExpectation<'a> = OptionExpectation(actual)
    
    /// Creates an expectation wrapper for Result values.
    /// Usage: expectResult(Ok 42).toBeOk(42)
    let expectResult (actual: Result<'a, 'b> when 'a : equality and 'b : equality) : ResultExpectation<'a, 'b> = ResultExpectation(actual)
    
    /// Creates an expectation wrapper for boolean values.
    /// Usage: expectBool(true).toBeTrue()
    let expectBool (actual: bool) : BoolExpectation = BoolExpectation(actual)
    
    /// Creates an expectation wrapper for numeric values with comparison methods.
    /// Usage: expectNum(10).toBeGreaterThan(5)
    let expectNum (actual: 'a when 'a : comparison) : NumericExpectation<'a> = NumericExpectation(actual)
    
    /// Creates an expectation wrapper for collections.
    /// Usage: expectSeq([1; 2; 3]).toContain(2)
    let expectSeq (actual: 'a seq when 'a : equality) : CollectionExpectation<'a> = CollectionExpectation(actual)
    
    /// Creates an expectation wrapper for strings with string-specific methods.
    /// Usage: expectStr("hello").toStartWith("hel")
    let expectStr (actual: string) : StringExpectation = StringExpectation(actual)
    
    /// Creates an expectation wrapper for integers with integer-specific methods.
    /// Usage: expectInt(4).toBeEven()
    let expectInt (actual: int) : IntExpectation = IntExpectation(actual)
    
    /// Creates an expectation wrapper for floats with closeness comparison.
    /// Usage: expectFloat(10.1).toBeCloseTo(10.0, 0.2)
    let expectFloat (actual: float) : FloatExpectation = FloatExpectation(actual)
    
    /// Asserts that a function throws an exception of the expected type.
    /// Usage: expectThrows<ArgumentException>(fun () -> invalidArg "x" "bad")
    let expectThrows<'TException when 'TException :> exn> (f: unit -> unit) : unit =
        try
            f()
            let expectedType = typeof<'TException>
            raise (AssertionException(sprintf "Expected %s to be thrown, but no exception was thrown" expectedType.Name,
                                     Some (box expectedType.Name), None))
        with
        | :? 'TException -> () // Expected exception type was thrown
        | ex ->
            let expectedType = typeof<'TException>
            let actualType = ex.GetType()
            if actualType = expectedType then ()
            else
                raise (AssertionException(sprintf "Expected %s, but %s was thrown" expectedType.Name actualType.Name,
                                         Some (box expectedType.Name), Some (box actualType.Name)))
    
    /// Asserts that a function throws an exception with a specific message.
    /// Usage: expectThrowsWithMessage("error message", fun () -> failwith "error message")
    let expectThrowsWithMessage (expectedMessage: string) (f: unit -> unit) : unit =
        try
            f()
            raise (AssertionException(sprintf "Expected exception with message %A, but no exception was thrown" expectedMessage,
                                     Some (box expectedMessage), None))
        with
        | ex when ex.Message = expectedMessage -> ()
        | ex ->
            raise (AssertionException(sprintf "Expected exception with message %A, but got message %A" expectedMessage ex.Message,
                                     Some (box expectedMessage), Some (box ex.Message)))
    
    /// Asserts that a function throws an exception containing a substring in the message.
    /// Usage: expectThrowsContaining("error", fun () -> failwith "this is an error message")
    let expectThrowsContaining (substring: string) (f: unit -> unit) : unit =
        try
            f()
            raise (AssertionException(sprintf "Expected exception containing %A, but no exception was thrown" substring,
                                     Some (box substring), None))
        with
        | ex when ex.Message.Contains(substring) -> ()
        | ex ->
            raise (AssertionException(sprintf "Expected exception containing %A, but got message %A" substring ex.Message,
                                     Some (box substring), Some (box ex.Message)))
    
    /// Asserts that a function does not throw an exception.
    /// Usage: expectNotToThrow(fun () -> someFunction())
    let expectNotToThrow (f: unit -> unit) : unit =
        try
            f()
        with ex ->
            raise (AssertionException(sprintf "Expected no exception, but %s was thrown: %s" (ex.GetType().Name) ex.Message,
                                     None, Some (box ex)))

