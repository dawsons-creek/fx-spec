# Core Matchers

Basic equality, null checks, boolean, Option, Result, and type matchers.

---

## Assertions

### expect

**Type:** `'a -> 'a`

Starts an assertion chain. Pass the actual value to test.

**Usage:**

```fsharp
expect actual |> should matcher
expect actual |> shouldNot matcher
```

**Example:**

```fsharp
expect (2 + 2) |> should (equal 4)
expect "hello" |> should (startWith "hel")
expect [1; 2; 3] |> should (contain 2)
```

---

### to'

**Type:** `Matcher<'a> -> 'a -> unit`

Applies a matcher for positive assertion. Throws `AssertionException` if matcher fails.

**Usage:**

```fsharp
expect actual |> should matcher
```

**Example:**

```fsharp
expect 10 |> should (beGreaterThan 5)
expect "test" |> should (haveStringLength 4)
```

---

### notTo'

**Type:** `Matcher<'a> -> 'a -> unit`

Applies a matcher for negative assertion. Throws `AssertionException` if matcher passes.

**Usage:**

```fsharp
expect actual |> shouldNot matcher
```

**Example:**

```fsharp
expect 5 |> shouldNot (equal 10)
expect "hello" |> shouldNot beEmptyString
expect [] |> shouldNot (contain 42)
```

---

## Equality

### equal

**Type:** `'a -> Matcher<'a>`

Matches if the actual value equals the expected value using F#'s structural equality.

**Usage:**

```fsharp
expect actual |> should (equal expected)
```

**Examples:**

```fsharp
// Primitives
expect 42 |> should (equal 42)
expect "hello" |> should (equal "hello")
expect true |> should (equal true)

// Collections (structural equality)
expect [1; 2; 3] |> should (equal [1; 2; 3])
expect {| Name = "Alice"; Age = 30 |} |> should (equal {| Name = "Alice"; Age = 30 |})

// Records and DUs
type Person = { Name: string; Age: int }
let person1 = { Name = "Alice"; Age = 30 }
let person2 = { Name = "Alice"; Age = 30 }
expect person1 |> should (equal person2)  // Passes
```

**Notes:**

- Uses F# `=` operator (structural equality)
- Works with any type that supports equality
- For reference equality, use `beSameAs`

---

## Null Checks

### beNil

**Type:** `Matcher<'a when 'a : null>`

Matches if the actual value is `null`.

**Usage:**

```fsharp
expect actual |> should beNil
```

**Examples:**

```fsharp
expect null |> should beNil
expect (null: string) |> should beNil

let maybeNull: string = getSomeValue()
if isNull maybeNull then
    expect maybeNull |> should beNil
```

**Notes:**

- Type constraint: `'a : null` (reference types only)
- Cannot use with value types (int, bool, etc.)
- For strings, consider `beNullOrEmpty` or `beNullOrWhitespace`

---

### notBeNil

**Type:** `Matcher<'a when 'a : null>`

Matches if the actual value is not `null`.

**Usage:**

```fsharp
expect actual |> should notBeNil
```

**Examples:**

```fsharp
expect "hello" |> should notBeNil
expect [1; 2; 3] |> should notBeNil

let result = database.Query()
expect result |> should notBeNil
```

---

## Boolean

### beTrue

**Type:** `Matcher<bool>`

Matches if the actual value is `true`.

**Usage:**

```fsharp
expect actual |> should beTrue
```

**Examples:**

```fsharp
expect true |> should beTrue
expect (2 > 1) |> should beTrue
expect (list.Any()) |> should beTrue
```

---

### beFalse

**Type:** `Matcher<bool>`

Matches if the actual value is `false`.

**Usage:**

```fsharp
expect actual |> should beFalse
```

**Examples:**

```fsharp
expect false |> should beFalse
expect (1 > 2) |> should beFalse
expect (list.IsEmpty) |> should beFalse
```

---

## Option

### beSome

**Type:** `'a -> Matcher<'a option>`

Matches if the actual Option is `Some` with the expected value.

**Usage:**

```fsharp
expect actual |> should (beSome expected)
```

**Examples:**

```fsharp
expect (Some 42) |> should (beSome 42)
expect (Some "hello") |> should (beSome "hello")

let result = tryFindUser(userId)
expect result |> should (beSome expectedUser)
```

**Failure Messages:**

```fsharp
// If actual is None
expect None |> should (beSome 42)
// => Expected Some 42, but found None

// If actual is Some with different value
expect (Some 10) |> should (beSome 42)
// => Expected Some 42, but found Some 10
```

---

### beNone

**Type:** `Matcher<'a option>`

Matches if the actual Option is `None`.

**Usage:**

```fsharp
expect actual |> should beNone
```

**Examples:**

```fsharp
expect None |> should beNone

let result = tryParseInt("not a number")
expect result |> should beNone
```

**Failure Message:**

```fsharp
expect (Some 42) |> should beNone
// => Expected None, but found Some 42
```

---

## Result

### beOk

**Type:** `'a -> Matcher<Result<'a, 'b>>`

Matches if the actual Result is `Ok` with the expected value.

**Usage:**

```fsharp
expect actual |> should (beOk expected)
```

**Examples:**

```fsharp
expect (Ok 42) |> should (beOk 42)
expect (Ok "success") |> should (beOk "success")

let result = validateEmail("test@example.com")
expect result |> should (beOk validEmail)
```

**Failure Messages:**

```fsharp
// If actual is Error
expect (Error "failed") |> should (beOk 42)
// => Expected Ok 42, but found Error "failed"

// If actual is Ok with different value
expect (Ok 10) |> should (beOk 42)
// => Expected Ok 42, but found Ok 10
```

---

### beError

**Type:** `'b -> Matcher<Result<'a, 'b>>`

Matches if the actual Result is `Error` with the expected value.

**Usage:**

```fsharp
expect actual |> should (beError expected)
```

**Examples:**

```fsharp
expect (Error "failed") |> should (beError "failed")
expect (Error 404) |> should (beError 404)

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
