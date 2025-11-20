namespace FX.Spec.AspNetCore

open System
open System.IO
open System.Text
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open FX.Core
open FX.Spec.Json

[<AutoOpen>]
module FxRouterHelpers =

    let private defaultStatus (ctx: HttpContext) =
        if ctx.Response.StatusCode = 0 then
            ctx.Response.StatusCode <- StatusCodes.Status200OK

    let private ensureResponseStream (ctx: HttpContext) =
        if isNull ctx.Response.Body then
            ctx.Response.Body <- new MemoryStream()

    let private writeError (ctx: HttpContext) (httpError: HttpError) =
        task {
            ensureResponseStream ctx
            ctx.Response.StatusCode <- HttpError.toStatusCode httpError
            ctx.Response.ContentType <- "application/problem+json"

            let errorDocument =
                {| status = httpError.StatusCode
                   title = httpError.Title
                   detail = httpError.Detail
                   instance = httpError.Instance |}

            let payload = JsonHelpers.toJson errorDocument
            let bytes = Encoding.UTF8.GetBytes(payload)

            do! ctx.Response.Body.WriteAsync(bytes, 0, bytes.Length)

            if ctx.Response.Body.CanSeek then
                ctx.Response.Body.Seek(0L, SeekOrigin.Begin) |> ignore
        }

    let private runStandardHandler (ctx: HttpContext) (handler: HttpContext -> ValueTask<Result<unit, HttpError>>) =
        task {
            let! outcome = handler ctx |> fun vt -> vt.AsTask()

            match outcome with
            | Ok() -> defaultStatus ctx
            | Error httpError -> do! writeError ctx httpError
        }

    let private runRawHandler (ctx: HttpContext) (handler: HttpContext -> Task) =
        task {
            do! handler ctx
            defaultStatus ctx
        }

    /// Execute a handler variant directly (for lower-level testing).
    let executeHandler (ctx: HttpContext) (handlerVariant: HandlerVariant voption) =
        task {
            match handlerVariant with
            | ValueNone -> ctx.Response.StatusCode <- StatusCodes.Status404NotFound
            | ValueSome variant ->
                match variant with
                | HandlerVariant.Standard handler -> do! runStandardHandler ctx handler
                | HandlerVariant.Raw handler -> do! runRawHandler ctx handler

            return ctx
        }

    /// Execute an FX route handler matched by the router for the supplied method/path.
    /// Returns the original context for fluent chaining.
    let executeRoute (router: Router) (method: HttpMethod) (path: string) (ctx: HttpContext) =
        task {
            let handlerVariant = router.Match(ctx, method, path)
            return! executeHandler ctx handlerVariant
        }
