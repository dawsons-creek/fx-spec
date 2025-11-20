module FX.Spec.AspNetCore.Tests.FxRouterHelpersSpecs

open System
open System.Text
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open FX.Spec.Core
open FX.Spec.Matchers
open FX.Spec.AspNetCore
open FX.Core

let private standardOkHandler: HttpContext -> ValueTask<Result<unit, HttpError>> =
    fun ctx ->
        ctx.Response.Headers["X-Handler"] <- "standard"
        ValueTask<Result<unit, HttpError>>(Ok())

let private standardErrorHandler (error: HttpError) : HttpContext -> ValueTask<Result<unit, HttpError>> =
    fun _ -> ValueTask<Result<unit, HttpError>>(Error error)

let private rawHandler (configure: HttpContext -> unit) : HttpContext -> Task =
    fun ctx ->
        configure ctx
        Task.CompletedTask

[<Tests>]
let specs =
    describe
        "FxRouterHelpers"
        [ context
              "executeRoute"
              [ itAsync
                    "returns 404 Not Found when the router has no matching handler"
                    (async {
                        let ctx = createTestContext () |> setRequest "GET" "/missing"

                        let! resultCtx = executeRoute Router.empty HttpMethod.GET "/missing" ctx |> Async.AwaitTask

                        expectBool(LanguagePrimitives.PhysicalEquality resultCtx ctx).toBeTrue ()
                        expectNum(ctx.Response.StatusCode).toEqual (StatusCodes.Status404NotFound)
                    })

                itAsync
                    "executes standard handlers and returns the same HttpContext for fluent chaining"
                    (async {
                        let ctx = createTestContext () |> setRequest "POST" "/todos"

                        ctx.Response.StatusCode <- 0

                        let router =
                            Router.ofTable [ HttpMethod.POST, "/todos", HandlerVariant.standard standardOkHandler ]

                        let! resultCtx = executeRoute router HttpMethod.POST "/todos" ctx |> Async.AwaitTask

                        expectBool(LanguagePrimitives.PhysicalEquality resultCtx ctx).toBeTrue ()
                        expectNum(ctx.Response.StatusCode).toEqual (StatusCodes.Status200OK)
                        expectStr(ctx.Response.Headers["X-Handler"].ToString()).toEqual ("standard")
                    })

                itAsync
                    "propagates HttpError responses from standard handlers as RFC 7807 Problem Details"
                    (async {
                        let error = HttpError.badRequest (Some "invalid payload")
                        let ctx = createTestContext () |> setRequest "POST" "/widgets"

                        let router =
                            Router.ofTable
                                [ HttpMethod.POST, "/widgets", HandlerVariant.standard (standardErrorHandler error) ]

                        let! _ = executeRoute router HttpMethod.POST "/widgets" ctx |> Async.AwaitTask

                        let body = getResponseBody ctx

                        expectNum(ctx.Response.StatusCode).toEqual (HttpError.toStatusCode error)
                        expectStr(ctx.Response.ContentType).toEqual ("application/problem+json")
                        expectBool(String.IsNullOrWhiteSpace(body)).toBeFalse ()
                        expectBool(body.Contains("\"status\"")).toBeTrue ()
                    })

                itAsync
                    "defaults the status code to 200 OK when raw handlers do not set it"
                    (async {
                        let ctx = createTestContext () |> setRequest "PATCH" "/settings"

                        ctx.Response.StatusCode <- 0

                        let router =
                            Router.ofTable [ HttpMethod.PATCH, "/settings", HandlerVariant.raw (rawHandler ignore) ]

                        let! _ = executeRoute router HttpMethod.PATCH "/settings" ctx |> Async.AwaitTask

                        expectNum(ctx.Response.StatusCode).toEqual (StatusCodes.Status200OK)
                    })

                itAsync
                    "respects status codes explicitly set by raw handlers"
                    (async {
                        let ctx = createTestContext () |> setRequest "DELETE" "/jobs/42"

                        let router =
                            Router.ofTable
                                [ HttpMethod.DELETE,
                                  "/jobs/42",
                                  HandlerVariant.raw (
                                      rawHandler (fun httpCtx ->
                                          httpCtx.Response.StatusCode <- StatusCodes.Status204NoContent)
                                  ) ]

                        let! _ = executeRoute router HttpMethod.DELETE "/jobs/42" ctx |> Async.AwaitTask

                        expectNum(ctx.Response.StatusCode).toEqual (StatusCodes.Status204NoContent)
                    }) ]

          context
              "executeHandler"
              [ itAsync
                    "sets 404 when provided with ValueNone"
                    (async {
                        let ctx = createTestContext ()

                        let! _ = executeHandler ctx ValueNone |> Async.AwaitTask

                        expectNum(ctx.Response.StatusCode).toEqual (StatusCodes.Status404NotFound)
                    })

                itAsync
                    "executes the supplied handler variant when ValueSome is provided"
                    (async {
                        let ctx = createTestContext () |> setRequest "GET" "/health"

                        ctx.Response.StatusCode <- 0

                        let! _ =
                            executeHandler ctx (ValueSome(HandlerVariant.standard standardOkHandler))
                            |> Async.AwaitTask

                        expectNum(ctx.Response.StatusCode).toEqual (StatusCodes.Status200OK)
                        expectStr(ctx.Response.Headers["X-Handler"].ToString()).toEqual ("standard")
                    }) ] ]
