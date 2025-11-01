# DSL Refactoring: to'/notTo' → should/shouldNot

**Date:** 2025-11-01  
**Status:** ✅ Complete

---

## Overview

Refactored the FxSpec assertion DSL to use `should`/`shouldNot` instead of `to'`/`notTo'` for better readability and F# idioms.

---

## Motivation

### Before (Less Readable)
```fsharp
expect result |> to' (equal 5)
expect result |> notTo' (equal 5)
```

**Issues:**
- `to'` adds noise without much value
- The pipe `|>` already conveys the relationship
- Apostrophe in `to'` is awkward
- Less F#-idiomatic

### After (More Readable)
```fsharp
expect result |> should (equal 5)
expect result |> shouldNot (equal 5)
```

**Benefits:**
- ✅ Reads naturally: "expect result should equal 5"
- ✅ Clear negation: `shouldNot` is unambiguous
- ✅ Familiar to F# developers (similar to FsUnit)
- ✅ Balances readability with conciseness
- ✅ No awkward apostrophes

---

## Changes Made

### 1. Core Implementation ✅

**File:** `src/FxSpec.Matchers/Assertions.fs`

**Changes:**
- Renamed `to'` → `should`
- Renamed `notTo'` → `shouldNot`
- Updated documentation comments

### 2. Test Files ✅

**Files Updated:**
- `tests/FxSpec.Core.Tests/*.fs` (all test files)
- `tests/FxSpec.Matchers.Tests/*.fs` (all test files)

**Method:** Automated find/replace with sed

### 3. Documentation ✅

**Files Updated:**
- `README.md`
- `QUICKSTART.md`
- `docs/index.md`
- `docs/quick-start.md`
- `docs/reference/dsl-api.md`
- `docs/reference/matchers/*.md` (all matcher docs)
- `docs/troubleshooting.md`
- All other `.md` files

**Method:** Automated find/replace with sed

### 4. Examples ✅

**Files Updated:**
- `examples/*.fsx` (all example files)

**Method:** Automated find/replace with sed

---

## Verification

### Build Status ✅
```
dotnet build
Result: Build succeeded, 0 warnings, 0 errors
```

### Test Status ✅
```
./run-tests.sh
Result: 52/52 tests passed
```

### Sample Verification ✅

**README.md:**
```fsharp
expect (2 + 2) |> should (equal 4)
```

**docs/quick-start.md:**
```fsharp
expect result |> should (equal 4)
expect greeting |> should (startWith "Hello")
expect 5 |> shouldNot (equal 10)
```

**tests/FxSpec.Core.Tests/TypesSpecs.fs:**
```fsharp
expect (TestResult.isPass result) |> should beTrue
expect (TestResult.isFail result) |> should beFalse
```

---

## Impact

### Readability Improvement

**Before:**
```fsharp
it "adds two numbers" (fun () ->
    expect (2 + 2) |> to' (equal 4)
)
```

**After:**
```fsharp
it "adds two numbers" (fun () ->
    expect (2 + 2) |> should (equal 4)
)
```

Much more natural to read!

### Alignment with F# Community

**FsUnit:**
```fsharp
result |> should equal 5
```

**FxSpec (new):**
```fsharp
expect result |> should (equal 5)
```

Very similar! The only difference is the `expect` wrapper for better error messages.

---

## Files Changed

### Modified Files
- **Core:** 1 file (`src/FxSpec.Matchers/Assertions.fs`)
- **Tests:** ~10 files (all test files)
- **Documentation:** ~15 files (all markdown files)
- **Examples:** ~3 files (all example files)

### Total Changes
- **Automated replacements:** ~500+ occurrences
- **Build:** ✅ Success
- **Tests:** ✅ 52/52 passing

---

## Breaking Changes

⚠️ **This is a breaking change** for any existing FxSpec code.

**Migration:**
```fsharp
// Old
expect x |> to' (equal y)
expect x |> notTo' (equal y)

// New
expect x |> should (equal y)
expect x |> shouldNot (equal y)
```

**However:** Since FxSpec is pre-release (MVP), this is the perfect time to make this change. No published users to break!

---

## Comparison with Other Frameworks

### RSpec (Ruby)
```ruby
expect(result).to eq(5)
expect(result).not_to eq(5)
```

### FsUnit (F#)
```fsharp
result |> should equal 5
result |> should not' (equal 5)
```

### FxSpec (new)
```fsharp
expect result |> should (equal 5)
expect result |> shouldNot (equal 5)
```

FxSpec now aligns better with F# idioms while maintaining clarity.

---

## Future Considerations

### Backward Compatibility (if needed)

If we ever need to support the old syntax, we could add deprecated aliases:

```fsharp
[<Obsolete("Use 'should' instead")>]
let to' = should

[<Obsolete("Use 'shouldNot' instead")>]
let notTo' = shouldNot
```

But since we're pre-release, this isn't necessary.

---

## Conclusion

The DSL refactoring is complete and successful:

- ✅ More readable syntax
- ✅ Better F# idioms
- ✅ All tests passing
- ✅ All documentation updated
- ✅ Zero build errors

**The new syntax is production-ready!** 🚀

---

**Refactoring completed:** 2025-11-01  
**Build status:** ✅ Success  
**Test status:** ✅ 52/52 passing  
**Documentation:** ✅ Updated

