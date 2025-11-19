namespace FxSpec.Http

/// Custom exception for HTTP assertion failures
exception HttpAssertionException of string

/// Helper module for HTTP assertions
module Assert =
    /// Throws an HttpAssertionException if the condition is false
    let isTrue condition message =
        if not condition then
            raise (HttpAssertionException message)

    /// Asserts that two values are equal
    let equal expected actual message =
        if expected <> actual then
            raise (HttpAssertionException(sprintf "%s\nExpected: %A\nActual: %A" message expected actual))
