// Test runner for FxSpec.Core.Tests
// This is a simple test runner until we build the full FxSpec runner

open FxSpec.Core.Tests

[<EntryPoint>]
let main argv =
    printfn "==================================="
    printfn "FxSpec.Core Test Suite"
    printfn "==================================="
    printfn ""
    
    try
        TypesTests.runAllTests()
        printfn ""
        SpecBuilderTests.runAllTests()
        printfn ""
        printfn "==================================="
        printfn "All tests passed! ✓"
        printfn "==================================="
        0 // Success
    with ex ->
        printfn ""
        printfn "==================================="
        printfn "Test failed! ✗"
        printfn "Error: %s" ex.Message
        printfn "==================================="
        1 // Failure

