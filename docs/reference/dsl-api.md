# DSL API Reference

Complete reference for FxSpec's Domain-Specific Language functions.

---

## Overview

FxSpec uses a clean, functional DSL to build test trees. The DSL provides functions for organizing tests, defining test cases, and managing test lifecycle with hooks.

---

## Core Functions

### Test Structure

FxSpec tests are simple values marked with the `[<Tests>]` attribute. No wrapper is needed.

**Usage:**

```fsharp
[<Tests>]
let myTests =
    describe "Feature" [
        it "works" (fun () ->
            expectBool(true).toBeTrue()
        )
    ]
```

**Example:**

```fsharp
[<Tests>]
let myTests =
    describe "Feature" [
        it "works" (fun () ->
            expectBool(true).toBeTrue()
        )
    ]
```

**Notes:**

- Tests are immutable tree structures built at declaration time
- Tests are not executed during building, only when the runner executes them
- Mark test functions with `[<Tests>]` attribute for discovery
- Test discovery works with both `TestNode` and `TestNode list` types

---

### describe

**Type:** `string -> TestNode list -> TestNode`

Groups related tests together with a description.

**Parameters:**

- `description` - String describing the group
- `tests` - List of child test nodes

**Usage:**

```fsharp
describe "description" [
    // child tests
]
```

**Example:**

```fsharp
describe "Calculator" [
    describe "addition" [
        it "adds positive numbers" (fun () ->
            expect(2 + 3).toEqual(5)
        )
    ]

    describe "subtraction" [
        it "subtracts numbers" (fun () ->
            expect(5 - 3).toEqual(2)
        )
    ]
]
```

**Output:**

```
Calculator
  addition
    ✓ adds positive numbers
  subtraction
    ✓ subtracts numbers
```

**Notes:**

- `describe` blocks can be nested arbitrarily deep
- Use `describe` for grouping by feature, class, or module
- Descriptions should be clear and descriptive

---

### context

**Type:** `string -> TestNode list -> TestNode`

Alias for `describe` that reads better when describing context or state.

**Parameters:**

- `description` - String describing the context
- `tests` - List of child test nodes

**Usage:**

```fsharp
context "when something is true" [
    // tests in this context
]
```

**Example:**

```fsharp
describe "Stack" [
    context "when empty" [
        it "has zero count" (fun () ->
            let stack = Stack<int>()
            expect(stack.Count).toEqual(0)
        )
    ]

    context "when not empty" [
        it "has non-zero count" (fun () ->
            let stack = Stack<int>()
            stack.Push(1)
            expect(stack.Count).toEqual(1)
        )
    ]
]
```

**Notes:**

- Functionally identical to `describe`
- Use `context` for "when" or "with" scenarios
- Improves readability in BDD-style tests

---

### it

**Type:** `string -> (unit -> unit) -> TestNode`

Defines an individual test case.

**Parameters:**

- `description` - String describing what the test does
- `test` - Function that performs assertions

**Usage:**

```fsharp
it "description" (fun () ->
    // test code and assertions
)
```

**Example:**

```fsharp
it "adds two numbers" (fun () ->
    let result = Calculator.add 2 3
    expect(result).toEqual(5)
)
```

**Notes:**

- Test function must be wrapped in `fun () ->` for lazy evaluation
- Descriptions should start with a verb (e.g., "returns", "throws", "creates")
- Tests should focus on one behavior
- If the test function throws an exception, the test fails

---

## Focus & Pending

### fit

**Type:** `string -> (unit -> unit) -> TestNode`

Focused test - runs only this test when focused tests exist.

**Parameters:**

- `description` - String describing the test
- `test` - Function that performs assertions

**Usage:**

```fsharp
fit "only run this test" (fun () ->
    expectBool(true).toBeTrue()
)
```

**Example:**

```fsharp
describe "My Suite" [
    fit "work on this test" (fun () ->  // Only this runs
        expect(2 + 2).toEqual(4)
    )

    it "this is skipped" (fun () ->    // Skipped
        expectBool(true).toBeTrue()
    )
]
```

**Output:**

```
My Suite
  ✓ work on this test
  ⊘ this is skipped (not focused)
```

**Notes:**

- Use during development to focus on specific tests
- When any `fit` or `fdescribe` exists, unfocused tests are skipped
- Remove all `fit` before committing code
- Multiple `fit` tests can exist - all focused tests run

---

### fdescribe

**Type:** `string -> TestNode list -> TestNode`

Focused group - runs only tests in focused groups.

**Parameters:**

- `description` - String describing the group
- `tests` - List of child test nodes

**Usage:**

```fsharp
fdescribe "only run this group" [
    // all tests in this group run
]
```

**Example:**

```fsharp
spec {
    fdescribe "Work on Calculator" [  // This group runs
        it "test 1" (fun () ->
            expectBool(true).toBeTrue()
        )
        it "test 2" (fun () ->
            expectBool(true).toBeTrue()
        )
    ]

    describe "Other Feature" [        // This group is skipped
        it "test 3" (fun () ->
            expectBool(true).toBeTrue()
        )
    ]
}
```

**Notes:**

- All tests within `fdescribe` run
- Tests outside `fdescribe` are skipped
- Can be nested - inner `fdescribe` focuses further
- Remove before committing

---

### xit

**Type:** `string -> (unit -> unit) -> TestNode`

Excluded test - skips this test.

**Parameters:**

- `description` - String describing the test
- `test` - Function (not executed)

**Usage:**

```fsharp
xit "not ready yet" (fun () ->
    // this code doesn't run
)
```

**Example:**

```fsharp
describe "Feature" [
        it "working test" (fun () ->
            expectBool(true).toBeTrue()
        )

        xit "broken test" (fun () ->  // Skipped
            expectBool(false).toBeTrue()
        )
    ]
```

**Output:**

```
Feature
  ✓ working test
  ⊘ broken test
```

**Notes:**

- Use for temporarily disabling broken tests
- Test function is never executed
- Skipped tests are reported in the summary
- Better than commenting out tests (maintains test count)

---

### pending

**Type:** `string -> (unit -> unit) -> TestNode`

Alias for `xit` that reads better for unfinished tests.

**Parameters:**

- `description` - String describing the test
- `test` - Function (not executed)

**Usage:**

```fsharp
pending "TODO: implement this test" (fun () ->
    ()
)
```

**Example:**

```fsharp
describe "Feature" [
        it "implemented test" (fun () ->
            expectBool(true).toBeTrue()
        )

        pending "write test for edge case" (fun () ->
            ()
        )
    ]
```

**Notes:**

- Functionally identical to `xit`
- Use for planned but unimplemented tests
- Can pass empty function `(fun () -> ())`

---

## Lifecycle Hooks

### beforeEach

**Type:** `(unit -> unit) -> TestNode`

Runs before each test in the current group.

**Parameters:**

- `action` - Function to run before each test

**Usage:**

```fsharp
beforeEach (fun () ->
    // setup code
)
```

**Example:**

```fsharp
describe "Database Tests" [
        let mutable connection = null

        beforeEach (fun () ->
            connection <- Database.connect()
            connection.BeginTransaction()
        )

        afterEach (fun () ->
            connection.RollbackTransaction()
            connection.Dispose()
        )

        it "queries data" (fun () ->
            let result = connection.Query("SELECT 1")
            expect result |> shouldNot beEmpty
        )

        it "inserts data" (fun () ->
            connection.Execute("INSERT INTO users VALUES (1, 'test')")
            let count = connection.Query("SELECT COUNT(*) FROM users")
            expect(count).toEqual(1)
        )
    ]
```

**Execution Order:**

```
beforeEach → test 1 → afterEach
beforeEach → test 2 → afterEach
```

**Notes:**

- Runs before **each** test in the group
- Useful for test isolation and setup
- Can access mutable variables from outer scope
- Multiple `beforeEach` hooks run in order of definition

---

### afterEach

**Type:** `(unit -> unit) -> TestNode`

Runs after each test in the current group.

**Parameters:**

- `action` - Function to run after each test

**Usage:**

```fsharp
afterEach (fun () ->
    // cleanup code
)
```

**Example:**

```fsharp
describe "File Tests" [
        let testFile = "test.txt"

        beforeEach (fun () ->
            File.WriteAllText(testFile, "test content")
        )

        afterEach (fun () ->
            if File.Exists(testFile) then
                File.Delete(testFile)
        )

        it "reads file" (fun () ->
            let content = File.ReadAllText(testFile)
            expect(content).toEqual("test content")
        )
    ]
```

**Notes:**

- Runs after **each** test, even if test fails
- Use for cleanup and resource disposal
- Guaranteed to run (unless unhandled exception in beforeEach)

---

### beforeAll

**Type:** `(unit -> unit) -> TestNode`

Runs once before all tests in the group.

**Parameters:**

- `action` - Function to run once before tests

**Usage:**

```fsharp
beforeAll (fun () ->
    // expensive setup
)
```

**Example:**

```fsharp
describe "API Tests" [
        let mutable server = null

        beforeAll (fun () ->
            server <- TestServer.start()
            server.SeedDatabase()
        )

        afterAll (fun () ->
            server.Stop()
        )

        it "test 1" (fun () ->
            let response = server.Get("/api/users")
            expect(response.Status).toEqual(200)
        )

        it "test 2" (fun () ->
            let response = server.Post("/api/users", { Name = "test" })
            expect(response.Status).toEqual(201)
        )
    ]
```

**Execution Order:**

```
beforeAll → test 1 → test 2 → afterAll
```

**Notes:**

- Runs only **once** before all tests in the group
- Use for expensive setup (database seeding, server start, etc.)
- Shared state across tests (be careful of test isolation)

---

### afterAll

**Type:** `(unit -> unit) -> TestNode`

Runs once after all tests in the group.

**Parameters:**

- `action` - Function to run once after all tests

**Usage:**

```fsharp
afterAll (fun () ->
    // cleanup expensive resources
)
```

**Example:**

```fsharp
describe "Integration Tests" [
        let mutable dockerContainer = null

        beforeAll (fun () ->
            dockerContainer <- Docker.startContainer("postgres:15")
        )

        afterAll (fun () ->
            Docker.stopContainer(dockerContainer)
        )

        // tests...
    ]
```

**Notes:**

- Runs only **once** after all tests in the group
- Use for expensive cleanup
- Guaranteed to run even if tests fail

---

## Complete Example

Here's a comprehensive example using all DSL features:

```fsharp
module UserServiceSpecs

open FxSpec.Core
open FxSpec.Matchers

[<Tests>]
let userServiceSpecs =
    spec {
        describe "UserService" [
            let mutable service = null
            let mutable db = null

            beforeAll (fun () ->
                db <- Database.createInMemory()
            )

            afterAll (fun () ->
                db.Dispose()
            )

            beforeEach (fun () ->
                db.Clear()
                service <- UserService(db)
            )

            describe "CreateUser" [
                context "when valid data is provided" [
                    it "creates a new user" (fun () ->
                        let user = service.CreateUser("Alice", "alice@example.com")
                        expect(user.Name).toEqual("Alice")
                        expect(user.Email).toEqual("alice@example.com")
                    )

                    it "assigns a unique ID" (fun () ->
                        let user = service.CreateUser("Bob", "bob@example.com")
                        expect user.Id |> should (beGreaterThan 0)
                    )
                ]

                context "when invalid data is provided" [
                    it "raises exception for empty name" (fun () ->
                        let action () = service.CreateUser("", "test@example.com")
                        expect action |> should raiseException
                    )

                    xit "validates email format" (fun () ->
                        // TODO: implement email validation
                        let action () = service.CreateUser("Test", "invalid-email")
                        expect action |> should raiseException
                    )
                ]
            ]

            describe "GetUser" [
                it "returns existing user" (fun () ->
                    let created = service.CreateUser("Charlie", "charlie@example.com")
                    let retrieved = service.GetUser(created.Id)
                    expect retrieved |> should (beSome created)
                )

                it "returns None for non-existent user" (fun () ->
                    let result = service.GetUser(9999)
                    expect result |> should beNone
                )
            ]
        ]
    }
```

---

## Best Practices

### Naming Conventions

```fsharp
// Good
describe "UserService"
it "creates a new user"
context "when user is authenticated"

// Avoid
describe "Test Suite 1"
it "test1"
context "scenario A"
```

### Test Organization

```fsharp
describe "FeatureName" [           // Top-level: feature/module/class
        describe "MethodOrFunction" [   // Second-level: method/function
            context "when condition" [  // Third-level: specific context
                it "expected behavior" (fun () -> ...)
            ]
        ]
    ]
```

### Hook Usage

- Use `beforeEach`/`afterEach` for test isolation
- Use `beforeAll`/`afterAll` for expensive setup
- Keep hooks simple and focused
- Avoid complex logic in hooks

### Focus & Pending

- Use `fit`/`fdescribe` temporarily during development
- Always remove before committing
- Use `xit`/`pending` to track incomplete tests
- Document why tests are pending

---

## See Also

- **[Quick Start](../quick-start.md)** - Get started with FxSpec
- **[Matchers Reference](matchers/core.md)** - Learn about assertions
- **[Contributing](../community/contributing.md)** - Contribute to FxSpec
