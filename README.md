# FxSpec: The Best F# RSpec-like BDD Test Library

> **Behavior-Driven Development meets Type Safety**  
> An elegant, powerful, and fully type-safe BDD testing framework for F#, inspired by RSpec

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0+-purple.svg)](https://dotnet.microsoft.com/)
[![F#](https://img.shields.io/badge/F%23-8.0+-blue.svg)](https://fsharp.org/)

## 📚 Documentation

**[View the complete documentation →](https://fxspec.github.io/fx-spec)**

- [Quick Start Guide](https://fxspec.github.io/fx-spec/quick-start)
- [DSL API Reference](https://fxspec.github.io/fx-spec/reference/dsl-api)
- [Matchers Reference](https://fxspec.github.io/fx-spec/reference/matchers/core)
- [Contributing Guide](https://fxspec.github.io/fx-spec/community/contributing)

## Why FxSpec?

FxSpec combines the **elegant, human-readable syntax of RSpec** with the **compile-time safety and functional purity of F#**. It's not just a port—it's a conceptual enhancement that leverages F#'s unique strengths.

### The Vision

```fsharp
open FxSpec.Core
open FxSpec.Matchers

[<Tests>]
let calculatorSpecs =
    spec {
        yield describe "Calculator" [
            context "when adding numbers" [
                it "adds two positive numbers" (fun () ->
                    expect (2 + 2) |> should (equal 4)
                )

                it "handles negative numbers" (fun () ->
                    expect (-1 + -1) |> should (equal -2)
                )
            ]

            context "when dividing" [
                it "raises exception for division by zero" (fun () ->
                    let action () = 10 / 0
                    expect action |> should raiseException
                )
            ]
        ]
    }
```

**Output:**
```
Calculator
  when adding numbers
    ✓ adds two positive numbers
    ✓ handles negative numbers
  when dividing
    ✓ raises exception for division by zero

3 examples, 0 failures
```

## Key Features

### 🎯 RSpec-Inspired Syntax
- Familiar `describe`, `context`, and `it` blocks
- Nested test organization
- Readable, specification-style tests

### 🔒 Type-Safe by Design
- Compile-time error detection
- Type-constrained matchers
- No runtime type surprises

### 🧩 Computation Expression DSL
- Natural F# syntax
- Powerful tree-building abstraction
- Compiler-verified structure

### 💎 Discriminated Union Results
- Explicit success/failure modeling
- Rich failure data for debugging
- Pattern matching support

### 🎨 Beautiful Console Output
- Powered by Spectre.Console
- Nested, colored output
- Comprehensive failure messages with diffs

### 🔧 Functional & Immutable
- Pure functions throughout
- Immutable test data
- Composable matchers

### 🚀 Pure F# Implementation
- No dependency on NUnit, xUnit, or MSTest
- Custom test discovery and execution
- Independent CLI runner

## Quick Start

### Installation (Coming Soon)

```bash
dotnet add package FxSpec
```

### Write Your First Test

```fsharp
open FxSpec.Core
open FxSpec.Matchers

[<Tests>]
let myFirstSpec =
    spec {
        yield describe "My Feature" [
            it "works correctly" (fun () ->
                expect true |> should beTrue
            )
        ]
    }
```

### Run Tests

```bash
dotnet fspec MyTests.dll
```

## Core Concepts

### 1. Describe & Context

Organize tests hierarchically:

```fsharp
describe "User" [
    context "when newly created" [
        it "has no posts" (fun () -> ...)
    ]

    context "when activated" [
        it "can log in" (fun () -> ...)
    ]
]
```

### 2. Expectations & Matchers

Fluent, type-safe assertions:

```fsharp
expect actual |> should (equal expected)
expect list |> should (contain item)
expect value |> should beNil
expect option |> should (beSome 42)
expect result |> should (beOk "success")
```

### 3. Hooks & Setup

Setup and teardown with hooks:

```fsharp
describe "Database" [
    let mutable connection = null

    beforeEach (fun () ->
        connection <- Database.connect()
    )

    afterEach (fun () ->
        connection.Dispose()
    )

    it "saves records" (fun () ->
        let result = connection.Save(record)
        expect result |> should beTrue
    )
]
```

### 4. Custom Matchers

Simple function-based matchers:

```fsharp
let beEven : Matcher<int> =
    fun actual ->
        if actual % 2 = 0 then Pass
        else Fail($"{actual} is not even", None, Some (box actual))

expect 4 |> should beEven
```

## Advanced Features

### Request Specs (API Testing)

> **Note:** Request specs are a planned feature for future releases.

```fsharp
// Future feature - not yet implemented
requestSpec {
    yield describe "Users API" [
        it "creates a user" (fun () ->
            let response =
                post "/api/users"
                |> withJson {| Name = "John" |}
            expect response |> should (haveStatusCode 201)
        )
    ]
}
```

### Comprehensive Matchers

- **Equality**: `equal`, `beNil`, `beSome`, `beNone`
- **Collections**: `contain`, `beEmpty`, `haveLength`
- **Numeric**: `beGreaterThan`, `beLessThan`, `beCloseTo`
- **Strings**: `startWith`, `endWith`, `matchRegex`
- **Exceptions**: `raiseException<T>`
- **Results**: `beOk`, `beError`

## Architecture Highlights

### Computation Expression as Tree Builder

The `spec` CE builds an immutable tree structure:

```fsharp
type TestNode =
    | Example of description: string * test: TestExecution
    | Group of description: string * tests: TestNode list
```

This separates **declaration** from **execution**, enabling:
- Test filtering and selection
- Multiple output formats
- Future parallel execution
- Metaprogramming capabilities

### Type-Safe Matcher System

Matchers return structured results:

```fsharp
type MatchResult =
    | Pass
    | Fail of message: string * expected: obj option * actual: obj option
```

Benefits:
- Single source of truth
- Rich failure data
- Composable and testable
- Impossible to forget failure messages

## Project Status

✅ **MVP Complete!** ✅

FxSpec is a fully functional F# BDD testing framework with:

1. ✅ Complete DSL implementation (spec, describe, it, context)
2. ✅ Comprehensive matcher library (50+ matchers)
3. ✅ Test discovery and execution
4. ✅ Beautiful console output with Spectre.Console
5. ✅ Hooks (beforeEach, afterEach, beforeAll, afterAll)
6. ✅ Focused and pending tests (fit, fdescribe, xit, pending)
7. ✅ Self-hosting (FxSpec tests itself - 166 tests passing!)

**Ready for:** Early adopters and feedback

## Documentation

📚 **[Complete Documentation](https://fxspec.github.io/fx-spec)** (Material for MkDocs)

- **[Quick Start Guide](docs/quick-start.md)** - Get started in 5 minutes
- **[DSL API Reference](docs/reference/dsl-api.md)** - Complete DSL documentation
- **[Matchers Reference](docs/reference/matchers/core.md)** - All available matchers
- **[Contributing Guide](docs/community/contributing.md)** - How to contribute

**Design Documents:**
- [Implementation Plan](IMPLEMENTATION_PLAN.md) - Detailed roadmap and phases
- [Technical Architecture](TECHNICAL_ARCHITECTURE.md) - Deep dive into design decisions
- [FxSpec vs RSpec](FXSPEC_VS_RSPEC.md) - Detailed comparison

## Roadmap

### Phase 1: Core DSL (Weeks 1-2) ✅
- [x] Design computation expression
- [x] Implement SpecBuilder
- [x] Add describe/context/it
- [x] State management (let', subject, hooks)

### Phase 2: Matchers (Weeks 2-3) ✅
- [x] MatchResult type
- [x] Core matchers
- [x] Custom matcher API

### Phase 3: Runner (Weeks 3-5)

#### 3.1: Dogfooding ✅
- [x] Rewrite Phase 1 tests using FxSpec itself
- [x] Create custom matchers for testing FxSpec internals
- [x] Validate framework usability
- [x] 30 examples, 0 failures - FxSpec tests itself!

#### 3.2: Test Execution ✅
- [x] Test discovery with `[<Tests>]` attribute
- [x] Execution engine
- [x] CLI tool with filtering
- [x] FxSpec runs its own tests!
- [ ] Scope stack with `let'` and hooks (deferred to Phase 5)

### Phase 4: Formatters (Week 5-6) ✅
- [x] Spectre.Console integration
- [x] Expected vs Actual diffs
- [x] Full test path in failures
- [x] Beautiful tables and panels
- [x] Color-coded output
- [x] Format selection (--format option)

### Phase 5: Pending & Focused Tests (Week 7) ✅
- [x] xit/pending for skipping tests
- [x] fit/fdescribe for focused execution
- [x] Automatic focused filtering
- [x] Skip reason display
- [x] Legacy test cleanup

### Phase 6: Hooks & Code Quality ✅
- [x] beforeEach/afterEach hooks
- [x] beforeAll/afterAll hooks
- [x] Code quality refactoring
- [x] Input validation for matchers
- [x] Extract magic numbers to constants
- [x] Functional refactoring (eliminate mutable variables)

## Future Enhancements

### Planned Features
- [ ] Request specs (API testing)
- [ ] Parallel test execution
- [ ] Custom formatters API
- [ ] State management with `let'` and `subject` (experimental)
- [ ] Property-based testing integration
- [ ] Code coverage integration

## Contributing

We welcome contributions! This is an ambitious project and we'd love your help.

**Ways to contribute:**
- 💡 Design feedback and ideas
- 🐛 Bug reports and testing
- 📝 Documentation improvements
- 💻 Code contributions
- 🎨 Example specs and use cases

## Design Principles

1. **Type Safety Over Flexibility**: Catch errors at compile time
2. **Functional Over Imperative**: Pure functions, immutable data
3. **Explicit Over Implicit**: Clear, readable code
4. **Composable Over Monolithic**: Small, reusable pieces
5. **Beautiful Over Minimal**: Great UX matters

## Inspiration & Credits

- **RSpec** - The gold standard for BDD testing
- **F# for Fun and Profit** - Computation expression insights
- **Spectre.Console** - Beautiful console output
- **The F# Community** - For building an amazing language

## License

MIT License - see [LICENSE](LICENSE) for details

## Contact

- **Issues**: [GitHub Issues](https://github.com/yourusername/fxspec/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/fxspec/discussions)

---

**Built with ❤️ and F# by developers who believe testing should be elegant, safe, and fun.**

