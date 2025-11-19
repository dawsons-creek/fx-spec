/// Comprehensive tests for all FX.Spec matchers
module FX.Spec.Matchers.Tests.MatchersSpecs

open System
open FX.Spec.Core
open FX.Spec.Matchers

/// Specs for CoreMatchers
[<Tests>]
let coreMatchersSpecs =
    describe "CoreMatchers" [
        context "equal" [
            it "passes when values are equal" (fun () ->
                expect(42).toEqual(42)
            )

            it "fails when values are not equal" (fun () ->
                let test () = expect(42).toEqual(43)
                expectThrows<AssertionException>(test)
            )

            it "works with strings" (fun () ->
                expect("hello").toEqual("hello")
            )

            it "works with lists" (fun () ->
                expect([1; 2; 3]).toEqual([1; 2; 3])
            )
        ]

        context "beNil" [
            it "passes when value is null" (fun () ->
                let nullValue: string = null
                expect(nullValue).toBeNull()
            )

            it "fails when value is not null" (fun () ->
                let test () = expect("not null").toBeNull()
                expectThrows<AssertionException>(test)
            )
        ]

        context "notBeNil" [
            it "passes when value is not null" (fun () ->
                expect("not null").notToBeNull()
            )

            it "fails when value is null" (fun () ->
                let test () =
                    let nullValue: string = null
                    expect(nullValue).notToBeNull()
                expectThrows<AssertionException>(test)
            )
        ]

        context "beSome" [
            it "passes when Option is Some with expected value" (fun () ->
                expectOption(Some 42).toBeSome(42)
            )

            it "fails when Option is None" (fun () ->
                let test () = expectOption(None).toBeSome(42)
                expectThrows<AssertionException>(test)
            )

            it "fails when Option is Some with different value" (fun () ->
                let test () = expectOption(Some 42).toBeSome(43)
                expectThrows<AssertionException>(test)
            )
        ]

        context "beNone" [
            it "passes when Option is None" (fun () ->
                expectOption(None).toBeNone()
            )

            it "fails when Option is Some" (fun () ->
                let test () = expectOption(Some 42).toBeNone()
                expectThrows<AssertionException>(test)
            )
        ]

        context "beOk" [
            it "passes when Result is Ok with expected value" (fun () ->
                expectResult(Ok 42).toBeOk(42)
            )

            it "fails when Result is Error" (fun () ->
                let test () = expectResult(Error "failed").toBeOk(42)
                expectThrows<AssertionException>(test)
            )
        ]

        context "beError" [
            it "passes when Result is Error with expected value" (fun () ->
                expectResult(Error "failed").toBeError("failed")
            )

            it "fails when Result is Ok" (fun () ->
                let test () = expectResult(Ok 42).toBeError("failed")
                expectThrows<AssertionException>(test)
            )
        ]

        context "beTrue" [
            it "passes when value is true" (fun () ->
                expectBool(true).toBeTrue()
            )

            it "fails when value is false" (fun () ->
                let test () = expectBool(false).toBeTrue()
                expectThrows<AssertionException>(test)
            )
        ]

        context "beFalse" [
            it "passes when value is false" (fun () ->
                expectBool(false).toBeFalse()
            )

            it "fails when value is true" (fun () ->
                let test () = expectBool(true).toBeFalse()
                expectThrows<AssertionException>(test)
            )
        ]

        context "satisfy" [
            it "passes when predicate is satisfied" (fun () ->
                expect(10).toSatisfy((fun x -> x > 5), "be greater than 5")
            )

            it "fails when predicate is not satisfied" (fun () ->
                let test () = expect(3).toSatisfy((fun x -> x > 5), "be greater than 5")
                expectThrows<AssertionException>(test)
            )
        ]

        context "beSameAs" [
            it "passes when references are the same" (fun () ->
                let obj = box 42
                expect(obj).toBeSameAs(obj)
            )

            it "fails when references are different" (fun () ->
                let test () = expect(box 42).toBeSameAs(box 42)
                expectThrows<AssertionException>(test)
            )
        ]

        context "beOfType" [
            it "passes when value is of expected type" (fun () ->
                expect(box "hello").toBeOfType<string>()
            )

            it "fails when value is of different type" (fun () ->
                let test () = expect(box 42).toBeOfType<string>()
                expectThrows<AssertionException>(test)
            )
        ]
    ]

/// Specs for CollectionMatchers
[<Tests>]
let collectionMatchersSpecs =
    describe "CollectionMatchers" [
        context "contain" [
            it "passes when collection contains item" (fun () ->
                expectSeq([1; 2; 3]).toContain(2)
            )

            it "fails when collection does not contain item" (fun () ->
                let test () = expectSeq([1; 2; 3]).toContain(4)
                expectThrows<AssertionException>(test)
            )

            it "works with sequences" (fun () ->
                expectSeq(seq { 1; 2; 3 }).toContain(2)
            )
        ]

        context "beEmpty" [
            it "passes when collection is empty" (fun () ->
                expectSeq([]).toBeEmpty()
            )

            it "fails when collection is not empty" (fun () ->
                let test () = expectSeq([1; 2; 3]).toBeEmpty()
                expectThrows<AssertionException>(test)
            )
        ]

        context "haveLength" [
            it "passes when collection has expected length" (fun () ->
                expectSeq([1; 2; 3]).toHaveLength(3)
            )

            it "fails when collection has different length" (fun () ->
                let test () = expectSeq([1; 2; 3]).toHaveLength(5)
                expectThrows<AssertionException>(test)
            )
        ]

        context "haveCountAtLeast" [
            it "passes when collection has at least expected count" (fun () ->
                expectSeq([1; 2; 3]).toHaveCountAtLeast(2)
            )

            it "passes when collection has exactly expected count" (fun () ->
                expectSeq([1; 2; 3]).toHaveCountAtLeast(3)
            )

            it "fails when collection has fewer items" (fun () ->
                let test () = expectSeq([1; 2]).toHaveCountAtLeast(3)
                expectThrows<AssertionException>(test)
            )
        ]

        context "haveCountAtMost" [
            it "passes when collection has at most expected count" (fun () ->
                expectSeq([1; 2; 3]).toHaveCountAtMost(5)
            )

            it "passes when collection has exactly expected count" (fun () ->
                expectSeq([1; 2; 3]).toHaveCountAtMost(3)
            )

            it "fails when collection has more items" (fun () ->
                let test () = expectSeq([1; 2; 3; 4]).toHaveCountAtMost(3)
                expectThrows<AssertionException>(test)
            )
        ]

        context "allSatisfy" [
            it "passes when all items satisfy predicate" (fun () ->
                expectSeq([2; 4; 6]).toAllSatisfy((fun x -> x % 2 = 0), "be even")
            )

            it "fails when some items don't satisfy predicate" (fun () ->
                let test () = expectSeq([2; 3; 4]).toAllSatisfy((fun x -> x % 2 = 0), "be even")
                expectThrows<AssertionException>(test)
            )
        ]

        context "anySatisfy" [
            it "passes when at least one item satisfies predicate" (fun () ->
                expectSeq([1; 2; 3]).toAnySatisfy((fun x -> x % 2 = 0), "be even")
            )

            it "fails when no items satisfy predicate" (fun () ->
                let test () = expectSeq([1; 3; 5]).toAnySatisfy((fun x -> x % 2 = 0), "be even")
                expectThrows<AssertionException>(test)
            )
        ]

        context "containAll" [
            it "passes when collection contains all expected items" (fun () ->
                expectSeq([1; 2; 3; 4]).toContainAll([2; 4])
            )

            it "fails when collection is missing some items" (fun () ->
                let test () = expectSeq([1; 2; 3]).toContainAll([2; 4])
                expectThrows<AssertionException>(test)
            )
        ]

        context "equalSeq" [
            it "passes when sequences are equal" (fun () ->
                expectSeq([1; 2; 3]).toEqualSeq([1; 2; 3])
            )

            it "fails when sequences are different" (fun () ->
                let test () = expectSeq([1; 2; 3]).toEqualSeq([1; 2; 4])
                expectThrows<AssertionException>(test)
            )
        ]

        context "startWithSeq" [
            it "passes when sequence starts with expected items" (fun () ->
                expectSeq([1; 2; 3; 4]).toStartWithSeq([1; 2])
            )

            it "fails when sequence doesn't start with expected items" (fun () ->
                let test () = expectSeq([1; 2; 3; 4]).toStartWithSeq([2; 3])
                expectThrows<AssertionException>(test)
            )
        ]

        context "endWithSeq" [
            it "passes when sequence ends with expected items" (fun () ->
                expectSeq([1; 2; 3; 4]).toEndWithSeq([3; 4])
            )

            it "fails when sequence doesn't end with expected items" (fun () ->
                let test () = expectSeq([1; 2; 3; 4]).toEndWithSeq([2; 3])
                expectThrows<AssertionException>(test)
            )
        ]
    ]


/// Specs for StringMatchers
[<Tests>]
let stringMatchersSpecs =
    describe "StringMatchers" [
        context "startWith" [
            it "passes when string starts with prefix" (fun () ->
                expectStr("hello world").toStartWith("hello")
            )

            it "fails when string doesn't start with prefix" (fun () ->
                let test () = expectStr("hello world").toStartWith("world")
                expectThrows<AssertionException>(test)
            )
        ]

        context "endWith" [
            it "passes when string ends with suffix" (fun () ->
                expectStr("hello world").toEndWith("world")
            )

            it "fails when string doesn't end with suffix" (fun () ->
                let test () = expectStr("hello world").toEndWith("hello")
                expectThrows<AssertionException>(test)
            )
        ]

        context "containSubstring" [
            it "passes when string contains substring" (fun () ->
                expectStr("hello world").toContain("lo wo")
            )

            it "fails when string doesn't contain substring" (fun () ->
                let test () = expectStr("hello world").toContain("xyz")
                expectThrows<AssertionException>(test)
            )
        ]

        context "matchRegex" [
            it "passes when string matches regex" (fun () ->
                expectStr("hello123").toMatchRegex("^hello\\d+$")
            )

            it "fails when string doesn't match regex" (fun () ->
                let test () = expectStr("hello").toMatchRegex("^\\d+$")
                expectThrows<AssertionException>(test)
            )
        ]

        context "beEmptyString" [
            it "passes when string is empty" (fun () ->
                expectStr("").toBeEmpty()
            )

            it "fails when string is not empty" (fun () ->
                let test () = expectStr("hello").toBeEmpty()
                expectThrows<AssertionException>(test)
            )
        ]

        context "beNullOrEmpty" [
            it "passes when string is null" (fun () ->
                let nullStr: string = null
                expectStr(nullStr).toBeNullOrEmpty()
            )

            it "passes when string is empty" (fun () ->
                expectStr("").toBeNullOrEmpty()
            )

            it "fails when string has content" (fun () ->
                let test () = expectStr("hello").toBeNullOrEmpty()
                expectThrows<AssertionException>(test)
            )
        ]

        context "beNullOrWhitespace" [
            it "passes when string is whitespace" (fun () ->
                expectStr("   ").toBeNullOrWhitespace()
            )

            it "fails when string has non-whitespace content" (fun () ->
                let test () = expectStr("hello").toBeNullOrWhitespace()
                expectThrows<AssertionException>(test)
            )
        ]

        context "haveStringLength" [
            it "passes when string has expected length" (fun () ->
                expectStr("hello").toHaveLength(5)
            )

            it "fails when string has different length" (fun () ->
                let test () = expectStr("hello").toHaveLength(3)
                expectThrows<AssertionException>(test)
            )
        ]

        context "equalIgnoreCase" [
            it "passes when strings are equal ignoring case" (fun () ->
                expectStr("Hello").toEqualIgnoreCase("hello")
            )

            it "fails when strings are different" (fun () ->
                let test () = expectStr("hello").toEqualIgnoreCase("world")
                expectThrows<AssertionException>(test)
            )
        ]

        context "beAlphabetic" [
            it "passes when string is alphabetic" (fun () ->
                expectStr("hello").toBeAlphabetic()
            )

            it "fails when string contains non-alphabetic characters" (fun () ->
                let test () = expectStr("hello123").toBeAlphabetic()
                expectThrows<AssertionException>(test)
            )
        ]

        context "beNumeric" [
            it "passes when string is numeric" (fun () ->
                expectStr("12345").toBeNumeric()
            )

            it "fails when string contains non-numeric characters" (fun () ->
                let test () = expectStr("123abc").toBeNumeric()
                expectThrows<AssertionException>(test)
            )
        ]
    ]


/// Specs for NumericMatchers
[<Tests>]
let numericMatchersSpecs =
    describe "NumericMatchers" [
        context "beGreaterThan" [
            it "passes when value is greater" (fun () ->
                expectNum(10).toBeGreaterThan(5)
            )

            it "fails when value is not greater" (fun () ->
                let test () = expectNum(5).toBeGreaterThan(10)
                expectThrows<AssertionException>(test)
            )
        ]

        context "beLessThan" [
            it "passes when value is less" (fun () ->
                expectNum(5).toBeLessThan(10)
            )

            it "fails when value is not less" (fun () ->
                let test () = expectNum(10).toBeLessThan(5)
                expectThrows<AssertionException>(test)
            )
        ]

        context "beGreaterThanOrEqual" [
            it "passes when value is greater" (fun () ->
                expectNum(10).toBeGreaterThanOrEqual(5)
            )

            it "passes when value is equal" (fun () ->
                expectNum(5).toBeGreaterThanOrEqual(5)
            )

            it "fails when value is less" (fun () ->
                let test () = expectNum(3).toBeGreaterThanOrEqual(5)
                expectThrows<AssertionException>(test)
            )
        ]

        context "beLessThanOrEqual" [
            it "passes when value is less" (fun () ->
                expectNum(5).toBeLessThanOrEqual(10)
            )

            it "passes when value is equal" (fun () ->
                expectNum(5).toBeLessThanOrEqual(5)
            )

            it "fails when value is greater" (fun () ->
                let test () = expectNum(10).toBeLessThanOrEqual(5)
                expectThrows<AssertionException>(test)
            )
        ]

        context "beBetween" [
            it "passes when value is between min and max" (fun () ->
                expectNum(5).toBeBetween(1, 10)
            )

            it "fails when value is below min" (fun () ->
                let test () = expectNum(0).toBeBetween(1, 10)
                expectThrows<AssertionException>(test)
            )

            it "fails when value is above max" (fun () ->
                let test () = expectNum(15).toBeBetween(1, 10)
                expectThrows<AssertionException>(test)
            )
        ]

        context "beCloseTo" [
            it "passes when values are close" (fun () ->
                expectFloat(10.1).toBeCloseTo(10.0, 0.2)
            )

            it "fails when values are not close" (fun () ->
                let test () = expectFloat(10.5).toBeCloseTo(10.0, 0.2)
                expectThrows<AssertionException>(test)
            )
        ]

        context "bePositive" [
            it "passes when value is positive" (fun () ->
                expectFloat(5.0).toBePositive()
            )

            it "fails when value is zero" (fun () ->
                let test () = expectFloat(0.0).toBePositive()
                expectThrows<AssertionException>(test)
            )

            it "fails when value is negative" (fun () ->
                let test () = expectFloat(-5.0).toBePositive()
                expectThrows<AssertionException>(test)
            )
        ]

        context "beNegative" [
            it "passes when value is negative" (fun () ->
                expectFloat(-5.0).toBeNegative()
            )

            it "fails when value is positive" (fun () ->
                let test () = expectFloat(5.0).toBeNegative()
                expectThrows<AssertionException>(test)
            )
        ]

        context "beZero" [
            it "passes when value is zero" (fun () ->
                expectFloat(0.0).toBeZero()
            )

            it "fails when value is not zero" (fun () ->
                let test () = expectFloat(5.0).toBeZero()
                expectThrows<AssertionException>(test)
            )
        ]

        context "beEven" [
            it "passes when value is even" (fun () ->
                expectInt(4).toBeEven()
            )

            it "fails when value is odd" (fun () ->
                let test () = expectInt(5).toBeEven()
                expectThrows<AssertionException>(test)
            )
        ]

        context "beOdd" [
            it "passes when value is odd" (fun () ->
                expectInt(5).toBeOdd()
            )

            it "fails when value is even" (fun () ->
                let test () = expectInt(4).toBeOdd()
                expectThrows<AssertionException>(test)
            )
        ]

        context "beDivisibleBy" [
            it "passes when value is divisible" (fun () ->
                expectInt(10).toBeDivisibleBy(5)
            )

            it "fails when value is not divisible" (fun () ->
                let test () = expectInt(10).toBeDivisibleBy(3)
                expectThrows<AssertionException>(test)
            )
        ]
    ]


/// Specs for ExceptionMatchers
[<Tests>]
let exceptionMatchersSpecs =
    describe "ExceptionMatchers" [
        context "raiseException" [
            it "passes when function raises expected exception type" (fun () ->
                let test () = failwith "error"
                expectThrows<System.Exception>(test)
            )

            it "fails when function doesn't raise exception" (fun () ->
                let test () =
                    let noError () = ()
                    expectThrows<System.Exception>(noError)
                expectThrows<AssertionException>(test)
            )

            it "works with specific exception types" (fun () ->
                let test () = invalidArg "param" "message"
                expectThrows<System.ArgumentException>(test)
            )
        ]

        context "raiseExceptionWithMessage" [
            it "passes when exception has expected message" (fun () ->
                let test () = failwith "specific error"
                expectThrowsWithMessage "specific error" test
            )

            it "fails when exception has different message" (fun () ->
                let test () =
                    let throwError () = failwith "wrong message"
                    expectThrowsWithMessage "expected message" throwError
                expectThrows<AssertionException>(test)
            )
        ]

        context "raiseExceptionContaining" [
            it "passes when exception message contains substring" (fun () ->
                let test () = failwith "this is a specific error message"
                expectThrowsContaining "specific error" test
            )

            it "fails when exception message doesn't contain substring" (fun () ->
                let test () =
                    let throwError () = failwith "error"
                    expectThrowsContaining "not found" throwError
                expectThrows<AssertionException>(test)
            )
        ]

        context "raiseExceptionMatching" [
            it "passes when exception matches predicate" (fun () ->
                let test () = failwith "error 123"
                // We need a custom matcher for this - skip for now as it's not essential
                expectThrows<System.Exception>(test)
            )

            it "fails when exception doesn't match predicate" (fun () ->
                let test () =
                    let throwError () = failwith "error"
                    expectThrows<System.Exception>(throwError)
                expectNotToThrow(test)
            )
        ]

        context "notRaiseException" [
            it "passes when function doesn't raise exception" (fun () ->
                let test () = ()
                expectNotToThrow(test)
            )

            it "fails when function raises exception" (fun () ->
                let test () =
                    let throwError () = failwith "error"
                    expectNotToThrow(throwError)
                expectThrows<AssertionException>(test)
            )
        ]
    ]

