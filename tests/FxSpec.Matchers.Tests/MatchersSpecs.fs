/// Comprehensive tests for all FxSpec matchers
module FxSpec.Matchers.Tests.MatchersSpecs

open System
open FxSpec.Core
open FxSpec.Matchers

/// Specs for CoreMatchers
[<Tests>]
let coreMatchersSpecs =
    spec {
        yield describe "CoreMatchers" [
            context "equal" [
                it "passes when values are equal" (fun () ->
                    expect 42 |> should (equal 42)
                )

                it "fails when values are not equal" (fun () ->
                    let test () = expect 42 |> should (equal 43)
                    expect test |> should raiseException<AssertionException>
                )

                it "works with strings" (fun () ->
                    expect "hello" |> should (equal "hello")
                )

                it "works with lists" (fun () ->
                    expect [1; 2; 3] |> should (equal [1; 2; 3])
                )
            ]

            context "beNil" [
                it "passes when value is null" (fun () ->
                    let nullValue: string = null
                    expect nullValue |> should beNil
                )

                it "fails when value is not null" (fun () ->
                    let test () = expect "not null" |> should beNil
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "notBeNil" [
                it "passes when value is not null" (fun () ->
                    expect "not null" |> should notBeNil
                )

                it "fails when value is null" (fun () ->
                    let test () =
                        let nullValue: string = null
                        expect nullValue |> should notBeNil
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "beSome" [
                it "passes when Option is Some with expected value" (fun () ->
                    expect (Some 42) |> should (beSome 42)
                )

                it "fails when Option is None" (fun () ->
                    let test () = expect None |> should (beSome 42)
                    expect test |> should raiseException<AssertionException>
                )

                it "fails when Option is Some with different value" (fun () ->
                    let test () = expect (Some 42) |> should (beSome 43)
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "beNone" [
                it "passes when Option is None" (fun () ->
                    expect None |> should beNone
                )

                it "fails when Option is Some" (fun () ->
                    let test () = expect (Some 42) |> should beNone
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "beOk" [
                it "passes when Result is Ok with expected value" (fun () ->
                    expect (Ok 42) |> should (beOk 42)
                )

                it "fails when Result is Error" (fun () ->
                    let test () = expect (Error "failed") |> should (beOk 42)
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "beError" [
                it "passes when Result is Error with expected value" (fun () ->
                    expect (Error "failed") |> should (beError "failed")
                )

                it "fails when Result is Ok" (fun () ->
                    let test () = expect (Ok 42) |> should (beError "failed")
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "beTrue" [
                it "passes when value is true" (fun () ->
                    expect true |> should beTrue
                )

                it "fails when value is false" (fun () ->
                    let test () = expect false |> should beTrue
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "beFalse" [
                it "passes when value is false" (fun () ->
                    expect false |> should beFalse
                )

                it "fails when value is true" (fun () ->
                    let test () = expect true |> should beFalse
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "satisfy" [
                it "passes when predicate is satisfied" (fun () ->
                    expect 10 |> should (satisfy (fun x -> x > 5) "be greater than 5")
                )

                it "fails when predicate is not satisfied" (fun () ->
                    let test () = expect 3 |> should (satisfy (fun x -> x > 5) "be greater than 5")
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "beSameAs" [
                it "passes when references are the same" (fun () ->
                    let obj = box 42
                    expect obj |> should (beSameAs obj)
                )

                it "fails when references are different" (fun () ->
                    let test () = expect (box 42) |> should (beSameAs (box 42))
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "beOfType" [
                it "passes when value is of expected type" (fun () ->
                    expect (box "hello") |> should beOfType<string>
                )

                it "fails when value is of different type" (fun () ->
                    let test () = expect (box 42) |> should beOfType<string>
                    expect test |> should raiseException<AssertionException>
                )
            ]
        ]
    }

/// Specs for CollectionMatchers
[<Tests>]
let collectionMatchersSpecs =
    spec {
        yield describe "CollectionMatchers" [
            context "contain" [
                it "passes when collection contains item" (fun () ->
                    expect [1; 2; 3] |> should (contain 2)
                )

                it "fails when collection does not contain item" (fun () ->
                    let test () = expect [1; 2; 3] |> should (contain 4)
                    expect test |> should raiseException<AssertionException>
                )

                it "works with sequences" (fun () ->
                    expect (seq { 1; 2; 3 }) |> should (contain 2)
                )
            ]

            context "beEmpty" [
                it "passes when collection is empty" (fun () ->
                    expect [] |> should beEmpty
                )

                it "fails when collection is not empty" (fun () ->
                    let test () = expect [1; 2; 3] |> should beEmpty
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "haveLength" [
                it "passes when collection has expected length" (fun () ->
                    expect [1; 2; 3] |> should (haveLength 3)
                )

                it "fails when collection has different length" (fun () ->
                    let test () = expect [1; 2; 3] |> should (haveLength 5)
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "haveCountAtLeast" [
                it "passes when collection has at least expected count" (fun () ->
                    expect [1; 2; 3] |> should (haveCountAtLeast 2)
                )

                it "passes when collection has exactly expected count" (fun () ->
                    expect [1; 2; 3] |> should (haveCountAtLeast 3)
                )

                it "fails when collection has fewer items" (fun () ->
                    let test () = expect [1; 2] |> should (haveCountAtLeast 3)
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "haveCountAtMost" [
                it "passes when collection has at most expected count" (fun () ->
                    expect [1; 2; 3] |> should (haveCountAtMost 5)
                )

                it "passes when collection has exactly expected count" (fun () ->
                    expect [1; 2; 3] |> should (haveCountAtMost 3)
                )

                it "fails when collection has more items" (fun () ->
                    let test () = expect [1; 2; 3; 4] |> should (haveCountAtMost 3)
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "allSatisfy" [
                it "passes when all items satisfy predicate" (fun () ->
                    expect [2; 4; 6] |> should (allSatisfy (fun x -> x % 2 = 0) "be even")
                )

                it "fails when some items don't satisfy predicate" (fun () ->
                    let test () = expect [2; 3; 4] |> should (allSatisfy (fun x -> x % 2 = 0) "be even")
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "anySatisfy" [
                it "passes when at least one item satisfies predicate" (fun () ->
                    expect [1; 2; 3] |> should (anySatisfy (fun x -> x % 2 = 0) "be even")
                )

                it "fails when no items satisfy predicate" (fun () ->
                    let test () = expect [1; 3; 5] |> should (anySatisfy (fun x -> x % 2 = 0) "be even")
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "containAll" [
                it "passes when collection contains all expected items" (fun () ->
                    expect [1; 2; 3; 4] |> should (containAll [2; 4])
                )

                it "fails when collection is missing some items" (fun () ->
                    let test () = expect [1; 2; 3] |> should (containAll [2; 4])
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "equalSeq" [
                it "passes when sequences are equal" (fun () ->
                    expect [1; 2; 3] |> should (equalSeq [1; 2; 3])
                )

                it "fails when sequences are different" (fun () ->
                    let test () = expect [1; 2; 3] |> should (equalSeq [1; 2; 4])
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "startWithSeq" [
                it "passes when sequence starts with expected items" (fun () ->
                    expect [1; 2; 3; 4] |> should (startWithSeq [1; 2])
                )

                it "fails when sequence doesn't start with expected items" (fun () ->
                    let test () = expect [1; 2; 3; 4] |> should (startWithSeq [2; 3])
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "endWithSeq" [
                it "passes when sequence ends with expected items" (fun () ->
                    expect [1; 2; 3; 4] |> should (endWithSeq [3; 4])
                )

                it "fails when sequence doesn't end with expected items" (fun () ->
                    let test () = expect [1; 2; 3; 4] |> should (endWithSeq [2; 3])
                    expect test |> should raiseException<AssertionException>
                )
            ]
        ]
    }


/// Specs for StringMatchers
[<Tests>]
let stringMatchersSpecs =
    spec {
        yield describe "StringMatchers" [
            context "startWith" [
                it "passes when string starts with prefix" (fun () ->
                    expect "hello world" |> should (startWith "hello")
                )

                it "fails when string doesn't start with prefix" (fun () ->
                    let test () = expect "hello world" |> should (startWith "world")
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "endWith" [
                it "passes when string ends with suffix" (fun () ->
                    expect "hello world" |> should (endWith "world")
                )

                it "fails when string doesn't end with suffix" (fun () ->
                    let test () = expect "hello world" |> should (endWith "hello")
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "containSubstring" [
                it "passes when string contains substring" (fun () ->
                    expect "hello world" |> should (containSubstring "lo wo")
                )

                it "fails when string doesn't contain substring" (fun () ->
                    let test () = expect "hello world" |> should (containSubstring "xyz")
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "matchRegex" [
                it "passes when string matches regex" (fun () ->
                    expect "hello123" |> should (matchRegex "^hello\\d+$")
                )

                it "fails when string doesn't match regex" (fun () ->
                    let test () = expect "hello" |> should (matchRegex "^\\d+$")
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "beEmptyString" [
                it "passes when string is empty" (fun () ->
                    expect "" |> should beEmptyString
                )

                it "fails when string is not empty" (fun () ->
                    let test () = expect "hello" |> should beEmptyString
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "beNullOrEmpty" [
                it "passes when string is null" (fun () ->
                    let nullStr: string = null
                    expect nullStr |> should beNullOrEmpty
                )

                it "passes when string is empty" (fun () ->
                    expect "" |> should beNullOrEmpty
                )

                it "fails when string has content" (fun () ->
                    let test () = expect "hello" |> should beNullOrEmpty
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "beNullOrWhitespace" [
                it "passes when string is whitespace" (fun () ->
                    expect "   " |> should beNullOrWhitespace
                )

                it "fails when string has non-whitespace content" (fun () ->
                    let test () = expect "hello" |> should beNullOrWhitespace
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "haveStringLength" [
                it "passes when string has expected length" (fun () ->
                    expect "hello" |> should (haveStringLength 5)
                )

                it "fails when string has different length" (fun () ->
                    let test () = expect "hello" |> should (haveStringLength 3)
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "equalIgnoreCase" [
                it "passes when strings are equal ignoring case" (fun () ->
                    expect "Hello" |> should (equalIgnoreCase "hello")
                )

                it "fails when strings are different" (fun () ->
                    let test () = expect "hello" |> should (equalIgnoreCase "world")
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "beAlphabetic" [
                it "passes when string is alphabetic" (fun () ->
                    expect "hello" |> should beAlphabetic
                )

                it "fails when string contains non-alphabetic characters" (fun () ->
                    let test () = expect "hello123" |> should beAlphabetic
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "beNumeric" [
                it "passes when string is numeric" (fun () ->
                    expect "12345" |> should beNumeric
                )

                it "fails when string contains non-numeric characters" (fun () ->
                    let test () = expect "123abc" |> should beNumeric
                    expect test |> should raiseException<AssertionException>
                )
            ]
        ]
    }


/// Specs for NumericMatchers
[<Tests>]
let numericMatchersSpecs =
    spec {
        yield describe "NumericMatchers" [
            context "beGreaterThan" [
                it "passes when value is greater" (fun () ->
                    expect 10 |> should (beGreaterThan 5)
                )

                it "fails when value is not greater" (fun () ->
                    let test () = expect 5 |> should (beGreaterThan 10)
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "beLessThan" [
                it "passes when value is less" (fun () ->
                    expect 5 |> should (beLessThan 10)
                )

                it "fails when value is not less" (fun () ->
                    let test () = expect 10 |> should (beLessThan 5)
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "beGreaterThanOrEqual" [
                it "passes when value is greater" (fun () ->
                    expect 10 |> should (beGreaterThanOrEqual 5)
                )

                it "passes when value is equal" (fun () ->
                    expect 5 |> should (beGreaterThanOrEqual 5)
                )

                it "fails when value is less" (fun () ->
                    let test () = expect 3 |> should (beGreaterThanOrEqual 5)
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "beLessThanOrEqual" [
                it "passes when value is less" (fun () ->
                    expect 5 |> should (beLessThanOrEqual 10)
                )

                it "passes when value is equal" (fun () ->
                    expect 5 |> should (beLessThanOrEqual 5)
                )

                it "fails when value is greater" (fun () ->
                    let test () = expect 10 |> should (beLessThanOrEqual 5)
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "beBetween" [
                it "passes when value is between min and max" (fun () ->
                    expect 5 |> should (beBetween 1 10)
                )

                it "fails when value is below min" (fun () ->
                    let test () = expect 0 |> should (beBetween 1 10)
                    expect test |> should raiseException<AssertionException>
                )

                it "fails when value is above max" (fun () ->
                    let test () = expect 15 |> should (beBetween 1 10)
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "beCloseTo" [
                it "passes when values are close" (fun () ->
                    expect 10.1 |> should (beCloseTo 10.0 0.2)
                )

                it "fails when values are not close" (fun () ->
                    let test () = expect 10.5 |> should (beCloseTo 10.0 0.2)
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "bePositive" [
                it "passes when value is positive" (fun () ->
                    expect 5 |> should bePositive
                )

                it "fails when value is zero" (fun () ->
                    let test () = expect 0 |> should bePositive
                    expect test |> should raiseException<AssertionException>
                )

                it "fails when value is negative" (fun () ->
                    let test () = expect -5 |> should bePositive
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "beNegative" [
                it "passes when value is negative" (fun () ->
                    expect -5 |> should beNegative
                )

                it "fails when value is positive" (fun () ->
                    let test () = expect 5 |> should beNegative
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "beZero" [
                it "passes when value is zero" (fun () ->
                    expect 0 |> should beZero
                )

                it "fails when value is not zero" (fun () ->
                    let test () = expect 5 |> should beZero
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "beEven" [
                it "passes when value is even" (fun () ->
                    expect 4 |> should beEven
                )

                it "fails when value is odd" (fun () ->
                    let test () = expect 5 |> should beEven
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "beOdd" [
                it "passes when value is odd" (fun () ->
                    expect 5 |> should beOdd
                )

                it "fails when value is even" (fun () ->
                    let test () = expect 4 |> should beOdd
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "beDivisibleBy" [
                it "passes when value is divisible" (fun () ->
                    expect 10 |> should (beDivisibleBy 5)
                )

                it "fails when value is not divisible" (fun () ->
                    let test () = expect 10 |> should (beDivisibleBy 3)
                    expect test |> should raiseException<AssertionException>
                )
            ]
        ]
    }


/// Specs for ExceptionMatchers
[<Tests>]
let exceptionMatchersSpecs =
    spec {
        yield describe "ExceptionMatchers" [
            context "raiseException" [
                it "passes when function raises expected exception type" (fun () ->
                    let test () = failwith "error"
                    expect test |> should raiseException<System.Exception>
                )

                it "fails when function doesn't raise exception" (fun () ->
                    let test () =
                        let noError () = ()
                        expect noError |> should raiseException<System.Exception>
                    expect test |> should raiseException<AssertionException>
                )

                it "works with specific exception types" (fun () ->
                    let test () = invalidArg "param" "message"
                    expect test |> should raiseException<System.ArgumentException>
                )
            ]

            context "raiseExceptionWithMessage" [
                it "passes when exception has expected message" (fun () ->
                    let test () = failwith "specific error"
                    expect test |> should (raiseExceptionWithMessage "specific error")
                )

                it "fails when exception has different message" (fun () ->
                    let test () =
                        let throwError () = failwith "wrong message"
                        expect throwError |> should (raiseExceptionWithMessage "expected message")
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "raiseExceptionContaining" [
                it "passes when exception message contains substring" (fun () ->
                    let test () = failwith "this is a specific error message"
                    expect test |> should (raiseExceptionContaining "specific error")
                )

                it "fails when exception message doesn't contain substring" (fun () ->
                    let test () =
                        let throwError () = failwith "error"
                        expect throwError |> should (raiseExceptionContaining "not found")
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "raiseExceptionMatching" [
                it "passes when exception matches predicate" (fun () ->
                    let test () = failwith "error 123"
                    expect test |> should (raiseExceptionMatching<System.Exception> (fun ex -> ex.Message.Contains("123")) "contains 123")
                )

                it "fails when exception doesn't match predicate" (fun () ->
                    let test () =
                        let throwError () = failwith "error"
                        expect throwError |> should (raiseExceptionMatching<System.Exception> (fun ex -> ex.Message.Contains("xyz")) "contains xyz")
                    expect test |> should raiseException<AssertionException>
                )
            ]

            context "notRaiseException" [
                it "passes when function doesn't raise exception" (fun () ->
                    let test () = ()
                    expect test |> should notRaiseException
                )

                it "fails when function raises exception" (fun () ->
                    let test () =
                        let throwError () = failwith "error"
                        expect throwError |> should notRaiseException
                    expect test |> should raiseException<AssertionException>
                )
            ]
        ]
    }

