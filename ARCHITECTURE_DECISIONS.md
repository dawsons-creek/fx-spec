# Architecture Decision Records (ADR)

## ADR-001: Use Computation Expressions for DSL

**Status**: Accepted

**Context**: We need a way to create an RSpec-like nested DSL in F# that is both readable and type-safe.

**Decision**: Use F# Computation Expressions as a tree builder, not for control flow.

**Alternatives Considered**:
1. Object-oriented builder pattern
2. Simple function calls with nesting
3. Quotations/meta-programming
4. Type providers

**Rationale**:
- Natural nesting syntax that mirrors RSpec
- Full compiler support and type checking
- Familiar to F# developers
- Flexible enough for custom operations
- Transforms to immutable data structure

**Consequences**:
- ✅ Type-safe DSL
- ✅ Compiler-verified structure
- ✅ Extensible via custom operations
- ⚠️ Requires understanding of CEs
- ⚠️ Some verbosity (e.g., `to'` instead of `to`)

---

## ADR-002: Discriminated Unions for Match Results

**Status**: Accepted

**Context**: Matchers need to return success/failure information with rich context for error reporting.

**Decision**: Use a `MatchResult` discriminated union with `Pass` and `Fail` cases.

**Alternatives Considered**:
1. Boolean returns (like RSpec)
2. Exception-only approach
3. `Result<unit, string>`
4. Custom class hierarchy

**Rationale**:
- Explicit modeling of all possible outcomes
- Rich failure data (message, expected, actual)
- Pattern matching support
- Single source of truth
- Impossible to forget failure message

**Consequences**:
- ✅ Type-safe results
- ✅ Rich error information
- ✅ Composable matchers
- ✅ Testable matcher logic
- ⚠️ Slightly more verbose than boolean

---

## ADR-003: Separate Declaration from Execution

**Status**: Accepted

**Context**: Tests need to be discovered, filtered, and executed in a flexible manner.

**Decision**: Build an immutable `TestNode` tree during declaration, execute it separately.

**Alternatives Considered**:
1. Immediate execution (like some test frameworks)
2. Lazy sequences
3. Continuation-based approach

**Rationale**:
- Enables test filtering before execution
- Supports multiple formatters
- Allows future parallel execution
- Clear separation of concerns
- Enables metaprogramming

**Consequences**:
- ✅ Flexible execution strategies
- ✅ Easy to implement filtering
- ✅ Multiple output formats
- ✅ Future parallelization
- ⚠️ Two-phase model (more complex)

---

## ADR-004: Scope Stack for State Management

**Status**: Accepted

**Context**: Need to implement RSpec's lexical scoping for `let`, `subject`, and hooks.

**Decision**: Use a stack of execution scopes during test execution.

**Alternatives Considered**:
1. Global mutable state
2. Reader monad
3. Implicit parameters
4. Dynamic scoping

**Rationale**:
- Correctly models RSpec's semantics
- Clear execution model
- Supports nested contexts
- Enables lazy evaluation and memoization
- Hooks execute in correct order

**Consequences**:
- ✅ Correct RSpec-like scoping
- ✅ Lazy, memoized `let'` bindings
- ✅ Proper hook ordering
- ⚠️ Stateful execution (managed carefully)
- ⚠️ Requires type casting for `let'` retrieval

---

## ADR-005: Reflection for Test Discovery

**Status**: Accepted

**Context**: Need to discover tests in compiled assemblies.

**Decision**: Use reflection to find `TestNode` values marked with `[<Tests>]` attribute.

**Alternatives Considered**:
1. Manual registration
2. Source generators
3. Compile-time code generation
4. Convention-based discovery

**Rationale**:
- Simple implementation
- Minimal user ceremony
- Standard .NET approach
- One-time cost (acceptable performance)
- Works with existing tooling

**Consequences**:
- ✅ Simple to implement
- ✅ Minimal user code
- ✅ Standard .NET pattern
- ⚠️ Reflection overhead (mitigated by caching)
- ⚠️ Runtime discovery only

---

## ADR-006: Pure F# Implementation (No NUnit/xUnit)

**Status**: Accepted

**Context**: Should we build on top of existing .NET test frameworks or create our own?

**Decision**: Build a completely independent framework with custom discovery and execution.

**Alternatives Considered**:
1. Build on top of xUnit
2. Build on top of NUnit
3. Use MSTest infrastructure
4. Hybrid approach

**Rationale**:
- Full control over execution model
- No impedance mismatch with existing frameworks
- Custom scope stack implementation
- Better error messages
- True BDD experience

**Consequences**:
- ✅ Complete control
- ✅ Custom execution semantics
- ✅ Better BDD experience
- ⚠️ More implementation work
- ⚠️ No existing IDE integration (initially)

---

## ADR-007: Spectre.Console for Output

**Status**: Accepted

**Context**: Need beautiful, informative console output.

**Decision**: Use Spectre.Console library for formatting.

**Alternatives Considered**:
1. Plain console output with ANSI codes
2. Custom formatting library
3. Colorful.Console
4. Crayon

**Rationale**:
- Rich API for colors, tables, trees
- Cross-platform support
- Active maintenance
- Great documentation
- Modern, beautiful output

**Consequences**:
- ✅ Beautiful output
- ✅ Rich formatting options
- ✅ Cross-platform
- ⚠️ External dependency
- ⚠️ Learning curve for contributors

---

## ADR-008: Tick Suffix for F# Keyword Conflicts

**Status**: Accepted

**Context**: Some DSL keywords conflict with F# keywords (`to`, `let`).

**Decision**: Use tick suffix (`to'`, `let'`) to avoid conflicts.

**Alternatives Considered**:
1. Different names entirely (`expect`, `bind`)
2. Backticks (`` `to` ``)
3. Underscores (`to_`, `let_`)
4. Capitalization (`To`, `Let`)

**Rationale**:
- Minimal visual difference
- Common F# convention
- Clear intent
- Doesn't break readability

**Consequences**:
- ✅ Avoids keyword conflicts
- ✅ F# convention
- ⚠️ Slightly different from RSpec
- ⚠️ Requires explanation in docs

---

## ADR-009: Lazy Evaluation for `let'` Bindings

**Status**: Accepted

**Context**: `let` in RSpec is lazy and memoized per test.

**Decision**: Implement `let'` as lazy factory functions with per-test memoization.

**Alternatives Considered**:
1. Eager evaluation
2. Lazy without memoization
3. Explicit caching API

**Rationale**:
- Matches RSpec semantics
- Efficient (only compute when needed)
- Memoization prevents repeated expensive operations
- Clear execution model

**Consequences**:
- ✅ RSpec-compatible behavior
- ✅ Efficient execution
- ✅ Predictable caching
- ⚠️ Requires scope stack management
- ⚠️ Type casting needed for retrieval

---

## ADR-010: Target .NET 8.0+

**Status**: Accepted

**Context**: Which .NET version should we target?

**Decision**: Target .NET 8.0+ for initial release.

**Alternatives Considered**:
1. .NET Standard 2.1 (wider compatibility)
2. .NET 6.0 (LTS)
3. .NET 9.0 (latest)

**Rationale**:
- Latest F# language features
- Long-term support (LTS)
- Modern runtime performance
- Good balance of features and adoption

**Consequences**:
- ✅ Latest F# features
- ✅ Best performance
- ✅ LTS support
- ⚠️ Excludes older .NET versions
- ⚠️ May need .NET Standard later

---

## Future Decisions to Make

### ADR-011: Parallel Execution Strategy (TBD)
- Task-based parallelism?
- Thread pool?
- Async workflows?

### ADR-012: Mocking Library Integration (TBD)
- Build custom mocking?
- Integrate with Foq?
- Use object expressions?

### ADR-013: Property-Based Testing Integration (TBD)
- Integrate FsCheck?
- Custom implementation?
- Separate package?

### ADR-014: IDE Integration Approach (TBD)
- VS Code extension?
- Visual Studio plugin?
- Language Server Protocol?

