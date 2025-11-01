namespace FxSpec.Matchers

// ExceptionMatchers.fs is now deprecated.
// All exception testing functionality has been moved to Assertions.fs
// with the new fluent API:
//
// Old style: expect (fun () -> failwith "error") |> should raiseException<Exception>
// New style: expectThrows<Exception>(fun () -> failwith "error")
//
// Available functions in Assertions module:
// - expectThrows<'TException>(f)
// - expectThrowsWithMessage(message, f)
// - expectThrowsContaining(substring, f)
// - expectNotToThrow(f)
//
// This file is kept as a placeholder to prevent confusion during migration.


