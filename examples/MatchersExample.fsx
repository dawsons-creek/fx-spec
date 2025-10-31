// Example demonstrating all FxSpec matchers
#r "../src/FxSpec.Matchers/bin/Debug/net9.0/FxSpec.Matchers.dll"

open FxSpec.Matchers

printfn "==================================="
printfn "FxSpec Matchers Examples"
printfn "==================================="
printfn ""

// Core Matchers
printfn "Core Matchers:"
expect 42 |> to' (equal 42)
printfn "  ✓ equal"

expect None |> to' beNone
printfn "  ✓ beNone"

expect (Some 42) |> to' (beSome 42)
printfn "  ✓ beSome"

expect (Ok 42) |> to' (beOk 42)
printfn "  ✓ beOk"

expect (Error "failed") |> to' (beError "failed")
printfn "  ✓ beError"

expect true |> to' beTrue
printfn "  ✓ beTrue"

expect false |> to' beFalse
printfn "  ✓ beFalse"

expect 10 |> to' (satisfy (fun x -> x > 5) "be greater than 5")
printfn "  ✓ satisfy"

printfn ""

// Collection Matchers
printfn "Collection Matchers:"
expect [1; 2; 3] |> to' (contain 2)
printfn "  ✓ contain"

expect [] |> to' beEmpty
printfn "  ✓ beEmpty"

expect [1; 2; 3] |> to' (haveLength 3)
printfn "  ✓ haveLength"

expect [2; 4; 6] |> to' (allSatisfy (fun x -> x % 2 = 0) "be even")
printfn "  ✓ allSatisfy"

expect [1; 2; 3] |> to' (anySatisfy (fun x -> x > 2) "be greater than 2")
printfn "  ✓ anySatisfy"

expect [1; 2; 3; 4] |> to' (containAll [2; 4])
printfn "  ✓ containAll"

expect [1; 2; 3] |> to' (equalSeq [1; 2; 3])
printfn "  ✓ equalSeq"

printfn ""

// Numeric Matchers
printfn "Numeric Matchers:"
expect 10 |> to' (beGreaterThan 5)
printfn "  ✓ beGreaterThan"

expect 5 |> to' (beLessThan 10)
printfn "  ✓ beLessThan"

expect 5 |> to' (beBetween 1 10)
printfn "  ✓ beBetween"

expect 3.14159 |> to' (beCloseTo 3.14 0.01)
printfn "  ✓ beCloseTo"

expect 5 |> to' bePositive
printfn "  ✓ bePositive"

expect -5 |> to' beNegative
printfn "  ✓ beNegative"

expect 0 |> to' beZero
printfn "  ✓ beZero"

expect 4 |> to' beEven
printfn "  ✓ beEven"

expect 5 |> to' beOdd
printfn "  ✓ beOdd"

expect 15 |> to' (beDivisibleBy 5)
printfn "  ✓ beDivisibleBy"

printfn ""

// String Matchers
printfn "String Matchers:"
expect "hello world" |> to' (startWith "hello")
printfn "  ✓ startWith"

expect "hello world" |> to' (endWith "world")
printfn "  ✓ endWith"

expect "hello world" |> to' (containSubstring "lo wo")
printfn "  ✓ containSubstring"

expect "hello123" |> to' (matchRegex "hello\\d+")
printfn "  ✓ matchRegex"

expect "" |> to' beEmptyString
printfn "  ✓ beEmptyString"

expect "hello" |> to' (haveStringLength 5)
printfn "  ✓ haveStringLength"

expect "HELLO" |> to' (equalIgnoreCase "hello")
printfn "  ✓ equalIgnoreCase"

expect "hello" |> to' beAlphabetic
printfn "  ✓ beAlphabetic"

expect "12345" |> to' beNumeric
printfn "  ✓ beNumeric"

printfn ""

// Exception Matchers
printfn "Exception Matchers:"
expect (fun () -> failwith "error") |> to' raiseException<System.Exception>
printfn "  ✓ raiseException"

expect (fun () -> failwith "specific error") |> to' (raiseExceptionWithMessage "specific error")
printfn "  ✓ raiseExceptionWithMessage"

expect (fun () -> failwith "error occurred") |> to' (raiseExceptionContaining "error")
printfn "  ✓ raiseExceptionContaining"

expect (fun () -> ignore (1 + 1)) |> to' notRaiseException
printfn "  ✓ notRaiseException"

printfn ""

// Negation
printfn "Negation:"
expect 42 |> notTo' (equal 99)
printfn "  ✓ notTo' (negated assertion)"

printfn ""
printfn "==================================="
printfn "All matchers working! ✓"
printfn "==================================="

