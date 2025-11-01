# Numeric Matchers

Numeric comparisons and range checks for numbers.

---

## Comparisons

### beGreaterThan

**Type:** `'a -> Matcher<'a> when 'a : comparison`

Matches if the actual value is greater than the expected value.

**Usage:**

```fsharp
expectNum(actual).toBeGreaterThan(expected)
```

**Examples:**

```fsharp
expectNum(10).toBeGreaterThan(5)
expectNum(3.14).toBeGreaterThan(3.0)
expectNum(100L).toBeGreaterThan(50L)

// After calculations
let score = calculateScore()
expectNum(score).toBeGreaterThan(0)
```

**Failure Message:**

```fsharp
expectNum(5).toBeGreaterThan(10)
// => Expected value to be greater than 10, but found 5
```

---

### beGreaterThanOrEqual

**Type:** `'a -> Matcher<'a> when 'a : comparison`

Matches if the actual value is greater than or equal to the expected value.

**Usage:**

```fsharp
expectNum(actual).toBeGreaterThanOrEqual(expected)
```

**Examples:**

```fsharp
expectNum(10).toBeGreaterThanOrEqual(10)  // Equal case
expectNum(10).toBeGreaterThanOrEqual(5)   // Greater case
expectNum(5.0).toBeGreaterThanOrEqual(5.0)

// Minimum validation
let age = user.Age
expectNum(age).toBeGreaterThanOrEqual(18)
```

**Failure Message:**

```fsharp
expectNum(5).toBeGreaterThanOrEqual(10)
// => Expected value to be greater than or equal to 10, but found 5
```

---

### beLessThan

**Type:** `'a -> Matcher<'a> when 'a : comparison`

Matches if the actual value is less than the expected value.

**Usage:**

```fsharp
expectNum(actual).toBeLessThan(expected)
```

**Examples:**

```fsharp
expectNum(5).toBeLessThan(10)
expectNum(2.5).toBeLessThan(3.0)
expectNum(-10).toBeLessThan(0)

// Maximum validation
let count = list.Length
expectNum(count).toBeLessThan(100)
```

**Failure Message:**

```fsharp
expectNum(15).toBeLessThan(10)
// => Expected value to be less than 10, but found 15
```

---

### beLessThanOrEqual

**Type:** `'a -> Matcher<'a> when 'a : comparison`

Matches if the actual value is less than or equal to the expected value.

**Usage:**

```fsharp
expectNum(actual).toBeLessThanOrEqual(expected)
```

**Examples:**

```fsharp
expectNum(5).toBeLessThanOrEqual(5)   // Equal case
expectNum(5).toBeLessThanOrEqual(10)  // Less case
expectNum(3.14).toBeLessThanOrEqual(4.0)

// Maximum limit
let responseTime = measureResponseTime()
expectNum(responseTime).toBeLessThanOrEqual(1000)  // Max 1000ms
```

**Failure Message:**

```fsharp
expectNum(15).toBeLessThanOrEqual(10)
// => Expected value to be less than or equal to 10, but found 15
```

---

## Ranges

### beBetween

**Type:** `'a -> 'a -> Matcher<'a> when 'a : comparison`

Matches if the actual value is between min and max (inclusive).

**Parameters:**

- `min` - Minimum value (inclusive)
- `max` - Maximum value (inclusive)

**Usage:**

```fsharp
expectNum(actual).toBeBetween(min, max)
```

**Examples:**

```fsharp
expectNum(5).toBeBetween(1, 10)    // Within range
expectNum(1).toBeBetween(1, 10)    // Minimum (inclusive)
expectNum(10).toBeBetween(1, 10)   // Maximum (inclusive)

// Age validation
let age = user.Age
expectNum(age).toBeBetween(18, 65)

// Percentage
let score = getScore()
expectNum(score).toBeBetween(0.0, 100.0)
```

**Failure Message:**

```fsharp
expectNum(15).toBeBetween(1, 10)
// => Expected value to be between 1 and 10, but found 15
```

**Notes:**

- Both bounds are inclusive
- Validates that `min <= max` at creation time
- Works with any comparable type (int, float, DateTime, etc.)

---

## Floating Point

### beCloseTo

**Type:** `float -> float -> Matcher<float>`

Matches if the actual floating-point value is close to the expected value within a tolerance.

**Parameters:**

- `expected` - Expected value
- `tolerance` - Maximum allowed difference

**Usage:**

```fsharp
expectFloat(actual).toBeCloseTo(expected, tolerance)
```

**Examples:**

```fsharp
expectFloat(3.14159).toBeCloseTo(3.14, 0.01)     // Within tolerance
expectFloat(3.14159).toBeCloseTo(3.14159, 0.0)   // Exact match

// Calculation precision
let result = Math.PI * 2.0
expectFloat(result).toBeCloseTo(6.283, 0.001)

// Financial calculations
let total = calculateTotal()
expectFloat(total).toBeCloseTo(99.99, 0.01)
```

**Failure Message:**

```fsharp
expectFloat(3.5).toBeCloseTo(3.14, 0.1)
// => Expected value to be close to 3.14 (±0.1), but found 3.5 (diff: 0.36)
```

**Notes:**

- Tolerance must be non-negative
- Does not accept NaN or Infinity
- Use for comparing floating-point results where exact equality is unreliable
- Default to smallest reasonable tolerance

---

## Sign Checks

### bePositive

**Type:** `Matcher<'a> when 'a : comparison and 'a : (static member Zero : 'a)`

Matches if the actual value is positive (> 0).

**Usage:**

```fsharp
expectNum(actual).toBePositive()
```

**Examples:**

```fsharp
expectNum(5).toBePositive()
expectNum(0.1).toBePositive()
expectNum(100L).toBePositive()

// After calculations
let profit = revenue - expenses
expectNum(profit).toBePositive()
```

**Failure Message:**

```fsharp
expectNum(-5).toBePositive()
// => Expected positive value, but found -5

expectNum(0).toBePositive()
// => Expected positive value, but found 0
```

**Notes:**

- Zero is not positive (use `beGreaterThanOrEqual 0` to include zero)
- Works with any numeric type that has a `Zero` member

---

### beNegative

**Type:** `Matcher<'a> when 'a : comparison and 'a : (static member Zero : 'a)`

Matches if the actual value is negative (< 0).

**Usage:**

```fsharp
expectNum(actual).toBeNegative()
```

**Examples:**

```fsharp
expectNum(-5).toBeNegative()
expectNum(-0.1).toBeNegative()
expectNum(-100L).toBeNegative()

// Testing debt
let balance = account.Balance
if balance < 0 then
    expectNum(balance).toBeNegative()
```

**Failure Message:**

```fsharp
expectNum(5).toBeNegative()
// => Expected negative value, but found 5

expectNum(0).toBeNegative()
// => Expected negative value, but found 0
```

---

### beZero

**Type:** `Matcher<'a> when 'a : equality and 'a : (static member Zero : 'a)`

Matches if the actual value is zero.

**Usage:**

```fsharp
expectNum(actual).toBeZero()
```

**Examples:**

```fsharp
expectNum(0).toBeZero()
expectNum(0.0).toBeZero()
expectNum(0L).toBeZero()

// After operations
let remainder = 10 % 2
expectNum(remainder).toBeZero()
```

**Failure Message:**

```fsharp
expectNum(5).toBeZero()
// => Expected zero, but found 5
```

---

## Integer Properties

### beEven

**Type:** `Matcher<int>`

Matches if the actual integer is even.

**Usage:**

```fsharp
expect actual |> should beEven
```

**Examples:**

```fsharp
expect 2 |> should beEven
expect 0 |> should beEven
expect -4 |> should beEven

// Testing parity
let count = list.Length
if shouldBeEven(count) then
    expect count |> should beEven
```

**Failure Message:**

```fsharp
expect 3 |> should beEven
// => Expected even number, but found 3
```

---

### beOdd

**Type:** `Matcher<int>`

Matches if the actual integer is odd.

**Usage:**

```fsharp
expect actual |> should beOdd
```

**Examples:**

```fsharp
expect 1 |> should beOdd
expect 3 |> should beOdd
expect -5 |> should beOdd

// Testing alternating pattern
let index = getCurrentIndex()
if shouldBeOnOddIndex(index) then
    expect index |> should beOdd
```

**Failure Message:**

```fsharp
expect 4 |> should beOdd
// => Expected odd number, but found 4
```

---

### beDivisibleBy

**Type:** `int -> Matcher<int>`

Matches if the actual value is divisible by the expected divisor.

**Usage:**

```fsharp
expect actual |> should (beDivisibleBy divisor)
```

**Examples:**

```fsharp
expect 15 |> should (beDivisibleBy 5)
expect 12 |> should (beDivisibleBy 3)
expect 100 |> should (beDivisibleBy 10)

// Page size validation
let itemCount = items.Length
expect itemCount |> should (beDivisibleBy pageSize)

// Even numbers are divisible by 2
expect 8 |> should (beDivisibleBy 2)
```

**Failure Message:**

```fsharp
expect 7 |> should (beDivisibleBy 3)
// => Expected 7 to be divisible by 3, but it is not
```

**Notes:**

- Divisor cannot be zero (throws `ArgumentException`)
- Works with negative numbers
- `beDivisibleBy 2` is equivalent to `beEven`

---

## Complete Examples

### Testing a Math Library

```fsharp
module MathLibrarySpecs

open FxSpec.Core
open FxSpec.Matchers
open System

[<Tests>]
let mathLibrarySpecs =
    spec {
        describe "MathHelpers" [
            describe "average" [
                it "calculates average of numbers" (fun () ->
                    let result = MathHelpers.average [1.0; 2.0; 3.0; 4.0; 5.0]
                    expect result |> should (equal 3.0)
                    expectNum(result).toBeGreaterThan(0.0)
                )

                it "handles floating-point precision" (fun () ->
                    let result = MathHelpers.average [1.0/3.0; 2.0/3.0; 3.0/3.0]
                    expectFloat(result).toBeCloseTo(0.666, 0.001)
                )
            ]

            describe "factorial" [
                it "calculates factorial of positive number" (fun () ->
                    let result = MathHelpers.factorial 5
                    expect result |> should (equal 120)
                    expectNum(result).toBePositive()
                )

                it "returns 1 for zero" (fun () ->
                    let result = MathHelpers.factorial 0
                    expect result |> should (equal 1)
                )
            ]

            describe "isPrime" [
                it "identifies prime numbers" (fun () ->
                    expect (MathHelpers.isPrime 2) |> should beTrue
                    expect (MathHelpers.isPrime 7) |> should beTrue
                    expect (MathHelpers.isPrime 13) |> should beTrue
                )

                it "rejects composite numbers" (fun () ->
                    expect (MathHelpers.isPrime 4) |> should beFalse
                    expect (MathHelpers.isPrime 9) |> should beFalse
                )

                it "rejects numbers less than 2" (fun () ->
                    expect (MathHelpers.isPrime 1) |> should beFalse
                    expect (MathHelpers.isPrime 0) |> should beFalse
                    expect (MathHelpers.isPrime -5) |> should beFalse
                )
            ]

            describe "clamp" [
                it "keeps value within range" (fun () ->
                    let result = MathHelpers.clamp 5 1 10
                    expect result |> should (equal 5)
                    expectNum(result).toBeBetween(1, 10)
                )

                it "clamps to minimum" (fun () ->
                    let result = MathHelpers.clamp -5 1 10
                    expect result |> should (equal 1)
                    expectNum(result).toBeGreaterThanOrEqual(1)
                )

                it "clamps to maximum" (fun () ->
                    let result = MathHelpers.clamp 15 1 10
                    expect result |> should (equal 10)
                    expectNum(result).toBeLessThanOrEqual(10)
                )
            ]
        ]
    }
```

### Testing Financial Calculations

```fsharp
module FinancialSpecs

open FxSpec.Core
open FxSpec.Matchers

[<Tests>]
let financialSpecs =
    spec {
        describe "Financial Calculations" [
            describe "calculateInterest" [
                it "calculates simple interest" (fun () ->
                    let principal = 1000.0
                    let rate = 0.05  // 5%
                    let time = 2.0   // 2 years

                    let interest = Financial.calculateSimpleInterest principal rate time
                    expectFloat(interest).toBeCloseTo(100.0, 0.01)
                    expectNum(interest).toBePositive()
                )

                it "handles edge case of zero rate" (fun () ->
                    let interest = Financial.calculateSimpleInterest 1000.0 0.0 2.0
                    expectNum(interest).toBeZero()
                )
            ]

            describe "calculateTax" [
                it "calculates tax for income bracket" (fun () ->
                    let income = 50000.0
                    let tax = Financial.calculateTax income

                    expectNum(tax).toBePositive()
                    expectNum(tax).toBeLessThan(income)
                    expectFloat(tax).toBeCloseTo(7500.0, 100.0)  // Approximate
                )

                it "returns zero tax for income below threshold" (fun () ->
                    let income = 5000.0
                    let tax = Financial.calculateTax income
                    expectNum(tax).toBeZero()
                )
            ]
        ]
    }
```

---

## See Also

- **[Core Matchers](core.md)** - Basic equality matchers
- **[Collection Matchers](collections.md)** - Collection matchers
- **[String Matchers](strings.md)** - String matchers
- **[Quick Start](../../quick-start.md)** - Get started with FxSpec
