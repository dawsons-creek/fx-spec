# Contributing to FX.Spec

Thank you for your interest in contributing to FxSpec! This document provides guidelines and instructions for contributing.

---

## Getting Started

### Prerequisites

- .NET 8.0 or .NET 9.0 SDK
- F# 8.0+ (included with .NET SDK)
- Git
- Basic F# knowledge
- Familiarity with BDD testing concepts

### Setting Up Development Environment

1. **Fork and clone the repository:**

```bash
git clone https://github.com/dawsons-creek/fx-spec.git
cd fx-spec
```

2. **Build the solution:**

```bash
dotnet build
```

3. **Run the tests:**

```bash
./run-tests.sh
```

You should see all tests passing. FX.Spec uses itself for testing (dogfooding).

---

## Building the Project

### Project Structure

```
fx-spec/
├── src/
│   ├── FX.Spec.Core/          # Core DSL and test tree
│   ├── FX.Spec.Matchers/      # Assertion matchers
│   └── FX.Spec.Runner/        # Test discovery and execution
├── tests/
│   ├── FX.Spec.Core.Tests/    # Tests for Core
│   └── FX.Spec.Matchers.Tests/# Tests for Matchers
├── examples/                  # Example usage
└── docs/                      # Documentation
```

### Build Commands

```bash
# Build everything
dotnet build

# Build specific project
dotnet build src/FX.Spec.Core/FX.Spec.Core.fsproj

# Clean build
dotnet clean
dotnet build

# Release build
dotnet build -c Release
```

---

## Running Tests

FX.Spec tests itself using its own framework.

### Run All Tests

```bash
./run-tests.sh
```

### Run Specific Tests

```bash
# Filter by test description
./run-tests.sh --filter "SpecBuilder"

# Use simple formatter
./run-tests.sh --format simple
```

### Run Tests Manually

```bash
# Build and run
dotnet build tests/FX.Spec.Core.Tests/FX.Spec.Core.Tests.fsproj
dotnet run --project src/FX.Spec.Runner/FX.Spec.Runner.fsproj -- \
  tests/FX.Spec.Core.Tests/bin/Debug/net9.0/FX.Spec.Core.Tests.dll
```

---

## Code Style

### F# Conventions

FX.Spec follows standard F# conventions:

- **Naming:**
  - `camelCase` for functions and values
  - `PascalCase` for types, modules, and DU cases
  - Prefix private functions with `private` keyword

- **Code Organization:**
  - Group related functions together
  - Use `/// XML comments` for public APIs
  - Keep functions focused and small (prefer < 25 lines)

- **Pattern Matching:**
  - Use `function` shorthand for single-argument pattern matching
  - Match exhaustively (compiler enforces this)
  - Put most common cases first

### Example

```fsharp
/// Builds the full path to a test node.
let rec buildTestPath (path: string list) (node: TestResultNode) : (string * TestResult * TimeSpan) list =
    match node with
    | ExampleResult (desc, result, duration) ->
        let fullPath = (path @ [desc]) |> String.concat " > "
        [(fullPath, result, duration)]
    | GroupResult (desc, children) ->
        let newPath = path @ [desc]
        children |> List.collect (buildTestPath newPath)
```

### Code Quality

- **No compiler warnings:** Code must compile without warnings
- **Follow best practices:**
  - Prefer pure functions
  - Minimize mutable state
  - Use type constraints appropriately
  - Avoid magic numbers (use named constants)
- **Write tests:** All new features must have tests
- **Document public APIs:** Use XML comments for all public functions

---

## Making Changes

### 1. Create a Branch

```bash
git checkout -b feature/my-new-feature
# or
git checkout -b fix/bug-description
```

### Branch Naming

- `feature/` - New features
- `fix/` - Bug fixes
- `docs/` - Documentation changes
- `refactor/` - Code refactoring

### 2. Write Code

- Follow F# conventions and code style
- Add tests for new functionality
- Update documentation if needed

### 3. Write Tests

All code changes must include tests. FX.Spec tests itself:

```fsharp
module MyNewFeatureSpecs

open FX.Spec.Core
open FX.Spec.Matchers

[<Tests>]
let myNewFeatureSpecs =
    spec {
        describe "MyNewFeature" [
            it "works as expected" (fun () ->
                let result = MyNewFeature.doSomething()
                expect result |> should (equal expectedValue)
            )
        ]
    }
```

### 4. Run Tests

```bash
./run-tests.sh
```

All tests must pass before submitting a pull request.

### 5. Commit Changes

```bash
git add .
git commit -m "feat: add new feature description"
```

#### Commit Message Format

Use conventional commits format:

```
type(scope): description

[optional body]
```

**Types:**
- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation changes
- `refactor:` - Code refactoring
- `test:` - Adding or updating tests
- `chore:` - Maintenance tasks

**Examples:**

```
feat(matchers): add bePositive and beNegative matchers
fix(runner): handle null test descriptions correctly
docs(quick-start): update installation instructions
refactor(core): simplify test tree building
```

---

## Pull Request Process

### 1. Push Your Branch

```bash
git push origin feature/my-new-feature
```

### 2. Open Pull Request

- Go to GitHub and open a pull request
- Fill in the PR template
- Link any related issues

### PR Title

Follow the same format as commit messages:

```
feat(matchers): add numeric comparison matchers
fix(runner): prevent crash on empty test suite
```

### PR Description

Include:

- **What:** What changes did you make?
- **Why:** Why are these changes needed?
- **How:** How did you implement the changes?
- **Testing:** How did you test the changes?

### Example PR Description

```markdown
## What

Add `bePositive`, `beNegative`, and `beZero` matchers for numeric comparisons.

## Why

Users need a convenient way to test if numbers are positive, negative, or zero without writing custom matchers or using comparison operators.

## How

- Added three new matchers in `NumericMatchers.fs`
- Used inline functions with generic numeric constraints
- Leveraged `LanguagePrimitives.GenericZero`

## Testing

- Added comprehensive tests in `NumericMatchersSpecs.fs`
- Tested with int, float, and decimal types
- All 166 tests passing
```

### 3. Code Review

- Address reviewer feedback
- Update code as needed
- Push changes to the same branch
- PR will update automatically

### 4. Merge

Once approved:
- Squash and merge (preferred)
- Merge commit (for multi-commit features)
- Rebase and merge (for clean history)

---

## Areas to Contribute

### Matchers

We always welcome new matchers! Ideas:

- **DateTime matchers:** `beToday`, `beBefore`, `beAfter`, `beInYear`
- **Async matchers:** Matchers for `Async<'T>` and `Task<'T>`
- **File/IO matchers:** `fileExist`, `directoryExist`, `haveExtension`
- **JSON matchers:** Matchers for JSON comparison

### Formatters

- Alternative output formats (JSON, JUnit XML, TAP)
- Integration with test reporting tools
- VS Code extension support

### Documentation

- Tutorial content
- How-to guides
- Example projects
- Blog posts

### Bug Fixes

Check [open issues](https://github.com/dawsons-creek/fx-spec/issues) for bugs to fix.

---

## Documentation

### Building Documentation

Documentation uses Material for MkDocs:

```bash
# Install dependencies (first time only)
uv venv
uv pip install mkdocs-material mkdocs-mermaid2-plugin

# Serve locally
uv run mkdocs serve

# Open http://127.0.0.1:8000
```

### Writing Documentation

- Use clear, concise language
- Include code examples
- Add practical use cases
- Follow existing structure

Documentation structure follows the [Diátaxis framework](https://diataxis.fr/):

- **Tutorials:** Learning-oriented lessons
- **How-To Guides:** Task-oriented directions
- **Reference:** Information-oriented descriptions
- **Explanation:** Understanding-oriented discussions

---

## Testing Guidelines

### Test Organization

```fsharp
spec {
    describe "FeatureName" [
        context "when specific condition" [
            it "behaves in expected way" (fun () ->
                // Arrange
                let input = setupTestData()

                // Act
                let result = performOperation(input)

                // Assert
                expect result |> should (equal expectedValue)
            )
        ]
    ]
}
```

### Test Coverage

- Test happy paths
- Test edge cases
- Test error conditions
- Test boundary values

### Good Test Examples

```fsharp
// Good: Clear, focused, descriptive
it "adds two positive numbers" (fun () ->
    expect (Calculator.add 2 3) |> should (equal 5)
)

// Good: Tests edge case
it "handles division by zero" (fun () ->
    expect (fun () -> Calculator.divide 10 0) |> should raiseException
)

// Less good: Unclear what's being tested
it "test1" (fun () ->
    expect (doSomething()) |> should (equal 42)
)
```

---

## Getting Help

- **Questions:** [Open a discussion](https://github.com/dawsons-creek/fx-spec/discussions)
- **Bugs:** [Open an issue](https://github.com/dawsons-creek/fx-spec/issues)
- **Chat:** Join our community chat (link TBD)

---

## Code of Conduct

### Our Standards

- Be respectful and inclusive
- Welcome newcomers
- Accept constructive criticism
- Focus on what's best for the community
- Show empathy

### Unacceptable Behavior

- Harassment or discrimination
- Trolling or insulting comments
- Personal or political attacks
- Publishing others' private information

### Enforcement

Violations may result in temporary or permanent ban from the project.

---

## License

By contributing to FxSpec, you agree that your contributions will be licensed under the same license as the project (MIT License).

---

## Recognition

Contributors are recognized in:

- GitHub contributors list
- Release notes
- Project documentation

Thank you for contributing to FxSpec!
