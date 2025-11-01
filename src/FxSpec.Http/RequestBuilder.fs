namespace FxSpec.Http

open System
open System.Net.Http
open System.Text
open System.Text.Json
open FxSpec.Matchers

[<AutoOpen>]
module RequestBuilder =

    /// Fluent builder for constructing HTTP requests
    type RequestBuilder(client: HttpClient, method: HttpMethod, path: string) =
        let mutable headers: (string * string) list = []
        let mutable body: obj option = None
        let mutable queryParams: (string * string) list = []

        /// Add a single header to the request
        member _.WithHeader(name: string, value: string) =
            headers <- (name, value) :: headers
            RequestBuilder(client, method, path)

        /// Add multiple headers to the request
        member _.WithHeaders(hdrs: (string * string) list) =
            headers <- hdrs @ headers
            RequestBuilder(client, method, path)

        /// Add a body to the request (will be JSON serialized)
        member _.WithBody(content: obj) =
            body <- Some content
            RequestBuilder(client, method, path)

        /// Add a JSON body to the request (alias for WithBody)
        member _.WithJsonBody(content: obj) =
            body <- Some content
            RequestBuilder(client, method, path)

        /// Add a single query parameter
        member _.WithQueryParam(name: string, value: string) =
            queryParams <- (name, value) :: queryParams
            RequestBuilder(client, method, path)

        /// Add multiple query parameters
        member _.WithQueryParams(qps: (string * string) list) =
            queryParams <- qps @ queryParams
            RequestBuilder(client, method, path)

        /// Send the HTTP request and return the response
        member _.Send() =
            let uri =
                if queryParams.IsEmpty then
                    path
                else
                    let query =
                        queryParams
                        |> List.map (fun (k, v) -> $"{k}={Uri.EscapeDataString(v)}")
                        |> String.concat "&"
                    $"{path}?{query}"

            let request = new HttpRequestMessage(method, uri)

            // Add headers
            headers |> List.iter (fun (name, value) ->
                request.Headers.TryAddWithoutValidation(name, value) |> ignore
            )

            // Add body if present
            match body with
            | Some content ->
                let json = JsonSerializer.Serialize(content)
                request.Content <- new StringContent(json, Encoding.UTF8, "application/json")
            | None -> ()

            client.SendAsync(request) |> Async.AwaitTask |> Async.RunSynchronously

    /// RSpec-style request spec DSL
    type RequestSpec(server: TestServer) =

        member _.Get(path: string) =
            RequestBuilder(server.Client, HttpMethod.Get, path)

        member _.Post(path: string) =
            RequestBuilder(server.Client, HttpMethod.Post, path)

        member _.Put(path: string) =
            RequestBuilder(server.Client, HttpMethod.Put, path)

        member _.Patch(path: string) =
            RequestBuilder(server.Client, HttpMethod.Patch, path)

        member _.Delete(path: string) =
            RequestBuilder(server.Client, HttpMethod.Delete, path)

        member _.Options(path: string) =
            RequestBuilder(server.Client, HttpMethod.Options, path)

        member _.Head(path: string) =
            RequestBuilder(server.Client, HttpMethod.Head, path)

    /// Create a request spec from a test server
    let request (server: TestServer) = RequestSpec(server)

    /// Response expectation builder for fluent chaining
    type ResponseExpectations(response: HttpResponseMessage) =

        member _.ToHaveStatus(expected: StatusCodeMatcher) =
            response |> should (haveStatus expected)
            ResponseExpectations(response)

        member _.ToHaveHeader(name: string, value: string) =
            response |> should (haveHeader name value)
            ResponseExpectations(response)

        member _.ToHaveBody(expected: string) =
            response |> should (haveBody expected)
            ResponseExpectations(response)

        member _.ToHaveJsonBody(expected: obj) =
            response |> should (haveJsonBody expected)
            ResponseExpectations(response)

        member _.ToIncludeInBody(substring: string) =
            let body = getBody response
            expectStr(body).toContain(substring)
            ResponseExpectations(response)

        member _.ToMatchJson<'T>(matcher: 'T -> bool) =
            let json = getJson<'T> response
            expectBool(matcher json).toBeTrue()
            ResponseExpectations(response)

        member _.Response = response

    /// Create fluent expectation API from a response
    let expect (response: HttpResponseMessage) = ResponseExpectations(response)
