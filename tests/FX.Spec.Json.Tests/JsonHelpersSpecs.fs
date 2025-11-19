module FX.Spec.Json.Tests.JsonHelpersSpecs

open System
open System.Text.Json
open FX.Spec.Core
open FX.Spec.Matchers
open FX.Spec.Json

[<CLIMutable>]
type private Sample = { Name: string; Count: int }

[<Tests>]
let specs =
    describe
        "JsonHelpers"
        [ context
              "round-tripping with default options"
              [ it "serializes and deserializes records preserving values" (fun () ->
                    let payload = { Name = "Gizmo"; Count = 5 }

                    let json = JsonHelpers.toJson payload
                    let parsed = JsonHelpers.parseJson<Sample> json

                    expectStr(parsed.Name).toEqual ("Gizmo")
                    expectNum(parsed.Count).toEqual (5)) ]

          context
              "whitespace and null input validation"
              [ it "rejects blank JSON payloads" (fun () ->
                    expectThrows<ArgumentException>(fun () ->
                        JsonHelpers.parseJson<Sample> "   " |> ignore))
                it "rejects null JSON strings" (fun () ->
                    expectThrows<ArgumentException>(fun () ->
                        JsonHelpers.parseJson<Sample> null |> ignore)) ]

          context
              "custom serialization options"
              [ it "uses the supplied options when parsing" (fun () ->
                    let options = JsonSerializerOptions(PropertyNameCaseInsensitive = false)
                    let json = """{"Name":"Widget","Count":42}"""

                    let parsed = JsonHelpers.parseJsonWith<Sample> options json

                    expectStr(parsed.Name).toEqual ("Widget")
                    expectNum(parsed.Count).toEqual (42))
                it "applies naming policy during serialization" (fun () ->
                    let options = JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase)

                    let payload = { Name = "Widget"; Count = 42 }
                    let json = JsonHelpers.toJsonWith options payload

                    expectStr(json).toContain ("\"name\"")
                    expectStr(json).toContain ("\"count\"")
                    expectBool(json.Contains("\"Name\"")).toBeFalse ()
                    expectBool(json.Contains("\"Count\"")).toBeFalse ()) ]

          context
              "option validation"
              [ it "rejects null options when parsing" (fun () ->
                    expectThrows<ArgumentException>(fun () ->
                        JsonHelpers.parseJsonWith<Sample> null """{"name":"Widget","count":1}""" |> ignore))
                it "rejects null options when serializing" (fun () ->
                    expectThrows<ArgumentException>(fun () ->
                        JsonHelpers.toJsonWith null { Name = "Widget"; Count = 1 } |> ignore)) ] ]
