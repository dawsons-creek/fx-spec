# FxSpec Dogfooding Strategy

## Overview

**Dogfooding** (eating your own dog food) means using FxSpec to test itself. This is a critical validation that the framework is actually usable, pleasant, and powerful.

## Why Dogfooding Matters

1. **Real-World Validation** - If we can't use FxSpec to test itself, how can we expect others to use it?
2. **Usability Testing** - We'll discover pain points and awkward APIs immediately
3. **Comprehensive Examples** - FxSpec's own test suite becomes the best documentation
4. **Quality Assurance** - A framework that tests itself is inherently more trustworthy
5. **Iterative Improvement** - We can refine the API based on our own experience

## The Migration Plan

### Phase 1: Foundation (COMPLETE ✅)
**Status**: Tests written in plain F# with manual assertions

```fsharp
// Current approach
let testSimpleExample() =
    let nodes = spec { yield it "test" (fun () -> ()) }
    match nodes with
    | [Example(desc, _)] when desc = "test" -> ()
    | _ -> failwith "Should create Example node"
```

**Why not dogfood yet?**
- No matchers yet (can't write `expect x |> to' (equal y)`)
- No runner yet (can't execute FxSpec tests)
- Need basic functionality working first

### Phase 2: Matchers Complete ✅
**Status**: Complete - matchers are ready

**What we have**:
- Full matcher system with `expect` and `to'`
- Core matchers: `equal`, `beTrue`, `beFalse`, `beNull`, `haveLength`, etc.
- Negation support with `notTo`
- Custom matcher API

### Phase 3.1: Dogfooding - Rewrite Phase 1 Tests ✅
**Status**: COMPLETE!

**What we did**:
1. ✅ Created custom matchers for testing FxSpec internals
2. ✅ Rewrote TypesTests as TypesSpecs using FxSpec DSL
3. ✅ Rewrote SpecBuilderTests as SpecBuilderSpecs using FxSpec DSL
4. ✅ Kept legacy tests running in parallel for validation
5. ✅ All 30 FxSpec-based tests pass with beautiful output

**Example transformation**:

```fsharp
// Before (plain F#)
let testNodeDescription() =
    let example = Example("test example", fun () -> Pass)
    let group = Group("test group", [])
    
    if TestNode.description example <> "test example" then
        failwith "Example description should match"
    if TestNode.description group <> "test group" then
        failwith "Group description should match"

// After (FxSpec)
[<Tests>]
let typeSpecs =
    spec {
        yield describe "TestNode" [
            describe "description" [
                it "returns the description of an Example" (fun () ->
                    let example = Example("test example", fun () -> Pass)
                    expect (TestNode.description example) 
                    |> to' (equal "test example")
                )
                
                it "returns the description of a Group" (fun () ->
                    let group = Group("test group", [])
                    expect (TestNode.description group)
                    |> to' (equal "test group")
                )
            ]
        ]
    }
```

**Benefits**:
- ✅ More readable and descriptive
- ✅ Better organized with describe/context
- ✅ Validates matcher system works
- ✅ Serves as matcher examples

### Phase 3: Runner Complete 🚀
**Status**: Future

**What we'll do**:
1. Use FxSpec runner to execute FxSpec's own tests
2. Remove temporary test runner
3. Use `dotnet fspec tests/FxSpec.Core.Tests.dll`

**Command**:
```bash
# Instead of:
dotnet run --project tests/FxSpec.Core.Tests

# We'll use:
dotnet fspec tests/FxSpec.Core.Tests/bin/Debug/net9.0/FxSpec.Core.Tests.dll
```

**Benefits**:
- ✅ Validates runner works correctly
- ✅ Tests discovery mechanism
- ✅ Tests execution engine
- ✅ Full integration test

### Phase 4: Formatters Complete 🎨
**Status**: Future

**What we'll do**:
1. FxSpec's test output will be beautiful
2. Failure messages will be comprehensive
3. We'll see our own formatter in action

**Expected output**:
```
FxSpec.Core
  Types
    TestResult
      ✓ isPass returns true for Pass
      ✓ isFail returns true for Fail
      ✓ isSkipped returns true for Skipped
    TestNode
      description
        ✓ returns the description of an Example
        ✓ returns the description of a Group
      countExamples
        ✓ counts single example as 1
        ✓ counts examples in groups correctly

18 examples, 0 failures
```

**Benefits**:
- ✅ Validates formatter works
- ✅ Beautiful output for our own tests
- ✅ Demonstrates nested structure

## Dogfooding Checklist

### Phase 3.1 (Dogfooding - Rewrite Tests) ✅
- [x] Rewrite TypesTests using FxSpec
- [x] Rewrite SpecBuilderTests using FxSpec
- [x] Create custom matchers for testing FxSpec internals
- [x] Validate all tests pass
- [x] Compare output with original tests
- [ ] Add StateManagementTests using FxSpec (future enhancement)

### Phase 3.2+ (Runner)
- [ ] Mark test modules with `[<Tests>]`
- [ ] Run tests using FxSpec runner
- [ ] Remove temporary test runner
- [ ] Update build scripts
- [ ] Document how to run tests

### Phase 4 (Formatters)
- [ ] Verify beautiful output
- [ ] Check failure messages
- [ ] Validate nested structure display
- [ ] Screenshot for documentation

### Phase 5 (Extensions)
- [ ] Use request specs to test any HTTP endpoints
- [ ] Use focused tests during development
- [ ] Demonstrate all advanced features

## Success Criteria

FxSpec will be considered "self-hosting" when:

1. ✅ All FxSpec tests are written using FxSpec DSL
2. ✅ FxSpec runner executes its own tests
3. ✅ FxSpec formatters display its own results
4. ✅ No external test framework dependencies
5. ✅ Test output is beautiful and informative
6. ✅ The experience is pleasant and productive

## Example: Full Self-Hosted Test

Here's what a complete FxSpec test will look like:

```fsharp
namespace FxSpec.Core.Tests

open FxSpec.Core

[<Tests>]
let matcherSpecs =
    spec {
        yield describe "Matchers" [
            describe "equal" [
                it "passes when values are equal" (fun () ->
                    let result = equal 42 42
                    expect result |> to' (equal Pass)
                )
                
                it "fails when values differ" (fun () ->
                    let result = equal 42 99
                    expect result |> to' (beFail)
                )
                
                it "provides helpful failure message" (fun () ->
                    let result = equal 42 99
                    match result with
                    | Fail (msg, _, _) ->
                        expect msg |> to' (contain "Expected 42")
                        expect msg |> to' (contain "but found 99")
                    | _ -> failwith "Should have failed"
                )
            ]
            
            describe "contain" [
                it "passes when item is in collection" (fun () ->
                    expect [1; 2; 3] |> to' (contain 2)
                )
                
                it "fails when item is not in collection" (fun () ->
                    expect (fun () -> 
                        expect [1; 2; 3] |> to' (contain 5)
                    ) |> to' raiseException<AssertionException>
                )
            ]
        ]
    }
```

## Benefits of This Approach

1. **Confidence** - If FxSpec can test itself, it can test anything
2. **Documentation** - Best examples come from real usage
3. **Quality** - We'll fix issues we encounter ourselves
4. **Credibility** - Users trust a framework that uses itself
5. **Iteration** - Rapid feedback on API design

## Timeline

- **Phase 1**: ✅ Complete (Core DSL)
- **Phase 2**: ✅ Complete (Matchers)
- **Phase 3.1**: ✅ Complete (Dogfooding - FxSpec tests itself!)
- **Phase 3.2+**: 🎯 Next (Test Runner - use FxSpec runner)
- **Phase 4**: 📅 Later (Formatters - beautiful output)
- **Phase 5**: 🚀 Future (Extensions - all features)

---

**The ultimate validation**: When we can run `dotnet fspec` on FxSpec's own test suite and get beautiful, comprehensive output, we'll know we've built something truly great! 🎉

