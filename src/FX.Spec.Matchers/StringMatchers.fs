namespace FX.Spec.Matchers

// StringMatchers.fs is now deprecated.
// All string assertion functionality has been moved to Assertions.fs
// with the new fluent API:
//
// Old style: expect "hello" |> should (startWith "hel")
// New style: expectStr("hello").toStartWith("hel")
//
// Available methods on StringExpectation:
// - toEqual(s)
// - toStartWith(s)
// - toEndWith(s)
// - toContain(s)
// - toBeEmpty()
// - toMatchRegex(pattern)
// - toBeNullOrEmpty()
// - toBeNullOrWhitespace()
// - toHaveLength(n)
// - toEqualIgnoreCase(s)
// - toBeAlphabetic()
// - toBeNumeric()
//
// This file is kept as a placeholder to prevent confusion during migration.

