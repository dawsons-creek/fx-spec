/// BDD specifications for fluent HTTP matchers
/// Following TDD/BDD: Write tests first, then implement
module FxSpec.Http.Tests.HttpMatcherSpecs

open System.Net
open System.Net.Http
open System.Text
open FxSpec.Core
open FxSpec.Matchers
open FxSpec.Http

// Helper to create HTTP responses for testing
let createResponse statusCode headers body =
    let response = new HttpResponseMessage(statusCode)
    response.Content <- new StringContent(body, Encoding.UTF8, "text/plain")
    for (name: string, value: string) in headers do
        response.Headers.TryAddWithoutValidation(name, value) |> ignore
    response

let createJsonResponse statusCode json =
    let response = new HttpResponseMessage(statusCode)
    response.Content <- new StringContent(json, Encoding.UTF8, "application/json")
    response

[<Tests>]
let httpMatcherSpecs =
    describe "Fluent HTTP API - expectHttp()" [
        
        context "toHaveStatus(code)" [
            it "passes when status code matches exactly" (fun () ->
                let response = createResponse HttpStatusCode.OK [] ""
                expectHttp(response).toHaveStatus(200)
            )
            
            it "fails when status code does not match" (fun () ->
                let response = createResponse HttpStatusCode.NotFound [] ""
                expectThrows<AssertionException>(fun () ->
                    expectHttp(response).toHaveStatus(200)
                )
            )
            
            it "works with various status codes" (fun () ->
                let response201 = createResponse HttpStatusCode.Created [] ""
                let response400 = createResponse HttpStatusCode.BadRequest [] ""
                let response500 = createResponse HttpStatusCode.InternalServerError [] ""
                
                expectHttp(response201).toHaveStatus(201)
                expectHttp(response400).toHaveStatus(400)
                expectHttp(response500).toHaveStatus(500)
            )
        ]
        
        context "toHaveStatusOk()" [
            it "passes when status is 200 OK" (fun () ->
                let response = createResponse HttpStatusCode.OK [] ""
                expectHttp(response).toHaveStatusOk()
            )
            
            it "fails when status is not 200" (fun () ->
                let response = createResponse HttpStatusCode.Created [] ""
                expectThrows<AssertionException>(fun () ->
                    expectHttp(response).toHaveStatusOk()
                )
            )
        ]
        
        context "toHaveStatusCreated()" [
            it "passes when status is 201 Created" (fun () ->
                let response = createResponse HttpStatusCode.Created [] ""
                expectHttp(response).toHaveStatusCreated()
            )
            
            it "fails when status is not 201" (fun () ->
                let response = createResponse HttpStatusCode.OK [] ""
                expectThrows<AssertionException>(fun () ->
                    expectHttp(response).toHaveStatusCreated()
                )
            )
        ]
        
        context "toHaveStatusBadRequest()" [
            it "passes when status is 400 Bad Request" (fun () ->
                let response = createResponse HttpStatusCode.BadRequest [] ""
                expectHttp(response).toHaveStatusBadRequest()
            )
            
            it "fails when status is not 400" (fun () ->
                let response = createResponse HttpStatusCode.OK [] ""
                expectThrows<AssertionException>(fun () ->
                    expectHttp(response).toHaveStatusBadRequest()
                )
            )
        ]
        
        context "toHaveStatusNotFound()" [
            it "passes when status is 404 Not Found" (fun () ->
                let response = createResponse HttpStatusCode.NotFound [] ""
                expectHttp(response).toHaveStatusNotFound()
            )
            
            it "fails when status is not 404" (fun () ->
                let response = createResponse HttpStatusCode.OK [] ""
                expectThrows<AssertionException>(fun () ->
                    expectHttp(response).toHaveStatusNotFound()
                )
            )
        ]
        
        context "toHaveStatusUnauthorized()" [
            it "passes when status is 401 Unauthorized" (fun () ->
                let response = createResponse HttpStatusCode.Unauthorized [] ""
                expectHttp(response).toHaveStatusUnauthorized()
            )
            
            it "fails when status is not 401" (fun () ->
                let response = createResponse HttpStatusCode.OK [] ""
                expectThrows<AssertionException>(fun () ->
                    expectHttp(response).toHaveStatusUnauthorized()
                )
            )
        ]
        
        context "toHaveHeader(name, value)" [
            it "passes when header exists with exact value" (fun () ->
                let response = createResponse HttpStatusCode.OK [("X-Custom", "test-value")] ""
                expectHttp(response).toHaveHeader("X-Custom", "test-value")
            )
            
            it "fails when header does not exist" (fun () ->
                let response = createResponse HttpStatusCode.OK [] ""
                expectThrows<AssertionException>(fun () ->
                    expectHttp(response).toHaveHeader("X-Missing", "value")
                )
            )
            
            it "fails when header exists with different value" (fun () ->
                let response = createResponse HttpStatusCode.OK [("X-Custom", "actual")] ""
                expectThrows<AssertionException>(fun () ->
                    expectHttp(response).toHaveHeader("X-Custom", "expected")
                )
            )
        ]
        
        context "toHaveHeader(name) - overload checking existence" [
            it "passes when header exists with any value" (fun () ->
                let response = createResponse HttpStatusCode.OK [("X-Custom", "any-value")] ""
                expectHttp(response).toHaveHeader("X-Custom")
            )
            
            it "fails when header does not exist" (fun () ->
                let response = createResponse HttpStatusCode.OK [] ""
                expectThrows<AssertionException>(fun () ->
                    expectHttp(response).toHaveHeader("X-Missing")
                )
            )
        ]
        
        context "toHaveContentType(mediaType)" [
            it "passes when Content-Type matches" (fun () ->
                let response = createJsonResponse HttpStatusCode.OK "{}"
                expectHttp(response).toHaveContentType("application/json")
            )
            
            it "fails when Content-Type does not match" (fun () ->
                let response = createResponse HttpStatusCode.OK [] ""
                expectThrows<AssertionException>(fun () ->
                    expectHttp(response).toHaveContentType("application/json")
                )
            )
            
            it "handles Content-Type with charset" (fun () ->
                let response = createJsonResponse HttpStatusCode.OK "{}"
                // Content-Type is "application/json; charset=utf-8"
                expectHttp(response).toHaveContentType("application/json")
            )
        ]
        
        context "toHaveBody(expected)" [
            it "passes when body matches exactly" (fun () ->
                let response = createResponse HttpStatusCode.OK [] "Hello, World!"
                expectHttp(response).toHaveBody("Hello, World!")
            )
            
            it "fails when body does not match" (fun () ->
                let response = createResponse HttpStatusCode.OK [] "Actual body"
                expectThrows<AssertionException>(fun () ->
                    expectHttp(response).toHaveBody("Expected body")
                )
            )
            
            it "works with empty body" (fun () ->
                let response = createResponse HttpStatusCode.NoContent [] ""
                expectHttp(response).toHaveBody("")
            )
        ]
        
        context "toHaveBodyContaining(substring)" [
            it "passes when body contains substring" (fun () ->
                let response = createResponse HttpStatusCode.OK [] "Hello, World!"
                expectHttp(response).toHaveBodyContaining("World")
            )
            
            it "fails when body does not contain substring" (fun () ->
                let response = createResponse HttpStatusCode.OK [] "Hello, World!"
                expectThrows<AssertionException>(fun () ->
                    expectHttp(response).toHaveBodyContaining("Missing")
                )
            )
            
            it "is case-sensitive" (fun () ->
                let response = createResponse HttpStatusCode.OK [] "Hello, World!"
                expectThrows<AssertionException>(fun () ->
                    expectHttp(response).toHaveBodyContaining("world")
                )
            )
        ]
        
        context "toHaveJsonBody<'T>(expected)" [
            it "passes when JSON deserializes to expected value" (fun () ->
                let response = createJsonResponse HttpStatusCode.OK """{"name":"John","age":30}"""
                let expected = {| name = "John"; age = 30 |}
                expectHttp(response).toHaveJsonBody(expected)
            )
            
            it "fails when JSON does not match" (fun () ->
                let response = createJsonResponse HttpStatusCode.OK """{"name":"John","age":30}"""
                let expected = {| name = "Jane"; age = 25 |}
                expectThrows<AssertionException>(fun () ->
                    expectHttp(response).toHaveJsonBody(expected)
                )
            )
            
            it "works with complex objects" (fun () ->
                let response = createJsonResponse HttpStatusCode.OK """{"users":[{"id":1},{"id":2}],"count":2}"""
                let expected = {| users = [|{| id = 1 |}; {| id = 2 |}|]; count = 2 |}
                expectHttp(response).toHaveJsonBody(expected)
            )
        ]
        
        context "method chaining" [
            it "allows chaining multiple matchers" (fun () ->
                let response = createJsonResponse HttpStatusCode.OK """{"status":"success"}"""
                response.Headers.TryAddWithoutValidation("X-Request-Id", "123") |> ignore
                
                let expectation = expectHttp(response)
                expectation.toHaveStatus(200)
                expectation.toHaveHeader("X-Request-Id", "123")
                expectation.toHaveContentType("application/json")
                expectation.toHaveBodyContaining("success")
            )
            
            it "can call multiple assertions on same expectation" (fun () ->
                let response = createResponse HttpStatusCode.OK [] "test"
                let expectation = expectHttp(response)
                
                expectation.toHaveStatus(200)
                expectation.toHaveBody("test")
            )
        ]
        
        context "async scenarios" [
            itAsync "works with async HTTP calls" (async {
                // Simulate async HTTP response
                let! response = async {
                    return createResponse HttpStatusCode.OK [("X-Async", "true")] "async body"
                }
                
                let expectation = expectHttp(response)
                expectation.toHaveStatusOk()
                expectation.toHaveHeader("X-Async", "true")
                expectation.toHaveBody("async body")
            })
            
            itAsync "handles real HttpClient responses" (async {
                // Create a real HttpResponseMessage like HttpClient would return
                let response = new HttpResponseMessage(HttpStatusCode.OK)
                response.Content <- new StringContent("real content", Encoding.UTF8, "text/plain")
                
                expectHttp(response).toHaveStatusOk()
            })
        ]
        
        context "error messages" [
            it "provides clear error for status mismatch" (fun () ->
                let response = createResponse HttpStatusCode.NotFound [] ""
                try
                    expectHttp(response).toHaveStatus(200)
                    failwith "Should have thrown AssertionException"
                with
                | :? AssertionException as ex ->
                    // Error message should contain expected and actual
                    expectStr(ex.Message).toContain("200")
                    expectStr(ex.Message).toContain("404")
            )

            it "provides clear error for header mismatch" (fun () ->
                let response = createResponse HttpStatusCode.OK [("X-Custom", "actual")] ""
                try
                    expectHttp(response).toHaveHeader("X-Custom", "expected")
                    failwith "Should have thrown AssertionException"
                with
                | :? AssertionException as ex ->
                    expectStr(ex.Message).toContain("X-Custom")
                    expectStr(ex.Message).toContain("expected")
            )

            it "provides clear error for missing header" (fun () ->
                let response = createResponse HttpStatusCode.OK [] ""
                try
                    expectHttp(response).toHaveHeader("X-Missing")
                    failwith "Should have thrown AssertionException"
                with
                | :? AssertionException as ex ->
                    expectStr(ex.Message).toContain("X-Missing")
            )
        ]
    ]

[<Tests>]
let requestBuilderSpecs =
    describe "RequestBuilder - Fluent API Chaining" [

        context "WithHeader chaining" [
            itAsync "preserves single header when chained with Send" (async {
                let port = System.Random().Next(8000, 9000)
                use server = createTestServerWithPort port (fun ct -> task {
                    use listener = new System.Net.HttpListener()
                    listener.Prefixes.Add($"http://localhost:{port}/")
                    listener.Start()

                    while not ct.IsCancellationRequested do
                        try
                            let! context = listener.GetContextAsync()
                            let hasHeader = context.Request.Headers.["X-Test-Header"] <> null
                            if hasHeader then
                                context.Response.StatusCode <- 200
                                context.Response.Close()
                            else
                                context.Response.StatusCode <- 400
                                context.Response.Close()
                        with _ -> ()
                })
                server.Start()

                let response =
                    (request server)
                        .Get("/test")
                        .WithHeader("X-Test-Header", "test-value")
                        .Send()

                expectHttp(response).toHaveStatusOk()
            })

            itAsync "preserves multiple headers when chained" (async {
                let port = System.Random().Next(8000, 9000)
                use server = createTestServerWithPort port (fun ct -> task {
                    use listener = new System.Net.HttpListener()
                    listener.Prefixes.Add($"http://localhost:{port}/")
                    listener.Start()

                    while not ct.IsCancellationRequested do
                        try
                            let! context = listener.GetContextAsync()
                            let hasHeader1 = context.Request.Headers.["X-Header-1"] = "value1"
                            let hasHeader2 = context.Request.Headers.["X-Header-2"] = "value2"

                            if hasHeader1 && hasHeader2 then
                                context.Response.StatusCode <- 200
                            else
                                context.Response.StatusCode <- 400
                            context.Response.Close()
                        with _ -> ()
                })
                server.Start()

                let response =
                    (request server)
                        .Get("/test")
                        .WithHeader("X-Header-1", "value1")
                        .WithHeader("X-Header-2", "value2")
                        .Send()

                expectHttp(response).toHaveStatusOk()
            })

            itAsync "preserves headers when using WithHeaders" (async {
                let port = System.Random().Next(8000, 9000)
                use server = createTestServerWithPort port (fun ct -> task {
                    use listener = new System.Net.HttpListener()
                    listener.Prefixes.Add($"http://localhost:{port}/")
                    listener.Start()

                    while not ct.IsCancellationRequested do
                        try
                            let! context = listener.GetContextAsync()
                            let hasHeader1 = context.Request.Headers.["X-Multi-1"] = "m1"
                            let hasHeader2 = context.Request.Headers.["X-Multi-2"] = "m2"

                            if hasHeader1 && hasHeader2 then
                                context.Response.StatusCode <- 200
                            else
                                context.Response.StatusCode <- 400
                            context.Response.Close()
                        with _ -> ()
                })
                server.Start()

                let response =
                    (request server)
                        .Get("/test")
                        .WithHeaders([("X-Multi-1", "m1"); ("X-Multi-2", "m2")])
                        .Send()

                expectHttp(response).toHaveStatusOk()
            })
        ]

        context "WithBody chaining" [
            itAsync "preserves body when chained with headers" (async {
                let port = System.Random().Next(8000, 9000)
                use server = createTestServerWithPort port (fun ct -> task {
                    use listener = new System.Net.HttpListener()
                    listener.Prefixes.Add($"http://localhost:{port}/")
                    listener.Start()

                    while not ct.IsCancellationRequested do
                        try
                            let! context = listener.GetContextAsync()
                            use reader = new System.IO.StreamReader(context.Request.InputStream)
                            let! body = reader.ReadToEndAsync()
                            let hasHeader = context.Request.Headers.["X-Custom"] = "test"

                            if hasHeader && body.Contains("\"name\"") then
                                context.Response.StatusCode <- 200
                            else
                                context.Response.StatusCode <- 400
                            context.Response.Close()
                        with _ -> ()
                })
                server.Start()

                let response =
                    (request server)
                        .Post("/test")
                        .WithHeader("X-Custom", "test")
                        .WithBody({| name = "John" |})
                        .Send()

                expectHttp(response).toHaveStatusOk()
            })
        ]

        context "WithQueryParam chaining" [
            itAsync "preserves single query parameter" (async {
                let port = System.Random().Next(8000, 9000)
                use server = createTestServerWithPort port (fun ct -> task {
                    use listener = new System.Net.HttpListener()
                    listener.Prefixes.Add($"http://localhost:{port}/")
                    listener.Start()

                    while not ct.IsCancellationRequested do
                        try
                            let! context = listener.GetContextAsync()
                            let hasParam = context.Request.QueryString.["key"] = "value"

                            if hasParam then
                                context.Response.StatusCode <- 200
                            else
                                context.Response.StatusCode <- 400
                            context.Response.Close()
                        with _ -> ()
                })
                server.Start()

                let response =
                    (request server)
                        .Get("/test")
                        .WithQueryParam("key", "value")
                        .Send()

                expectHttp(response).toHaveStatusOk()
            })

            itAsync "preserves multiple query parameters when chained" (async {
                let port = System.Random().Next(8000, 9000)
                use server = createTestServerWithPort port (fun ct -> task {
                    use listener = new System.Net.HttpListener()
                    listener.Prefixes.Add($"http://localhost:{port}/")
                    listener.Start()

                    while not ct.IsCancellationRequested do
                        try
                            let! context = listener.GetContextAsync()
                            let hasParam1 = context.Request.QueryString.["foo"] = "bar"
                            let hasParam2 = context.Request.QueryString.["baz"] = "qux"

                            if hasParam1 && hasParam2 then
                                context.Response.StatusCode <- 200
                            else
                                context.Response.StatusCode <- 400
                            context.Response.Close()
                        with _ -> ()
                })
                server.Start()

                let response =
                    (request server)
                        .Get("/test")
                        .WithQueryParam("foo", "bar")
                        .WithQueryParam("baz", "qux")
                        .Send()

                expectHttp(response).toHaveStatusOk()
            })
        ]

        context "complex chaining scenarios" [
            itAsync "preserves all configurations when fully chained" (async {
                let port = System.Random().Next(8000, 9000)
                use server = createTestServerWithPort port (fun ct -> task {
                    use listener = new System.Net.HttpListener()
                    listener.Prefixes.Add($"http://localhost:{port}/")
                    listener.Start()

                    while not ct.IsCancellationRequested do
                        try
                            let! context = listener.GetContextAsync()
                            use reader = new System.IO.StreamReader(context.Request.InputStream)
                            let! body = reader.ReadToEndAsync()

                            let hasHeader = context.Request.Headers.["Authorization"] = "Bearer token123"
                            let hasParam = context.Request.QueryString.["filter"] = "active"
                            let hasBody = body.Contains("\"data\"")

                            if hasHeader && hasParam && hasBody then
                                context.Response.StatusCode <- 201
                            else
                                context.Response.StatusCode <- 400
                            context.Response.Close()
                        with _ -> ()
                })
                server.Start()

                let response =
                    (request server)
                        .Post("/api/items")
                        .WithHeader("Authorization", "Bearer token123")
                        .WithQueryParam("filter", "active")
                        .WithJsonBody({| data = "test" |})
                        .Send()

                expectHttp(response).toHaveStatusCreated()
            })
        ]
    ]
