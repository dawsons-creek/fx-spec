module FX.Spec.AspNetCore.Tests.FxContextHelpersSpecs

open System
open System.IO
open System.Text
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open FX.Spec.Core
open FX.Spec.Matchers
open FX.Spec.AspNetCore

[<CLIMutable>]
type private SampleDto = { Name: string; Count: int }

type private SampleService() =
    member val Value = "registered" with get, set

[<Tests>]
let specs =
    describe
        "FxContextHelpers"
        [ context
              "createTestContext"
              [ it "provides DI services and logging infrastructure by default" (fun () ->
                    let ctx = createTestContext ()
                    let provider = ctx.RequestServices

                    expectBool(provider |> isNull).toBeFalse ()
                    expectStr(ctx.Request.Protocol).toEqual ("HTTP/1.1")
                    expectBool(ctx.Response.Body.CanSeek).toBeTrue ()

                    let loggerFactory = provider.GetService<ILoggerFactory>()
                    expectBool(loggerFactory |> isNull).toBeFalse ()

                    let logger = loggerFactory.CreateLogger("spec")
                    logger.LogInformation("hello from spec"))
                it "allows callers to register additional services" (fun () ->
                    let ctx =
                        createTestContextWith (fun services -> services.AddSingleton<string>("registered") |> ignore)

                    let resolved = ctx.RequestServices.GetService<string>()
                    expectStr(resolved).toEqual ("registered")) ]

          context
              "setRequest"
              [ it "normalizes method casing and ensures leading slash on the path" (fun () ->
                    let ctx = createTestContext () |> setRequest "post" "todos"

                    expectStr(ctx.Request.Method).toEqual ("POST")
                    expectStr(ctx.Request.Path.ToString()).toEqual ("/todos")) ]

          context
              "setAcceptHeader"
              [ it "applies the specified Accept header value" (fun () ->
                    let ctx = createTestContext () |> setAcceptHeader "application/xml"

                    expectStr(ctx.Request.Headers["Accept"].ToString()).toEqual ("application/xml")) ]

          context
              "setContentType"
              [ it "updates Content-Type header and property consistently" (fun () ->
                    let ctx = createTestContext () |> setContentType "application/vnd.api+json"

                    expectStr(ctx.Request.ContentType).toEqual ("application/vnd.api+json")
                    expectStr(ctx.Request.Headers["Content-Type"].ToString()).toEqual ("application/vnd.api+json")) ]

          context
              "setJsonBody"
              [ it "writes JSON payload, resets stream position, and sets content length" (fun () ->
                    let payload = """{"title":"Write specs"}"""

                    let ctx = createTestContext () |> setJsonBody payload

                    use reader = new StreamReader(ctx.Request.Body, Encoding.UTF8, true, 1024, true)
                    let content = reader.ReadToEnd()

                    expectStr(content).toEqual (payload)
                    expectBool(ctx.Request.ContentLength.HasValue).toBeTrue ()
                    expectNum(int ctx.Request.ContentLength.Value).toEqual (Encoding.UTF8.GetByteCount(payload))
                    expectStr(ctx.Request.ContentType).toEqual ("application/json")) ]

          context
              "getResponseBody"
              [ it "returns the response payload and preserves the stream position" (fun () ->
                    let ctx = createTestContext ()
                    let payload = "integration-output"
                    let bytes = Encoding.UTF8.GetBytes(payload)

                    ctx.Response.Body.Write(bytes, 0, bytes.Length)
                    ctx.Response.Body.Position <- 0L

                    let responseText = getResponseBody ctx

                    expectStr(responseText).toEqual (payload)
                    expectNum(int ctx.Response.Body.Position).toEqual (0)) ]

          context
              "parseJson"
              [ it "deserializes response JSON using case-insensitive property matching" (fun () ->
                    let ctx = createTestContext ()
                    let json = """{"name":"Gadget","COUNT":7}"""
                    let bytes = Encoding.UTF8.GetBytes(json)

                    ctx.Response.Body.Write(bytes, 0, bytes.Length)
                    ctx.Response.Body.Position <- 0L

                    let parsed = parseResponseJson<SampleDto> ctx

                    expectStr(parsed.Name).toEqual ("Gadget")
                    expectNum(parsed.Count).toEqual (7)) ] ]
