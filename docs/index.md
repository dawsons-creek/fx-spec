# FxSpec

<div class="grid cards" markdown>

-   :material-rocket-launch:{ .lg .middle } **Write Specifications That Test and Document Your F# Code**

    ---

    A type-safe BDD framework that creates executable documentation with exceptional debugging support.

    Tests that describe behavior, compile-time safety, and filtered stack traces that accelerate development.

</div>

[Get Started in 5 Minutes](quick-start.md){ .md-button .md-button--primary }
[View on GitHub](https://github.com/dawsons-creek/fx-spec){ .md-button }

---

## Why FxSpec?

<div class="grid cards" markdown>

-   :material-file-document:{ .lg .middle } **Specifications as Documentation**

    ---

    Tests describe expected behavior in plain language, creating living documentation that stays synchronized with your code.

    When behavior changes, your documentation updates automatically.

-   :material-shield-check:{ .lg .middle } **Type-Safe Expectations**

    ---

    Leverage F#'s type system with context-aware matchers and IntelliSense support.

    Type-specific expectations prevent runtime errors and guide you to the right assertions.

-   :material-bug:{ .lg .middle } **Exceptional Debugging**

    ---

    Filtered stack traces show only YOUR code, not framework internals. Clickable file links jump directly to errors.

    Hierarchical output preserves test context, pinpointing issues instantly.

-   :material-lambda:{ .lg .middle } **Pure Functional Design**

    ---

    Immutable test trees, composable structures, no hidden state. Pure F# with clean, functional syntax.

    Built by F# developers, for F# developers.

</div>

---

## Quick Example

Write tests that read like specifications:

```fsharp
module UserAccountSpecs

open FxSpec.Core
open FxSpec.Matchers

[<Tests>]
let userAccountSpecs =
    describe "User account lifecycle" [
        context "during registration" [
            it "requires a valid email address" (fun () ->
                let result = User.register "invalid-email"
                expectResult(result).toBeError("Invalid email format")
            )

            it "prevents duplicate email registration" (fun () ->
                let existing = "user@example.com"
                Database.seed(existing)
                let result = User.register existing
                expectResult(result).toBeError("Email already registered")
            )
        ]

        context "after successful registration" [
            it "creates an inactive account requiring email verification" (fun () ->
                let user = User.register "new@example.com" |> Result.get
                expectBool(user.IsActive).toBeFalse()
                expectOption(user.VerificationToken).toBeSome()
            )
        ]
    ]
```

**Output:**
```
User account lifecycle
  during registration
    ✓ requires a valid email address
    ✓ prevents duplicate email registration
  after successful registration
    ✓ creates an inactive account requiring email verification

3 examples, 0 failures
```

---

## Writing Specifications

### :material-file-tree:{ .lg } Hierarchical Organization

Structure tests to mirror your system's behavior and requirements:

```fsharp
describe "Payment Processing" [
    context "when payment method is credit card" [
        context "with sufficient funds" [
            it "processes payment successfully" (fun () -> ...)
            it "sends confirmation email to customer" (fun () -> ...)
        ]

        context "with insufficient funds" [
            it "declines the transaction" (fun () -> ...)
            it "notifies customer of declined payment" (fun () -> ...)
        ]
    ]
]
```

The hierarchical structure creates documentation that stakeholders can read and understand.

### :material-text:{ .lg } Natural Language Descriptions

Tests describe what the system should do, not how it does it:

```fsharp
it "preserves user data during session timeout" (fun () -> ...)
it "encrypts sensitive information before storage" (fun () -> ...)
it "validates input against business rules" (fun () -> ...)
```

---

## Ensuring Correctness

### :material-check-all:{ .lg } Type-Specific Matchers

FxSpec provides specialized matchers that prevent runtime errors:

```fsharp
// IntelliSense shows only applicable methods
expectNum(42).toBeGreaterThan(0)      // Works with any numeric type
expectFloat(3.14).toBeCloseTo(3.1, 0.1)  // Float-specific precision
expectStr("hello").toStartWith("h")    // String-specific operations
expectSeq([1; 2]).toContain(1)        // Collection-specific checks
expectOption(result).toBeSome(42)     // Option-specific assertions
expectResult(result).toBeOk()         // Result type handling
```

**Compile-time validation** ensures you can't use the wrong matcher for your data type.

### :material-list-status:{ .lg } Comprehensive Assertion Library

50+ assertion methods covering all common scenarios:

- **Core**: `toEqual`, `notToEqual`
- **Collections**: `toContain`, `toBeEmpty`, `toHaveLength`, `toContainAll`
- **Numeric**: `toBeGreaterThan`, `toBeLessThan`, `toBeCloseTo`, `toBePositive`, `toBeNegative`
- **Strings**: `toStartWith`, `toEndWith`, `toMatchRegex`, `toHaveLength`
- **Exceptions**: `expectThrows<T>`, `expectThrowsWithMessage<T>`, `expectNotToThrow`
- **HTTP**: `toHaveStatus`, `toHaveStatusOk`, `toHaveHeader`, `toHaveJsonBody`

---

## Debugging Failures

### :material-filter:{ .lg } Filtered Stack Traces

When errors occur, FxSpec shows **only the relevant parts of your code**:

```
✗ processes user data

  Calculator > processes user data

  DivideByZeroException: Attempted to divide by zero.

  Stack trace:
    at Calculator.divide(Int32 x, Int32 y)
       in Calculator.fs:42 (Calculator)
    at Calculator.processUserData(User user)
       in Calculator.fs:67 (Calculator)
```

**What's filtered out:**
- Framework internals (FxSpec.Core, FxSpec.Runner)
- .NET runtime frames (System.Reflection, System.Runtime)
- F# compiler-generated noise

**What you see:**
- Clear exception type and message
- Only YOUR code in the stack trace
- Precise line numbers
- Project names for context

### :material-link:{ .lg } Clickable File Links

**Cmd/Ctrl+Click on file paths** to jump directly to the error location in VS Code. No hunting through source files.

### :material-chart-tree:{ .lg } Hierarchical Context

Test output preserves the full path to failures:

```
User account lifecycle
  during registration
    ✗ requires a valid email address

      User account lifecycle > during registration > requires a valid email address

      Expected: Error "Invalid email format"
      Actual:   Ok User { Email = "invalid-email"; ... }
```

The full test path helps you understand exactly which scenario failed.

### :material-compare:{ .lg } Diff Visualization

See exactly what's different when assertions fail:

```
Expected vs Actual:
╭─────────────────────────────────────────────╮
│ Expected: { Name = "Alice"; Age = 30 }      │
│ Actual:   { Name = "Alice"; Age = 31 }      │
│                                      ^^      │
╰─────────────────────────────────────────────╯
```

---

## Managing Test Execution

### :material-target:{ .lg } Focus on Specific Tests

During development, run only the tests you're working on:

```fsharp
describe "Feature" [
    fit "work on this test" (fun () ->  // Only this runs
        expect(2 + 2).toEqual(4)
    )

    it "this test is skipped" (fun () ->
        expectBool(true).toBeTrue()
    )
]
```

Use `fit` for individual tests or `fdescribe` for entire groups.

### :material-clock-outline:{ .lg } Mark Pending Work

Track incomplete tests without breaking your build:

```fsharp
describe "Feature" [
    it "completed test" (fun () ->
        expectBool(true).toBeTrue()
    )

    xit "not ready yet" (fun () ->
        // This doesn't run
    )

    pending "TODO: implement validation test" (fun () ->
        ()
    )
]
```

Pending tests appear in the summary so you don't forget them.

### :material-cog:{ .lg } Setup and Teardown

Manage test lifecycle with hooks:

```fsharp
describe "Database operations" [
    let mutable connection = null

    beforeEach (fun () ->
        connection <- Database.connect()
        connection.BeginTransaction()
    )

    afterEach (fun () ->
        connection.RollbackTransaction()
        connection.Dispose()
    )

    it "saves user data" (fun () ->
        let result = connection.Save(newUser)
        expectResult(result).toBeOk()
    )
]
```

Hooks ensure test isolation and proper resource cleanup.

---

## Technical Architecture

FxSpec builds an **immutable test tree at compile time**, separating test declaration from execution. This functional approach enables:

- **Test filtering** - Run specific tests without re-compiling
- **Multiple output formats** - Same tests, different presentations
- **Guaranteed test isolation** - Tests can't affect each other
- **Composable test structures** - Build tests from smaller pieces

The **fluent API uses F#'s type system** to provide context-aware methods. IntelliSense shows only assertions that make sense for your data type, preventing mismatches at compile time.

```fsharp
// Type system prevents misuse
expectNum(42).toBeGreaterThan(0)      // ✓ Valid
expectNum(42).toStartWith("4")        // ✗ Compile error - no such method

expectStr("hello").toStartWith("h")   // ✓ Valid
expectStr("hello").toBeGreaterThan(5) // ✗ Compile error - no such method
```

---

## Features at a Glance

### Core Capabilities

- ✅ Hierarchical test organization with `describe`/`context`/`it`
- ✅ 50+ type-specific assertion methods
- ✅ Async/await support with `itAsync`
- ✅ HTTP testing matchers for web APIs
- ✅ Lifecycle hooks (`beforeEach`, `afterEach`, `beforeAll`, `afterAll`)
- ✅ Focus and pending tests (`fit`, `xit`, `pending`)

### Developer Experience

- ✅ Filtered stack traces showing only your code
- ✅ Clickable file links (Cmd/Ctrl+Click to jump to errors)
- ✅ Colored, hierarchical console output
- ✅ Diff visualization for mismatched values
- ✅ IntelliSense support for type-specific matchers
- ✅ Self-hosting (FxSpec tests itself with 71+ passing tests)

### Pure F# Design

- ✅ No dependencies on xUnit, NUnit, or MSTest
- ✅ Immutable test trees with functional composition
- ✅ Pure functions throughout
- ✅ Custom test discovery and execution
- ✅ Independent CLI runner

---

## Getting Started

<div class="grid cards" markdown>

-   :fontawesome-solid-rocket:{ .lg .middle } **[Quick Start](quick-start.md)**

    ---

    Get your first test running in 5 minutes.

-   :fontawesome-solid-book:{ .lg .middle } **[DSL Reference](reference/dsl-api.md)**

    ---

    Learn about `describe`, `it`, and all DSL functions.

-   :fontawesome-solid-check:{ .lg .middle } **[Matchers](reference/matchers/core.md)**

    ---

    Explore the complete matcher library.

-   :fontawesome-solid-users:{ .lg .middle } **[Contributing](community/contributing.md)**

    ---

    Join the community and contribute to FxSpec.

</div>

---

## For Developers Familiar with Other Frameworks

If you've used BDD frameworks before, you'll recognize the describe/it syntax. FxSpec adapts these patterns to F#'s functional paradigm:

**Key differences from other BDD frameworks:**

- F# uses **lists** `[]` instead of blocks `{}`
- Tests are wrapped in `fun () ->` for lazy evaluation
- **Type-specific expectations**: `expectSeq`, `expectStr`, `expectNum`, etc.
- Fluent API with method chaining: `expect(x).toEqual(y)`
- Immutable test trees enable powerful filtering and composition

**Example:**

```fsharp
describe "Calculator" [
    it "adds numbers" (fun () ->
        expect(2 + 2).toEqual(4)
    )
]
```

The syntax is straightforward and leverages F#'s strengths: type safety, immutability, and functional composition.

---

## What's Next?

Ready to dive in?

1. **[Quick Start](quick-start.md)** - Install FxSpec and write your first test
2. **[DSL API](reference/dsl-api.md)** - Learn all the DSL functions
3. **[Matchers](reference/matchers/core.md)** - Explore the matcher library
4. **[HTTP Testing](reference/http.md)** - Test web APIs with fluent matchers

Questions or feedback? [Open an issue on GitHub](https://github.com/dawsons-creek/fx-spec/issues)

---

## Inspired By

FxSpec draws inspiration from the best in class:

- **RSpec** - The gold standard for BDD testing in Ruby
- **Expecto** - F# testing framework philosophy
- **Spectre.Console** - Beautiful console output
- **F# for Fun and Profit** - Functional programming insights
