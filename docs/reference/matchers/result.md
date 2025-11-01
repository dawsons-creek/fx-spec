# Result Matchers

FxSpec provides comprehensive matchers for testing F#'s `Result<'T, 'E>` type, which is the idiomatic way to handle errors in functional F# code.

## Overview

The `Result<'T, 'E>` type represents either success (`Ok 'T`) or failure (`Error 'E`). FxSpec's Result matchers allow you to assert on both the state and the value of Result types.

```fsharp
open FxSpec.Matchers

// State-only matchers
expectResult(result).toBeOk()     // Just check it succeeded
expectResult(result).toBeError()  // Just check it failed

// Value-specific matchers
expectResult(result).toBeOk(42)           // Check success with specific value
expectResult(result).toBeError("failed")  // Check error with specific message
```

## Result Matchers

### expectResult(result)

Creates a Result expectation wrapper that provides Result-specific assertion methods.

```fsharp
let result: Result<int, string> = Ok 42
expectResult(result).toBeOk(42)
```

## State-Only Matchers

These matchers only check whether the Result is `Ok` or `Error`, without caring about the specific value.

### toBeOk()

Asserts that the Result is `Ok` with any value.

**Use Cases:**
- Checking operation success without caring about the specific return value
- Validating that no errors occurred
- Testing scenarios where the exact value is unpredictable

```fsharp
describe "Authentication" [
    it "successfully authenticates valid user" (fun () ->
        let result = authenticate(validToken)
        expectResult(result).toBeOk()  // Don't care about the specific user data
    )
    
    it "grants access to authorized user" (fun () ->
        let result = authorizeUser(userId, permission)
        expectResult(result).toBeOk()  // Just check authorization succeeded
    )
]
```

### toBeError()

Asserts that the Result is `Error` with any error value.

**Use Cases:**
- Checking that validation failed without specifying the exact error
- Testing error handling paths
- Verifying that security checks rejected invalid input

```fsharp
describe "Validation" [
    it "rejects invalid email format" (fun () ->
        let result = validateEmail("not-an-email")
        expectResult(result).toBeError()  // Don't care about specific error message
    )
    
    it "fails when database is unavailable" (fun () ->
        let result = connectToDatabase(invalidConnectionString)
        expectResult(result).toBeError()  // Just verify it failed
    )
]
```

## Value-Specific Matchers

These matchers check both the Result state and the specific value contained within.

### toBeOk(expected: 'T)

Asserts that the Result is `Ok` with a specific value.

```fsharp
describe "Calculator" [
    it "adds numbers correctly" (fun () ->
        let result = calculate "2 + 2"
        expectResult(result).toBeOk(4)
    )
    
    it "returns correct user data" (fun () ->
        let result = findUser(123)
        expectResult(result).toBeOk({ Id = 123; Name = "John" })
    )
]
```

### toBeError(expected: 'E)

Asserts that the Result is `Error` with a specific error value.

```fsharp
describe "Error Handling" [
    it "returns specific validation error" (fun () ->
        let result = validateAge(-5)
        expectResult(result).toBeError("Age must be positive")
    )
    
    type ValidationError = { Field: string; Message: string }
    
    it "returns structured error" (fun () ->
        let result = validateUser(invalidUser)
        expectResult(result).toBeError({ 
            Field = "Email"
            Message = "Invalid email format" 
        })
    )
]
```

## Common Patterns

### Web API Error Handling

Result types are commonly used in web APIs for handling validation, authentication, and business logic errors:

```fsharp
describe "User Registration API" [
    it "succeeds with valid data" (fun () ->
        let result = registerUser(validUser)
        expectResult(result).toBeOk()  // Just verify registration succeeded
    )
    
    it "fails with duplicate email" (fun () ->
        let result = registerUser(existingEmailUser)
        expectResult(result).toBeError()  // Verify it fails
    )
    
    it "returns specific validation error for missing fields" (fun () ->
        let result = registerUser(incompleteUser)
        expectResult(result).toBeError("Email is required")
    )
]
```

### Database Operations

```fsharp
type DbError = NotFound | ConnectionError | Timeout

describe "Database Repository" [
    it "successfully retrieves existing record" (fun () ->
        let result = repository.Get(validId)
        expectResult(result).toBeOk()  // Don't need to verify entire record
    )
    
    it "returns NotFound for missing record" (fun () ->
        let result = repository.Get(missingId)
        expectResult(result).toBeError(NotFound)
    )
    
    it "handles connection errors" (fun () ->
        disconnectDatabase()
        let result = repository.Get(anyId)
        expectResult(result).toBeError(ConnectionError)
    )
]
```

### Authorization and Security

```fsharp
type AuthError = Unauthorized | Forbidden | TokenExpired

describe "Authorization Service" [
    it "grants access to authorized user" (fun () ->
        let result = authorize(validToken, resource)
        expectResult(result).toBeOk()  // Access granted
    )
    
    it "denies access to unauthorized user" (fun () ->
        let result = authorize(invalidToken, resource)
        expectResult(result).toBeError()  // Access denied
    )
    
    it "returns specific error for expired token" (fun () ->
        let result = authorize(expiredToken, resource)
        expectResult(result).toBeError(TokenExpired)
    )
]
```

### Async Result Patterns

Result types work seamlessly with async workflows:

```fsharp
describe "Async Operations" [
    itAsync "fetches data successfully" (async {
        let! result = fetchDataAsync(validId)
        expectResult(result).toBeOk()
    })
    
    itAsync "handles async errors" (async {
        let! result = fetchDataAsync(invalidId)
        expectResult(result).toBeError()
    })
    
    itAsync "validates async operation result" (async {
        let! result = processPaymentAsync(payment)
        expectResult(result).toBeOk({ 
            TransactionId = "txn_123"
            Status = "Completed" 
        })
    })
]
```

### Railway-Oriented Programming

Result types enable railway-oriented programming (ROP) patterns:

```fsharp
describe "Pipeline Processing" [
    it "succeeds when all steps pass" (fun () ->
        let result = 
            validateInput(data)
            |> Result.bind processData
            |> Result.bind saveToDatabase
            |> Result.map formatOutput
        
        expectResult(result).toBeOk()
    )
    
    it "fails at validation step" (fun () ->
        let result = 
            validateInput(invalidData)
            |> Result.bind processData  // Won't execute
            |> Result.bind saveToDatabase  // Won't execute
        
        expectResult(result).toBeError()
    )
]
```

## Discriminated Union Errors

F# discriminated unions make excellent error types:

```fsharp
type ValidationError =
    | EmptyField of field: string
    | InvalidFormat of field: string * message: string
    | OutOfRange of field: string * min: int * max: int

describe "Validation with DU Errors" [
    it "detects empty required field" (fun () ->
        let result = validateName("")
        expectResult(result).toBeError(EmptyField "Name")
    )
    
    it "detects invalid email format" (fun () ->
        let result = validateEmail("not-valid")
        expectResult(result).toBeError(
            InvalidFormat("Email", "Must contain @")
        )
    )
    
    it "detects out of range value" (fun () ->
        let result = validateAge(150)
        expectResult(result).toBeError(
            OutOfRange("Age", 0, 120)
        )
    )
]
```

## Error Messages

FxSpec provides clear error messages for Result assertions:

### toBeOk() Failure

```
Expected Ok, but found Error "something went wrong"
Expected: Ok
Actual: Error "something went wrong"
```

### toBeOk(value) Failure

```
Expected Ok 42, but found Ok 100
Expected: 42
Actual: 100
```

### toBeError() Failure

```
Expected Error, but found Ok 42
Expected: Error
Actual: Ok 42
```

### toBeError(value) Failure

```
Expected Error "Unauthorized", but found Error "Forbidden"
Expected: "Unauthorized"
Actual: "Forbidden"
```

## Best Practices

### 1. Use State-Only Matchers When Appropriate

When you don't need to verify the specific value, use state-only matchers:

```fsharp
// Good - just checking success
expectResult(result).toBeOk()

// Unnecessary - don't care about the specific value
expectResult(result).toBeOk(someValue)  // If you don't need to verify value
```

### 2. Use Value-Specific Matchers for Critical Values

When the exact value matters, verify it:

```fsharp
// Good - verifying specific error
expectResult(result).toBeError(NotFound)

// Risky - might miss wrong error type
expectResult(result).toBeError()
```

### 3. Combine with Other Matchers

Result matchers work well with other FxSpec features:

```fsharp
describe "Complex Validation" [
    it "validates and processes data" (fun () ->
        let result = validateAndProcess(data)
        
        // First check it succeeded
        expectResult(result).toBeOk()
        
        // Then verify the result value
        match result with
        | Ok value -> expectNum(value.Score).toBeGreaterThan(0)
        | Error _ -> failwith "Unexpected error"
    )
]
```

### 4. Test Both Success and Failure Paths

Always test both happy path and error scenarios:

```fsharp
describe "User Service" [
    context "with valid input" [
        it "creates user successfully" (fun () ->
            let result = createUser(validData)
            expectResult(result).toBeOk()
        )
    ]
    
    context "with invalid input" [
        it "rejects empty name" (fun () ->
            let result = createUser({ validData with Name = "" })
            expectResult(result).toBeError()
        )
        
        it "rejects invalid email" (fun () ->
            let result = createUser({ validData with Email = "invalid" })
            expectResult(result).toBeError()
        )
    ]
]
```

## Type Signatures

```fsharp
// Create Result expectation
expectResult : Result<'T, 'E> -> ResultExpectation<'T, 'E>
    when 'T : equality and 'E : equality

// State-only matchers
toBeOk : unit -> unit
toBeError : unit -> unit

// Value-specific matchers
toBeOk : expected:'T -> unit
toBeError : expected:'E -> unit
```

## Related Documentation

- [Core Matchers](core.md) - Basic equality and type matchers
- [Async Support](../dsl-api.md#async-tests) - Using `itAsync` with Result types
- [HTTP Testing](../http.md) - HTTP responses often use Result patterns
- [Quick Start](../../quick-start.md) - Getting started with FxSpec

## See Also

- [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/) - Scott Wlaschin's ROP pattern
- [F# Result Type](https://learn.microsoft.com/en-us/dotnet/fsharp/language-reference/results) - Official F# documentation
