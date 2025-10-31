# Contributing to FxSpec

Thank you for your interest in contributing to FxSpec! This document will help you get started.

## 🎯 Project Vision

FxSpec aims to be the best F# BDD testing framework by combining RSpec's elegant syntax with F#'s type safety and functional programming power.

## 📋 Current Status

**Phase**: Planning Complete, Ready for Implementation

We have comprehensive documentation and a clear roadmap. Now we need contributors to help build it!

## 🚀 Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- F# 8.0 or later
- Your favorite editor (VS Code with Ionide, Visual Studio, or Rider)
- Git

### Setting Up Development Environment

```bash
# Clone the repository
git clone https://github.com/yourusername/fxspec.git
cd fxspec

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests (once we have them!)
dotnet test
```

## 📚 Essential Reading

Before contributing, please read:

1. **[README.md](README.md)** - Project overview
2. **[IMPLEMENTATION_PLAN.md](IMPLEMENTATION_PLAN.md)** - Detailed roadmap
3. **[TECHNICAL_ARCHITECTURE.md](TECHNICAL_ARCHITECTURE.md)** - Design deep dive
4. **[ARCHITECTURE_DECISIONS.md](ARCHITECTURE_DECISIONS.md)** - Key decisions and rationale

## 🎯 How to Contribute

### 1. Pick a Task

Check the [task list](#task-breakdown) below or look at GitHub Issues for:
- 🟢 **Good First Issues** - Great for newcomers
- 🔵 **Core Features** - Essential functionality
- 🟡 **Enhancements** - Nice-to-have features
- 🔴 **Advanced** - Complex features

### 2. Claim the Task

Comment on the issue saying you'd like to work on it. We'll assign it to you.

### 3. Create a Branch

```bash
git checkout -b feature/your-feature-name
```

### 4. Write Code

Follow our [coding standards](#coding-standards) and write tests!

### 5. Submit a Pull Request

- Write a clear description
- Reference the issue number
- Ensure all tests pass
- Request review

## 📝 Coding Standards

### F# Style Guide

```fsharp
// ✅ Good: Use camelCase for functions and values
let calculateTotal items =
    items |> List.sum

// ✅ Good: Use PascalCase for types and modules
type TestResult =
    | Pass
    | Fail of exn option

module TestRunner =
    let execute node = ...

// ✅ Good: Use pipe operator for data flow
let result =
    data
    |> transform
    |> validate
    |> process

// ✅ Good: Pattern match exhaustively
match result with
| Pass -> printfn "Success"
| Fail ex -> printfn "Failed: %A" ex
| Skipped reason -> printfn "Skipped: %s" reason

// ❌ Bad: Incomplete pattern matching
match result with
| Pass -> printfn "Success"
| _ -> printfn "Other"  // Too vague!

// ✅ Good: Use type annotations for public APIs
let equal (expected: 'a) : Matcher<'a> =
    fun actual -> ...

// ✅ Good: Document complex functions
/// Executes a test node and returns the result.
/// Manages scope stack and hook execution.
let executeNode (scopeStack: ExecutionScope list) (node: TestNode) : TestResultNode =
    ...
```

### Testing Standards

```fsharp
// ✅ Every public function should have tests
[<Tests>]
let matcherTests =
    spec {
        describe "equal matcher" {
            it "passes when values are equal" {
                let result = equal 42 42
                expect result |> to' (equal Pass)
            }
            
            it "fails when values differ" {
                let result = equal 42 99
                match result with
                | Fail _ -> ()  // Expected
                | _ -> failwith "Should have failed"
            }
        }
    }
```

### Commit Messages

```
feat: Add equal matcher for basic equality testing
fix: Correct scope stack ordering in nested contexts
docs: Update quick start guide with new examples
test: Add tests for SpecBuilder.Combine method
refactor: Simplify MatchResult pattern matching
```

## 🏗️ Project Structure

```
FxSpec/
├── src/
│   ├── FxSpec.Core/              # Core DSL and types
│   │   ├── Types.fs              # TestNode, TestResult, etc.
│   │   ├── SpecBuilder.fs        # Computation expression
│   │   └── StateManagement.fs    # let', subject, hooks
│   ├── FxSpec.Matchers/          # Assertion system
│   │   ├── MatchResult.fs        # Result types
│   │   ├── Core.fs               # expect, to'
│   │   └── Matchers.fs           # equal, beNil, etc.
│   ├── FxSpec.Runner/            # Test execution
│   │   ├── Discovery.fs          # Reflection-based discovery
│   │   ├── Executor.fs           # Execution engine
│   │   └── CLI.fs                # Command-line interface
│   ├── FxSpec.Formatters/        # Output formatting
│   │   └── DocumentationFormatter.fs
│   └── FxSpec.Extensions/        # Advanced features
│       └── RequestSpec.fs        # API testing
├── tests/
│   ├── FxSpec.Core.Tests/
│   ├── FxSpec.Matchers.Tests/
│   └── FxSpec.Runner.Tests/
└── examples/
    ├── BasicExamples/
    └── WebApiExamples/
```

## 🎯 Task Breakdown

### Phase 1: Core DSL (Good First Issues 🟢)

- [ ] Define `TestResult`, `TestExecution`, `TestNode` types
- [ ] Implement `SpecBuilder` with `Run`, `Yield`, `Combine`
- [ ] Add `describe` and `context` custom operations
- [ ] Implement `it` for test examples
- [ ] Add state management (`let'`, `subject`, `before`, `after`)

### Phase 2: Assertion System (Core Features 🔵)

- [ ] Define `MatchResult` discriminated union
- [ ] Implement `expect` and `to'` functions
- [ ] Create `equal` matcher
- [ ] Create `beNil` matcher
- [ ] Create `contain` matcher
- [ ] Create `raiseException` matcher
- [ ] Add numeric matchers (`beGreaterThan`, etc.)
- [ ] Add collection matchers (`beEmpty`, `haveLength`)

### Phase 3: Test Runner (Core Features 🔵)

- [ ] Create `TestsAttribute`
- [ ] Implement reflection-based discovery
- [ ] Design scope stack structure
- [ ] Implement recursive execution engine
- [ ] Create `TestResultNode` tree
- [ ] Build CLI with Argu
- [ ] Add test filtering

### Phase 4: Console Reporting (Enhancements 🟡)

- [ ] Integrate Spectre.Console
- [ ] Create `DocumentationFormatter`
- [ ] Implement nested output
- [ ] Add colored symbols
- [ ] Design failure messages
- [ ] Add diff view
- [ ] Create summary statistics

### Phase 5: Advanced Features (Advanced 🔴)

- [ ] Create `RequestSpecBuilder`
- [ ] Add HTTP verb operations
- [ ] Create HTTP matchers
- [ ] Add pending/skip functionality
- [ ] Add focused test execution
- [ ] Document custom matcher API

## 🧪 Testing Your Changes

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/FxSpec.Core.Tests

# Run with verbose output
dotnet test -v detailed

# Watch mode (requires dotnet-watch)
dotnet watch test
```

## 📖 Documentation

When adding new features:

1. **Update README.md** if it's a user-facing feature
2. **Update QUICKSTART.md** with examples
3. **Add XML documentation** to public APIs
4. **Create examples** in the examples/ directory

## 🐛 Reporting Bugs

When reporting bugs, please include:

1. **Description** - What happened?
2. **Expected behavior** - What should have happened?
3. **Minimal reproduction** - Smallest code that reproduces the issue
4. **Environment** - .NET version, OS, etc.

## 💡 Suggesting Features

We love feature suggestions! Please:

1. **Check existing issues** first
2. **Explain the use case** - Why is this needed?
3. **Provide examples** - Show how it would work
4. **Consider alternatives** - Are there other approaches?

## 🤝 Code Review Process

1. **Automated checks** must pass (build, tests, linting)
2. **At least one approval** from a maintainer
3. **All comments addressed** or discussed
4. **Documentation updated** if needed

## 🎓 Learning Resources

### F# Resources
- [F# for Fun and Profit](https://fsharpforfunandprofit.com/)
- [F# Documentation](https://docs.microsoft.com/en-us/dotnet/fsharp/)
- [Computation Expressions](https://fsharpforfunandprofit.com/series/computation-expressions/)

### BDD Resources
- [RSpec Documentation](https://rspec.info/)
- [BDD Introduction](https://dannorth.net/introducing-bdd/)

### Testing Resources
- [Testing in F#](https://fsharpforfunandprofit.com/posts/low-risk-ways-to-use-fsharp-at-work-3/)

## 📞 Getting Help

- **GitHub Discussions** - Ask questions, share ideas
- **GitHub Issues** - Report bugs, request features
- **Pull Request Comments** - Get feedback on your code

## 🙏 Thank You!

Every contribution, no matter how small, helps make FxSpec better. We appreciate your time and effort!

---

**Happy coding! Let's build the best F# BDD framework together! 🚀**

