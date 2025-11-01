// Example demonstrating all FxSpec matchers
#r "../src/FxSpec.Matchers/bin/Debug/net9.0/FxSpec.Matchers.dll"

open FxSpec.Matchers

printfn "==================================="
printfn "FxSpec Matchers Examples"
printfn "==================================="
printfn ""

// Core Matchers
printfn "Core Matchers:"
expect 42 |> should (equal 42)
printfn "  ✓ equal"

expect None |> should beNone
printfn "  ✓ beNone"

expect (Some 42) |> should (beSome 42)
printfn "  ✓ beSome"

expect (Ok 42) |> should (beOk 42)
printfn "  ✓ beOk"

expect (Error "failed") |> should (beError "failed")
printfn "  ✓ beError"

expect true |> should beTrue
printfn "  ✓ beTrue"

expect false |> should beFalse
printfn "  ✓ beFalse"

expect 10 |> should (satisfy (fun x -> x > 5) "be greater than 5")
printfn "  ✓ satisfy"

printfn ""

// Collection Matchers
printfn "Collection Matchers:"
expect [1; 2; 3] |> should (contain 2)
printfn "  ✓ contain"

expect [] |> should beEmpty
printfn "  ✓ beEmpty"

expect [1; 2; 3] |> should (haveLength 3)
printfn "  ✓ haveLength"

expect [2; 4; 6] |> should (allSatisfy (fun x -> x % 2 = 0) "be even")
printfn "  ✓ allSatisfy"

expect [1; 2; 3] |> should (anySatisfy (fun x -> x > 2) "be greater than 2")
printfn "  ✓ anySatisfy"

expect [1; 2; 3; 4] |> should (containAll [2; 4])
printfn "  ✓ containAll"

expect [1; 2; 3] |> should (equalSeq [1; 2; 3])
printfn "  ✓ equalSeq"

printfn ""

// Numeric Matchers
printfn "Numeric Matchers:"
expect 10 |> should (beGreaterThan 5)
printfn "  ✓ beGreaterThan"

expect 5 |> should (beLessThan 10)
printfn "  ✓ beLessThan"

expect 5 |> should (beBetween 1 10)
printfn "  ✓ beBetween"

expect 3.14159 |> should (beCloseTo 3.14 0.01)
printfn "  ✓ beCloseTo"

expect 5 |> should bePositive
printfn "  ✓ bePositive"

expect -5 |> should beNegative
printfn "  ✓ beNegative"

expect 0 |> should beZero
printfn "  ✓ beZero"

expect 4 |> should beEven
printfn "  ✓ beEven"

expect 5 |> should beOdd
printfn "  ✓ beOdd"

expect 15 |> should (beDivisibleBy 5)
printfn "  ✓ beDivisibleBy"

printfn ""

// String Matchers
printfn "String Matchers:"
expect "hello world" |> should (startWith "hello")
printfn "  ✓ startWith"

expect "hello world" |> should (endWith "world")
printfn "  ✓ endWith"

expect "hello world" |> should (containSubstring "lo wo")
printfn "  ✓ containSubstring"

expect "hello123" |> should (matchRegex "hello\\d+")
printfn "  ✓ matchRegex"

expect "" |> should beEmptyString
printfn "  ✓ beEmptyString"

expect "hello" |> should (haveStringLength 5)
printfn "  ✓ haveStringLength"

expect "HELLO" |> should (equalIgnoreCase "hello")
printfn "  ✓ equalIgnoreCase"

expect "hello" |> should beAlphabetic
printfn "  ✓ beAlphabetic"

expect "12345" |> should beNumeric
printfn "  ✓ beNumeric"

printfn ""

// Exception Matchers
printfn "Exception Matchers:"
expect (fun () -> failwith "error") |> should raiseException<System.Exception>
printfn "  ✓ raiseException"

expect (fun () -> failwith "specific error") |> should (raiseExceptionWithMessage "specific error")
printfn "  ✓ raiseExceptionWithMessage"

expect (fun () -> failwith "error occurred") |> should (raiseExceptionContaining "error")
printfn "  ✓ raiseExceptionContaining"

expect (fun () -> ignore (1 + 1)) |> should notRaiseException
printfn "  ✓ notRaiseException"

printfn ""

// Negation
printfn "Negation:"
expect 42 |> shouldNot (equal 99)
printfn "  ✓ notTo' (negated assertion)"

printfn ""
printfn "==================================="
printfn "All matchers working! ✓"
printfn "==================================="

