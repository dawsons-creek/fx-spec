# FxSpec Quick Start Guide

> **📚 For the complete guide, see [docs/quick-start.md](docs/quick-start.md)**

## Installation

FxSpec is not yet published to NuGet. To use it, build from source:

```bash
git clone https://github.com/fxspec/fx-spec.git
cd fx-spec
dotnet build
```

## Your First Spec

```fsharp
module CalculatorSpecs

open FxSpec.Core
open FxSpec.Matchers

[<Tests>]
let calculatorSpecs =
    spec {
        yield describe "Calculator" [
            describe "addition" [
                it "adds two positive numbers" (fun () ->
                    expect (2 + 2) |> should (equal 4)
                )

                it "adds negative numbers" (fun () ->
                    expect (-1 + -1) |> should (equal -2)
                )
            ]

            describe "division" [
                it "divides evenly" (fun () ->
                    expect (10 / 2) |> should (equal 5)
                )

                it "raises exception for division by zero" (fun () ->
                    let action () = 10 / 0
                    expect action |> should raiseException
                )
            ]
        ]
    }
```

## Running Tests

Build your test project and run with the FxSpec runner:

```bash
# Build tests
dotnet build tests/MyProject.Tests/MyProject.Tests.fsproj

# Run tests
dotnet run --project src/FxSpec.Runner/FxSpec.Runner.fsproj -- \
  tests/MyProject.Tests/bin/Debug/net9.0/MyProject.Tests.dll

# Or use the convenience script
./run-tests.sh
./run-tests.sh --filter "Calculator"
./run-tests.sh --format simple
```

## Expected Output

```
Calculator
  addition
    ✓ adds two positive numbers (1ms)
    ✓ adds negative numbers (0ms)
  division
    ✓ divides evenly (0ms)
    ✓ raises exception for division by zero (2ms)

┌───────┬────────┬────────┬─────────┬──────────┐
│ Total │ Passed │ Failed │ Skipped │ Duration │
│   4   │   4    │   0    │    0    │  0.01s   │
└───────┴────────┴────────┴─────────┴──────────┘
```

## Using Hooks for Setup/Teardown

```fsharp
[<Tests>]
let databaseSpecs =
    spec {
        yield describe "Database Tests" [
            let mutable connection = null

            beforeEach (fun () ->
                connection <- Database.connect()
                connection.BeginTransaction()
            )

            afterEach (fun () ->
                connection.RollbackTransaction()
                connection.Dispose()
            )

            it "queries data" (fun () ->
                let result = connection.Query("SELECT 1")
                expect result |> shouldNot beEmpty
            )

            it "inserts data" (fun () ->
                connection.Execute("INSERT INTO users VALUES (1, 'test')")
                let count = connection.QuerySingle<int>("SELECT COUNT(*) FROM users")
                expect count |> should (equal 1)
            )
        ]
    }
```

## Common Matchers

```fsharp
// Equality
expect 42 |> should (equal 42)
expect "hello" |> should (equal "hello")

// Collections
expect [1; 2; 3] |> should (contain 2)
expect [] |> should beEmpty
expect [1; 2; 3] |> should (haveLength 3)

// Strings
expect "hello world" |> should (startWith "hello")
expect "test.txt" |> should (endWith ".txt")
expect "test@example.com" |> should (matchRegex @"^\w+@\w+\.\w+$")

// Numeric
expect 10 |> should (beGreaterThan 5)
expect 3.14159 |> should (beCloseTo 3.14 0.01)

// Options
expect (Some 42) |> should (beSome 42)
expect None |> should beNone

// Booleans
expect true |> should beTrue
expect false |> should beFalse

// Exceptions
let action () = failwith "error"
expect action |> should raiseException
```

## Focused and Pending Tests

```fsharp
spec {
    yield describe "My Suite" [
        // Focus on specific test during development
        fit "only run this test" (fun () ->
            expect true |> should beTrue
        )

        // This test will be skipped
        it "this is skipped" (fun () ->
            expect false |> should beTrue
        )

        // Mark test as pending
        xit "not ready yet" (fun () ->
            expect false |> should beTrue
        )
    ]
}
```

## Next Steps

For complete documentation, see:

- **[Full Quick Start Guide](docs/quick-start.md)** - Comprehensive getting started guide
- **[DSL API Reference](docs/reference/dsl-api.md)** - All DSL functions
- **[Matchers Reference](docs/reference/matchers/core.md)** - All available matchers
- **[Contributing Guide](docs/community/contributing.md)** - How to contribute

## Key Differences from xUnit/NUnit

| Feature | xUnit/NUnit | FxSpec |
|---------|-------------|--------|
| Test organization | Attributes (`[<Fact>]`) | DSL (`describe`, `it`) |
| Assertions | `Assert.Equal(expected, actual)` | `expect actual \|> should (equal expected)` |
| Setup/Teardown | `[<SetUp>]`, `[<TearDown>]` | `beforeEach`, `afterEach` |
| Test discovery | Attributes | `[<Tests>]` attribute on spec |
| Hierarchical tests | Classes and methods | Nested `describe` blocks |
| Focused tests | Not built-in | `fit`, `fdescribe` |
| Pending tests | `[<Ignore>]` | `xit`, `pending` |

## Tips

1. **Always use `yield`** for top-level nodes in `spec { }`
2. **Wrap test code** in `(fun () -> ...)` for lazy evaluation
3. **Use `describe`** for grouping by feature/class
4. **Use `context`** for grouping by state/condition
5. **Use `beforeEach`** for test isolation
6. **Use `beforeAll`** for expensive setup
7. **Remove `fit`/`fdescribe`** before committing

---

**Happy Testing with FxSpec!** 🚀
