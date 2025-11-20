# Reference Documentation

Complete API reference for FX.Spec.

---

## Quick Links

<div class="grid cards" markdown>

-   :material-code-braces:{ .lg } **[DSL API](dsl-api.md)**

    ---

    Complete reference for `describe`, `it`, `context`, and all DSL functions.

-   :material-check-circle:{ .lg } **[Matchers](matchers/core.md)**

    ---

    All available matchers organized by category.

</div>

---

## API Categories

### Core DSL

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

### Expectations

- **[expect](matchers/core.md#expect)** - Create a generic expectation
- **[expectBool](matchers/core.md#expectbool)** - Create a boolean expectation
- **[expectOption](matchers/core.md#expectoption)** - Create an Option expectation
- **[expectResult](matchers/core.md#expectresult)** - Create a Result expectation
- **[expectSeq](matchers/collections.md)** - Create a collection expectation
- **[expectStr](matchers/strings.md)** - Create a string expectation
- **[expectNum](matchers/numeric.md)** - Create a numeric expectation
- **[expectInt](matchers/numeric.md)** - Create an integer expectation
- **[expectFloat](matchers/numeric.md)** - Create a float expectation
- **[expectThrows](matchers/exceptions.md)** - Assert an exception is thrown
- **[expectHttp](http.md)** - Create an HTTP response expectation
- **[expectJson](json.md)** - Create a JSON expectation
- **[expectJsonApi](json-api.md)** - Create a JSON:API expectation

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

### [HTTP Matchers](http.md)

Testing HTTP responses (status, headers, body).

### [JSON Matchers](json.md)

Testing JSON content with path-based navigation.

### [JSON:API Matchers](json-api.md)

Testing JSON:API documents (resources, relationships, includes).

---

## Integration Testing

### [ASP.NET Core](aspnetcore.md)

Helpers for testing ASP.NET Core handlers and middleware with in-memory contexts.
