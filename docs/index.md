# FxSpec

<div class="grid cards" markdown>

-   :material-rocket-launch:{ .lg .middle } **Pure F# BDD Testing Framework**

    ---

    Write beautiful, expressive tests with F#'s type safety and functional programming.
    Inspired by RSpec, built for F#.

</div>

[Get Started in 5 Minutes](quick-start.md){ .md-button .md-button--primary }
[View on GitHub](https://github.com/fxspec/fx-spec){ .md-button }

---

## Why FxSpec?

<div class="grid cards" markdown>

-   :material-lambda:{ .lg .middle } **Pure F#**

    ---

    No dependencies on xUnit, NUnit, or MSTest. 100% idiomatic F# with computation expressions.

    Built by F# developers, for F# developers.

-   :material-shield-check:{ .lg .middle } **Type-Safe**

    ---

    Leverage F#'s type system to catch errors at compile time. Matchers are type-constrained.

    Impossible to compare incompatible types.

-   :material-test-tube:{ .lg .middle } **Self-Hosting**

    ---

    FxSpec tests itself using its own framework. Dogfooding ensures quality.

    If it's good enough for FxSpec, it's good enough for your project.

-   :material-language-ruby:{ .lg .middle } **RSpec-Inspired**

    ---

    Familiar BDD syntax: `describe`, `it`, `expect`, and rich matchers.

    Coming from RSpec, Jest, or pytest? You'll feel at home.

</div>

---

## Quick Example

```fsharp
module CalculatorSpecs

open FxSpec.Core
open FxSpec.Matchers

[<Tests>]
let calculatorSpecs =
    spec {
        describe "Calculator" [
            context "when adding numbers" [
                it "returns the sum" (fun () ->
                    let result = Calculator.add 2 3
                    expect result |> should (equal 5)
                )

                it "handles negative numbers" (fun () ->
                    let result = Calculator.add -1 -2
                    expect result |> should (equal -3)
                )
            ]

            context "when dividing numbers" [
                it "returns the quotient" (fun () ->
                    let result = Calculator.divide 10 2
                    expect result |> should (equal 5)
                )

                it "handles division by zero" (fun () ->
                    let action () = Calculator.divide 10 0
                    expect action |> should raiseException
                )
            ]
        ]
    }
```

---

## Features

### :material-check-circle:{ .lg } Expressive DSL

Write tests that read like documentation. The `spec` computation expression creates an immutable test tree that's easy to understand and maintain.

### :material-check-circle:{ .lg } Rich Matchers

Comprehensive matcher library for all common assertions:

- **Core matchers**: `equal`, `beNil`, `beSome`, `beTrue`
- **Collections**: `contain`, `beEmpty`, `haveLength`, `allSatisfy`
- **Strings**: `startWith`, `endWith`, `matchRegex`
- **Numeric**: `beGreaterThan`, `beLessThan`, `beCloseTo`
- **Exceptions**: `raiseException`, `raiseExceptionOfType`

### :material-check-circle:{ .lg } Beautiful Output

Tests results with Spectre.Console:

- Color-coded pass/fail indicators
- Hierarchical test structure
- Diff visualization for failures
- Performance timing

### :material-check-circle:{ .lg } Focused & Pending Tests

Development workflow features:

- `fit` - Focus on specific tests
- `fdescribe` - Focus on test groups
- `xit` / `pending` - Skip tests temporarily

### :material-check-circle:{ .lg } Hooks & Setup

Lifecycle hooks for test setup and teardown:

- `beforeEach` / `afterEach` - Run before/after each test
- `beforeAll` / `afterAll` - Run once per group

---

## Philosophy

!!! quote "Type-Safe Testing"
    FxSpec leverages F#'s type system to make testing safer and more maintainable. Matchers are type-constrained, computation expressions provide compile-time validation, and the test tree is immutable.

!!! quote "Behavior-Driven Development"
    Tests should describe behavior, not implementation. FxSpec's DSL encourages writing tests that serve as living documentation of your system's behavior.

!!! quote "Functional Throughout"
    Pure functions, immutable data, and functional composition. FxSpec embraces F#'s functional programming paradigm throughout its design.

---

## Getting Started

<div class="grid cards" markdown>

-   :fontawesome-solid-rocket:{ .lg .middle } **[Quick Start](quick-start.md)**

    ---

    Get your first test running in 5 minutes.

-   :fontawesome-solid-book:{ .lg .middle } **[DSL Reference](reference/dsl-api.md)**

    ---

    Learn about `spec`, `describe`, `it`, and all DSL functions.

-   :fontawesome-solid-check:{ .lg .middle } **[Matchers](reference/matchers/core.md)**

    ---

    Explore the complete matcher library.

-   :fontawesome-solid-users:{ .lg .middle } **[Contributing](community/contributing.md)**

    ---

    Join the community and contribute to FxSpec.

</div>

---

## Comparison

### Coming from xUnit/NUnit?

FxSpec uses BDD-style testing instead of attribute-based testing:

=== "xUnit/NUnit"

    ```fsharp
    [<Fact>]
    let ``adds two numbers`` () =
        let result = Calculator.add 2 3
        Assert.Equal(5, result)
    ```

=== "FxSpec"

    ```fsharp
    it "adds two numbers" (fun () ->
        let result = Calculator.add 2 3
        expect result |> should (equal 5)
    )
    ```

**Benefits:**

- Tests read like specifications
- Better organization with `describe`/`context`
- Hierarchical structure
- Richer failure messages

### Coming from RSpec/Jest?

FxSpec's syntax will feel familiar:

=== "RSpec (Ruby)"

    ```ruby
    describe "Calculator" do
      it "adds numbers" do
        expect(2 + 2).to eq(4)
      end
    end
    ```

=== "FxSpec (F#)"

    ```fsharp
    describe "Calculator" [
        it "adds numbers" (fun () ->
            expect (2 + 2) |> should (equal 4)
        )
    ]
    ```

=== "Jest (JavaScript)"

    ```javascript
    describe("Calculator", () => {
      it("adds numbers", () => {
        expect(2 + 2).toBe(4)
      })
    })
    ```

**Key differences:**

- F# uses lists `[]` instead of blocks `{}`
- Matchers use F#'s pipe operator `|>`
- Tests are wrapped in `fun () ->` for lazy evaluation

---

## What's Next?

Ready to dive in?

1. **[Quick Start](quick-start.md)** - Install FxSpec and write your first test
2. **[DSL API](reference/dsl-api.md)** - Learn all the DSL functions
3. **[Matchers](reference/matchers/core.md)** - Explore the matcher library

Questions or feedback? [Open an issue on GitHub](https://github.com/fxspec/fx-spec/issues)
