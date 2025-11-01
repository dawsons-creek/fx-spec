# FxSpec: Technical Architecture

## Overview

FxSpec is a pure F# BDD testing framework that uses computation expressions to create an RSpec-like DSL while maintaining full type safety and functional programming principles.

## Core Architecture Patterns

### 1. Computation Expression as Tree Builder

**Key Insight**: The `spec` computation expression doesn't manage control flow—it builds an immutable tree data structure.

```fsharp
// User writes this:
let mySpec =
    spec {
        describe "Calculator" {
            it "adds two numbers" {
                expect (2 + 2) |> should (equal 4)
            }
        }
    }

// Compiler transforms to:
let mySpec =
    spec.Run(
        spec.Describe("Calculator",
            spec.Yield("adds two numbers", fun () ->
                expect (2 + 2) |> should (equal 4)
            )
        )
    )

// Which produces:
Group("Calculator", [
    Example("adds two numbers", <thunk>)
])
```

### 2. Separation of Declaration and Execution

**Declaration Phase** (Compile-time):
- Computation expression builds `TestNode` tree
- All structure is captured in immutable data
- Type checking ensures correctness

**Execution Phase** (Runtime):
- Runner traverses the tree
- Manages stateful scope stack
- Executes tests and collects results

This separation enables:
- Metaprogramming (filtering, reordering)
- Parallel execution (future)
- Multiple formatters
- Test introspection

### 3. Type-Safe Matchers with Discriminated Unions

**Traditional Approach** (RSpec):
```ruby
# Returns boolean, separate methods for messages
matcher.matches?(actual)  # => true/false
matcher.failure_message   # => string
```

**FxSpec Approach**:
```fsharp
type MatchResult<'a> =
    | Pass
    | Fail of message: string * expected: obj option * actual: obj option

let equal expected actual =
    if actual = expected then Pass
    else Fail($"Expected {expected}, got {actual}", Some expected, Some actual)
```

**Benefits**:
- Single source of truth
- Impossible to forget failure message
- Rich data for formatting
- Composable and testable

### 4. Lexical Scoping via Scope Stack

**RSpec Behavior**:
```ruby
describe "Outer" do
  let(:x) { 1 }
  
  describe "Inner" do
    let(:y) { 2 }
    
    it "has access to both" do
      expect(x + y).to eq(3)  # x from outer, y from inner
    end
  end
end
```

**FxSpec Implementation**:
```fsharp
type ExecutionScope = {
    LetBindings: Map<string, unit -> obj>
    Cache: ConcurrentDictionary<string, obj>
    BeforeHooks: (unit -> unit) list
    AfterHooks: (unit -> unit) list
}

type ScopeStack = ExecutionScope list

// When executing "Inner" example:
// Stack = [OuterScope; InnerScope]
// Lookup searches from inner to outer
```

**Execution Flow**:
1. Enter Group → Push new scope
2. Execute Example → Consult entire stack
3. Exit Group → Pop scope

## Detailed Component Design

### Core Types Module

```fsharp
module FxSpec.Core.Types

type TestResult =
    | Pass
    | Fail of exn option
    | Skipped of reason: string

type TestExecution = unit -> TestResult

type TestNode =
    | Example of description: string * test: TestExecution
    | Group of description: string * tests: TestNode list

type TestResultNode =
    | ExampleResult of description: string * result: TestResult * duration: TimeSpan
    | GroupResult of description: string * results: TestResultNode list
```

### SpecBuilder Implementation

```fsharp
type SpecBuilder() =
    // Core CE methods
    member _.Yield(description: string, test: unit -> unit) : TestNode list =
        let execution () =
            try
                test()
                Pass
            with ex ->
                Fail(Some ex)
        [Example(description, execution)]
    
    member _.Combine(a: TestNode list, b: TestNode list) : TestNode list =
        a @ b
    
    member _.Delay(f: unit -> TestNode list) : TestNode list =
        f()
    
    member _.Zero() : TestNode list =
        []
    
    member _.Run(nodes: TestNode list) : TestNode list =
        nodes
    
    // Custom operations
    [<CustomOperation("describe")>]
    member _.Describe(state: TestNode list, description: string, [<ProjectionParameter>] f: unit -> TestNode list) =
        state @ [Group(description, f())]
    
    [<CustomOperation("context")>]
    member _.Context(state: TestNode list, description: string, [<ProjectionParameter>] f: unit -> TestNode list) =
        state @ [Group(description, f())]
    
    [<CustomOperation("it")>]
    member _.It(state: TestNode list, description: string, test: unit -> unit) =
        state @ (this.Yield(description, test))

let spec = SpecBuilder()
```

### Matcher System

```fsharp
module FxSpec.Matchers

type MatchResult =
    | Pass
    | Fail of message: string * expected: obj option * actual: obj option

type Matcher<'a> = 'a -> MatchResult

// Core assertion functions
let expect (actual: 'a) : 'a = actual

let to' (matcher: Matcher<'a>) (actual: 'a) : unit =
    match matcher actual with
    | Pass -> ()
    | Fail(msg, exp, act) ->
        raise (AssertionException(msg, exp, act))

let notTo' (matcher: Matcher<'a>) (actual: 'a) : unit =
    match matcher actual with
    | Pass -> raise (AssertionException("Expected not to match, but it did", None, None))
    | Fail _ -> ()

// Example matchers
let equal (expected: 'a) : Matcher<'a> =
    fun actual ->
        if actual = expected then
            Pass
        else
            Fail($"Expected {expected}, but got {actual}",
                 Some (box expected),
                 Some (box actual))

let beNil<'a when 'a : null> : Matcher<'a> =
    fun actual ->
        if obj.ReferenceEquals(actual, null) then
            Pass
        else
            Fail("Expected null, but got non-null value",
                 Some null,
                 Some (box actual))

let contain (expected: 'a) : Matcher<'a seq> =
    fun actual ->
        if Seq.contains expected actual then
            Pass
        else
            Fail($"Expected collection to contain {expected}",
                 Some (box expected),
                 Some (box actual))
```

### Execution Engine

```fsharp
module FxSpec.Runner.Executor

type ExecutionScope = {
    LetBindings: Map<string, unit -> obj>
    Cache: ConcurrentDictionary<string, obj>
    BeforeHooks: (unit -> unit) list
    AfterHooks: (unit -> unit) list
}

let emptyScope = {
    LetBindings = Map.empty
    Cache = ConcurrentDictionary()
    BeforeHooks = []
    AfterHooks = []
}

let rec executeNode (scopeStack: ExecutionScope list) (node: TestNode) : TestResultNode =
    match node with
    | Group(description, children) ->
        let newScope = emptyScope  // Would be populated from DSL
        let newStack = newScope :: scopeStack
        let results = children |> List.map (executeNode newStack)
        GroupResult(description, results)
    
    | Example(description, test) ->
        let sw = Stopwatch.StartNew()
        
        // Execute before hooks (outer to inner)
        scopeStack
        |> List.rev
        |> List.iter (fun scope ->
            scope.BeforeHooks |> List.iter (fun hook -> hook()))
        
        // Execute test
        let result = 
            try
                test()
            with ex ->
                Fail(Some ex)
        
        // Execute after hooks (inner to outer)
        scopeStack
        |> List.iter (fun scope ->
            scope.AfterHooks |> List.iter (fun hook -> hook()))
        
        sw.Stop()
        ExampleResult(description, result, sw.Elapsed)
```

## Key Design Decisions

### 1. Why Computation Expressions?

**Alternatives Considered**:
- Object-oriented builder pattern
- Quotations/meta-programming
- Simple function calls

**Why CE Won**:
- Natural nesting syntax
- Type-safe
- Familiar to F# developers
- Compiler support for transformations

### 2. Why Discriminated Unions for Results?

**Alternatives Considered**:
- Boolean returns
- Exception-only approach
- Result<unit, string>

**Why DU Won**:
- Explicit modeling of all cases
- Rich failure data
- Pattern matching support
- Impossible states are impossible

### 3. Why Reflection for Discovery?

**Alternatives Considered**:
- Manual registration
- Source generators
- Compile-time code generation

**Why Reflection Won**:
- Simple implementation
- Minimal user ceremony
- Standard .NET approach
- Performance acceptable (one-time cost)

## Performance Considerations

### Hot Paths
1. **Test Execution**: Minimize allocations in execution loop
2. **Matcher Evaluation**: Keep matchers pure and fast
3. **Scope Lookup**: Use efficient data structures (Map, Dictionary)

### Cold Paths
1. **Discovery**: Reflection is acceptable (one-time)
2. **Formatting**: Can be slower, happens after tests

### Optimization Strategies
- Lazy evaluation for `let'` bindings
- Memoization within test scope
- Parallel execution (future)
- Incremental test discovery (future)

## Extension Points

### Custom Matchers
Users can create matchers by writing functions that return `MatchResult`:

```fsharp
let beEven : Matcher<int> =
    fun actual ->
        if actual % 2 = 0 then Pass
        else Fail($"{actual} is not even", None, Some (box actual))
```

### Custom Builders
Specialized builders can extend `SpecBuilder`:

```fsharp
type RequestSpecBuilder() =
    inherit SpecBuilder()
    
    let mutable httpClient = None
    
    [<CustomOperation("get")>]
    member _.Get(state, url: string) =
        // HTTP logic
        state
```

### Custom Formatters
Implement `IFormatter` interface:

```fsharp
type IFormatter =
    abstract member Format: TestResultNode list -> unit
```

## Testing Strategy

### Unit Tests
- Each component tested in isolation
- Matchers tested with all edge cases
- Builder tested for correct tree construction

### Integration Tests
- Full pipeline: DSL → Execution → Formatting
- Real test suites as fixtures
- Verify scope stack behavior

### Meta-Testing
- Use FxSpec to test itself (dogfooding)
- Ensures framework is usable
- Validates design decisions

