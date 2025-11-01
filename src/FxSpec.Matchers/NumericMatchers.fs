namespace FxSpec.Matchers

// NumericMatchers.fs is now deprecated.
// All numeric assertion functionality has been moved to Assertions.fs
// with the new fluent API:
//
// Old style: expect 10 |> should (beGreaterThan 5)
// New style: expectNum(10).toBeGreaterThan(5)
//
// Available methods on NumericExpectation:
// - toEqual(n)
// - toBeGreaterThan(n)
// - toBeLessThan(n)
// - toBeGreaterThanOrEqual(n)
// - toBeLessThanOrEqual(n)
// - toBeBetween(min, max)
// - toBeCloseTo(expected, tolerance)
// - toBePositive()
// - toBeNegative()
// - toBeZero()
//
// Available methods on IntExpectation (via expectInt):
// - toBeEven()
// - toBeOdd()
// - toBeDivisibleBy(divisor)
//
// This file is kept as a placeholder to prevent confusion during migration.


