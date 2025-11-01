# FxSpec: The Best F# RSpec-like BDD Test Library

> **Behavior-Driven Development meets Type Safety**  
> An elegant, powerful, and fully type-safe BDD testing framework for F#, inspired by RSpec

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0+-purple.svg)](https://dotnet.microsoft.com/)
[![F#](https://img.shields.io/badge/F%23-8.0+-blue.svg)](https://fsharp.org/)

## Why FxSpec?

FxSpec combines the **elegant, human-readable syntax of RSpec** with the **compile-time safety and functional purity of F#**. It's not just a port—it's a conceptual enhancement that leverages F#'s unique strengths.

### The Vision

```fsharp
open FxSpec

[<Tests>]
let calculatorSpecs =
    spec {
        describe "Calculator" {
            context "when adding numbers" {
                it "adds two positive numbers" {
                    expect (2 + 2) |> to' (equal 4)
                }
                
                it "handles negative numbers" {
                    expect (-1 + -1) |> to' (equal -2)
                }
            }
            
            context "when dividing" {
                it "raises exception for division by zero" {
                    expect (fun () -> 10 / 0 |> ignore)
                    |> to' raiseException<DivideByZeroException>
                }
            }
        }
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
open FxSpec

[<Tests>]
let myFirstSpec =
    spec {
        describe "My Feature" {
            it "works correctly" {
                expect true |> to' (equal true)
            }
        }
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
describe "User" {
    context "when newly created" {
        it "has no posts" { ... }
    }
    
    context "when activated" {
        it "can log in" { ... }
    }
}
```

### 2. Expectations & Matchers

Fluent, type-safe assertions:

```fsharp
expect actual |> to' (equal expected)
expect list |> to' (contain item)
expect value |> to' beNil
expect option |> to' (beSome 42)
expect result |> to' (beOk "success")
```

### 3. State Management

Lazy-loaded variables and hooks:

```fsharp
describe "Database" {
    let' "connection" (fun () -> openConnection())
    
    before (fun () -> setupDatabase())
    after (fun () -> cleanupDatabase())
    
    it "saves records" {
        let conn = get "connection" :?> Connection
        // Test using connection
    }
}
```

### 4. Custom Matchers

Simple function-based matchers:

```fsharp
let beEven : Matcher<int> =
    fun actual ->
        if actual % 2 = 0 then Pass
        else Fail($"{actual} is not even", None, Some (box actual))

expect 4 |> to' beEven
```

## Advanced Features

### Request Specs (API Testing)

```fsharp
requestSpec {
    describe "Users API" {
        it "creates a user" {
            post "/api/users"
            |> withJson {| Name = "John" |}
            |> expect |> to' (haveStatusCode 201)
        }
    }
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

### Lexical Scoping with Scope Stack

Correctly implements RSpec's scoping semantics:
- `let'` variables are lazily evaluated
- Memoized per test execution
- Hooks execute in correct order
- Nested contexts inherit outer scope

## Project Status

🚧 **Currently in Planning Phase** 🚧

This project is in active design and planning. We're building the best F# BDD framework by:

1. ✅ Comprehensive architectural design
2. ✅ Detailed implementation plan
3. ✅ Technical specifications
4. 🔄 Core implementation (starting soon)
5. ⏳ Community feedback and iteration

## Documentation

- **[Implementation Plan](IMPLEMENTATION_PLAN.md)** - Detailed roadmap and phases
- **[Technical Architecture](TECHNICAL_ARCHITECTURE.md)** - Deep dive into design decisions
- **[Quick Start Guide](QUICKSTART.md)** - Get started quickly
- **[FxSpec vs RSpec](FXSPEC_VS_RSPEC.md)** - Detailed comparison
- **[Original Design Doc](docs/Designing%20an%20F%23%20RSpec%20Clone.md)** - Architectural blueprint

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

### Phase 4: Formatters (Weeks 5-6)
- [ ] Spectre.Console integration
- [ ] Documentation formatter
- [ ] Failure messages with diffs

### Phase 5: Extensions (Weeks 6-8)
- [ ] Request specs
- [ ] Pending/focused tests
- [ ] Advanced features

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

