# Test Runner

The FX.Spec test runner discovers and executes tests from compiled assemblies.

---

## Basic Usage

```bash
dotnet run --project src/FX.Spec.Runner/FX.Spec.Runner.fsproj -- <assembly-path> [options]
```

Or use the convenience script:

```bash
./run-tests.sh [options]
```

---

## Command-Line Options

### Assembly Path

**Required.** Path to the compiled test assembly (`.dll` file).

```bash
dotnet run --project src/FX.Spec.Runner/FX.Spec.Runner.fsproj -- tests/MyTests.dll
```

---

### `--filter`, `-f`

Filter tests by description (case-sensitive substring match).

**Usage:**

```bash
--filter <pattern>
-f <pattern>
```

**Examples:**

```bash
# Run only tests with "Calculator" in the description
./run-tests.sh --filter "Calculator"

# Run only tests with "User" in the description
./run-tests.sh -f "User"

# Filter works on full test path (describe > context > it)
./run-tests.sh --filter "when adding numbers"
```

**How it works:**

- Matches against the full test path: `"SpecBuilder > simple examples > creates a single Example node"`
- Case-sensitive
- Substring match (not regex)
- Filters both groups and individual tests

---

### `--format`

Choose output format.

**Usage:**

```bash
--format <format>
```

**Formats:**

- `documentation` (default) - Rich Spectre.Console output with colors, tables, and panels
- `doc` - Alias for `documentation`
- `simple` - Plain text output

**Examples:**

```bash
# Use documentation format (default)
./run-tests.sh --format documentation

# Use simple format
./run-tests.sh --format simple

# Short alias
./run-tests.sh --format doc
```

**Documentation Format:**

```
SpecBuilder
  simple examples
    ✓ creates a single Example node (2ms)
    ✓ creates multiple nodes (1ms)
  nested describe blocks
    ✓ creates nested Group structures (0ms)

┌───────┬────────┬────────┬─────────┬──────────┐
│ Total │ Passed │ Failed │ Skipped │ Duration │
│   52  │   52   │   0    │    0    │  0.15s   │
└───────┴────────┴────────┴─────────┴──────────┘
```

**Error Output with Stack Traces:**

When tests fail due to exceptions in your code, FX.Spec provides rich, actionable error information:

```
  ✗ processes user data   (2ms)

    Calculator > processes user data

    DivideByZeroException: Attempted to divide by zero.

    Stack trace:
      at Calculator.divide(Int32 x, Int32 y)
         in Calculator.fs:42 (Calculator)
      at Calculator.processUserData(User user)
         in Calculator.fs:67 (Calculator)
```

**Features:**
-  Clear exception type and message
-  Filtered stack traces showing only YOUR code (framework internals removed)
- 🔗 Clickable file links (Cmd/Ctrl+Click in supported terminals to jump to the error)
- 📍 Precise file names and line numbers
-  Color-coded for visual clarity

**Simple Format:**

```
SpecBuilder > simple examples > creates a single Example node: PASS
SpecBuilder > simple examples > creates multiple nodes: PASS
SpecBuilder > nested describe blocks > creates nested Group structures: PASS

52 examples, 0 failures
```

---

### `--verbose`, `-v`

Enable verbose output (shows stack traces on errors).

**Usage:**

```bash
--verbose
-v
```

**Example:**

```bash
./run-tests.sh --verbose
./run-tests.sh -v
```

---

### `--help`, `-h`

Show help message and exit.

**Usage:**

```bash
--help
-h
```

---

## Exit Codes

The runner returns different exit codes based on test results:

| Exit Code | Meaning |
|-----------|---------|
| `0` | All tests passed |
| `1` | One or more tests failed |
| `1` | Fatal error (assembly not found, etc.) |

**CI/CD Integration:**

```bash
#!/bin/bash
dotnet build tests/MyProject.Tests/MyProject.Tests.fsproj
dotnet run --project src/FX.Spec.Runner/FX.Spec.Runner.fsproj -- \
  tests/MyProject.Tests/bin/Debug/net9.0/MyProject.Tests.dll

if [ $? -eq 0 ]; then
    echo " All tests passed"
else
    echo " Tests failed"
    exit 1
fi
```

---

## Examples

### Run All Tests

```bash
# Build and run
dotnet build tests/FX.Spec.Core.Tests/FX.Spec.Core.Tests.fsproj
dotnet run --project src/FX.Spec.Runner/FX.Spec.Runner.fsproj -- \
  tests/FX.Spec.Core.Tests/bin/Debug/net9.0/FX.Spec.Core.Tests.dll

# Or use the script
./run-tests.sh
```

### Run Filtered Tests

```bash
# Run only Calculator tests
./run-tests.sh --filter "Calculator"

# Run only tests in a specific context
./run-tests.sh --filter "when adding numbers"

# Run tests from a specific describe block
./run-tests.sh --filter "SpecBuilder"
```

### Run with Different Formats

```bash
# Documentation format (default, colorful)
./run-tests.sh --format documentation

# Simple format (plain text, good for CI)
./run-tests.sh --format simple
```

### Combine Options

```bash
# Filter + format
./run-tests.sh --filter "Calculator" --format simple

# Filter + verbose
./run-tests.sh -f "User" -v

# All options
./run-tests.sh --filter "Database" --format documentation --verbose
```

---

## Test Discovery

The runner discovers tests using reflection:

1. **Loads the assembly** from the provided path
2. **Scans all types** for static members marked with `[<Tests>]`
3. **Collects TestNode lists** from these members
4. **Applies focused filtering** if any `fit` or `fdescribe` tests exist
5. **Applies user filter** if `--filter` option is provided

### Requirements for Test Discovery

Your test module must:

1. Have a static member (let-binding or property)
2. Be marked with `[<Tests>]` attribute
3. Return `TestNode list`
4. Be in a compiled assembly

**Example:**

```fsharp
module MyTests

open FX.Spec.Core
open FX.Spec.Matchers

[<Tests>]  // ← Required for discovery
let mySpecs =
    spec {
        yield describe "Feature" [
            it "works" (fun () ->
                expect true |> should beTrue
            )
        ]
    }
```

---

## Focused Tests

When using `fit` or `fdescribe`, the runner automatically filters to run only focused tests:

```fsharp
[<Tests>]
let specs =
    spec {
        yield describe "Suite" [
            fit "only this runs" (fun () ->  // ← Focused
                expect true |> should beTrue
            )

            it "this is skipped" (fun () ->
                expect false |> should beTrue
            )
        ]
    }
```

**Output:**

```
Suite
  ✓ only this runs (1ms)
  ⊘ this is skipped (skipped - not focused)

1 example, 0 failures, 1 skipped
```

!!! warning "Remove focused tests before committing"
    Focused tests are for development only. Remove `fit`/`fdescribe` before committing code.

---

## Pending Tests

Tests marked with `xit` or `pending` are skipped:

```fsharp
[<Tests>]
let specs =
    spec {
        yield describe "Suite" [
            it "this runs" (fun () ->
                expect true |> should beTrue
            )

            xit "this is skipped" (fun () ->
                expect false |> should beTrue
            )

            pending "not implemented yet" (fun () ->
                failwith "TODO"
            )
        ]
    }
```

**Output:**

```
Suite
  ✓ this runs (0ms)
  ⊘ this is skipped (pending)
  ⊘ not implemented yet (pending)

1 example, 0 failures, 2 skipped
```

---

## CI/CD Integration

### GitHub Actions

```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '9.0.x'

      - name: Build
        run: dotnet build

      - name: Run Tests
        run: |
          dotnet run --project src/FX.Spec.Runner/FX.Spec.Runner.fsproj -- \
            tests/MyProject.Tests/bin/Debug/net9.0/MyProject.Tests.dll \
            --format simple
```

### GitLab CI

```yaml
test:
  image: mcr.microsoft.com/dotnet/sdk:9.0
  script:
    - dotnet build
    - dotnet run --project src/FX.Spec.Runner/FX.Spec.Runner.fsproj --
        tests/MyProject.Tests/bin/Debug/net9.0/MyProject.Tests.dll
        --format simple
```

### Azure Pipelines

```yaml
steps:
- task: DotNetCoreCLI@2
  displayName: 'Build Tests'
  inputs:
    command: 'build'
    projects: 'tests/**/*.fsproj'

- script: |
    dotnet run --project src/FX.Spec.Runner/FX.Spec.Runner.fsproj -- \
      tests/MyProject.Tests/bin/Debug/net9.0/MyProject.Tests.dll \
      --format simple
  displayName: 'Run Tests'
```

---

## Troubleshooting

### "No tests found in assembly"

**Cause:** No members marked with `[<Tests>]` attribute.

**Solution:**

```fsharp
//  Correct
[<Tests>]
let mySpecs = spec { ... }

//  Wrong - missing attribute
let mySpecs = spec { ... }
```

### "Assembly not found"

**Cause:** Incorrect path to assembly or assembly not built.

**Solution:**

```bash
# Build first
dotnet build tests/MyProject.Tests/MyProject.Tests.fsproj

# Then run with correct path
dotnet run --project src/FX.Spec.Runner/FX.Spec.Runner.fsproj -- \
  tests/MyProject.Tests/bin/Debug/net9.0/MyProject.Tests.dll
```

### "No tests match the filter"

**Cause:** Filter pattern doesn't match any test descriptions.

**Solution:**

- Check filter is case-sensitive
- Use partial matches: `--filter "Calc"` instead of `--filter "Calculator tests"`
- Remove filter to see all test names

---

## See Also

- [DSL API Reference](dsl-api.md) - Complete DSL documentation
- [Quick Start Guide](../quick-start.md) - Getting started
- [Troubleshooting](../troubleshooting.md) - Common issues and solutions

