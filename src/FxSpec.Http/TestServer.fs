namespace FxSpec.Http

open System
open System.Net.Http
open System.Text
open System.Text.Json
open System.Threading
open System.Threading.Tasks

/// Configuration for test server
type TestServerConfig = {
    Port: int option
    StartupDelay: int
}

/// Test server for HTTP integration testing
/// Manages server lifecycle and provides HTTP client for testing
type TestServer private (port: int, runServer: CancellationToken -> Task, cts: CancellationTokenSource, startupDelayMs: int) =
    let mutable started = false
    let mutable serverTask: Task option = None
    let mutable httpClient: HttpClient option = None

    member _.Port = port
    member _.BaseUrl = $"http://localhost:{port}"

    member _.Client =
        match httpClient with
        | Some client -> client
        | None ->
            let client = new HttpClient(BaseAddress = Uri($"http://localhost:{port}"))
            httpClient <- Some client
            client

    member _.Start() =
        if not started then
            serverTask <- Some (Task.Run(fun () -> runServer cts.Token))
            Task.Delay(startupDelayMs).Wait()
            started <- true

    member _.Stop() =
        if started then
            cts.Cancel()
            match serverTask with
            | Some task ->
                try
                    task.Wait(TimeSpan.FromSeconds(5.0)) |> ignore
                with _ -> ()
            | None -> ()
            started <- false

    interface IDisposable with
        member this.Dispose() =
            this.Stop()
            match httpClient with
            | Some client -> client.Dispose()
            | None -> ()
            cts.Dispose()

    /// Create a test server with a custom run function
    /// runServer: CancellationToken -> Task
    static member Create(runServer: CancellationToken -> Task, ?port: int, ?startupDelay: int) =
        let actualPort = defaultArg port (Random().Next(5000, 9000))
        let actualStartupDelay = defaultArg startupDelay 100
        let cts = new CancellationTokenSource()
        new TestServer(actualPort, runServer, cts, actualStartupDelay)

[<AutoOpen>]
module TestServerHelpers =

    /// Create a test server with a custom run function
    let createTestServer runServer = TestServer.Create(runServer)

    /// Create a test server with port specification
    let createTestServerWithPort port runServer = TestServer.Create(runServer, port)

    // HTTP request helpers - synchronous wrappers for cleaner test code

    /// Send GET request
    let get (path: string) (client: HttpClient) =
        client.GetAsync(path) |> Async.AwaitTask |> Async.RunSynchronously

    /// Send POST request with JSON body
    let post (path: string) (body: obj) (client: HttpClient) =
        let json = JsonSerializer.Serialize(body)
        let content = new StringContent(json, Encoding.UTF8, "application/json")
        client.PostAsync(path, content) |> Async.AwaitTask |> Async.RunSynchronously

    /// Send PUT request with JSON body
    let put (path: string) (body: obj) (client: HttpClient) =
        let json = JsonSerializer.Serialize(body)
        let content = new StringContent(json, Encoding.UTF8, "application/json")
        client.PutAsync(path, content) |> Async.AwaitTask |> Async.RunSynchronously

    /// Send DELETE request
    let delete (path: string) (client: HttpClient) =
        client.DeleteAsync(path) |> Async.AwaitTask |> Async.RunSynchronously

    /// Send PATCH request with JSON body
    let patch (path: string) (body: obj) (client: HttpClient) =
        let json = JsonSerializer.Serialize(body)
        let content = new StringContent(json, Encoding.UTF8, "application/json")
        client.PatchAsync(path, content) |> Async.AwaitTask |> Async.RunSynchronously

    // Response helpers

    /// Get response body as string
    let getBody (response: HttpResponseMessage) =
        response.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously

    /// Deserialize response body as JSON
    let getJson<'T> (response: HttpResponseMessage) =
        let body = getBody response
        JsonSerializer.Deserialize<'T>(body)

    /// Get response status code as int
    let getStatus (response: HttpResponseMessage) =
        int response.StatusCode

    /// Get response header value
    let getHeader (name: string) (response: HttpResponseMessage) =
        if response.Headers.Contains(name) then
            response.Headers.GetValues(name) |> Seq.head |> Some
        elif response.Content.Headers.Contains(name) then
            response.Content.Headers.GetValues(name) |> Seq.head |> Some
        else
            None
