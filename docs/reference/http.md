# HTTP Testing

FX.Spec provides a fluent API for testing HTTP responses with comprehensive matchers for status codes, headers, and body content.

## Overview

The `expectHttp()` function creates an HTTP response expectation that provides type-safe, chainable matchers specifically designed for testing web APIs and HTTP services.

```fsharp
open FX.Spec.Http

itAsync "tests API response" (async {
    let! response = client.GetAsync("/api/users")
    expectHttp(response).toHaveStatusOk()
})
```

## Status Code Matchers

### toHaveStatus(code: int)

Asserts that the HTTP response has the specified status code.

```fsharp
expectHttp(response).toHaveStatus(200)
expectHttp(response).toHaveStatus(404)
expectHttp(response).toHaveStatus(500)
```

### Semantic Status Matchers

FX.Spec provides semantic matchers for common HTTP status codes:

| Matcher | Status Code | Description |
|---------|-------------|-------------|
| `toHaveStatusOk()` | 200 | OK |
| `toHaveStatusCreated()` | 201 | Created |
| `toHaveStatusNoContent()` | 204 | No Content |
| `toHaveStatusBadRequest()` | 400 | Bad Request |
| `toHaveStatusUnauthorized()` | 401 | Unauthorized |
| `toHaveStatusNotFound()` | 404 | Not Found |
| `toHaveStatusUnprocessableEntity()` | 422 | Unprocessable Entity |
| `toHaveStatusInternalServerError()` | 500 | Internal Server Error |

```fsharp
describe "User API" [
    itAsync "creates user successfully" (async {
        let! response = client.PostAsync("/api/users", content)
        expectHttp(response).toHaveStatusCreated()
    })
    
    itAsync "returns 404 for missing user" (async {
        let! response = client.GetAsync("/api/users/999")
        expectHttp(response).toHaveStatusNotFound()
    })
    
    itAsync "requires authentication" (async {
        let! response = client.GetAsync("/api/admin")
        expectHttp(response).toHaveStatusUnauthorized()
    })
]
```

## Header Matchers

### toHaveHeader(name: string, value: string)

Asserts that the response contains a header with the specified name and exact value.

```fsharp
expectHttp(response).toHaveHeader("Content-Type", "application/json")
expectHttp(response).toHaveHeader("X-Custom-Header", "custom-value")
```

### toHaveHeader(name: string)

Asserts that the response contains a header with the specified name (regardless of value).

```fsharp
expectHttp(response).toHaveHeader("ETag")
expectHttp(response).toHaveHeader("X-Request-Id")
```

### toHaveContentType(mediaType: string)

Convenience method for asserting the Content-Type header. Automatically handles charset and other parameters.

```fsharp
expectHttp(response).toHaveContentType("application/json")
expectHttp(response).toHaveContentType("text/html")
expectHttp(response).toHaveContentType("application/xml")
```

**Example:**

```fsharp
describe "API Content Types" [
    itAsync "returns JSON for API requests" (async {
        let! response = client.GetAsync("/api/users")
        expectHttp(response)
            .toHaveStatusOk()
            .toHaveContentType("application/json")
    })
    
    itAsync "includes custom headers" (async {
        let! response = client.GetAsync("/api/users")
        expectHttp(response)
            .toHaveHeader("X-API-Version", "1.0")
            .toHaveHeader("X-Rate-Limit-Remaining")
    })
]
```

## Body Matchers

### toHaveBody(expected: string)

Asserts that the response body exactly matches the expected string.

```fsharp
expectHttp(response).toHaveBody("Hello, World!")
expectHttp(response).toHaveBody("")  // Empty body
```

### toHaveBodyContaining(substring: string)

Asserts that the response body contains the specified substring (case-sensitive).

```fsharp
expectHttp(response).toHaveBodyContaining("success")
expectHttp(response).toHaveBodyContaining("error")
expectHttp(response).toHaveBodyContaining("user123")
```

### toHaveJsonBody<'T>(expected: 'T)

Asserts that the response body is valid JSON that deserializes to the expected value. Automatically normalizes JSON formatting for comparison.

```fsharp
// Anonymous records
expectHttp(response).toHaveJsonBody({| name = "John"; age = 30 |})

// Complex objects
expectHttp(response).toHaveJsonBody({| 
    users = [| {| id = 1 |}; {| id = 2 |} |]
    count = 2 
|})

// Custom types
type User = { Id: int; Name: string; Email: string }
expectHttp(response).toHaveJsonBody({ Id = 1; Name = "John"; Email = "john@example.com" })
```

**Example:**

```fsharp
describe "API Response Bodies" [
    itAsync "returns JSON user data" (async {
        let! response = client.GetAsync("/api/users/1")
        expectHttp(response)
            .toHaveStatusOk()
            .toHaveJsonBody({| 
                id = 1
                name = "John Doe"
                email = "john@example.com"
            |})
    })
    
    itAsync "returns error message" (async {
        let! response = client.GetAsync("/api/users/invalid")
        expectHttp(response)
            .toHaveStatusBadRequest()
            .toHaveBodyContaining("Invalid user ID")
    })
]
```

## Multiple Assertions

You can call multiple assertions on the same expectation:

```fsharp
describe "Complete API Test" [
    itAsync "validates full response" (async {
        let! response = client.PostAsync("/api/users", content)
        
        let expectation = expectHttp(response)
        expectation.toHaveStatusCreated()
        expectation.toHaveHeader("Location", "/api/users/123")
        expectation.toHaveContentType("application/json")
        expectation.toHaveBodyContaining("\"id\":123")
        expectation.toHaveJsonBody({| id = 123; name = "John" |})
    })
]
```

## Async HTTP Testing

FxSpec's `itAsync` works seamlessly with F#'s `async` workflows and .NET's `Task`-based APIs:

```fsharp
open System.Net.Http
open System.Text
open FX.Spec.Http

describe "User API Integration Tests" [
    let client = new HttpClient(BaseAddress = Uri("http://localhost:5000"))
    
    itAsync "creates and retrieves user" (async {
        // Create user
        let json = """{"name":"Jane","email":"jane@example.com"}"""
        let content = new StringContent(json, Encoding.UTF8, "application/json")
        let! createResponse = client.PostAsync("/api/users", content) |> Async.AwaitTask
        
        expectHttp(createResponse)
            .toHaveStatusCreated()
            .toHaveHeader("Location")
        
        // Retrieve user
        let! getResponse = client.GetAsync("/api/users/1") |> Async.AwaitTask
        expectHttp(getResponse)
            .toHaveStatusOk()
            .toHaveJsonBody({| id = 1; name = "Jane"; email = "jane@example.com" |})
    })
    
    itAsync "handles validation errors" (async {
        let json = """{"name":""}"""  // Invalid: empty name
        let content = new StringContent(json, Encoding.UTF8, "application/json")
        let! response = client.PostAsync("/api/users", content) |> Async.AwaitTask
        
        expectHttp(response)
            .toHaveStatusBadRequest()
            .toHaveBodyContaining("Name is required")
    })
]
```

## Error Messages

FX.Spec provides clear, actionable error messages when HTTP assertions fail:

### Status Code Mismatch

```
Expected HTTP status 200, but got 404
Expected: 200
Actual: 404
```

### Header Mismatch

```
Expected header 'Content-Type' to have value 'application/json', but got 'text/html'
Expected: application/json
Actual: text/html
```

### Missing Header

```
Expected header 'X-Custom-Header' not found in response
Expected: X-Custom-Header
```

### Body Mismatch

```
Expected body to contain 'success', but it was not found.

Actual body:
{"status":"error","message":"Something went wrong"}
```

### JSON Body Mismatch

```
Expected JSON body:
{"id":1,"name":"John"}

Actual JSON body:
{"id":1,"name":"Jane"}
```

## Migration Guide: Old API to New API

FX.Spec previously used a `should`-based API. The new `expectHttp()` API is more consistent with the rest of the framework and provides better error messages.

### Old API (Deprecated)

```fsharp
// Old style - still works but deprecated
response |> should (haveStatus beOk)
response |> should (haveHeader "Content-Type" "application/json")
response |> shouldBeOk
response |> shouldHaveJsonBody expected
```

### New API (Recommended)

```fsharp
// New style - fluent and consistent
expectHttp(response).toHaveStatusOk()
expectHttp(response).toHaveHeader("Content-Type", "application/json")
expectHttp(response).toHaveStatusOk()
expectHttp(response).toHaveJsonBody(expected)
```

### Migration Steps

1. Replace `response |> should (haveStatus beOk)` with `expectHttp(response).toHaveStatusOk()`
2. Replace `response |> shouldBeOk` with `expectHttp(response).toHaveStatusOk()`
3. Replace `response |> should (haveHeader name value)` with `expectHttp(response).toHaveHeader(name, value)`
4. Replace `response |> shouldHaveJsonBody expected` with `expectHttp(response).toHaveJsonBody(expected)`

The old API will be maintained for backward compatibility but may be removed in a future major version.

## Best Practices

### 1. Use Semantic Status Matchers

Prefer semantic matchers over numeric codes for better readability:

```fsharp
// Good
expectHttp(response).toHaveStatusOk()
expectHttp(response).toHaveStatusNotFound()

// Works, but less expressive
expectHttp(response).toHaveStatus(200)
expectHttp(response).toHaveStatus(404)
```

### 2. Check Status Before Body

Always verify the status code before asserting on body content:

```fsharp
// Good
expectHttp(response).toHaveStatusOk()
expectHttp(response).toHaveBodyContaining("success")

// Risky - might fail on wrong status
expectHttp(response).toHaveBodyContaining("success")
```

### 3. Use toHaveJsonBody for JSON APIs

For JSON responses, use `toHaveJsonBody` instead of string comparisons:

```fsharp
// Good - type-safe, handles formatting
expectHttp(response).toHaveJsonBody({| id = 1; name = "John" |})

// Fragile - whitespace sensitive
expectHttp(response).toHaveBody("""{"id":1,"name":"John"}""")
```

### 4. Store Expectation for Multiple Assertions

When making multiple assertions, store the expectation:

```fsharp
let expectation = expectHttp(response)
expectation.toHaveStatusOk()
expectation.toHaveContentType("application/json")
expectation.toHaveBodyContaining("data")
```

## See Also

- [Async Support](../dsl-api.md#async-tests) - Using `itAsync` for asynchronous tests
- [Core Matchers](matchers/core.md) - General-purpose matchers
- [Quick Start](../quick-start.md) - Getting started with FX.Spec
