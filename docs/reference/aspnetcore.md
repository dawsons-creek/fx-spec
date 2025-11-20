# ASP.NET Core Testing

FX.Spec.AspNetCore provides a set of functional helpers for integration testing ASP.NET Core applications by manipulating `HttpContext` directly. This allows for fast, in-memory testing of handlers and middleware without spinning up a full test server.

## Overview

The library focuses on a "Context-first" approach:

1.  **Create** a test `HttpContext`.
2.  **Configure** the request (method, path, headers, body).
3.  **Execute** your handler/middleware against the context.
4.  **Inspect** the response on the context.

```fsharp
open FX.Spec.AspNetCore
open FX.Spec.Json

it "creates a user" (fun () ->
    let ctx = 
        createTestContext()
        |> setRequest "POST" "/users"
        |> setJsonBody (toJson {| name = "John" |})

    // Execute your handler (implementation specific)
    // do! myHandler ctx

    expect(ctx.Response.StatusCode).toEqual(201)
    
    let response = parseResponseJson<User>(ctx)
    expect(response.Name).toEqual("John")
)
```

## Installation

Add the ASP.NET Core package to your test project:

```bash
dotnet add package FX.Spec.AspNetCore
```

## Context Helpers

The `FxContextHelpers` module is automatically opened when you import the namespace.

### Creating Contexts

#### `createTestContext()`

Creates a lightweight `DefaultHttpContext` with:
-   Initialized `RequestServices` (empty `ServiceCollection`).
-   Buffered `Response.Body` (MemoryStream).
-   Logging configured (Debug level).

#### `createTestContextWith(configureServices)`

Creates a context with custom dependency injection services.

```fsharp
let ctx = createTestContextWith (fun services ->
    services.AddSingleton<IUserRepository, MockUserRepository>() |> ignore
)
```

### Request Configuration

These functions match the fluent style, returning the modified `HttpContext`.

#### `setRequest(method: string) (path: string)`

Sets the HTTP method and request path.

```fsharp
ctx |> setRequest "GET" "/api/items/1"
```

#### `setJsonBody(json: string)`

Sets the request body content and sets `Content-Type` to `application/json`. Use with `toJson` helper from `FX.Spec.Json`.

```fsharp
ctx |> setJsonBody (toJson {| id = 1; name = "Test" |})
```

#### `setRequestBody(content: string)`

Sets the raw string content of the request body without modifying headers.

#### Header Helpers

*   `setAcceptHeader(mediaType: string)`
*   `setContentType(mediaType: string)`

### Response Inspection

#### `getResponseBody(ctx)`

Reads the response stream as a UTF-8 string without consuming it (resets stream position). Safe to call multiple times.

```fsharp
let body = getResponseBody ctx
expectStr(body).toContain("error")
```

#### `parseResponseJson<'T>(ctx)`

Deserializes the response body to type `'T` using `FX.Spec.Json` defaults (F# type support enabled).

```fsharp
let result = parseResponseJson<MyResult>(ctx)
expect(result.Id).toEqual(1)
```

## Router Helpers

If you are using the `FX.Core` functional router, you can use `FxRouterHelpers` to dispatch requests directly to your route definitions.

### `executeRoute`

Matches the request against a `Router` and executes the corresponding handler.

**Signature:**
`executeRoute (router: Router) (method: HttpMethod) (path: string) (ctx: HttpContext) : Task<HttpContext>`

```fsharp
open FX.Core

// Define your router
let appRouter = 
    router [
        get "/users" listUsers
        post "/users" createUser
    ]

// Test a route
itAsync "routes to create user" (async {
    let ctx = 
        createTestContext() 
        |> setRequest "POST" "/users"
    
    // Execute the router against the context
    let! resultCtx = 
        executeRoute appRouter HttpMethod.POST "/users" ctx 
        |> Async.AwaitTask
    
    expect(resultCtx.Response.StatusCode).toEqual(200)
})
```
