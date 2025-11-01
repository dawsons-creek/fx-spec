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

### Phase 3: Test Runner (Week 3-5)

**Goal**: Build discovery, execution, and result aggregation

#### 3.1 Dogfooding - Rewrite Phase 1 Tests Using FxSpec

**Goal**: Validate the framework by using it to test itself

This is a critical milestone where we prove FxSpec is actually usable and pleasant to work with. We'll rewrite all Phase 1 tests using the FxSpec DSL and matchers we've built.

**What to rewrite**:
- `tests/FxSpec.Core.Tests/TypesTests.fs` - Tests for core types and helper functions
- `tests/FxSpec.Core.Tests/SpecBuilderTests.fs` - Tests for the spec builder DSL

**Approach**:
1. Keep existing tests as regression suite initially
2. Create new FxSpec-based test files alongside old ones
3. Validate both produce same results
4. Once confident, replace old tests with new ones

**Example transformation**:

```fsharp
// OLD: Plain F# with manual assertions
let testSimpleExample() =
    let nodes = spec { yield it "test" (fun () -> ()) }
    match nodes with
    | [Example(desc, _)] when desc = "test" -> ()
    | _ -> failwith "Should create Example node"

// NEW: Using FxSpec itself
let specBuilderSpecs =
    spec {
        describe "SpecBuilder" {
            describe "simple examples" {
                it "creates an Example node" {
                    let nodes = spec { yield it "test" (fun () -> ()) }
                    expect nodes |> to' (haveLength 1)
                    expect (List.head nodes) |> to' (beExample "test")
                }
            }
        }
    }
```

**Benefits**:
- ✅ Validates the framework works in real-world usage
- ✅ Ensures the DSL is actually pleasant to use
- ✅ Catches usability issues early
- ✅ Serves as comprehensive examples for users
- ✅ Builds confidence in the framework

**Deliverables**:
- Rewritten test files using FxSpec DSL
- Custom matchers for testing FxSpec internals (e.g., `beExample`, `beGroup`)
- Validation that all tests still pass
- **🎯 FxSpec now tests itself using its own DSL and matchers!**

#### 3.2 Test Discovery
- `TestsAttribute` for marking test suites
- Reflection-based assembly scanning
- Find all `TestNode` values with attribute

#### 3.3 Execution Context
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

### Phase 6: Code Quality & Refactoring (Week 8-9)

**Goal**: Address code review findings and ensure adherence to best practices

Based on comprehensive code review, this phase focuses on refactoring areas that violate our coding guidelines (simplicity, pure functions, explicit code) and F# best practices.

#### 6.1 Critical Fix: Redesign StateManagement.fs

**Problem**: Current implementation uses ThreadLocal mutable state, violating "Favor Pure Functions" principle.

**Current approach** (problematic):
```fsharp
// ❌ Global mutable state via ThreadLocal
let private currentScopeStack = new System.Threading.ThreadLocal<ScopeStack>(fun () -> ScopeStack.empty)

let let' name (factory: unit -> 'a) : unit =
    // Mutates ThreadLocal state - side effects!
    currentScopeStack.Value <- updatedScope :: (ScopeStack.pop currentScopeStack.Value)
```

**New approach** (pure & functional):
```fsharp
// ✅ Hooks are part of the TestNode structure
type GroupHooks = {
    LetBindings: Map<string, unit -> obj>
    BeforeEach: (unit -> unit) list
    AfterEach: (unit -> unit) list
    BeforeAll: (unit -> unit) option
    AfterAll: (unit -> unit) option
}

type TestNode =
    | Example of description: string * test: TestExecution
    | Group of description: string * hooks: GroupHooks * tests: TestNode list
    | FocusedExample of description: string * test: TestExecution
    | FocusedGroup of description: string * hooks: GroupHooks * tests: TestNode list
```

**Benefits**:
- ✅ No mutable state
- ✅ Pure functions throughout
- ✅ Explicit data flow
- ✅ Thread-safe by design
- ✅ Testable and predictable

**Tasks**:
1. Update `TestNode` type to include `GroupHooks`
2. Update `SpecBuilder` to collect hooks during tree building
3. Update `Executor` to use hooks from tree (not ThreadLocal)
4. Remove all ThreadLocal usage
5. Update tests to use new hook API
6. Verify all existing tests pass

#### 6.2 Refactor Discovery.fs - Break Down Long Functions

**Problem**: `discoverTests` function is 62 lines with 4+ nesting levels, violating "Limit Nesting (<3 layers)" and "Function Length (25-30 lines)" guidelines.

**Refactoring**:
```fsharp
// ✅ Extract helper functions
let private hasTestsAttribute (member': MemberInfo) =
    member'.GetCustomAttributes(typeof<TestsAttribute>, false).Length > 0

let private isTestNodeList (propertyType: Type) =
    propertyType = typeof<TestNode list>

let private extractTestValue (getValue: unit -> obj) : TestNode list =
    try
        getValue() :?> TestNode list
    with ex ->
        printfn "Warning: Could not extract test value: %s" ex.Message
        []

let private discoverFromProperties (typ: Type) : TestNode list =
    let bindingFlags = BindingFlags.Public ||| BindingFlags.Static
    typ.GetProperties(bindingFlags)
    |> Array.filter hasTestsAttribute
    |> Array.filter (fun p -> isTestNodeList p.PropertyType)
    |> Array.collect (fun p -> extractTestValue (fun () -> p.GetValue(null)))
    |> Array.toList

let private discoverFromFields (typ: Type) : TestNode list =
    let bindingFlags = BindingFlags.Public ||| BindingFlags.Static
    typ.GetFields(bindingFlags)
    |> Array.filter hasTestsAttribute
    |> Array.filter (fun f -> isTestNodeList f.FieldType)
    |> Array.collect (fun f -> extractTestValue (fun () -> f.GetValue(null)))
    |> Array.toList

let private discoverFromType (typ: Type) : TestNode list =
    try
        discoverFromProperties typ @ discoverFromFields typ
    with ex ->
        printfn "Warning: Could not inspect type %s: %s" typ.FullName ex.Message
        []

// ✅ Main function now clean and short (15 lines)
let discoverTests (assembly: Assembly) : TestNode list =
    try
        let allTests =
            assembly.GetTypes()
            |> Array.collect (discoverFromType >> List.toArray)
            |> Array.toList

        TestNode.filterFocused allTests
    with ex ->
        printfn "Error discovering tests: %s" ex.Message
        []
```

**Tasks**:
1. Extract helper functions as shown above
2. Reduce nesting depth to max 2-3 levels
3. Keep main function under 25 lines
4. Verify discovery still works correctly

#### 6.3 Replace Mutable Variables with Functional Alternatives

**Problem**: `DiffFormatter.compareStrings` uses mutable variable, violating "Favor Pure Functions".

**Current**:
```fsharp
// ❌ Mutable state
let mutable firstDiff = -1
for i in 0 .. maxLen - 1 do
    // ...
    if expChar <> actChar && firstDiff = -1 then
        firstDiff <- i
```

**Refactored**:
```fsharp
// ✅ Pure functional approach
let compareStrings (expected: string) (actual: string) =
    if expected = actual then None
    else
        let maxLen = max expected.Length actual.Length
        let firstDiff =
            seq { 0 .. maxLen - 1 }
            |> Seq.tryFindIndex (fun i ->
                let expChar = if i < expected.Length then Some expected.[i] else None
                let actChar = if i < actual.Length then Some actual.[i] else None
                expChar <> actChar)

        firstDiff |> Option.map (sprintf "First difference at position %d")
```

**Tasks**:
1. Replace mutable variables with `Seq.tryFindIndex` or similar
2. Ensure tests still pass
3. Verify performance is acceptable

#### 6.4 Consolidate Null-Checking Patterns

**Problem**: StringMatchers has repetitive null-checking logic, violating "Simplicity First".

**Refactoring**:
```fsharp
// ✅ Extract common pattern
let private nullSafeStringMatch
    (expected: string)
    (actual: string)
    (predicate: string -> string -> bool)
    (successMessage: string)
    (failureMessage: string -> string -> string) =
    match actual, expected with
    | null, null -> Pass
    | null, _ ->
        Fail("Expected string, but found null", Some (box expected), Some null)
    | _, null ->
        Fail("Expected null, but found string", Some null, Some (box actual))
    | a, e when predicate a e -> Pass
    | a, e ->
        Fail(failureMessage e a, Some (box e), Some (box a))

// ✅ Use it in matchers
let startWith (expected: string) : Matcher<string> =
    nullSafeStringMatch expected
        >> fun actual ->
            if actual.StartsWith(expected) then Pass
            else Fail(sprintf "Expected string to start with '%s', but found '%s'" expected actual,
                     Some (box expected), Some (box actual))
```

**Tasks**:
1. Extract common null-checking pattern
2. Apply to all string matchers
3. Reduce code duplication

#### 6.5 Add Input Validation to Matchers

**Problem**: Missing validation violates "Validate Inputs" guideline.

**Examples**:
```fsharp
// ✅ Add validation to beCloseTo
let beCloseTo (expected: float) (tolerance: float) : Matcher<float> =
    if tolerance < 0.0 then
        invalidArg (nameof tolerance) "Tolerance must be non-negative"
    if Double.IsNaN(tolerance) || Double.IsNaN(expected) then
        invalidArg (nameof expected) "Expected and tolerance must be valid numbers"
    fun actual ->
        let diff = abs (actual - expected)
        if diff <= tolerance then Pass
        else Fail(sprintf "Expected value to be close to %f (±%f), but found %f (diff: %f)"
                         expected tolerance actual diff,
                 Some (box expected), Some (box actual))

// ✅ Add validation to beBetween
let beBetween (min: 'a when 'a : comparison) (max: 'a when 'a : comparison) : Matcher<'a> =
    if min > max then
        invalidArg (nameof min) "Minimum value must be less than or equal to maximum value"
    fun actual ->
        if actual >= min && actual <= max then Pass
        else Fail(sprintf "Expected value to be between %A and %A, but found %A" min max actual,
                 Some (box (min, max)), Some (box actual))
```

**Tasks**:
1. Add validation to numeric matchers
2. Add validation to collection matchers where appropriate
3. Document validation behavior

#### 6.6 Extract Magic Numbers to Named Constants

**Problem**: Unexplained magic numbers violate "Explicit Code".

**Examples**:
```fsharp
// SimpleFormatter.fs
let private maxStackTraceLines = 3
let private maxCollectionItemsToShow = 10

// CollectionMatchers.fs
let private maxItemsInErrorMessage = 10

// Then use these constants:
|> Array.truncate maxStackTraceLines
|> Seq.truncate maxItemsInErrorMessage
```

**Tasks**:
1. Identify all magic numbers
2. Extract to named constants with clear names
3. Place constants at module level with documentation

#### 6.7 Improve Pattern Matching Consistency

**Problem**: Inconsistent use of `function` shorthand vs explicit `match`.

**Guideline**: Use `function` shorthand for single-argument pattern matching on the last parameter.

**Examples**:
```fsharp
// ✅ Good - use function shorthand
let formatResult = function
    | Pass -> "✓"
    | Fail _ -> "✗"
    | Skipped _ -> "⊘"

// ✅ Good - explicit match when multiple arguments or not last parameter
let compareResults result1 result2 =
    match result1, result2 with
    | Pass, Pass -> Pass
    | Fail msg, _ -> Fail msg
    | _, Fail msg -> Fail msg
```

**Tasks**:
1. Review all pattern matching in codebase
2. Apply consistent style
3. Prefer `function` where appropriate

#### 6.8 Documentation Updates

**Tasks**:
1. Update CLAUDE.md with new hook API
2. Update examples to use new StateManagement approach
3. Document validation behavior in matchers
4. Add migration guide for hook changes

**Deliverables**:
- ✅ StateManagement redesigned without ThreadLocal
- ✅ Discovery.fs refactored into smaller functions
- ✅ All mutable variables replaced with functional alternatives
- ✅ Null-checking consolidated in StringMatchers
- ✅ Input validation added to numeric/collection matchers
- ✅ Magic numbers extracted to named constants
- ✅ Pattern matching style consistent throughout
- ✅ All tests passing
- ✅ Code review compliance: A grade
- ✅ Updated documentation

**Success Criteria**:
- Zero ThreadLocal usage
- No functions over 30 lines
- Nesting depth ≤ 3 levels everywhere
- No mutable variables except where absolutely necessary
- All guidelines from CLAUDE.md followed
- Code review findings addressed

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
| Technical debt accumulation | Phase 6 refactoring, code review after each phase, adherence to coding guidelines |

## Timeline Summary

- **Week 1-2**: Core DSL ✓
- **Week 2-3**: Matchers ✓
- **Week 3-5**: Runner ✓
- **Week 5-6**: Formatters ✓
- **Week 6-8**: Extensions ✓
- **Week 8-9**: Code Quality & Refactoring
- **Week 9+**: Polish, docs, examples

## Next Steps

1. Set up project structure and build configuration
2. Implement Phase 1: Core DSL
3. Write comprehensive tests for each component
4. Iterate based on real-world usage
5. Gather community feedback

