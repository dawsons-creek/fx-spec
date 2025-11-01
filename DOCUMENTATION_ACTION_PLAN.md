# FxSpec Documentation - Action Plan

**Based on:** DOCUMENTATION_REVIEW.md  
**Date:** 2025-11-01

---

## Critical Fixes (Do First - 3-4 hours)

### 1. Fix Syntax Errors in README.md

**Files:** `README.md`  
**Lines to fix:** 25-49, 112-123, 137-147, 163-177  
**Change:** Replace `{}` with `[]`, add `(fun () -> ...)` wrappers

**Before:**
```fsharp
describe "Calculator" {
    it "adds numbers" {
        expect (2 + 2) |> should (equal 4)
    }
}
```

**After:**
```fsharp
describe "Calculator" [
    it "adds numbers" (fun () ->
        expect (2 + 2) |> should (equal 4)
    )
]
```

---

### 2. Fix Syntax Errors in QUICKSTART.md

**Files:** `QUICKSTART.md`  
**Lines to fix:** 12-40, 69-89, 94-110  
**Change:** Same as README.md

**Alternative:** Consider deprecating QUICKSTART.md in favor of docs/quick-start.md

---

### 3. Remove/Mark Incomplete Features

**Files:** `README.md`, `QUICKSTART.md`  
**Action:** Remove or clearly mark as "Experimental" or "Coming Soon"

**Features to address:**
- `let'` function (state management)
- `subject` function (state management)
- `before` / `after` (use `beforeEach` / `afterEach` instead)

**Add note:**
```markdown
> **Note:** State management with `let'` and `subject` is experimental and not yet production-ready. 
> Use `beforeEach` / `afterEach` hooks for test setup instead.
```

---

### 4. Add/Fix `yield` Keyword

**Decision needed:** 
- Option A: Add `yield` to all examples
- Option B: Make `yield` optional in SpecBuilder (better UX)

**Recommendation:** Option B - modify SpecBuilder to make `yield` optional

**If Option A (quick fix):**
Update all examples to include `yield`:
```fsharp
spec {
    yield describe "Feature" [  // Add yield
        it "test" (fun () -> ...)
    ]
}
```

---

### 5. Update Repository URLs

**Files:** All documentation files  
**Find:** `yourorg/fx-spec`, `https://github.com/yourorg/fx-spec`  
**Replace with:** Actual repository URL

**Files affected:**
- mkdocs.yml
- README.md
- docs/index.md
- docs/quick-start.md
- docs/community/contributing.md

---

## High Priority (Next - 10-15 hours)

### 6. Add Runner Documentation

**New file:** `docs/reference/runner.md`

**Content:**
- Command-line options
- Filtering tests (`--filter`)
- Output formats (`--format`)
- Exit codes
- CI/CD integration examples

---

### 7. Add Troubleshooting Guide

**New file:** `docs/troubleshooting.md`

**Content:**
- Tests not discovered (missing `[<Tests>]`)
- Compilation errors (missing opens, wrong types)
- Matcher type errors
- Runner issues
- Common mistakes

---

### 8. Add Custom Matchers Guide

**New file:** `docs/guides/custom-matchers.md`

**Content:**
- How to create custom matchers
- Matcher function signature
- Best practices
- Testing custom matchers
- Examples

---

### 9. Create Examples Repository

**New directory:** `examples/`

**Examples to add:**
- Calculator (already exists, verify)
- Web API testing
- Database testing
- File I/O testing
- Async/Task testing

---

### 10. Add FAQ Section

**New file:** `docs/faq.md`

**Questions:**
- Why use FxSpec over xUnit?
- How does it compare to Expecto?
- Can I use it with existing test frameworks?
- How do I run tests in CI/CD?
- How do I debug tests?
- Can I run tests in parallel?

---

## Medium Priority (Later - 20-30 hours)

### 11. Add Architecture Documentation

**New file:** `docs/architecture/index.md`

**Content from:** TECHNICAL_ARCHITECTURE.md

**Sections:**
- Test tree structure
- Execution model
- Matcher system
- Hook system
- Extension points

---

### 12. Add Migration Guides

**New files:**
- `docs/guides/migrating-from-xunit.md`
- `docs/guides/migrating-from-expecto.md`
- `docs/guides/migrating-from-nunit.md`

---

### 13. Add Advanced Topics

**New files:**
- `docs/guides/testing-async.md`
- `docs/guides/testing-with-mocks.md`
- `docs/guides/integration-testing.md`

---

## Polish (Final - 2-3 hours)

### 14. Consistent Terminology

**Standardize:**
- Use "test" consistently (not "example" or "spec")
- Use "matcher" consistently (not "assertion")
- Use "group" or "describe block" consistently

---

### 15. Add Missing Metadata

**All files:**
- Add version numbers where appropriate
- Add "Last updated" dates
- Verify license information

---

### 16. Verify MkDocs Features

**Check:**
- [ ] Code copy buttons work
- [ ] Search works
- [ ] Dark mode works
- [ ] Mobile view works
- [ ] Navigation works
- [ ] "Edit this page" links work

---

## Testing Checklist

Before considering documentation complete:

- [ ] All F# code examples compile
- [ ] All F# code examples run successfully
- [ ] All internal links work
- [ ] All external links work
- [ ] No placeholder text remains
- [ ] Repository URLs are correct
- [ ] Installation instructions are accurate
- [ ] Contributing guide is accurate

---

## Estimated Timeline

**Critical fixes:** 1 day (3-4 hours)  
**High priority:** 2-3 days (10-15 hours)  
**Medium priority:** 4-5 days (20-30 hours)  
**Polish:** 0.5 day (2-3 hours)

**Total:** 8-10 days of focused work

---

## Success Criteria

Documentation is ready when:

1. ✅ All code examples compile and run
2. ✅ No syntax errors in any examples
3. ✅ All features documented match implementation
4. ✅ Troubleshooting guide helps users solve common problems
5. ✅ New users can get started in < 10 minutes
6. ✅ All links work
7. ✅ Documentation builds without errors
8. ✅ Mobile view is usable
9. ✅ Search finds relevant content
10. ✅ Contributing guide enables contributions

---

## Next Steps

1. Review this action plan
2. Prioritize based on release timeline
3. Assign tasks (if team) or schedule work (if solo)
4. Start with critical fixes
5. Test each change
6. Get feedback from beta users
7. Iterate based on feedback

