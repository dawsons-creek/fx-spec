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
