# FxSpec

> **Behavior-Driven Development meets Type Safety**
> An elegant, powerful, and fully type-safe BDD testing framework for F#

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0+-purple.svg)](https://dotnet.microsoft.com/)
[![F#](https://img.shields.io/badge/F%23-8.0+-blue.svg)](https://fsharp.org/)

## 📚 Documentation

**[View the complete documentation →](https://dawsons-creek.github.io/fx-spec)**

- [Quick Start Guide](https://dawsons-creek.github.io/fx-spec/quick-start)
- [DSL API Reference](https://dawsons-creek.github.io/fx-spec/reference/dsl-api)
- [Matchers Reference](https://dawsons-creek.github.io/fx-spec/reference/matchers/core)
- [Contributing Guide](https://dawsons-creek.github.io/fx-spec/community/contributing)

## Why FxSpec?

FxSpec brings **elegant, human-readable BDD syntax** together with the **compile-time safety and functional purity of F#**. Write tests that read like specifications while maintaining the type safety and composability that F# developers expect.

### The Vision

```fsharp
open FxSpec.Core
open FxSpec.Matchers

[<Tests>]
let calculatorSpecs =
    describe "Calculator" [
        context "when adding numbers" [
            it "adds two positive numbers" (fun () ->
                expect(2 + 2).toEqual(4)
            )

            it "handles negative numbers" (fun () ->
                expect(-1 + -1).toEqual(-2)
            )
        ]

        context "when dividing" [
            it "raises exception for division by zero" (fun () ->
                expectThrows<System.DivideByZeroException>(fun () -> 10 / 0 |> ignore)
            )
        ]
    ]
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

### 🎯 Expressive BDD Syntax
- Intuitive `describe`, `context`, and `it` blocks
- Nested test organization
- Readable, specification-style tests

### 🔒 Type-Safe by Design
- Compile-time error detection
- Type-specific expectation methods
- No runtime type surprises

### 💎 Fluent Assertion API
- Natural method chaining: `expect(x).toEqual(y)`
- Reads like English: "expect X to equal Y"
- Type-specific expectations with IntelliSense support

### 🎨 Discriminated Union Results
- Explicit success/failure modeling
- Rich failure data for debugging
- Pattern matching support

### 🎨 Beautiful Console Output
- Powered by Spectre.Console
- Nested, colored output
- Comprehensive failure messages with diffs
- **Rich stack traces with clickable file links** - Jump directly to errors in your code
- Filtered stack traces showing only YOUR code, not framework internals

### 🔧 Functional & Immutable
- Pure functions throughout
- Immutable test data
- Composable test structures

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
    describe "My Feature" [
        it "works correctly" (fun () ->
            expectBool(true).toBeTrue()
        )
    ]
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
// Basic equality
expect(actual).toEqual(expected)

// Collections
expectSeq(list).toContain(item)
expectSeq(list).toHaveLength(3)

// Options
expectOption(option).toBeSome(42)
expectOption(option).toBeNone()

// Results
expectResult(result).toBeOk("success")
expectResult(result).toBeError("failure")

// Booleans
expectBool(value).toBeTrue()
expectBool(value).toBeFalse()

// Numbers
expectNum(value).toBeGreaterThan(0)
expectFloat(value).toBeCloseTo(3.14, 0.01)

// Strings
expectStr(text).toStartWith("Hello")
expectStr(text).toContain("world")
expectStr(text).toMatchRegex(@"\d+")

// Exceptions
expectThrows<ArgumentException>(fun () -> doSomething())
expectThrowsWithMessage<InvalidOperationException>("error message", fun () -> doSomething())
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
        expectBool(result).toBeTrue()
    )
]
```

### 4. Focused and Pending Tests

Control test execution:

```fsharp
describe "Feature" [
    fit "runs only this test" (fun () ->
        expect(1 + 1).toEqual(2)
    )
    
    it "skips this test" (fun () ->
        expect(2 + 2).toEqual(4)
    )
    
    xit "pending test - not implemented yet" (fun () ->
        failwith "TODO"
    )
]
```

## Advanced Features

### Type-Specific Expectations

FxSpec provides specialized expectation functions for different types:

```fsharp
// Generic expectations
expect(value).toEqual(expected)
expect(value).notToEqual(unexpected)

// Numeric expectations (works with any numeric type)
expectNum(42).toBeGreaterThan(0)
expectNum(42).toBeLessThan(100)

// Integer-specific
expectInt(42).toBePositive()
expectInt(-5).toBeNegative()
expectInt(0).toBeZero()

// Float-specific
expectFloat(3.14159).toBeCloseTo(3.14, 0.01)
expectFloat(1.0).toBePositive()

// String expectations
expectStr("hello world").toStartWith("hello")
expectStr("hello world").toEndWith("world")
expectStr("hello world").toContain("lo wo")
expectStr("test123").toMatchRegex(@"\w+\d+")

// Collection expectations
expectSeq([1; 2; 3]).toHaveLength(3)
expectSeq([1; 2; 3]).toContain(2)
expectSeq([1; 2; 3]).toContainAll([1; 3])
expectSeq([]).toBeEmpty()

// Option expectations
expectOption(Some 42).toBeSome(42)
expectOption(None).toBeNone()

// Result expectations  
expectResult(Ok "success").toBeOk("success")
expectResult(Error "failed").toBeError("failed")

// Boolean expectations
expectBool(true).toBeTrue()
expectBool(false).toBeFalse()

// Exception expectations
expectThrows<ArgumentException>(fun () -> raise (ArgumentException()))
expectThrowsWithMessage<InvalidOperationException>("error", fun () -> 
    raise (InvalidOperationException("error"))
)
expectNotToThrow(fun () -> printfn "safe operation")

// HTTP expectations (with expectHttp)
expectHttp(response).toHaveStatusOk()
expectHttp(response).toHaveStatus(201)
expectHttp(response).toHaveHeader("Content-Type", "application/json")
expectHttp(response).toHaveBodyContaining("success")
expectHttp(response).toHaveJsonBody({| name = "John"; age = 30 |})
```

### Async Test Support

FxSpec supports asynchronous tests with `itAsync`:

```fsharp
describe "Async API" [
    itAsync "fetches user data" (async {
        let! user = getUserAsync 123
        expectOption(user).toBeSome()
    })
    
    itAsync "handles HTTP requests" (async {
        use client = new HttpClient()
        let! response = client.GetAsync("https://api.example.com/users") |> Async.AwaitTask
        expectHttp(response).toHaveStatusOk()
    })
]
```

### HTTP Testing

FxSpec provides comprehensive HTTP matchers for testing web APIs:

```fsharp
open FxSpec.Http

describe "User API" [
    itAsync "creates a new user" (async {
        let! response = client.PostAsync("/api/users", jsonContent)
        
        // Status matchers
        expectHttp(response).toHaveStatusCreated()
        expectHttp(response).toHaveStatus(201)
        
        // Header matchers
        expectHttp(response).toHaveHeader("Content-Type", "application/json")
        expectHttp(response).toHaveContentType("application/json")
        
        // Body matchers
        expectHttp(response).toHaveBodyContaining("user")
        expectHttp(response).toHaveJsonBody({| id = 1; name = "John" |})
    })
    
    itAsync "returns 404 for missing user" (async {
        let! response = client.GetAsync("/api/users/999")
        expectHttp(response).toHaveStatusNotFound()
    })
]
```

### Exceptional Developer Experience

When errors occur in your code under test, FxSpec provides **outstanding debugging support**:

**Beautiful, Actionable Error Output:**
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

**Key Features:**
- 🎯 **Clear exception type and message** - Know exactly what went wrong
- 🔍 **Filtered stack traces** - See only YOUR code, not framework internals  
- 🔗 **Clickable file links** - Cmd/Ctrl+Click to jump directly to the error in VS Code
- 📍 **Precise line numbers** - No hunting for the source of the problem
- 🎨 **Color-coded output** - Visual hierarchy for quick scanning

FxSpec automatically filters out:
- Framework internals (FxSpec.Core, FxSpec.Runner)
- .NET runtime frames (System.Reflection, System.Runtime)
- F# compiler-generated code noise

This means **you see only the relevant parts of the stack trace from YOUR code**, making debugging fast and efficient.

### Comprehensive Matchers

- **Equality**: `toEqual`, `notToEqual`
- **Collections**: `toContain`, `toBeEmpty`, `toHaveLength`, `toContainAll`
- **Numeric**: `toBeGreaterThan`, `toBeLessThan`, `toBeCloseTo`, `toBePositive`, `toBeNegative`, `toBeZero`
- **Strings**: `toStartWith`, `toEndWith`, `toContain`, `toMatchRegex`, `toHaveLength`
- **Exceptions**: `expectThrows<T>`, `expectThrowsWithMessage<T>`, `expectNotToThrow`
- **Options**: `toBeSome`, `toBeNone`
- **Results**: `toBeOk`, `toBeError`
- **Booleans**: `toBeTrue`, `toBeFalse`
- **HTTP**: `toHaveStatus`, `toHaveStatusOk`, `toHaveHeader`, `toHaveContentType`, `toHaveBody`, `toHaveBodyContaining`, `toHaveJsonBody`

## Architecture Highlights

### Simplified DSL

FxSpec uses a clean, straightforward syntax without computation expressions:

```fsharp
[<Tests>]
let myTests =
    describe "Feature" [
        it "works" (fun () ->
            expect(result).toEqual(expected)
        )
    ]
```

The DSL builds an immutable tree structure:

```fsharp
type TestNode =
    | Example of description: string * test: TestExecution
    | Group of description: string * hooks: GroupHooks * tests: TestNode list
    | FocusedExample of description: string * test: TestExecution
    | FocusedGroup of description: string * hooks: GroupHooks * tests: TestNode list
    // ... and more
```

This separates **declaration** from **execution**, enabling:
- Test filtering and selection
- Multiple output formats
- Focused and pending test support
- Beautiful hierarchical output

### Type-Safe Fluent API

The fluent API uses wrapper types for type-specific operations:

```fsharp
type Expectation<'a>(actual: 'a) =
    member _.toEqual(expected: 'a) = ...
    member _.notToEqual(expected: 'a) = ...

type NumericExpectation<'a when 'a :> IComparable<'a>>(actual: 'a) =
    member _.toBeGreaterThan(expected: 'a) = ...
    member _.toBeLessThan(expected: 'a) = ...
```

Benefits:
- IntelliSense shows only applicable methods
- Compile-time type checking
- Clear, readable test code
- Impossible to use wrong assertion type

## Project Status

✅ **MVP Complete!** ✅

FxSpec is a fully functional F# BDD testing framework with:

1. ✅ Complete DSL implementation (describe, it, context)
2. ✅ Fluent assertion API with type-specific expectations
3. ✅ Comprehensive matcher library (50+ assertion methods)
4. ✅ Test discovery and execution
5. ✅ Beautiful console output with Spectre.Console
6. ✅ **Rich error output with clickable file links and filtered stack traces**
7. ✅ Hooks (beforeEach, afterEach, beforeAll, afterAll)
8. ✅ Focused and pending tests (fit, fdescribe, xit, pending)
9. ✅ Self-hosting (FxSpec tests itself - 71 tests passing!)

**Ready for:** Early adopters and feedback

## Future Enhancements
- [ ] Custom formatters API
- [ ] Parallel test execution
- [ ] Property-based testing integration
- [ ] Code coverage integration
- [ ] Watch mode for continuous testing
- [ ] Custom assertion messages

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

## Inspired By

FxSpec draws inspiration from the best in class:

- **RSpec** - The gold standard for BDD testing in Ruby
- **Expecto** - F# testing framework philosophy
- **Spectre.Console** - Beautiful console output
- **F# for Fun and Profit** - Functional programming insights

## License

MIT License - see [LICENSE](LICENSE) for details

## Contact

- **Issues**: [GitHub Issues](https://github.com/dawsons-creek/fx-spec/issues)
- **Discussions**: [GitHub Discussions](https://github.com/dawsons-creek/fx-spec/discussions)

---

**Built with ❤️ and F# by developers who believe testing should be elegant, safe, and fun.**

