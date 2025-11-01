# Quick Start

Get your first FxSpec test running in 5 minutes.

---

## Prerequisites

- .NET 8.0 or .NET 9.0 SDK
- F# 8.0+ (included with .NET SDK)
- Basic F# knowledge

---

## Installation

### Step 1: Create a Test Project

Create a new F# console project for your tests:

```bash
dotnet new console -lang F# -n MyProject.Tests
cd MyProject.Tests
```

### Step 2: Add FxSpec Packages

Add the FxSpec packages to your project:

```bash
dotnet add package FxSpec.Core
dotnet add package FxSpec.Matchers
dotnet add package FxSpec.Runner
```

!!! info "Package Status"
    FxSpec packages are not yet published to NuGet. For now, you'll need to build from source or reference the local projects.

---

## Your First Test

### Step 1: Create a Test File

Create a new file called `MyFirstSpecs.fs` in your test project:

```fsharp
module MyFirstSpecs

open FxSpec.Core
open FxSpec.Matchers

[<Tests>]
let myFirstSpecs =
    describe "My First Test Suite" [
        it "passes!" (fun () ->
            expectBool(true).toBeTrue()
        )

        it "checks equality" (fun () ->
            let result = 2 + 2
            expect(result).toEqual(4)
        )

        it "works with strings" (fun () ->
            let greeting = "Hello, FxSpec!"
            expectStr(greeting).toStartWith("Hello")
        )
    ]
```

### Step 2: Update Your .fsproj

Make sure your test file is included in the project:

```xml
<ItemGroup>
  <Compile Include="MyFirstSpecs.fs" />
  <Compile Include="Program.fs" />
</ItemGroup>
```

### Step 3: Update Program.fs

Replace the contents of `Program.fs` with:

```fsharp
[<EntryPoint>]
let main args =
    // FxSpec will automatically discover tests
    0
```

### Step 4: Run Your Tests

Run the tests using the FxSpec runner:

```bash
dotnet build
dotnet run
```

You should see beautiful output like this:

```
My First Test Suite
  ✓ passes! (2ms)
  ✓ checks equality (1ms)
  ✓ works with strings (1ms)

┌─────┬────────┬────────┬─────────┬──────────┐
│ ... │ Passed │ Failed │ Skipped │ Duration │
│  3  │   3    │   0    │    0    │  0.01s   │
└─────┴────────┴────────┴─────────┴──────────┘
```

!!! success "Congratulations!"
    You've just written and run your first FxSpec tests!

---

## Understanding the Basics

Let's break down what you just wrote:

### The Test Structure

```fsharp
[<Tests>]
let myFirstSpecs =
    describe "My First Test Suite" [
        // Individual tests go here
    ]
```

FxSpec tests are simple values marked with the `[<Tests>]` attribute. The test discovery system finds these automatically.

### The `describe` Function

```fsharp
describe "My First Test Suite" [
    // Individual tests go here
]
```

`describe` groups related tests together. You can nest multiple `describe` blocks to create a hierarchy.

### The `it` Function

```fsharp
it "passes!" (fun () ->
    expectBool(true).toBeTrue()
)
```

`it` defines an individual test case:

- First parameter: Test description (string)
- Second parameter: Test function that makes assertions

### The Fluent Expectation API

```fsharp
expect(actual).toEqual(expected)
expectBool(value).toBeTrue()
expectSeq(list).toContain(item)
```

FxSpec provides type-specific expectation functions that return fluent wrappers:

- `expect(value)` - Generic expectations for any type
- `expectBool(value)` - Boolean assertions
- `expectNum(value)` - Numeric comparisons
- `expectSeq(value)` - Collection assertions
- `expectStr(value)` - String matching
- And more...

---

## Next Steps

Now that you have FxSpec running, explore more features:

### Organize with Context

Use `context` to add more structure:

```fsharp
describe "Calculator" [
    context "when adding positive numbers" [
        it "returns the sum" (fun () ->
            expect(2 + 3).toEqual(5)
        )
    ]

    context "when adding negative numbers" [
        it "handles negatives correctly" (fun () ->
            expect(-2 + -3).toEqual(-5)
        )
    ]
]
```

### Use Type-Specific Expectations

Explore the type-specific expectation functions:

```fsharp
// Collections
expectSeq([1; 2; 3]).toContain(2)
expectSeq([]).toBeEmpty()
expectSeq([1; 2; 3]).toHaveLength(3)

// Strings
expectStr("hello world").toEndWith("world")
expectStr("test@example.com").toMatchRegex(@"^\w+@\w+\.\w+$")

// Numeric
expectNum(10).toBeGreaterThan(5)
expectFloat(3.14159).toBeCloseTo(3.14, 0.01)

// Options
expectOption(Some 42).toBeSome(42)
expectOption(None).toBeNone()

// Results
expectResult(Ok "success").toBeOk("success")
expectResult(Error "failed").toBeError("failed")

// Exceptions
expectThrows<System.ArgumentException>(fun () -> 
    invalidArg "param" "message"
)
```

### Negative Assertions

Use `.notTo...` methods for negative assertions:

```fsharp
expect(5).notToEqual(10)
expectStr("hello").notToStartWith("bye")
expectSeq([1; 2; 3]).notToBeEmpty()
```

### Focus on Specific Tests

During development, focus on specific tests:

```fsharp
describe "My Suite" [
    fit "only run this test" (fun () ->  // (1)!
        expectBool(true).toBeTrue()
    )

    it "this test will be skipped" (fun () ->
        expectBool(false).toBeTrue()
    )
]
```

1. `fit` (focused it) runs only this test. Use `fdescribe` to focus an entire group.

### Skip Tests Temporarily

Mark tests as pending:

```fsharp
describe "My Suite" [
    it "working test" (fun () ->
        expectBool(true).toBeTrue()
    )

    xit "not ready yet" (fun () ->  // (1)!
        expectBool(false).toBeTrue()
    )

    pending "TODO: implement this test" (fun () ->  // (2)!
        ()
    )
]
```

1. `xit` (excluded it) skips this test
2. `pending` is an alias for `xit` that reads better for unfinished tests

### Setup and Teardown

Use hooks for test setup:

```fsharp
describe "Database Tests" [
    let mutable connection = null

    beforeEach (fun () ->
        connection <- Database.connect()
    )

    afterEach (fun () ->
        connection.Dispose()
    )

    it "queries the database" (fun () ->
        let result = connection.Query("SELECT 1")
        expectSeq(result).notToBeEmpty()
    )
]
```

---

## Testing Your Own Code

Here's a complete example testing a simple calculator:

```fsharp
// Calculator.fs
module Calculator

let add x y = x + y
let subtract x y = x - y
let multiply x y = x * y
let divide x y =
    if y = 0 then
        invalidArg (nameof y) "Cannot divide by zero"
    else
        x / y
```

```fsharp
// CalculatorSpecs.fs
module CalculatorSpecs

open FxSpec.Core
open FxSpec.Matchers

[<Tests>]
let calculatorSpecs =
    describe "Calculator" [
        describe "add" [
            it "adds positive numbers" (fun () ->
                expect(Calculator.add 2 3).toEqual(5)
            )

            it "adds negative numbers" (fun () ->
                expect(Calculator.add -1 -2).toEqual(-3)
            )

            it "adds mixed numbers" (fun () ->
                expect(Calculator.add 10 -5).toEqual(5)
            )
        ]

        describe "divide" [
            it "divides evenly" (fun () ->
                expect(Calculator.divide 10 2).toEqual(5)
            )

            it "raises exception on division by zero" (fun () ->
                expectThrows<System.ArgumentException>(fun () -> 
                    Calculator.divide 10 0 |> ignore
                )
            )
        ]
    ]
```

---

## Running Tests

### Basic Run

```bash
dotnet run
```

### Run Specific Tests

```bash
dotnet run -- --filter "Calculator"
```

### Choose Output Format

```bash
# Documentation format (default, colorful)
dotnet run -- --format documentation

# Simple format (plain text)
dotnet run -- --format simple
```

---

## Async Testing

FxSpec supports asynchronous tests using `itAsync`:

```fsharp
open System.Net.Http

[<Tests>]
let asyncSpecs =
    describe "Async Operations" [
        itAsync "fetches data from API" (async {
            use client = new HttpClient()
            let! response = client.GetAsync("https://api.github.com") |> Async.AwaitTask
            expectHttp(response).toHaveStatusOk()
        })
        
        itAsync "handles async computations" (async {
            let! result = async {
                return 42
            }
            expect(result).toEqual(42)
        })
    ]
```

**Key Points:**
- Use `itAsync` instead of `it` for async tests
- Wrap test in `async { }` computation expression
- Use `let!` to await async operations
- Use `Async.AwaitTask` to convert .NET Tasks to F# Async

For more details, see [DSL API Reference](reference/dsl-api.md#itasync).

---

## Result Testing

Test F# Result types with state-only or value-specific matchers:

```fsharp
[<Tests>]
let resultSpecs =
    describe "Result Matchers" [
        it "checks success state" (fun () ->
            let result = Ok 42
            expectResult(result).toBeOk()  // Just check it succeeded
        )
        
        it "checks specific success value" (fun () ->
            let result = Ok "success"
            expectResult(result).toBeOk("success")  // Check value too
        )
        
        it "checks error state" (fun () ->
            let result = Error "failed"
            expectResult(result).toBeError()  // Just check it failed
        )
    ]
```

For more details, see [Result Matchers](reference/matchers/result.md).

---

## HTTP Testing

Test HTTP responses with the fluent HTTP API:

```fsharp
open FxSpec.Http
open System.Net.Http

[<Tests>]
let httpSpecs =
    describe "HTTP API Tests" [
        itAsync "validates API response" (async {
            use client = new HttpClient()
            let! response = client.GetAsync("https://api.example.com/users") |> Async.AwaitTask
            
            expectHttp(response).toHaveStatusOk()
            expectHttp(response).toHaveContentType("application/json")
            expectHttp(response).toHaveBodyContaining("users")
        })
    ]
```

For more details, see [HTTP Testing](reference/http.md).

---

## What's Next?

You now have a solid foundation in FxSpec. Continue learning:

<div class="grid cards" markdown>

-   :fontawesome-solid-book:{ .lg } **[DSL API Reference](reference/dsl-api.md)**

    ---

    Complete reference for all DSL functions

-   :fontawesome-solid-check:{ .lg } **[Core Matchers](reference/matchers/core.md)**

    ---

    Learn about all available matchers

-   :fontawesome-solid-code:{ .lg } **[Contributing](community/contributing.md)**

    ---

    Help improve FxSpec

</div>

---

## Troubleshooting

### Tests Not Discovered

Make sure your test module has the `[<Tests>]` attribute:

```fsharp
[<Tests>]  // Don't forget this!
let mySpecs = describe "..." [...]
```

### Compilation Errors

Common issues:

1. **Missing opens**: Make sure you have both `open FxSpec.Core` and `open FxSpec.Matchers`
2. **Wrong expectation type**: Use the appropriate type-specific function (e.g., `expectSeq` for collections, `expectStr` for strings)
3. **Missing parentheses**: Remember to wrap your test in `fun () ->` for lazy evaluation
4. **Method not available**: IntelliSense will show you the available methods for each expectation type

### Need Help?

- [Open an issue on GitHub](https://github.com/fxspec/fx-spec/issues)
- Check existing issues for similar problems
- Read the [Contributing Guide](community/contributing.md) to submit bug reports
