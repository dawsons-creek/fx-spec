# FxSpec Quick Start Guide

## Installation (Future)

```bash
dotnet add package FxSpec
```

## Your First Spec

```fsharp
open FxSpec

[<Tests>]
let calculatorSpecs =
    spec {
        describe "Calculator" {
            describe "addition" {
                it "adds two positive numbers" {
                    expect (2 + 2) |> to' (equal 4)
                }
                
                it "adds negative numbers" {
                    expect (-1 + -1) |> to' (equal -2)
                }
            }
            
            describe "division" {
                it "divides evenly" {
                    expect (10 / 2) |> to' (equal 5)
                }
                
                it "raises exception for division by zero" {
                    expect (fun () -> 10 / 0 |> ignore)
                    |> to' raiseException<DivideByZeroException>
                }
            }
        }
    }
```

## Running Tests

```bash
dotnet fspec MyTests.dll
dotnet fspec MyTests.dll --filter "Calculator"
dotnet fspec MyTests.dll --format documentation
```

## Expected Output

```
Calculator
  addition
    ✓ adds two positive numbers
    ✓ adds negative numbers
  division
    ✓ divides evenly
    ✓ raises exception for division by zero

4 examples, 0 failures
```

## Using State Management

### Lazy Variables with `let'`

```fsharp
[<Tests>]
let userSpecs =
    spec {
        describe "User" {
            let' "user" (fun () -> 
                // Expensive operation, only runs once per test
                createUser "john@example.com"
            )
            
            it "has an email" {
                let user = get "user" :?> User
                expect user.Email |> to' (equal "john@example.com")
            }
            
            it "has a default role" {
                let user = get "user" :?> User
                expect user.Role |> to' (equal "Member")
            }
        }
    }
```

### Subject

```fsharp
[<Tests>]
let stackSpecs =
    spec {
        describe "Stack" {
            subject (fun () -> Stack<int>())
            
            it "starts empty" {
                let stack = getSubject() :?> Stack<int>
                expect stack.Count |> to' (equal 0)
            }
            
            context "when items are pushed" {
                before (fun () ->
                    let stack = getSubject() :?> Stack<int>
                    stack.Push(1)
                    stack.Push(2)
                )
                
                it "has the correct count" {
                    let stack = getSubject() :?> Stack<int>
                    expect stack.Count |> to' (equal 2)
                }
                
                it "pops in LIFO order" {
                    let stack = getSubject() :?> Stack<int>
                    expect (stack.Pop()) |> to' (equal 2)
                    expect (stack.Pop()) |> to' (equal 1)
                }
            }
        }
    }
```

## Hooks

```fsharp
[<Tests>]
let databaseSpecs =
    spec {
        describe "Database operations" {
            before (fun () ->
                printfn "Setting up database connection"
                setupDatabase()
            )
            
            after (fun () ->
                printfn "Tearing down database"
                teardownDatabase()
            )
            
            it "inserts a record" {
                // Database is set up before this runs
                insertRecord { Id = 1; Name = "Test" }
                expect (recordExists 1) |> to' (equal true)
                // Database is torn down after this runs
            }
        }
    }
```

## Core Matchers

### Equality
```fsharp
expect actual |> to' (equal expected)
expect actual |> notTo' (equal unexpected)
```

### Null/None Checking
```fsharp
expect value |> to' beNil
expect option |> to' beNone
expect option |> to' (beSome 42)
```

### Result Matching
```fsharp
expect result |> to' (beOk 42)
expect result |> to' (beError "failed")
```

### Collections
```fsharp
expect [1; 2; 3] |> to' (contain 2)
expect [] |> to' beEmpty
expect [1; 2; 3] |> to' (haveLength 3)
```

### Numeric Comparisons
```fsharp
expect 10 |> to' (beGreaterThan 5)
expect 3 |> to' (beLessThan 10)
expect 3.14159 |> to' (beCloseTo 3.14 0.01)
```

### Exceptions
```fsharp
expect (fun () -> failwith "error")
|> to' raiseException<Exception>

expect (fun () -> invalidArg "x" "bad")
|> to' raiseException<ArgumentException>
```

### String Matching
```fsharp
expect "hello world" |> to' (startWith "hello")
expect "hello world" |> to' (endWith "world")
expect "hello world" |> to' (matchRegex "h.*d")
```

## Custom Matchers

Creating custom matchers is simple—just write a function that returns `MatchResult`:

```fsharp
let beEven : Matcher<int> =
    fun actual ->
        if actual % 2 = 0 then
            Pass
        else
            Fail($"{actual} is not even", None, Some (box actual))

let beDivisibleBy (divisor: int) : Matcher<int> =
    fun actual ->
        if actual % divisor = 0 then
            Pass
        else
            Fail($"{actual} is not divisible by {divisor}",
                 Some (box divisor),
                 Some (box actual))

// Usage
expect 4 |> to' beEven
expect 15 |> to' (beDivisibleBy 5)
```

## Request Specs (API Testing)

```fsharp
open FxSpec.Extensions

[<Tests>]
let apiSpecs =
    requestSpec {
        describe "Users API" {
            it "returns all users" {
                get "/api/users"
                |> expect |> to' (haveStatusCode 200)
                |> expect |> to' (haveHeader "Content-Type" "application/json")
            }
            
            it "creates a new user" {
                post "/api/users"
                |> withJson {| Name = "John"; Email = "john@example.com" |}
                |> expect |> to' (haveStatusCode 201)
                |> expect |> to' (haveJsonBody {| Id = 1; Name = "John" |})
            }
        }
    }
```

## Best Practices

### 1. Descriptive Test Names
```fsharp
// Good
it "returns 404 when user is not found" { ... }

// Bad
it "test1" { ... }
```

### 2. One Assertion Per Test (Generally)
```fsharp
// Good
it "has correct name" {
    expect user.Name |> to' (equal "John")
}

it "has correct email" {
    expect user.Email |> to' (equal "john@example.com")
}

// Acceptable when testing related properties
it "has correct user details" {
    expect user.Name |> to' (equal "John")
    expect user.Email |> to' (equal "john@example.com")
}
```

### 3. Use Context for Different Scenarios
```fsharp
describe "User authentication" {
    context "with valid credentials" {
        it "logs in successfully" { ... }
    }
    
    context "with invalid credentials" {
        it "returns error" { ... }
    }
    
    context "when account is locked" {
        it "denies access" { ... }
    }
}
```

### 4. Keep Tests Independent
```fsharp
// Each test should be able to run in isolation
// Use before/after hooks for setup/teardown
// Don't rely on test execution order
```

## Next Steps

- Read the [Implementation Plan](IMPLEMENTATION_PLAN.md)
- Review [Technical Architecture](TECHNICAL_ARCHITECTURE.md)
- Check out [Examples](examples/)
- Contribute to the project!

