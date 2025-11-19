# Troubleshooting

Common issues and solutions when using FX.Spec.

---

## Test Discovery Issues

### "No tests found in assembly"

**Symptoms:**

```
FX.Spec Test Runner
==================

No tests found in assembly
```

**Causes & Solutions:**

#### Missing `[<Tests>]` Attribute

**Problem:** Test module doesn't have the `[<Tests>]` attribute.

```fsharp
//  Wrong - no attribute
let mySpecs =
    spec {
        yield describe "Feature" [
            it "test" (fun () -> ...)
        ]
    }
```

**Solution:** Add the `[<Tests>]` attribute:

```fsharp
//  Correct
[<Tests>]
let mySpecs =
    spec {
        yield describe "Feature" [
            it "test" (fun () -> ...)
        ]
    }
```

#### Wrong Return Type

**Problem:** Test member doesn't return `TestNode list`.

```fsharp
//  Wrong - returns unit
[<Tests>]
let mySpecs () =
    spec {
        yield describe "Feature" [
            it "test" (fun () -> ...)
        ]
    }
```

**Solution:** Return `TestNode list`:

```fsharp
//  Correct
[<Tests>]
let mySpecs =
    spec {
        yield describe "Feature" [
            it "test" (fun () -> ...)
        ]
    }
```

#### Non-Static Member

**Problem:** Test member is not static.

```fsharp
//  Wrong - instance member
type MyTests() =
    [<Tests>]
    member this.Specs =
        spec { ... }
```

**Solution:** Make it static:

```fsharp
//  Correct
type MyTests =
    [<Tests>]
    static member Specs =
        spec { ... }

// Or use module-level let binding
module MyTests

[<Tests>]
let specs =
    spec { ... }
```

---

## Compilation Errors

### "The value or constructor 'spec' is not defined"

**Problem:** Missing `open FX.Spec.Core`.

**Solution:**

```fsharp
open FX.Spec.Core  // ← Add this
open FX.Spec.Matchers

[<Tests>]
let mySpecs = spec { ... }
```

---

### "The value or constructor 'expect' is not defined"

**Problem:** Missing `open FX.Spec.Matchers`.

**Solution:**

```fsharp
open FX.Spec.Core
open FX.Spec.Matchers  // ← Add this

[<Tests>]
let mySpecs =
    spec {
        yield describe "Feature" [
            it "test" (fun () ->
                expect true |> should beTrue
            )
        ]
    }
```

---

### "This expression was expected to have type 'TestNode list' but here has type 'TestNode'"

**Problem:** Missing `yield` keyword in spec block.

```fsharp
//  Wrong
spec {
    describe "Feature" [  // Missing yield
        it "test" (fun () -> ...)
    ]
}
```

**Solution:** Add `yield`:

```fsharp
//  Correct
spec {
    yield describe "Feature" [
        it "test" (fun () -> ...)
    ]
}
```

---

### "This expression was expected to have type 'unit -> unit' but here has type 'unit'"

**Problem:** Test code not wrapped in a function.

```fsharp
//  Wrong
it "test" (
    expect true |> should beTrue  // Not wrapped
)
```

**Solution:** Wrap in `(fun () -> ...)`:

```fsharp
//  Correct
it "test" (fun () ->
    expect true |> should beTrue
)
```

---

## Matcher Type Errors

### "Type mismatch" with Matchers

**Problem:** Matcher type doesn't match actual value type.

```fsharp
//  Wrong - comparing int to string
expect 42 |> should (equal "42")
```

**Solution:** Ensure types match:

```fsharp
//  Correct
expect 42 |> should (equal 42)
expect "42" |> should (equal "42")
```

---

### "This expression was expected to have type 'Matcher<'a>' but here has type 'MatchResult'"

**Problem:** Calling matcher function instead of passing it.

```fsharp
//  Wrong - calling equal with ()
expect 42 |> should (equal 42 ())
```

**Solution:** Don't call the matcher:

```fsharp
//  Correct
expect 42 |> should (equal 42)
```

---

### "The type 'string' does not support the comparison constraint"

**Problem:** Using numeric matchers on non-comparable types.

```fsharp
//  Wrong - can't compare functions
let f = fun x -> x + 1
expect f |> should (beGreaterThan (fun x -> x))
```

**Solution:** Use appropriate matchers for the type:

```fsharp
//  Correct - use equality for functions
let f = fun x -> x + 1
expect (f 5) |> should (equal 6)
```

---

## Runtime Errors

### "Assembly not found" or "Could not load file or assembly"

**Problem:** Assembly path is incorrect or assembly not built.

**Solution:**

```bash
# 1. Build the test project first
dotnet build tests/MyProject.Tests/MyProject.Tests.fsproj

# 2. Use the correct path to the built assembly
dotnet run --project src/FX.Spec.Runner/FX.Spec.Runner.fsproj -- \
  tests/MyProject.Tests/bin/Debug/net9.0/MyProject.Tests.dll
```

---

### "No tests match the filter"

**Problem:** Filter pattern doesn't match any test descriptions.

**Symptoms:**

```
Filtering tests by: MyTest
Running 0 filtered examples

No tests match the filter
```

**Solutions:**

1. **Check case sensitivity** - filters are case-sensitive:

```bash
#  Wrong
./run-tests.sh --filter "calculator"

#  Correct
./run-tests.sh --filter "Calculator"
```

2. **Use partial matches**:

```bash
#  Too specific
./run-tests.sh --filter "Calculator > addition > adds two numbers"

#  Better
./run-tests.sh --filter "addition"
```

3. **Remove filter to see all test names**:

```bash
./run-tests.sh
```

---

### Tests Run But All Fail with "Object reference not set"

**Problem:** Hooks or test setup not running correctly.

**Common Causes:**

1. **Using mutable variables without initialization**:

```fsharp
//  Wrong
describe "Database" [
    let mutable connection = null  // null reference

    it "queries data" (fun () ->
        connection.Query("SELECT 1")  // NullReferenceException
    )
]
```

**Solution:** Use `beforeEach` to initialize:

```fsharp
//  Correct
describe "Database" [
    let mutable connection = null

    beforeEach (fun () ->
        connection <- Database.connect()
    )

    it "queries data" (fun () ->
        connection.Query("SELECT 1")
    )
]
```

---

## Test Execution Issues

### Tests Pass Individually But Fail When Run Together

**Problem:** Tests are not isolated - they share state.

**Solution:** Use `beforeEach` and `afterEach` for proper isolation:

```fsharp
describe "User Tests" [
    let mutable user = null

    beforeEach (fun () ->
        user <- createUser()  // Fresh user for each test
    )

    afterEach (fun () ->
        deleteUser(user)  // Clean up after each test
    )

    it "test 1" (fun () -> ...)
    it "test 2" (fun () -> ...)
]
```

---

### Focused Tests Not Running

**Problem:** Using `fit` or `fdescribe` but all tests still run.

**Cause:** Focused filtering only works when tests are discovered together.

**Solution:** Ensure all tests are in the same assembly and discovered together:

```fsharp
//  Correct - both in same spec
[<Tests>]
let specs =
    spec {
        yield describe "Suite" [
            fit "only this runs" (fun () -> ...)
            it "this is skipped" (fun () -> ...)
        ]
    }
```

---

### Pending Tests Still Running

**Problem:** Using `xit` but test still executes.

**Cause:** Typo in function name.

```fsharp
//  Wrong - typo
xIt "test" (fun () -> ...)  // Capital I
```

**Solution:** Use correct function name:

```fsharp
//  Correct
xit "test" (fun () -> ...)  // lowercase i
```

---

## Performance Issues

### Tests Running Slowly

**Causes & Solutions:**

#### Using `beforeEach` for Expensive Setup

**Problem:** Expensive setup runs before every test.

```fsharp
//  Slow - database created for each test
beforeEach (fun () ->
    createDatabase()  // Expensive!
)
```

**Solution:** Use `beforeAll` for expensive setup:

```fsharp
//  Fast - database created once
beforeAll (fun () ->
    createDatabase()
)

beforeEach (fun () ->
    clearData()  // Fast cleanup
)
```

#### Not Cleaning Up Resources

**Problem:** Resources accumulate during test run.

**Solution:** Use `afterEach` or `afterAll`:

```fsharp
describe "File Tests" [
    let mutable tempFile = ""

    beforeEach (fun () ->
        tempFile <- Path.GetTempFileName()
    )

    afterEach (fun () ->
        File.Delete(tempFile)  // Clean up
    )

    it "test" (fun () -> ...)
]
```

---

## IDE Integration Issues

### Tests Not Discovered in IDE

**Problem:** FX.Spec doesn't integrate with standard .NET test explorers.

**Explanation:** FX.Spec uses its own test runner, not the standard .NET test framework.

**Workaround:** Run tests from command line:

```bash
./run-tests.sh
```

**Future:** IDE integration is planned for future releases.

---

### Syntax Highlighting Issues

**Problem:** F# syntax highlighting doesn't work well with FX.Spec DSL.

**Solution:** This is a limitation of current F# tooling. The code is valid F# even if highlighting is imperfect.

---

## Getting Help

If you encounter an issue not covered here:

1. **Check the documentation**:
   - [Quick Start Guide](quick-start.md)
   - [DSL API Reference](reference/dsl-api.md)
   - [Test Runner](reference/runner.md)

2. **Search existing issues**: [GitHub Issues](https://github.com/dawsons-creek/fx-spec/issues)

3. **Ask for help**:
   - Open a new issue with:
     - FX.Spec version
     - .NET version
     - Minimal reproduction code
     - Error messages
     - What you've tried

4. **Contribute**: Found a bug? [Submit a PR](community/contributing.md)!

---

## Common Gotchas

### 1. Forgetting `yield`

```fsharp
//  Won't compile
spec {
    describe "Test" [ ... ]
}

//  Correct
spec {
    yield describe "Test" [ ... ]
}
```

### 2. Not Wrapping Test Code in Function

```fsharp
//  Won't compile
it "test" (expect true |> should beTrue)

//  Correct
it "test" (fun () -> expect true |> should beTrue)
```

### 3. Using `{}` Instead of `[]`

```fsharp
//  Wrong syntax
describe "Test" {
    it "test" { ... }
}

//  Correct syntax
describe "Test" [
    it "test" (fun () -> ...)
]
```

### 4. Forgetting `[<Tests>]` Attribute

```fsharp
//  Tests won't be discovered
let mySpecs = spec { ... }

//  Tests will be discovered
[<Tests>]
let mySpecs = spec { ... }
```

### 5. Wrong `open` Statements

```fsharp
//  Missing opens
let specs = spec { ... }  // Error: 'spec' not defined

//  Correct opens
open FX.Spec.Core
open FX.Spec.Matchers

let specs = spec { ... }
```

---

## See Also

- [Quick Start Guide](quick-start.md) - Getting started
- [DSL API Reference](reference/dsl-api.md) - Complete DSL documentation
- [Test Runner](reference/runner.md) - Runner options and usage
- [Contributing](community/contributing.md) - How to contribute

