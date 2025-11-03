# DSL API Reference

Complete reference for FxSpec's Domain-Specific Language functions.

---

## Overview

FxSpec uses a clean, functional DSL to build specification trees. The DSL provides functions for organizing specifications, defining behavioral requirements, and managing test lifecycle with hooks.

Think of your tests as **living documentation** - they describe what your system does, not just verify it works.

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

Defines an individual specification that describes a single behavior.

**Parameters:**

- `description` - String describing the expected behavior (reads like documentation)
- `test` - Function that verifies the behavior

**Usage:**

```fsharp
it "description of expected behavior" (fun () ->
    // verification code
)
```

**Example:**

```fsharp
it "calculates the sum of two positive integers" (fun () ->
    let result = Calculator.add 2 3
    expect(result).toEqual(5)
)

it "raises an exception when input is invalid" (fun () ->
    expectThrows<ArgumentException>(fun () ->
        Calculator.divide 10 0 |> ignore
    )
)
```

**Writing Good Specifications:**

- Describe **what** the code does, not **how** it does it
- Write complete sentences: "returns X when Y" not just "test add"
- Focus on one behavior per specification
- Use business/domain language, not technical jargon

**Notes:**

- Test function must be wrapped in `fun () ->` for lazy evaluation
- Descriptions should start with a verb (e.g., "returns", "throws", "creates", "validates")
- If the test function throws an exception, the test fails

---

### itAsync

**Type:** `string -> Async<unit> -> TestNode`

Defines an asynchronous test case that runs in an async workflow.

**Parameters:**

- `description` - String describing what the test does
- `test` - Async computation that performs assertions

**Usage:**

```fsharp
itAsync "description" (async {
    // async test code
})
```

**Example:**

```fsharp
itAsync "fetches user data" (async {
    let! user = getUserAsync 123
    expectOption(user).toBeSome()
})
```

**With Task Interop:**

```fsharp
itAsync "calls HTTP API" (async {
    use client = new HttpClient()
    let! response = client.GetAsync("https://api.example.com/users") |> Async.AwaitTask
    expectHttp(response).toHaveStatusOk()
})
```

**With Result Types:**

```fsharp
itAsync "validates async operation" (async {
    let! result = processDataAsync(data)
    expectResult(result).toBeOk()
})
```

**Notes:**

- Use for tests that perform I/O operations (database, HTTP, file system)
- Test runs using `Async.RunSynchronously` internally
- Can use `let!` to await async operations
- Can use `Async.AwaitTask` to work with .NET Task-based APIs
- Compatible with all FxSpec matchers and hooks
- Can mix `it` and `itAsync` in the same test suite

---

## Async Testing Patterns

### HTTP API Testing

```fsharp
open System.Net.Http
open FxSpec.Http

describe "User API" [
    let client = new HttpClient(BaseAddress = Uri("http://localhost:5000"))
    
    itAsync "creates user successfully" (async {
        let json = """{"name":"John","email":"john@example.com"}"""
        let content = new StringContent(json, Encoding.UTF8, "application/json")
        let! response = client.PostAsync("/api/users", content) |> Async.AwaitTask
        
        expectHttp(response).toHaveStatusCreated()
        expectHttp(response).toHaveJsonBody({| id = 1; name = "John" |})
    })
    
    itAsync "retrieves user by ID" (async {
        let! response = client.GetAsync("/api/users/1") |> Async.AwaitTask
        expectHttp(response).toHaveStatusOk()
    })
]
```

### Database Operations

```fsharp
describe "User Repository" [
    let connectionString = "Server=localhost;Database=test"
    
    itAsync "saves user to database" (async {
        use! connection = openConnectionAsync(connectionString)
        let! result = repository.SaveAsync(connection, newUser)
        expectResult(result).toBeOk()
    })
    
    itAsync "retrieves user from database" (async {
        use! connection = openConnectionAsync(connectionString)
        let! user = repository.GetAsync(connection, userId)
        expectOption(user).toBeSome()
    })
]
```

### Async Result Patterns

```fsharp
describe "Async Result Workflows" [
    itAsync "handles successful async operation" (async {
        let! result = fetchDataAsync(validId)
        expectResult(result).toBeOk()
    })
    
    itAsync "handles async errors" (async {
        let! result = fetchDataAsync(invalidId)
        expectResult(result).toBeError()
    })
    
    itAsync "chains async Result operations" (async {
        let! result = 
            validateInputAsync(data)
            |> AsyncResult.bind processDataAsync
            |> AsyncResult.bind saveToDbAsync
        
        expectResult(result).toBeOk()
    })
]
```

### Parallel Async Operations

```fsharp
describe "Parallel Operations" [
    itAsync "runs multiple operations in parallel" (async {
        let! results = 
            [1..10]
            |> List.map (fun id -> fetchUserAsync id)
            |> Async.Parallel
        
        expectSeq(results).toHaveLength(10)
    })
    
    itAsync "handles parallel failures gracefully" (async {
        let operations = [
            fetchUserAsync 1
            fetchUserAsync 999  // This will fail
            fetchUserAsync 3
        ]
        
        let! results = Async.Parallel operations
        expectSeq(results |> Array.filter Result.isOk).toHaveLength(2)
    })
]
```

### File I/O

```fsharp
describe "File Operations" [
    itAsync "reads file asynchronously" (async {
        let! content = File.ReadAllTextAsync("test.txt") |> Async.AwaitTask
        expectStr(content).toContain("expected text")
    })
    
    itAsync "writes file asynchronously" (async {
        let! _ = File.WriteAllTextAsync("output.txt", "test") |> Async.AwaitTask
        let! content = File.ReadAllTextAsync("output.txt") |> Async.AwaitTask
        expectStr(content).toEqual("test")
    })
]
```

### Async Hooks

Hooks can be async-aware when working with async tests:

```fsharp
describe "Integration Tests" [
    let mutable client = Unchecked.defaultof<HttpClient>
    
    beforeEach (fun () ->
        client <- new HttpClient(BaseAddress = Uri("http://localhost:5000"))
    )
    
    afterEach (fun () ->
        client.Dispose()
    )
    
    itAsync "test 1" (async {
        let! response = client.GetAsync("/health") |> Async.AwaitTask
        expectHttp(response).toHaveStatusOk()
    })
    
    itAsync "test 2" (async {
        let! response = client.GetAsync("/api/users") |> Async.AwaitTask
        expectHttp(response).toHaveStatusOk()
    })
]
```

**Notes on Async Testing:**
- `itAsync` internally uses `Async.RunSynchronously`, so tests still run synchronously at the top level
- Use `Async.AwaitTask` to convert .NET Tasks to F# Async
- Async tests can be focused with `fitAsync` or skipped with `xitAsync`
- Async tests work with all lifecycle hooks (`beforeEach`, `afterEach`, etc.)
- Mix sync and async tests freely in the same suite

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

### xitAsync

**Type:** `string -> Async<unit> -> TestNode`

Excluded async test - skips this async test.

**Parameters:**

- `description` - String describing the test
- `test` - Async computation (not executed)

**Usage:**

```fsharp
xitAsync "not ready yet" (async {
    // this code doesn't run
})
```

**Example:**

```fsharp
describe "API Tests" [
    itAsync "working test" (async {
        let! response = client.GetAsync("/api/users") |> Async.AwaitTask
        expectHttp(response).toHaveStatusOk()
    })
    
    xitAsync "broken async test" (async {  // Skipped
        let! data = failingOperationAsync()
        expectResult(data).toBeOk()
    })
]
```

**Notes:**

- Same behavior as `xit` but for async tests
- Use for temporarily disabling broken async tests
- Test computation is never executed

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

Here's a comprehensive example using all DSL features with specification-focused descriptions:

```fsharp
module UserServiceSpecs

open FxSpec.Core
open FxSpec.Matchers

[<Tests>]
let userServiceSpecs =
    describe "UserService user management" [
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

        context "when creating a new user account" [
            context "with valid registration data" [
                it "creates a user account with the provided name and email" (fun () ->
                    let user = service.CreateUser("Alice", "alice@example.com")
                    expect(user.Name).toEqual("Alice")
                    expect(user.Email).toEqual("alice@example.com")
                )

                it "assigns a unique identifier to the new user" (fun () ->
                    let user = service.CreateUser("Bob", "bob@example.com")
                    expectNum(user.Id).toBeGreaterThan(0)
                )
            ]

            context "with invalid registration data" [
                it "rejects registration when name is empty" (fun () ->
                    expectThrows<ArgumentException>(fun () ->
                        service.CreateUser("", "test@example.com") |> ignore
                    )
                )

                xit "validates email format before accepting registration" (fun () ->
                    // TODO: implement email validation
                    expectThrows<ArgumentException>(fun () ->
                        service.CreateUser("Test", "invalid-email") |> ignore
                    )
                )
            ]
        ]

        context "when retrieving user information" [
            it "returns the user account when ID exists in system" (fun () ->
                let created = service.CreateUser("Charlie", "charlie@example.com")
                let retrieved = service.GetUser(created.Id)
                expectOption(retrieved).toBeSome(created)
            )

            it "returns None when requested user ID does not exist" (fun () ->
                let result = service.GetUser(9999)
                expectOption(result).toBeNone()
            )
        ]
    ]
```

---

## Best Practices

### Writing Specifications as Documentation

```fsharp
// Good - Reads like documentation
describe "User authentication system"
it "allows login with valid credentials"
it "locks account after three failed login attempts"
context "when password has expired"

// Avoid - Too technical or vague
describe "AuthService tests"
it "test_login"
it "edge case 1"
context "scenario A"
```

**Tips for good specifications:**
- Use business domain language
- Write what the system does, not what the code does
- Complete sentences that non-developers can understand
- Focus on behavior, not implementation

### Organizing Specifications

```fsharp
describe "Feature or Component Name" [     // Top: What you're documenting
    context "under specific conditions" [  // Middle: When/where
        it "expected observable behavior" (fun () -> ...) // Bottom: What happens
    ]
]
```

**Example hierarchy:**
```fsharp
describe "Shopping cart" [
    context "when cart is empty" [
        it "displays a message prompting user to add items" (fun () -> ...)
        it "disables the checkout button" (fun () -> ...)
    ]

    context "when cart contains items" [
        it "calculates the correct total including tax" (fun () -> ...)
        it "enables the checkout button" (fun () -> ...)
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
