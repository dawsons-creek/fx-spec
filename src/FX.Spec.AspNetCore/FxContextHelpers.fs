namespace FX.Spec.AspNetCore

open System
open System.IO
open System.Text
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Primitives
open FX.Spec.Json

[<AutoOpen>]
module FxContextHelpers =

    let private ensureLeadingSlash (path: string) =
        if String.IsNullOrWhiteSpace(path) then
            "/"
        elif path.StartsWith("/", StringComparison.Ordinal) then
            path
        else
            "/" + path

    let internal replaceBodyStream (streamFactory: unit -> Stream) (ctx: HttpContext) =
        if isNull ctx then
            invalidArg (nameof ctx) "HttpContext must not be null."
        elif isNull ctx.Request then
            ctx
        else
            ctx.Request.Body <- streamFactory ()
            ctx

    /// Create a test HttpContext with DI, logging, and in-memory response body configured.
    /// Allows callers to customize the service collection prior to building the provider.
    let createTestContextWith (configureServices: IServiceCollection -> unit) =
        let services = ServiceCollection()

        services.AddLogging(fun builder ->
            builder.ClearProviders() |> ignore
            builder.SetMinimumLevel(LogLevel.Debug) |> ignore
            builder.AddFilter(fun _ _ -> true) |> ignore)
        |> ignore

        configureServices services

        let serviceProvider = services.BuildServiceProvider()

        let ctx = DefaultHttpContext()
        ctx.RequestServices <- serviceProvider
        ctx.Request.Protocol <- "HTTP/1.1"
        ctx.Response.Body <- new MemoryStream()
        ctx

    /// Create a test HttpContext with default service configuration.
    let createTestContext () = createTestContextWith ignore

    /// Set request method and path on an HttpContext, returning the same context for fluent usage.
    let setRequest (method: string) (path: string) (ctx: HttpContext) =
        if String.IsNullOrWhiteSpace(method) then
            invalidArg (nameof method) "HTTP method must be provided."

        let normalizedPath = ensureLeadingSlash path
        ctx.Request.Method <- method.ToUpperInvariant()
        ctx.Request.Path <- PathString(normalizedPath)
        ctx

    /// Apply an Accept header to the request.
    let setAcceptHeader (mediaType: string) (ctx: HttpContext) =
        if String.IsNullOrWhiteSpace(mediaType) then
            invalidArg (nameof mediaType) "Media type must be provided."

        ctx.Request.Headers["Accept"] <- StringValues(mediaType)
        ctx

    /// Apply a Content-Type header to the request.
    let setContentType (mediaType: string) (ctx: HttpContext) =
        if String.IsNullOrWhiteSpace(mediaType) then
            invalidArg (nameof mediaType) "Media type must be provided."

        ctx.Request.ContentType <- mediaType
        ctx.Request.Headers["Content-Type"] <- StringValues(mediaType)
        ctx

    /// Set the request body to a string without modifying Content-Type.
    let setRequestBody (content: string) (ctx: HttpContext) =
        let bytes =
            if isNull content then
                Array.empty
            else
                Encoding.UTF8.GetBytes(content)

        let ctx =
            ctx
            |> replaceBodyStream (fun () ->
                let ms = new MemoryStream()
                ms.Write(bytes, 0, bytes.Length)
                ms.Position <- 0L
                ms :> Stream)

        ctx.Request.ContentLength <- Nullable<int64>(int64 bytes.Length)
        ctx

    /// Set the request body to a JSON string and ensure Content-Type is application/json.
    let setJsonBody (json: string) (ctx: HttpContext) =
        ctx
        |> setContentType "application/json"
        |> setRequestBody json

    /// Get the response body as a UTF-8 string without consuming the stream for subsequent reads.
    let getResponseBody (ctx: HttpContext) =
        match ctx.Response.Body with
        | null -> String.Empty
        | body ->
            if body.CanSeek then
                body.Seek(0L, SeekOrigin.Begin) |> ignore

            use reader = new StreamReader(body, Encoding.UTF8, true, 1024, true)
            let content = reader.ReadToEnd()

            if body.CanSeek then
                body.Seek(0L, SeekOrigin.Begin) |> ignore

            content

    /// Parse the response body as JSON into the requested type.
    let parseResponseJson<'T> (ctx: HttpContext) : 'T =
        let payload = getResponseBody ctx

        if String.IsNullOrWhiteSpace(payload) then
            invalidOp "Response body is empty; unable to parse JSON."

        JsonHelpers.parseJson<'T> payload
