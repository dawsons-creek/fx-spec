module FX.Spec.Core.Tests.MatcherSafetySpecs

open System
open FX.Spec.Core
open FX.Spec.Matchers

[<Tests>]
let matcherSafetySpecs =
    describe
        "Matcher Safety Verification"
        [

          context
              "toEndWithSeq safety check"
              [ it "raises a clear assertion error when expected suffix is longer than actual sequence" (fun () ->
                    let actual = [ 1; 2 ]
                    let expected = [ 1; 2; 3 ]

                    // Verify it throws AssertionException, not ArgumentException or similar from internal logic
                    let ex =
                        try
                            expectSeq(actual).toEndWithSeq (expected)
                            failwith "Should have thrown AssertionException"
                        with
                        | :? AssertionException as e -> e
                        | ex -> failwithf "Expected AssertionException but got %s" (ex.GetType().Name)

                    // Verify message explains the length issue
                    expectStr(ex.Message).toContain ("sequence was shorter"))

                it "still passes when expected suffix matches and lengths are valid" (fun () ->
                    let actual = [ 1; 2; 3 ]
                    let expected = [ 2; 3 ]
                    expectSeq(actual).toEndWithSeq (expected)) ]

          context
              "toBeOfType null handling"
              [ it "handles null actual value gracefully with a clear message" (fun () ->
                    let actual: obj = null

                    // Verify it throws AssertionException, not NullReferenceException
                    let ex =
                        try
                            expect(actual).toBeOfType<string> ()
                            failwith "Should have thrown AssertionException"
                        with
                        | :? AssertionException as e -> e
                        | :? NullReferenceException -> failwith "Should not throw NullReferenceException"
                        | ex -> failwithf "Expected AssertionException but got %s" (ex.GetType().Name)

                    // Verify message mentions null
                    expectStr(ex.Message).toContain ("found null")
                    expectStr(ex.Message).toContain ("Expected type String"))

                it "still passes for correct type" (fun () ->
                    let actual: obj = "hello"
                    expect(actual).toBeOfType<string> ())

                it "fails correctly for wrong type (non-null)" (fun () ->
                    let actual: obj = 123

                    let ex =
                        try
                            expect(actual).toBeOfType<string> ()
                            failwith "Should have thrown AssertionException"
                        with
                        | :? AssertionException as e -> e
                        | ex -> failwithf "Expected AssertionException but got %s" (ex.GetType().Name)

                    expectStr(ex.Message).toContain ("found type Int32")) ] ]
