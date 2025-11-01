namespace FxSpec.Matchers

// CollectionMatchers.fs is now deprecated.
// All collection assertion functionality has been moved to Assertions.fs
// with the new fluent API:
//
// Old style: expect [1; 2; 3] |> should (contain 2)
// New style: expectSeq([1; 2; 3]).toContain(2)
//
// Available methods on CollectionExpectation:
// - toContain(item)
// - toBeEmpty()
// - toHaveLength(n)
// - toHaveCountAtLeast(n)
// - toHaveCountAtMost(n)
// - toAllSatisfy(predicate, description)
// - toAnySatisfy(predicate, description)
// - toContainAll(expectedSeq)
// - toEqualSeq(expectedSeq)
// - toStartWithSeq(expectedSeq)
// - toEndWithSeq(expectedSeq)
//
// This file is kept as a placeholder to prevent confusion during migration.


