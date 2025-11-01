# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

FxSpec is a pure F# BDD testing framework inspired by RSpec. It uses computation expressions to create a type-safe, functional DSL for behavior-driven development testing. The framework can test itself—FxSpec's own tests are written using FxSpec.

## Build & Test Commands

### Building the Solution
```bash
# Build everything
dotnet build

# Build specific projects
dotnet build src/FxSpec.Core/FxSpec.Core.fsproj
dotnet build src/FxSpec.Matchers/FxSpec.Matchers.fsproj
dotnet build src/FxSpec.Runner/FxSpec.Runner.fsproj
dotnet build tests/FxSpec.Core.Tests/FxSpec.Core.Tests.fsproj
```

### Running Tests
```bash
# Run tests using the FxSpec runner (self-hosting)
./run-tests.sh

# Run tests with filtering
./run-tests.sh --filter "SpecBuilder"

# Run tests with specific format
./run-tests.sh --format simple

# Build and run manually
dotnet build tests/FxSpec.Core.Tests/FxSpec.Core.Tests.fsproj
dotnet run --project src/FxSpec.Runner/FxSpec.Runner.fsproj -- \
  tests/FxSpec.Core.Tests/bin/Debug/net9.0/FxSpec.Core.Tests.dll
```

### Running Single Tests
Since FxSpec is its own test runner, you can filter tests by description:
```bash
./run-tests.sh --filter "matcher name"
./run-tests.sh --filter "SpecBuilder"
```

## Core Architecture

### Three-Layer Separation of Concerns

1. **FxSpec.Core** - The DSL and core types
   - `Types.fs`: Core types (`TestNode`, `TestResult`, `TestResultNode`)
   - `SpecBuilder.fs`: Computation expression builder and helpers (`spec`, `it`, `describe`, `context`)
   - `StateManagement.fs`: Scope stack, `let'`, hooks (deferred to Phase 6)

2. **FxSpec.Matchers** - The assertion system
   - `MatchResult.fs`: Match result types and `AssertionException`
   - `Assertions.fs`: Core functions (`expect`, `to'`, `notTo'`)
   - `CoreMatchers.fs`: Basic matchers (`equal`, `beNil`, `beSome`, `beNone`)
   - `CollectionMatchers.fs`: Collection matchers (`contain`, `beEmpty`, `haveLength`)
   - `StringMatchers.fs`: String matchers (`startWith`, `endWith`, `matchRegex`)
   - `NumericMatchers.fs`: Numeric comparisons (`beGreaterThan`, `beLessThan`)
   - `ExceptionMatchers.fs`: Exception matchers (`raiseException`)

3. **FxSpec.Runner** - Test discovery and execution
   - `Discovery.fs`: Reflection-based test discovery with `[<Tests>]` attribute
   - `Executor.fs`: Test tree execution engine
   - `SimpleFormatter.fs`: Simple text output
   - `DocumentationFormatter.fs`: Rich Spectre.Console output with colors
   - `DiffFormatter.fs`: Diff display for expected vs actual values
   - `Program.fs`: CLI with filtering and format options

### Key Architectural Patterns

**Computation Expression as Tree Builder**
- The `spec { ... }` CE doesn't execute tests—it builds an immutable `TestNode` tree
- Separation of declaration (compile-time) and execution (runtime)
- Enables filtering, multiple formatters, and metaprogramming

**Type-Safe Matchers with Discriminated Unions**
```fsharp
type MatchResult =
    | Pass
    | Fail of message: string * expected: obj option * actual: obj option
```
- Single source of truth for pass/fail
- Impossible to forget failure messages
- Rich data for formatting diffs

**Test Node Structure**
```fsharp
type TestNode =
    | Example of description: string * test: TestExecution
    | Group of description: string * tests: TestNode list
    | FocusedExample of description: string * test: TestExecution
    | FocusedGroup of description: string * tests: TestNode list
```

**Focused Test Execution**
- `fit` (focused it) and `fdescribe` (focused describe) allow running specific tests
- When any focused test exists, only focused tests run
- `TestNode.filterFocused` transforms focused nodes to regular nodes

## Writing Tests

### Basic Test Structure
```fsharp
[<Tests>]
let mySpecs =
    spec {
        describe "Feature Name" [
            context "when condition" [
                it "behaves this way" (fun () ->
                    expect actual |> should (equal expected)
                )
            ]
        ]
    }
```

### Skipping and Focusing Tests
```fsharp
// Skip a test
xit "not ready yet" (fun () -> ...)
pending "not ready yet" (fun () -> ...)

// Focus on specific tests
fit "only run this" (fun () -> ...)
fdescribe "only run this group" [ ... ]
```

### Custom Matchers
Matchers are functions of type `'a -> MatchResult`:
```fsharp
let beEven : Matcher<int> =
    fun actual ->
        if actual % 2 = 0 then Pass
        else Fail($"{actual} is not even", None, Some (box actual))
```

## File Organization Pattern

Files are organized by feature/function (not by type):
- Core types and DSL in `FxSpec.Core`
- All matchers in `FxSpec.Matchers` (grouped by category)
- All runner components in `FxSpec.Runner`
- Tests mirror the structure: `FxSpec.Core.Tests`, `FxSpec.Matchers.Tests`

## F# Conventions Used

- Use `camelCase` for functions and values
- Use `PascalCase` for types, modules, and DU cases
- Prefer pipe operator `|>` for data flow
- Pattern match exhaustively (compiler warns on incomplete matches)
- Use type annotations for public APIs
- Document complex functions with XML comments `///`
- Test files end with `Specs.fs` (e.g., `TypesSpecs.fs`, `MatchersSpecs.fs`)

## Important Design Decisions

1. **Pure F# Implementation**: No dependency on NUnit, xUnit, or MSTest
2. **Self-Hosting**: FxSpec tests itself using its own test framework
3. **Computation Expressions**: Natural F# DSL rather than attributes or reflection-heavy approach
4. **Immutable Data**: All test trees are immutable; execution creates new result trees
5. **Functional Throughout**: Pure functions, minimal side effects
6. **Type Safety**: Matchers are type-constrained; impossible to compare incompatible types

## Current Phase: Phase 6 - Hooks

The project is currently implementing beforeEach/afterEach and beforeAll/afterAll hooks. Previous phases have implemented:
- ✅ Phase 1: Core DSL with computation expressions
- ✅ Phase 2: Matchers system with comprehensive matchers
- ✅ Phase 3.1: Dogfooding (FxSpec testing itself)
- ✅ Phase 3.2: Test discovery and execution
- ✅ Phase 4: Formatters with Spectre.Console
- ✅ Phase 5: Pending and focused tests (xit, fit, fdescribe)
- 🔄 Phase 6: Hooks implementation

## Dependencies

Key external dependencies:
- **Spectre.Console**: Rich console output with colors, tables, and panels
- **.NET 9.0**: Target framework (also supports .NET 8.0)
- **F# 8.0+**: Language features including computation expressions

## Testing Philosophy

- Use FxSpec's own DSL to test FxSpec (dogfooding)
- Tests are executable documentation
- Custom matchers for testing internal types (see `FxSpecMatchers.fs`)
- All tests marked with `[<Tests>]` attribute for discovery
