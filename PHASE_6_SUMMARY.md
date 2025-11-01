# Phase 6 Complete: Hooks & State Management 🪝

## Overview

Phase 6 is complete! We've successfully implemented a comprehensive hooks system for FxSpec, including `beforeEach`, `afterEach`, `beforeAll`, and `afterAll` hooks. The implementation follows industry best practices and matches the behavior of popular testing frameworks like Jest, RSpec, and Jasmine.

## What We Built

### Hook Types

1. **beforeAll** - Runs once before all tests in a group
2. **beforeEach** - Runs before each test in a group
3. **afterEach** - Runs after each test in a group (even on failure)
4. **afterAll** - Runs once after all tests in a group (even on failure)

### Hook Execution Order

```
Group Start
├─ beforeAll (once)
├─ Test 1
│  ├─ beforeEach (outer-to-inner)
│  ├─ test execution
│  └─ afterEach (inner-to-outer)
├─ Test 2
│  ├─ beforeEach (outer-to-inner)
│  ├─ test execution
│  └─ afterEach (inner-to-outer)
└─ afterAll (once)
```

### Example Usage

```fsharp
spec {
    yield describe "Database tests" [
        // Runs once before all tests
        beforeAll (fun () ->
            database.connect()
        )
        
        // Runs before each test
        beforeEach (fun () ->
            database.beginTransaction()
        )
        
        // Runs after each test
        afterEach (fun () ->
            database.rollback()
        )
        
        // Runs once after all tests
        afterAll (fun () ->
            database.disconnect()
        )
        
        it "inserts a record" (fun () ->
            database.insert({ id = 1; name = "test" })
            expect (database.count()) |> to' (equal 1)
        )
        
        it "updates a record" (fun () ->
            database.insert({ id = 1; name = "test" })
            database.update(1, { id = 1; name = "updated" })
            expect (database.get(1).name) |> to' (equal "updated")
        )
    ]
}
```

## Implementation Details

### Architecture

1. **GroupHooks Type** - Stores hooks for each group
   - `BeforeAll: (unit -> unit) list`
   - `BeforeEach: (unit -> unit) list`
   - `AfterEach: (unit -> unit) list`
   - `AfterAll: (unit -> unit) list`

2. **Hook Nodes** - Special TestNode variants
   - `BeforeAllHook of (unit -> unit)`
   - `BeforeEachHook of (unit -> unit)`
   - `AfterEachHook of (unit -> unit)`
   - `AfterAllHook of (unit -> unit)`

3. **Hook Processing** - During group construction
   - Hook nodes are separated from test nodes
   - Hooks are collected into GroupHooks
   - Groups store their hooks

4. **Hook Execution** - During test execution
   - beforeAll runs once at group start
   - beforeEach accumulates from outer to inner groups
   - afterEach accumulates from inner to outer groups
   - afterAll runs once at group end
   - afterEach runs even on test failure

### Key Features

✅ **Nested Hook Support** - Hooks accumulate through nested groups  
✅ **Failure Resilience** - afterEach/afterAll run even on test failure  
✅ **Correct Ordering** - beforeEach (outer-to-inner), afterEach (inner-to-outer)  
✅ **Clean API** - Simple, intuitive function calls  
✅ **Type Safe** - Full F# type safety  

## Test Coverage

**11 comprehensive hook tests:**

1. beforeEach runs before each test ✓
2. beforeEach runs in outer-to-inner order ✓
3. afterEach runs after each test ✓
4. afterEach runs in inner-to-outer order ✓
5. afterEach runs even when test fails ✓
6. beforeEach and afterEach run in correct order ✓
7. beforeAll runs once before all tests ✓
8. afterAll runs once after all tests ✓
9. afterAll runs even when tests fail ✓
10. beforeAll and afterAll run in correct order ✓
11. All hooks work together correctly ✓

**Total test count: 52 tests (41 core + 11 hooks)**

## Files Modified

### Core Implementation
- `src/FxSpec.Core/Types.fs` - Added GroupHooks and hook nodes
- `src/FxSpec.Core/SpecBuilder.fs` - Added hook functions
- `src/FxSpec.Runner/Executor.fs` - Hook execution logic
- `src/FxSpec.Runner/Discovery.fs` - Handle hooks in filtering

### Tests
- `tests/FxSpec.Core.Tests/HooksSpecs.fs` - 11 comprehensive hook tests
- `tests/FxSpec.Core.Tests/FxSpec.Core.Tests.fsproj` - Added Runner reference
- `tests/FxSpec.Core.Tests/*.fs` - Updated for new Group signature

## Commits

```
66077a4 test: Phase 6 - Fix hook tests and verify implementation
e614e52 feat: Phase 6 - Implement hooks system (Green phase)
ba88ba1 wip: Phase 6 - Add hook tests (TDD approach)
```

## Benefits

### 1. Setup/Teardown Management
- Clean database state between tests
- Resource initialization and cleanup
- Test isolation

### 2. Code Reuse
- Share setup code across tests
- Reduce duplication
- Maintain DRY principle

### 3. Industry Standard
- Matches Jest, RSpec, Jasmine behavior
- Familiar to developers
- Easy migration from other frameworks

### 4. Reliability
- Guaranteed cleanup even on failure
- Prevents resource leaks
- Maintains test independence

## Comparison with Other Frameworks

### vs. Jest (JavaScript)
- ✅ Same hook names
- ✅ Same execution order
- ✅ Same failure behavior
- ✅ Nested hook support

### vs. RSpec (Ruby)
- ✅ Similar to before/after hooks
- ✅ Same execution semantics
- ✅ Nested context support

### vs. xUnit (C#)
- ✅ More flexible than constructor/dispose
- ✅ Better nested group support
- ✅ More granular control

## What's Next?

Phase 6 is complete! Possible future enhancements:

1. **Async Hooks** - Support for async setup/teardown
2. **Hook Timeouts** - Prevent hanging hooks
3. **Hook Errors** - Better error reporting for hook failures
4. **Shared State** - `let'` for scope-based state management

## Conclusion

Phase 6 successfully implements a production-ready hooks system for FxSpec. The implementation is clean, well-tested, and follows industry best practices. With 52 tests passing and comprehensive hook coverage, FxSpec now provides developers with powerful tools for managing test setup and teardown.

🪝 **FxSpec hooks are fully functional and production-ready!**

