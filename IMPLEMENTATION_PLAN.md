# FxSpec: Implementation Plan for F# RSpec-like BDD Framework

## Executive Summary

This plan outlines the implementation of **FxSpec**, a pure F# BDD testing framework inspired by RSpec. The framework will leverage F#'s unique strengths—computation expressions, discriminated unions, type safety, and immutability—to create a testing experience that is both elegant and robust.

## Core Design Principles

1. **Pure F# Implementation**: No dependencies on existing .NET test frameworks (NUnit, xUnit, MSTest)
2. **Type Safety First**: Leverage F#'s type system to catch errors at compile time
3. **Functional & Immutable**: Embrace functional programming principles throughout
4. **Excellent Developer Experience**: Beautiful console output and clear error messages
5. **RSpec Feature Parity**: Support the full spectrum of RSpec's core capabilities

## Project Structure

```
FxSpec/
├── src/
│   ├── FxSpec.Core/           # Core DSL and types
│   ├── FxSpec.Matchers/       # Assertion system
│   ├── FxSpec.Runner/         # Test discovery and execution
│   ├── FxSpec.Formatters/     # Console output
│   └── FxSpec.Extensions/     # Request/Feature specs
├── tests/
│   ├── FxSpec.Core.Tests/
│   ├── FxSpec.Matchers.Tests/
│   └── FxSpec.Runner.Tests/
└── examples/
    ├── BasicExamples/
    ├── WebApiExamples/
    └── AdvancedExamples/
```

## Implementation Phases

### Phase 1: Core DSL and Tree Structure (Week 1-2)

**Goal**: Build the foundational computation expression-based DSL

#### 1.1 Core Types
```fsharp
type TestResult =
    | Pass
    | Fail of exn option
    | Skipped of reason: string

type TestExecution = unit -> TestResult

type TestNode =
    | Example of description: string * test: TestExecution
    | Group of description: string * tests: TestNode list
```

#### 1.2 SpecBuilder Computation Expression
- Implement `SpecBuilder` class with:
  - `Run()` - Entry point
  - `Yield()` - For `it` blocks
  - `Combine()` - For sequencing
  - `Zero()` - Empty blocks
  - `Delay()` - Lazy evaluation

#### 1.3 Custom Operations
- `describe` - Primary grouping
- `context` - Alias for describe
- `it` - Individual test examples

#### 1.4 State Management
- `let'` - Lazy-loaded variables with memoization
- `subject` - Primary object under test
- `before` / `after` - Setup/teardown hooks
- `beforeEach` / `afterEach` - Per-example hooks

**Deliverables**:
- `FxSpec.Core` library with DSL
- Unit tests demonstrating tree building
- Example specs showing nested structure

### Phase 2: Assertion System (Week 2-3)

**Goal**: Create a type-safe, fluent matcher system

#### 2.1 MatchResult Type
```fsharp
type MatchResult<'a> =
    | Pass
    | Fail of message: string * expected: obj option * actual: obj option
```

#### 2.2 Core Functions
- `expect` - Identity function for syntax
- `to'` - Assertion engine that throws on Fail
- `notTo'` - Negated assertions

#### 2.3 Core Matchers
- `equal` - Generic equality
- `beNil` - Null checking
- `beSome` / `beNone` - Option matchers
- `beOk` / `beError` - Result matchers
- `contain` - Collection membership
- `beEmpty` - Empty collections
- `haveLength` - Collection size
- `raiseException<'T>` - Exception testing

#### 2.4 Numeric & Comparison Matchers
- `beGreaterThan` / `beLessThan`
- `beGreaterThanOrEqual` / `beLessThanOrEqual`
- `beCloseTo` - Floating point comparison

**Deliverables**:
- `FxSpec.Matchers` library
- Comprehensive matcher tests
- Documentation for custom matcher authoring
- **🎯 Rewrite Phase 1 tests using FxSpec itself (dogfooding begins!)**

### Phase 3: Test Runner (Week 3-5)

**Goal**: Build discovery, execution, and result aggregation

#### 3.1 Test Discovery
- `TestsAttribute` for marking test suites
- Reflection-based assembly scanning
- Find all `TestNode` values with attribute

#### 3.2 Execution Context
```fsharp
type ExecutionScope = {
    LetBindings: Map<string, Lazy<obj>>
    BeforeHooks: (unit -> unit) list
    AfterHooks: (unit -> unit) list
}

type ScopeStack = ExecutionScope list
```

#### 3.3 Execution Engine
- Recursive `executeNode` function
- Scope stack management
- Hook execution in correct order
- Exception handling and wrapping

#### 3.4 Result Tree
```fsharp
type TestResultNode =
    | ExampleResult of description: string * result: TestResult
    | GroupResult of description: string * results: TestResultNode list
```

#### 3.5 CLI Tool
- Use Argu for argument parsing
- Support `--filter` for test selection
- Support `--format` for output style
- Exit codes (0 = success, 1 = failures)

**Deliverables**:
- `FxSpec.Runner` library and executable
- Integration tests for full execution pipeline
- CLI documentation
- **🎯 FxSpec now runs its own tests using its own runner!**

### Phase 4: Console Reporting (Week 5-6)

**Goal**: Create beautiful, informative output

#### 4.1 Spectre.Console Integration
- Add dependency
- Create `DocumentationFormatter`
- Implement tree traversal

#### 4.2 Output Features
- Nested indentation for hierarchy
- Green ✓ for passing tests
- Red ✗ for failing tests
- Yellow ⊘ for skipped tests
- Summary statistics

#### 4.3 Failure Messages
- Full nested description path
- Matcher-specific message
- Expected vs Actual diff
- Cleaned stack trace
- Source file and line number

**Deliverables**:
- `FxSpec.Formatters` library
- Beautiful console output
- Screenshot examples in docs

### Phase 5: Advanced Features (Week 6-8)

**Goal**: Add specialized capabilities

#### 5.1 Request Specs (API Testing)
- `RequestSpecBuilder` extending `SpecBuilder`
- HTTP verb operations: `get`, `post`, `put`, `delete`
- Request builders: `withJson`, `withHeaders`, `withBody`
- HTTP matchers: `haveStatusCode`, `haveHeader`, `haveJsonBody`
- Integration with `WebApplicationFactory`

#### 5.2 Test Control
- `xit` / `pending` - Skip tests
- `fit` / `fdescribe` - Focused execution
- `beforeAll` / `afterAll` - Suite-level hooks

#### 5.3 Extensibility
- Document matcher extension API
- Provide helper functions for custom matchers
- Example custom matchers

**Deliverables**:
- `FxSpec.Extensions` library
- API testing examples
- Extension documentation

## Technical Decisions

### Dependencies
- **Spectre.Console** - Rich console output
- **Argu** - CLI argument parsing
- **FSharp.Core** - Only required dependency for core

### Target Framework
- .NET 8.0+ (for latest F# features)
- Consider .NET Standard 2.1 for wider compatibility

### Naming Conventions
- Use tick (') suffix to avoid F# keyword conflicts (`to'`, `let'`)
- Use camelCase for functions (F# convention)
- Use PascalCase for types and modules

## Success Criteria

1. **Functional Completeness**: All RSpec core features implemented
2. **Type Safety**: No runtime type errors in framework code
3. **Performance**: Execute 1000+ tests in < 5 seconds
4. **Usability**: Clear, actionable error messages
5. **Documentation**: Comprehensive guides and examples

## Future Enhancements (Post-MVP)

- Parallel test execution
- Property-based testing integration (FsCheck)
- Mocking library (inspired by rspec-mocks)
- IDE integration (VS Code/Ionide)
- Feature specs with Playwright
- JSON/XML output formatters
- Code coverage integration
- Watch mode for continuous testing

## Risk Mitigation

| Risk | Mitigation |
|------|------------|
| Computation expression complexity | Start simple, iterate, extensive testing |
| Scope stack bugs | Comprehensive unit tests, clear documentation |
| Performance issues | Profile early, optimize hot paths |
| Poor error messages | User testing, iterate on feedback |
| Reflection overhead | Minimize reflection, cache results |

## Timeline Summary

- **Week 1-2**: Core DSL ✓
- **Week 2-3**: Matchers ✓
- **Week 3-5**: Runner ✓
- **Week 5-6**: Formatters ✓
- **Week 6-8**: Extensions ✓
- **Week 8+**: Polish, docs, examples

## Next Steps

1. Set up project structure and build configuration
2. Implement Phase 1: Core DSL
3. Write comprehensive tests for each component
4. Iterate based on real-world usage
5. Gather community feedback

