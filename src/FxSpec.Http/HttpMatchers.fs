namespace FxSpec.Http

open System
open System.Net.Http
open System.Text.Json
open FxSpec.Matchers

[<AutoOpen>]
module HttpMatchers =

    // Legacy `should (haveStatus ...)` API has been removed.
    // Use the modern expectHttp() API instead:
    //
    // Old: response |> should (haveStatus beOk)
    // New: expectHttp(response).toHaveStatusOk()
    //
    // Old: response |> should (haveHeader "X-Foo" "bar")
    // New: expectHttp(response).toHaveHeader("X-Foo", "bar")
    //
    // Old: response |> should (haveBody "content")
    // New: expectHttp(response).toHaveBody("content")

    // ===== FLUENT API (expectHttp) =====
    
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
