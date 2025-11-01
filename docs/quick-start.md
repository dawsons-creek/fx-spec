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
    spec {
        describe "My First Test Suite" [
            it "passes!" (fun () ->
                expect true |> should beTrue
            )

            it "checks equality" (fun () ->
                let result = 2 + 2
                expect result |> should (equal 4)
            )

            it "works with strings" (fun () ->
                let greeting = "Hello, FxSpec!"
                expect greeting |> should (startWith "Hello")
            )
        ]
    }
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

### The `spec` Builder

```fsharp
spec {
    // Your test definitions go here
}
```

The `spec` computation expression is the container for all your tests. It builds an immutable test tree.

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
    expect true |> should beTrue
)
```

`it` defines an individual test case:

- First parameter: Test description (string)
- Second parameter: Test function that makes assertions

### The `expect` Function

```fsharp
expect actual |> should (equal expected)
```

`expect` starts an assertion:

- `actual` - The value you're testing
- `to'` - Applies a matcher (positive assertion)
- `equal expected` - The matcher function

---

## Next Steps

Now that you have FxSpec running, explore more features:

### Organize with Context

Use `context` to add more structure:

```fsharp
spec {
    describe "Calculator" [
        context "when adding positive numbers" [
            it "returns the sum" (fun () ->
                expect (2 + 3) |> should (equal 5)
            )
        ]

        context "when adding negative numbers" [
            it "handles negatives correctly" (fun () ->
                expect (-2 + -3) |> should (equal -5)
            )
        ]
    ]
}
```

### Use More Matchers

Explore the rich matcher library:

```fsharp
// Collections
expect [1; 2; 3] |> should (contain 2)
expect [] |> should beEmpty
expect [1; 2; 3] |> should (haveLength 3)

// Strings
expect "hello world" |> should (endWith "world")
expect "test@example.com" |> should (matchRegex @"^\w+@\w+\.\w+$")

// Numeric
expect 10 |> should (beGreaterThan 5)
expect 3.14159 |> should (beCloseTo 3.14 0.01)

// Options
expect (Some 42) |> should (beSome 42)
expect None |> should beNone

// Results
expect (Ok "success") |> should (beOk "success")
expect (Error "failed") |> should (beError "failed")
```

### Negative Assertions

Use `notTo'` for negative assertions:

```fsharp
expect 5 |> shouldNot (equal 10)
expect "hello" |> shouldNot (startWith "bye")
expect [1; 2; 3] |> shouldNot beEmpty
```

### Focus on Specific Tests

During development, focus on specific tests:

```fsharp
spec {
    describe "My Suite" [
        fit "only run this test" (fun () ->  // (1)!
            expect true |> should beTrue
        )

        it "this test will be skipped" (fun () ->
            expect false |> should beTrue
        )
    ]
}
```

1. `fit` (focused it) runs only this test. Use `fdescribe` to focus an entire group.

### Skip Tests Temporarily

Mark tests as pending:

```fsharp
spec {
    describe "My Suite" [
        it "working test" (fun () ->
            expect true |> should beTrue
        )

        xit "not ready yet" (fun () ->  // (1)!
            expect false |> should beTrue
        )

        pending "TODO: implement this test" (fun () ->  // (2)!
            ()
        )
    ]
}
```

1. `xit` (excluded it) skips this test
2. `pending` is an alias for `xit` that reads better for unfinished tests

### Setup and Teardown

Use hooks for test setup:

```fsharp
spec {
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
            expect result |> shouldNot beEmpty
        )
    ]
}
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
    spec {
        describe "Calculator" [
            describe "add" [
                it "adds positive numbers" (fun () ->
                    expect (Calculator.add 2 3) |> should (equal 5)
                )

                it "adds negative numbers" (fun () ->
                    expect (Calculator.add -1 -2) |> should (equal -3)
                )

                it "adds mixed numbers" (fun () ->
                    expect (Calculator.add 10 -5) |> should (equal 5)
                )
            ]

            describe "divide" [
                it "divides evenly" (fun () ->
                    expect (Calculator.divide 10 2) |> should (equal 5)
                )

                it "raises exception on division by zero" (fun () ->
                    let action () = Calculator.divide 10 0
                    expect action |> should raiseException
                )
            ]
        ]
    }
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
let mySpecs = spec { ... }
```

### Compilation Errors

Common issues:

1. **Missing opens**: Make sure you have both `open FxSpec.Core` and `open FxSpec.Matchers`
2. **Wrong matcher type**: Matchers are type-constrained. You can't use `beEmpty` on a number, for example.
3. **Missing parentheses**: Remember to wrap your test in `fun () ->` for lazy evaluation

### Need Help?

- [Open an issue on GitHub](https://github.com/fxspec/fx-spec/issues)
- Check existing issues for similar problems
- Read the [Contributing Guide](community/contributing.md) to submit bug reports
