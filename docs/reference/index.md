# Reference Documentation

Complete API reference for FxSpec.

---

## Quick Links

<div class="grid cards" markdown>

-   :material-code-braces:{ .lg } **[DSL API](dsl-api.md)**

    ---

    Complete reference for `spec`, `describe`, `it`, `context`, and all DSL functions.

-   :material-check-circle:{ .lg } **[Matchers](matchers/core.md)**

    ---

    All available matchers organized by category.

</div>

---

## API Categories

### Core DSL

- **[spec](dsl-api.md#spec)** - Computation expression builder for test trees
- **[describe](dsl-api.md#describe)** - Group related tests
- **[context](dsl-api.md#context)** - Add context to test groups (alias for `describe`)
- **[it](dsl-api.md#it)** - Define individual test cases

### Focus & Pending

- **[fit](dsl-api.md#fit)** - Focus on specific test
- **[fdescribe](dsl-api.md#fdescribe)** - Focus on test group
- **[xit](dsl-api.md#xit)** - Skip a test
- **[pending](dsl-api.md#pending)** - Mark test as pending (alias for `xit`)

### Hooks

- **[beforeEach](dsl-api.md#beforeeach)** - Run before each test
- **[afterEach](dsl-api.md#aftereach)** - Run after each test
- **[beforeAll](dsl-api.md#beforeall)** - Run once before all tests in group
- **[afterAll](dsl-api.md#afterall)** - Run once after all tests in group

### Assertions

- **[expect](matchers/core.md#expect)** - Start an assertion
- **[to'](matchers/core.md#to)** - Apply a matcher (positive assertion)
- **[notTo'](matchers/core.md#notto)** - Apply a matcher (negative assertion)

---

## Matcher Categories

### [Core Matchers](matchers/core.md)

Basic equality, null checks, boolean, and option matchers.

### [Collection Matchers](matchers/collections.md)

Matchers for lists, arrays, sequences, and other collections.

### [String Matchers](matchers/strings.md)

String-specific matchers for prefixes, suffixes, patterns, and more.

### [Numeric Matchers](matchers/numeric.md)

Numeric comparisons and range checks.

### [Exception Matchers](matchers/exceptions.md)

Testing exception throwing behavior.
