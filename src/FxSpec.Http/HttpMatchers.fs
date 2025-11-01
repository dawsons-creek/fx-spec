namespace FxSpec.Http

open System
open System.Net.Http
open System.Text.Json
open FxSpec.Matchers

[<AutoOpen>]
module HttpMatchers =

    // ===== OLD API (Deprecated - for backwards compatibility) =====
    
    /// Status code matchers for fluent HTTP assertions
    type StatusCodeMatcher =
        | Be of int
        | BeOk
        | BeCreated
        | BeNoContent
        | BeBadRequest
        | BeNotFound
        | BeUnauthorized
        | BeUnprocessableEntity
        | BeInternalServerError

    /// Get the integer status code from a matcher
    let private getStatusCode = function
        | Be code -> code
        | BeOk -> 200
        | BeCreated -> 201
        | BeNoContent -> 204
        | BeBadRequest -> 400
        | BeNotFound -> 404
        | BeUnauthorized -> 401
        | BeUnprocessableEntity -> 422
        | BeInternalServerError -> 500

    /// Response matcher type
    type ResponseMatcher =
        | HaveStatus of StatusCodeMatcher
        | HaveHeader of string * string
        | HaveBody of string
        | HaveJsonBody of obj

    /// Apply a response matcher to an HTTP response (deprecated)
    let should (matcher: ResponseMatcher) (response: HttpResponseMessage) =
        match matcher with
        | HaveStatus statusMatcher ->
            let expected = getStatusCode statusMatcher
            let actual = int response.StatusCode
            if expected <> actual then
                failwith $"Expected status {expected}, got {actual}"

        | HaveHeader (name, value) ->
            let headers = response.Headers
            let contentHeaders = response.Content.Headers

            let found =
                if headers.Contains(name) then
                    headers.GetValues(name) |> Seq.tryFind ((=) value)
                elif contentHeaders.Contains(name) then
                    contentHeaders.GetValues(name) |> Seq.tryFind ((=) value)
                else
                    None

            if Option.isNone found then
                failwith $"Expected header '{name}: {value}' not found"

        | HaveBody expectedBody ->
            let actualBody = response.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously
            if expectedBody <> actualBody then
                failwith "Response body mismatch"

        | HaveJsonBody expected ->
            let actualBody = response.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously
            let expectedJson = JsonSerializer.Serialize(expected, JsonSerializerOptions(WriteIndented = false))
            if expectedJson <> actualBody then
                failwith "Response JSON body mismatch"

    // Fluent matcher constructors (deprecated)
    let haveStatus status = HaveStatus status
    let haveHeader name value = HaveHeader (name, value)
    let haveBody body = HaveBody body
    let haveJsonBody body = HaveJsonBody body

    // Status code helpers (deprecated)
    let beOk = BeOk
    let beCreated = BeCreated
    let beNoContent = BeNoContent
    let beBadRequest = BeBadRequest
    let beNotFound = BeNotFound
    let beUnauthorized = BeUnauthorized
    let beUnprocessableEntity = BeUnprocessableEntity
    let beInternalServerError = BeInternalServerError
    let be code = Be code

    // Quick assertion helpers for common patterns (deprecated)
    let shouldBeOk response =
        response |> should (haveStatus beOk)
        response

    let shouldBeCreated response =
        response |> should (haveStatus beCreated)
        response

    let shouldBeNotFound response =
        response |> should (haveStatus beNotFound)
        response

    let shouldBeBadRequest response =
        response |> should (haveStatus beBadRequest)
        response

    let shouldBeUnauthorized response =
        response |> should (haveStatus beUnauthorized)
        response

    let shouldHaveHeader name value response =
        response |> should (haveHeader name value)
        response

    let shouldHaveJsonBody expected response =
        response |> should (haveJsonBody expected)
        response

    // ===== NEW FLUENT API (expectHttp) =====
    
    /// Fluent HTTP response expectation type
    /// Follows the same pattern as ResultExpectation, OptionExpectation, etc.
    type HttpResponseExpectation(response: HttpResponseMessage) =
        member _.Response = response
        
        // Status code matchers
        member _.toHaveStatus(expected: int) =
            let actual = int response.StatusCode
            if actual <> expected then
                raise (AssertionException($"Expected HTTP status {expected}, but got {actual}", Some (box expected), Some (box actual)))
        
        member _.toHaveStatusOk() =
            let actual = int response.StatusCode
            if actual <> 200 then
                raise (AssertionException($"Expected HTTP status 200 (OK), but got {actual}", Some (box 200), Some (box actual)))
        
        member _.toHaveStatusCreated() =
            let actual = int response.StatusCode
            if actual <> 201 then
                raise (AssertionException($"Expected HTTP status 201 (Created), but got {actual}", Some (box 201), Some (box actual)))
        
        member _.toHaveStatusNoContent() =
            let actual = int response.StatusCode
            if actual <> 204 then
                raise (AssertionException($"Expected HTTP status 204 (No Content), but got {actual}", Some (box 204), Some (box actual)))
        
        member _.toHaveStatusBadRequest() =
            let actual = int response.StatusCode
            if actual <> 400 then
                raise (AssertionException($"Expected HTTP status 400 (Bad Request), but got {actual}", Some (box 400), Some (box actual)))
        
        member _.toHaveStatusUnauthorized() =
            let actual = int response.StatusCode
            if actual <> 401 then
                raise (AssertionException($"Expected HTTP status 401 (Unauthorized), but got {actual}", Some (box 401), Some (box actual)))
        
        member _.toHaveStatusNotFound() =
            let actual = int response.StatusCode
            if actual <> 404 then
                raise (AssertionException($"Expected HTTP status 404 (Not Found), but got {actual}", Some (box 404), Some (box actual)))
        
        member _.toHaveStatusUnprocessableEntity() =
            let actual = int response.StatusCode
            if actual <> 422 then
                raise (AssertionException($"Expected HTTP status 422 (Unprocessable Entity), but got {actual}", Some (box 422), Some (box actual)))
        
        member _.toHaveStatusInternalServerError() =
            let actual = int response.StatusCode
            if actual <> 500 then
                raise (AssertionException($"Expected HTTP status 500 (Internal Server Error), but got {actual}", Some (box 500), Some (box actual)))
        
        // Header matchers
        member _.toHaveHeader(name: string, value: string) =
            let headers = response.Headers
            let contentHeaders = response.Content.Headers
            
            let found =
                if headers.Contains(name) then
                    headers.GetValues(name) |> Seq.tryFind ((=) value)
                elif contentHeaders.Contains(name) then
                    contentHeaders.GetValues(name) |> Seq.tryFind ((=) value)
                else
                    None
            
            match found with
            | Some _ -> ()
            | None ->
                // Check if header exists with different value
                let headerExists = headers.Contains(name) || contentHeaders.Contains(name)
                if headerExists then
                    let actualValues =
                        if headers.Contains(name) then
                            headers.GetValues(name) |> String.concat ", "
                        else
                            contentHeaders.GetValues(name) |> String.concat ", "
                    raise (AssertionException($"Expected header '{name}' to have value '{value}', but got '{actualValues}'", Some (box value), Some (box actualValues)))
                else
                    raise (AssertionException($"Expected header '{name}' with value '{value}' not found in response", Some (box $"{name}: {value}"), None))
        
        member _.toHaveHeader(name: string) =
            let headers = response.Headers
            let contentHeaders = response.Content.Headers
            
            let exists = headers.Contains(name) || contentHeaders.Contains(name)
            if not exists then
                raise (AssertionException($"Expected header '{name}' not found in response", Some (box name), None))
        
        member _.toHaveContentType(expectedMediaType: string) =
            let contentType = response.Content.Headers.ContentType
            if contentType = null then
                raise (AssertionException($"Expected Content-Type '{expectedMediaType}', but Content-Type header is missing", Some (box expectedMediaType), None))
            
            // Compare media type without charset/boundary parameters
            let actualMediaType = contentType.MediaType
            if actualMediaType <> expectedMediaType then
                raise (AssertionException($"Expected Content-Type '{expectedMediaType}', but got '{actualMediaType}'", Some (box expectedMediaType), Some (box actualMediaType)))
        
        // Body matchers
        member _.toHaveBody(expected: string) =
            let actual = response.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously
            if actual <> expected then
                raise (AssertionException($"Expected body:\n{expected}\n\nActual body:\n{actual}", Some (box expected), Some (box actual)))
        
        member _.toHaveBodyContaining(substring: string) =
            let actual = response.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously
            if not (actual.Contains(substring)) then
                raise (AssertionException($"Expected body to contain '{substring}', but it was not found.\n\nActual body:\n{actual}", Some (box substring), Some (box actual)))
        
        member _.toHaveJsonBody<'T>(expected: 'T) =
            let actualBody = response.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously
            let expectedJson = JsonSerializer.Serialize(expected, JsonSerializerOptions(WriteIndented = false))
            let actualJson = 
                try
                    // Re-serialize to normalize formatting
                    let parsed = JsonSerializer.Deserialize<'T>(actualBody)
                    JsonSerializer.Serialize(parsed, JsonSerializerOptions(WriteIndented = false))
                with ex ->
                    raise (AssertionException($"Failed to parse response body as JSON: {ex.Message}\n\nActual body:\n{actualBody}", None, Some (box actualBody)))
            
            if actualJson <> expectedJson then
                raise (AssertionException($"Expected JSON body:\n{expectedJson}\n\nActual JSON body:\n{actualJson}", Some (box expectedJson), Some (box actualJson)))
    
    /// Create a fluent HTTP expectation
    let expectHttp (response: HttpResponseMessage) =
        HttpResponseExpectation(response)
