# Core Expectations

Basic equality checks, booleans, Option, Result, and common assertions.

---

## Expectation Functions

FxSpec provides type-specific expectation functions that return fluent wrappers with relevant assertion methods.

### expect

**Type:** `'a -> Expectation<'a>`

Creates a generic expectation for any type.

**Usage:**

```fsharp
expect(actual).toEqual(expected)
expect(actual).notToEqual(unexpected)
```

**Example:**

```fsharp
expect(2 + 2).toEqual(4)
expect("hello").notToEqual("world")
expect([1; 2; 3]).toEqual([1; 2; 3])
```

**Available Methods:**
- `.toEqual(expected)` - Asserts equality using F#'s structural equality
- `.notToEqual(unexpected)` - Asserts inequality

---

### expectBool

**Type:** `bool -> BoolExpectation`

Creates an expectation for boolean values.

**Usage:**

```fsharp
expectBool(actual).toBeTrue()
expectBool(actual).toBeFalse()
```

**Examples:**

```fsharp
expectBool(true).toBeTrue()
expectBool(false).toBeFalse()
expectBool(10 > 5).toBeTrue()
expectBool("".Length = 0).toBeTrue()
```

**Available Methods:**
- `.toBeTrue()` - Asserts the value is true
- `.toBeFalse()` - Asserts the value is false

---

### expectOption

**Type:** `'a option -> OptionExpectation<'a>`

Creates an expectation for Option values.

**Usage:**

```fsharp
expectOption(actual).toBeSome(expected)
expectOption(actual).toBeNone()
```

**Examples:**

```fsharp
expectOption(Some 42).toBeSome(42)
expectOption(None).toBeNone()

let result = tryParse "123"
expectOption(result).toBeSome(123)

let notFound = Map.tryFind "key" emptyMap
expectOption(notFound).toBeNone()
```

**Available Methods:**
- `.toBeSome(expected)` - Asserts the Option is Some with the expected value
- `.toBeNone()` - Asserts the Option is None

---

### expectResult

**Type:** `Result<'a, 'b> -> ResultExpectation<'a, 'b>`

Creates an expectation for Result values.

**Usage:**

```fsharp
expectResult(actual).toBeOk(expected)
expectResult(actual).toBeError(expected)
```

**Examples:**

```fsharp
expectResult(Ok "success").toBeOk("success")
expectResult(Error "failed").toBeError("failed")

let parseResult = Int32.TryParse("42")
expectResult(parseResult).toBeOk(42)

let divisionResult = divide 10 0
expectResult(divisionResult).toBeError("Division by zero")
```

**Available Methods:**
- `.toBeOk(expected)` - Asserts the Result is Ok with the expected value
- `.toBeError(expected)` - Asserts the Result is Error with the expected error

---

## Equality Assertions

The `expect()` function provides basic equality checking:

**Examples:**

```fsharp
// Primitives
expect(42).toEqual(42)
expect("hello").toEqual("hello")
expect(true).toEqual(true)

// Collections (structural equality)
expect([1; 2; 3]).toEqual([1; 2; 3])
expect({| Name = "Alice"; Age = 30 |}).toEqual({| Name = "Alice"; Age = 30 |})

// Records and DUs
type Person = { Name: string; Age: int }
let person1 = { Name = "Alice"; Age = 30 }
let person2 = { Name = "Alice"; Age = 30 }
expect(person1).toEqual(person2)  // Passes

// Negative assertions
expect(5).notToEqual(10)
expect("hello").notToEqual("world")
```

**Notes:**

- Uses F# `=` operator (structural equality)
- Works with any type that supports equality
- Provides clear diff output on failure

let result = validateAge(-1)
expect result |> should (beError "Age must be positive")
```

**Failure Messages:**

```fsharp
// If actual is Ok
expect (Ok 42) |> should (beError "failed")
// => Expected Error "failed", but found Ok 42

// If actual is Error with different value
expect (Error "error1") |> should (beError "error2")
// => Expected Error "error2", but found Error "error1"
```

---

## Type Checking

### beOfType

**Type:** `Matcher<obj>`

Matches if the actual value is of the specified type.

**Usage:**

```fsharp
expect actual |> should (beOfType<TargetType>)
```

**Examples:**

```fsharp
expect (box "hello") |> should (beOfType<string>)
expect (box 42) |> should (beOfType<int>)
expect (box [1; 2; 3]) |> should (beOfType<int list>)

// With inheritance
type Animal = { Name: string }
type Dog = { Name: string; Breed: string }

let animal: obj = box { Name = "Buddy"; Breed = "Labrador" }
expect animal |> should (beOfType<Dog>)
```

**Failure Message:**

```fsharp
expect (box 42) |> should (beOfType<string>)
// => Expected type string, but found type Int32
```

---

## Reference Equality

### beSameAs

**Type:** `'a -> Matcher<'a>`

Matches if the actual value is the same reference as the expected value.

**Usage:**

```fsharp
expect actual |> should (beSameAs expected)
```

**Examples:**

```fsharp
let list1 = [1; 2; 3]
let list2 = list1
let list3 = [1; 2; 3]

expect list2 |> should (beSameAs list1)  // Passes (same reference)
expect list3 |> shouldNot (beSameAs list1)  // Passes (different reference, same value)

// Singletons
let singleton = SingletonService.Instance
expect (SingletonService.Instance) |> should (beSameAs singleton)
```

**Notes:**

- Uses `obj.ReferenceEquals`
- Different from `equal` which uses structural equality
- Useful for testing singletons, caching, or memoization

---

## Custom Predicates

### satisfy

**Type:** `(('a -> bool) -> string -> Matcher<'a>)`

Matches if the actual value satisfies the predicate.

**Parameters:**

- `predicate` - Function to test the value
- `description` - Human-readable description of the predicate

**Usage:**

```fsharp
expect actual |> should (satisfy predicate "description")
```

**Examples:**

```fsharp
// Simple predicate
expect 10 |> should (satisfy (fun x -> x > 5) "be greater than 5")

// Complex predicate
expect "hello" |> should (satisfy
    (fun s -> s.Length > 3 && s.StartsWith("h"))
    "be longer than 3 chars and start with 'h'"
)

// Domain validation
type Email = Email of string
let isValidEmail (Email email) =
    email.Contains("@") && email.Contains(".")

expect (Email "test@example.com") |> should (satisfy isValidEmail "be a valid email")
```

**Failure Message:**

```fsharp
expect 3 |> should (satisfy (fun x -> x > 5) "be greater than 5")
// => Expected value to satisfy 'be greater than 5', but 3 did not
```

**Notes:**

- Use for custom validation logic
- Description is shown in failure messages
- Consider creating a dedicated matcher for commonly used predicates

---

## Complete Examples

### Testing a User Registration Function

```fsharp
module UserRegistrationSpecs

open FxSpec.Core
open FxSpec.Matchers

type ValidationError =
    | EmailInvalid
    | PasswordTooShort
    | UsernameTaken

type User = { Id: int; Email: string; Username: string }

[<Tests>]
let userRegistrationSpecs =
    spec {
        describe "UserService.Register" [
            context "when valid data is provided" [
                it "returns Ok with new user" (fun () ->
                    let result = UserService.register("alice", "alice@example.com", "password123")

                    match result with
                    | Ok user ->
                        expect user.Username |> should (equal "alice")
                        expect user.Email |> should (equal "alice@example.com")
                        expect user.Id |> should (beGreaterThan 0)
                    | Error _ ->
                        failwith "Expected Ok but got Error"
                )
            ]

            context "when email is invalid" [
                it "returns Error EmailInvalid" (fun () ->
                    let result = UserService.register("alice", "not-an-email", "password123")
                    expect result |> should (beError EmailInvalid)
                )
            ]

            context "when password is too short" [
                it "returns Error PasswordTooShort" (fun () ->
                    let result = UserService.register("alice", "alice@example.com", "123")
                    expect result |> should (beError PasswordTooShort)
                )
            ]
        ]
    }
```

---

## See Also

- **[Collection Matchers](collections.md)** - Matchers for lists, arrays, and sequences
- **[String Matchers](strings.md)** - String-specific matchers
- **[Numeric Matchers](numeric.md)** - Numeric comparisons
- **[Exception Matchers](exceptions.md)** - Exception testing
- **[Quick Start](../../quick-start.md)** - Get started with FxSpec
