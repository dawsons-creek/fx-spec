// Example demonstrating all FxSpec matchers
#r "../src/FxSpec.Matchers/bin/Debug/net9.0/FxSpec.Matchers.dll"

open FxSpec.Matchers

printfn "==================================="
printfn "FxSpec Matchers Examples"
printfn "==================================="
printfn ""

// Core Matchers
printfn "Core Matchers:"
expect(42).toEqual(42)
printfn "  ✓ toEqual"

expectOption(None).toBeNone()
printfn "  ✓ toBeNone"

expectOption(Some 42).toBeSome(42)
printfn "  ✓ toBeSome"

expectResult(Ok 42).toBeOk(42)
printfn "  ✓ toBeOk"

expectResult(Error "failed").toBeError("failed")
printfn "  ✓ toBeError"

expectBool(true).toBeTrue()
printfn "  ✓ toBeTrue"

expectBool(false).toBeFalse()
printfn "  ✓ toBeFalse"

printfn ""

// Numeric Matchers
printfn "Numeric Matchers:"
expectNum(10).toBeGreaterThan(5)
printfn "  ✓ toBeGreaterThan"

expectNum(5).toBeLessThan(10)
printfn "  ✓ toBeLessThan"

expectNum(10).toBeGreaterThanOrEqual(10)
printfn "  ✓ toBeGreaterThanOrEqual"

expectNum(5).toBeLessThanOrEqual(5)
printfn "  ✓ toBeLessThanOrEqual"

printfn ""

// Collection Matchers
printfn "Collection Matchers:"
expectSeq([1; 2; 3]).toContain(2)
printfn "  ✓ toContain"

expectSeq([]).toBeEmpty()
printfn "  ✓ toBeEmpty"

expectSeq([1; 2; 3]).toHaveLength(3)
printfn "  ✓ toHaveLength"

printfn ""

// String Matchers
printfn "String Matchers:"
expectStr("hello world").toStartWith("hello")
printfn "  ✓ toStartWith"

expectStr("hello world").toEndWith("world")
printfn "  ✓ toEndWith"

expectStr("hello world").toContain("lo wo")
printfn "  ✓ toContain"

expectStr("").toBeEmpty()
printfn "  ✓ toBeEmpty"

printfn ""
printfn "==================================="
printfn "All matchers working! ✓"
printfn "==================================="

