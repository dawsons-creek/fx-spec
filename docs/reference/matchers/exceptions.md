# Exception Matchers

Matchers for testing exception throwing behavior.

---

## Basic Exception Testing

### raiseException

**Type:** `Matcher<unit -> unit> when 'T :> exn`

Matches if the function raises an exception of the specified type.

**Usage:**

```fsharp
expectThrows<ExceptionType>(action)
```

**Examples:**

```fsharp
// Basic exception
expectThrows<System.Exception>(fun () -> failwith "error")

// Specific exception type
expectThrows<System.ArgumentException>(fun () -> invalidArg "param" "error")

// Division by zero
expectThrows<System.DivideByZeroException>(fun () -> 1 / 0 |> ignore)

// Custom exceptions
type ValidationException() = inherit exn()
expect (fun () -> raise (ValidationException())) |> should raiseException<ValidationException>
```

**Failure Messages:**

```fsharp
// No exception thrown
expectThrows<Exception>(fun () -> 1 + 1)
// => Expected an exception of type Exception to be thrown, but nothing was thrown

// Wrong exception type
expectThrows<InvalidOperationException>(fun () -> invalidArg "x" "error")
// => Expected an exception of type InvalidOperationException, but an exception of type ArgumentException was thrown: error
```

**Notes:**

- Use generic type parameter to specify expected exception type
- Catches exact type and derived types
- Use most specific exception type possible for clarity

---

## Message Matching

### raiseExceptionWithMessage

**Type:** `string -> Matcher<unit -> unit>`

Matches if the function raises an exception with the specified message (exact match).

**Usage:**

```fsharp
expectThrowsWithMessage(action, "expected message")
```

**Examples:**

```fsharp
expectThrowsWithMessage(fun () -> failwith "error", "error")
expectThrowsWithMessage(fun () -> invalidArg "x" "must be positive", "must be positive (Parameter 'x')")

// Custom error messages
let validateAge age =
    if age < 0 then
        invalidArg (nameof age) "Age cannot be negative"

expectThrowsWithMessage(fun () -> validateAge -5, "Age cannot be negative (Parameter 'age')")
```

**Failure Messages:**

```fsharp
// No exception
expectThrowsWithMessage(fun () -> 1 + 1, "error")
// => Expected an exception with message 'error' to be thrown, but nothing was thrown

// Different message
expectThrowsWithMessage(fun () -> failwith "wrong", "error")
// => Expected exception message 'error', but found 'wrong'
```

**Notes:**

- Message must match exactly (case-sensitive)
- For partial matching, use `raiseExceptionContaining`
- Be aware of .NET exception message formatting (e.g., ArgumentException adds parameter name)

---

### raiseExceptionContaining

**Type:** `string -> Matcher<unit -> unit>`

Matches if the function raises an exception containing the specified substring.

**Usage:**

```fsharp
expect action |> should (raiseExceptionContaining "substring")
```

**Examples:**

```fsharp
expect (fun () -> failwith "file not found") |> should (raiseExceptionContaining "not found")
expect (fun () -> failwith "error: invalid input") |> should (raiseExceptionContaining "error:")

// Partial message matching
let validateEmail email =
    if not (email.Contains("@")) then
        invalidOp "Email must contain @"

expect (fun () -> validateEmail "invalid") |> should (raiseExceptionContaining "@")
```

**Failure Messages:**

```fsharp
// No exception
expect (fun () -> 1 + 1) |> should (raiseExceptionContaining "error")
// => Expected an exception containing 'error' to be thrown, but nothing was thrown

// Substring not found
expect (fun () -> failwith "wrong") |> should (raiseExceptionContaining "error")
// => Expected exception message to contain 'error', but found 'wrong'
```

**Notes:**

- Case-sensitive substring matching
- More flexible than exact message matching
- Good for testing error messages that include dynamic content (IDs, timestamps, etc.)

---

## Advanced Exception Testing

### raiseExceptionMatching

**Type:** `('T -> bool) -> string -> Matcher<unit -> unit> when 'T :> exn`

Matches if the function raises an exception of the specified type that satisfies the predicate.

**Parameters:**

- `predicate` - Function to test the exception
- `description` - Human-readable description of the predicate

**Usage:**

```fsharp
expect action |> should (raiseExceptionMatching<ExceptionType> predicate "description")
```

**Examples:**

```fsharp
// Check exception property
type CustomException(code: int) =
    inherit exn()
    member _.ErrorCode = code

expect (fun () -> raise (CustomException(404))) |> to'
    (raiseExceptionMatching<CustomException>
        (fun ex -> ex.ErrorCode = 404)
        "have error code 404")

// ArgumentException with specific parameter name
expect (fun () -> invalidArg "userId" "error") |> to'
    (raiseExceptionMatching<ArgumentException>
        (fun ex -> ex.ParamName = "userId")
        "have parameter name 'userId'")

// Multiple conditions
expect (fun () -> raise (CustomException(500))) |> to'
    (raiseExceptionMatching<CustomException>
        (fun ex -> ex.ErrorCode >= 500 && ex.ErrorCode < 600)
        "have 5xx error code")
```

**Failure Messages:**

```fsharp
// Exception doesn't match predicate
expect (fun () -> raise (CustomException(200))) |> to'
    (raiseExceptionMatching<CustomException>
        (fun ex -> ex.ErrorCode = 404)
        "have error code 404")
// => Expected exception to match 'have error code 404', but it did not: [exception message]

// Wrong exception type
expect (fun () -> failwith "error") |> to'
    (raiseExceptionMatching<ArgumentException>
        (fun ex -> ex.ParamName = "x")
        "have parameter name 'x'")
// => Expected an exception of type ArgumentException, but an exception of type Exception was thrown
```

**Notes:**

- Combines type checking and predicate matching
- Description appears in error messages
- Use for complex exception validation
- Predicate receives strongly-typed exception

---

## Negative Testing

### notRaiseException

**Type:** `Matcher<unit -> unit>`

Matches if the function does not raise any exception.

**Usage:**

```fsharp
expect action |> should notRaiseException
```

**Examples:**

```fsharp
expect (fun () -> 1 + 1) |> should notRaiseException
expect (fun () -> printfn "hello") |> should notRaiseException

// Safe operations
let safeDivide x y =
    if y = 0 then 0
    else x / y

expect (fun () -> safeDivide 10 0 |> ignore) |> should notRaiseException
```

**Failure Message:**

```fsharp
expect (fun () -> failwith "error") |> should notRaiseException
// => Expected no exception to be thrown, but Exception was thrown: error

expect (fun () -> 1 / 0 |> ignore) |> should notRaiseException
// => Expected no exception to be thrown, but DivideByZeroException was thrown: Attempted to divide by zero.
```

**Notes:**

- Useful for testing error handling code
- Can also use `notTo' raiseException<exn>`
- Shows exception type and message in failure

---

## Complete Examples

### Testing Input Validation

```fsharp
module ValidationSpecs

open FX.Spec.Core
open FX.Spec.Matchers
open System

type ValidationError =
    | Required of field: string
    | InvalidFormat of field: string
    | OutOfRange of field: string * min: int * max: int

exception ValidationException of ValidationError

module Validator =
    let validateAge age =
        if age < 0 then
            invalidArg (nameof age) "Age cannot be negative"
        elif age > 150 then
            invalidArg (nameof age) "Age must be less than 150"
        else
            age

    let validateEmail email =
        if String.IsNullOrWhiteSpace(email) then
            raise (ValidationException(Required "email"))
        elif not (email.Contains("@")) then
            raise (ValidationException(InvalidFormat "email"))
        else
            email

[<Tests>]
let validationSpecs =
    spec {
        describe "Validator" [
            describe "validateAge" [
                context "when age is valid" [
                    it "returns age without exception" (fun () ->
                        expect (fun () -> Validator.validateAge 25 |> ignore) |> should notRaiseException
                    )
                ]

                context "when age is negative" [
                    it "raises ArgumentException" (fun () ->
                        expect (fun () -> Validator.validateAge -5 |> ignore) |> to'
                            raiseException<ArgumentException>
                    )

                    it "has descriptive error message" (fun () ->
                        expect (fun () -> Validator.validateAge -5 |> ignore) |> to'
                            (raiseExceptionContaining "cannot be negative")
                    )

                    it "includes parameter name" (fun () ->
                        expect (fun () -> Validator.validateAge -5 |> ignore) |> to'
                            (raiseExceptionMatching<ArgumentException>
                                (fun ex -> ex.ParamName = "age")
                                "have parameter name 'age'")
                    )
                ]

                context "when age is too large" [
                    it "raises ArgumentException with message" (fun () ->
                        expect (fun () -> Validator.validateAge 200 |> ignore) |> to'
                            (raiseExceptionContaining "less than 150")
                    )
                ]
            ]

            describe "validateEmail" [
                context "when email is null or empty" [
                    it "raises ValidationException with Required error" (fun () ->
                        expect (fun () -> Validator.validateEmail "" |> ignore) |> to'
                            (raiseExceptionMatching<ValidationException>
                                (fun ex ->
                                    match ex.Data0 with
                                    | Required field -> field = "email"
                                    | _ -> false)
                                "have Required error for email field")
                    )
                ]

                context "when email format is invalid" [
                    it "raises ValidationException with InvalidFormat error" (fun () ->
                        expect (fun () -> Validator.validateEmail "notanemail" |> ignore) |> to'
                            raiseException<ValidationException>
                    )
                ]

                context "when email is valid" [
                    it "does not raise exception" (fun () ->
                        expect (fun () -> Validator.validateEmail "test@example.com" |> ignore) |> to'
                            notRaiseException
                    )
                ]
            ]
        ]
    }
```

### Testing Error Handling

```fsharp
module ErrorHandlingSpecs

open FX.Spec.Core
open FX.Spec.Matchers
open System
open System.IO

module FileService =
    let readFile path =
        if not (File.Exists(path)) then
            raise (FileNotFoundException($"File not found: {path}"))
        File.ReadAllText(path)

    let safeReadFile path =
        try
            Ok (readFile path)
        with
        | :? FileNotFoundException as ex -> Error ex.Message
        | ex -> Error $"Unexpected error: {ex.Message}"

[<Tests>]
let errorHandlingSpecs =
    spec {
        describe "FileService" [
            describe "readFile" [
                it "raises FileNotFoundException for missing file" (fun () ->
                    expect (fun () -> FileService.readFile "nonexistent.txt" |> ignore) |> to'
                        raiseException<FileNotFoundException>
                )

                it "includes filename in exception message" (fun () ->
                    let path = "missing.txt"
                    expect (fun () -> FileService.readFile path |> ignore) |> to'
                        (raiseExceptionContaining path)
                )
            ]

            describe "safeReadFile" [
                it "returns Error for missing file" (fun () ->
                    let result = FileService.safeReadFile "nonexistent.txt"
                    expect result |> should (satisfy Result.isError "be Error")
                )

                it "does not raise exception" (fun () ->
                    expect (fun () -> FileService.safeReadFile "nonexistent.txt" |> ignore) |> to'
                        notRaiseException
                )
            ]
        ]
    }
```

---

## Best Practices

### Be Specific

```fsharp
// Good - specific exception type
expectThrows<ArgumentNullException>(action)

// Less good - generic exception type
expectThrows<Exception>(action)
```

### Test Exception Messages

```fsharp
// Good - validates error communication
expect action |> should (raiseExceptionContaining "must be positive")

// Less good - only checks exception type
expectThrows<ArgumentException>(action)
```

### Use Lambdas for Lazy Evaluation

```fsharp
// Good - lazy evaluation
expect (fun () -> riskyOperation()) |> should raiseException<Exception>

// Wrong - evaluates immediately, exception thrown before matcher
expect riskyOperation() |> should raiseException<Exception>  // Compilation error
```

### Prefer Result Types for Errors

```fsharp
// Consider using Result types instead of exceptions for expected errors
type ValidationResult = Result<ValidatedData, ValidationError>

let validate data : ValidationResult =
    if isValid data then
        Ok (ValidatedData data)
    else
        Error (ValidationError "Invalid data")

// Test with Result matchers
expect (validate invalidData) |> should (beError (ValidationError "Invalid data"))
```

---

## See Also

- **[Core Matchers](core.md)** - Basic matchers including Result matchers
- **[Quick Start](../../quick-start.md)** - Get started with FX.Spec
- **[DSL API](../dsl-api.md)** - Complete DSL reference
