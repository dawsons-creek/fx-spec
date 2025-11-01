# FxSpec DSL Syntax Exploration

**Date:** 2025-11-01  
**Status:** Analysis Complete

---

## Goal

Explore ways to reduce syntactic noise in FxSpec while maintaining BDD structure:

**Current Syntax:**
```fsharp
[<Tests>]
let calculatorSpecs =
    spec {
        yield describe "Calculator" [
            it "adds numbers" (fun () ->
                expect (2 + 2) |> should (equal 4)
            )
        ]
    }
```

**Desired Syntax:**
```fsharp
[<Tests>]
let calculatorSpecs = [
    describe "Calculator" [
        it "adds numbers" {
            expect (2 + 2) |> should (equal 4)
        }
    ]
]
```

---

## What We Tried

### 1. Remove `spec { }` wrapper ✅ POSSIBLE

**Approach:** Use list syntax directly

```fsharp
[<Tests>]
let calculatorSpecs = [
    describe "Calculator" [...]
]
```

**Result:** This works! Tests can be defined as a simple list.

---

### 2. Remove `yield` keyword ❌ NOT POSSIBLE

**Problem:** F# computation expressions require `yield` for sequence builders.

**Attempted Solutions:**
- Use `For` member → doesn't help
- Use `return` → only allows one item
- Make `describe` return `TestNode list` → semantically wrong

**Conclusion:** `yield` is fundamental to F# computation expressions. Cannot be removed without removing `spec { }` entirely.

---

### 3. Remove `(fun () -> ...)` wrapper ❌ NOT FEASIBLE

**Problem:** Tests must be lazy (not executed during discovery).

**Why it's needed:**
```fsharp
// Without fun () ->
it "test" (
    expect true |> should beTrue  // Executes immediately!
)

// With fun () ->
it "test" (fun () ->
    expect true |> should beTrue  // Executes during test run
)
```

**Attempted Solution:** Use computation expression builder

```fsharp
type ItBuilder(description: string) =
    member _.Delay(f: unit -> unit) = ...
    member _.Run(node) = node

let it description = ItBuilder(description)

// Usage
it "test" {
    expect true |> should beTrue
}
```

**Problem:** This REPLACES the old `it` function. F# doesn't support overloading by return type, so we can't have both:
- `it "test" (fun () -> ...)` (old style)
- `it "test" { ... }` (new style)

**Impact:** Would break ALL existing tests (52 tests across 5 files).

---

## Conclusions

### What's Possible ✅

1. **Remove `spec { }` wrapper**
   ```fsharp
   [<Tests>]
   let specs = [
       describe "Feature" [...]
   ]
   ```

2. **Use `should`/`shouldNot` instead of `to'`/`notTo'`** (Already done!)
   ```fsharp
   expect result |> should (equal 5)
   ```

### What's Not Possible ❌

1. **Remove `yield` keyword** - Fundamental to F# computation expressions
2. **Remove `(fun () -> ...)` wrapper** - Required for lazy evaluation
3. **Use `() =` syntax** - Not compatible with function calls

---

## Recommendations

### Option A: Keep Current Syntax (Recommended)

**Syntax:**
```fsharp
[<Tests>]
let calculatorSpecs =
    spec {
        yield describe "Calculator" [
            it "adds numbers" (fun () ->
                expect (2 + 2) |> should (equal 4)
            )
        ]
    }
```

**Pros:**
- No breaking changes
- Familiar to existing users
- Standard F# patterns

**Cons:**
- Verbose (`yield`, `spec { }`, `(fun () -> ...)`)

---

### Option B: Offer Alternative Syntax (Experimental)

Keep the old syntax but also offer a new computation expression style:

```fsharp
// Old style (still works)
it "test" (fun () ->
    expect true |> should beTrue
)

// New style (opt-in)
test "test" {
    expect true |> should beTrue
}
```

**Implementation:**
```fsharp
type TestBuilder(description: string) =
    member _.Delay(f: unit -> unit) = it description f
    member _.Run(node) = node

let test description = TestBuilder(description)
```

**Pros:**
- Removes `(fun () -> ...)` for those who want it
- Backward compatible
- Users can choose

**Cons:**
- Two ways to do the same thing
- Confusing for newcomers
- More API surface

---

### Option C: Simplify with List Syntax

Remove `spec { }` and use list syntax:

```fsharp
[<Tests>]
let calculatorSpecs = [
    describe "Calculator" [
        it "adds numbers" (fun () ->
            expect (2 + 2) |> should (equal 4)
        )
    ]
]
```

**Pros:**
- Removes `spec { }` and `yield`
- Still keeps BDD structure
- Simpler for simple test suites

**Cons:**
- Still has `(fun () -> ...)`
- Loses computation expression benefits

---

## Final Recommendation

**Keep the current syntax** with the `should`/`shouldNot` improvement we already made.

**Rationale:**
1. The `(fun () -> ...)` wrapper is **fundamental** to F# lazy evaluation
2. Removing it would require breaking ALL existing tests
3. The current syntax is **standard F#** - familiar to F# developers
4. Other F# test frameworks (Expecto, FsUnit) have similar patterns

**What we've already improved:**
```fsharp
// Before
expect result |> to' (equal 5)

// After
expect result |> should (equal 5)
```

This is a **significant readability improvement** without breaking changes!

---

## Comparison with Other Frameworks

### Expecto
```fsharp
test "my test" {
    Expect.equal (2 + 2) 4 ""
}
```
- Uses computation expression
- No `(fun () -> ...)` needed
- But different API design

### FsUnit
```fsharp
[<Test>]
let ``my test`` () =
    2 + 2 |> should equal 4
```
- Uses NUnit attributes
- Function-based, not DSL
- No BDD structure

### FxSpec (Current)
```fsharp
it "my test" (fun () ->
    expect (2 + 2) |> should (equal 4)
)
```
- BDD structure with `describe`/`context`
- Explicit lazy evaluation
- Type-safe matchers

**FxSpec's strength is BDD structure, not minimal syntax.**

---

## Summary

We explored removing syntactic noise from FxSpec but found that:

1. ✅ **`should`/`shouldNot`** - Already implemented, great improvement!
2. ✅ **Remove `spec { }`** - Possible with list syntax
3. ❌ **Remove `yield`** - Not possible with computation expressions
4. ❌ **Remove `(fun () -> ...)`** - Not feasible without breaking changes

**The current syntax is good!** The `should`/`shouldNot` change we made is the right balance of readability and F# idioms.

---

**Conclusion:** Keep current syntax, document it well, and focus on other improvements (documentation, features, performance).

