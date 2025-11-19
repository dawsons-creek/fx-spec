module FX.Spec.JsonApi.Tests.JsonApiHelpersSpecs

open System
open System.Text.Json
open FX.Spec.Core
open FX.Spec.Matchers
open FX.Spec.Json
open FX.Spec.JsonApi

[<CLIMutable>]
type private Widget = { id: string; name: string }

[<CLIMutable>]
type private Meta = { count: int }

[<CLIMutable>]
type private WidgetEnvelope = { data: Widget array; meta: Meta }

[<CLIMutable>]
type private WidgetDocument = { data: Widget }

[<Tests>]
let specs =
    describe
        "JsonApiHelpers"
        [ context
              "parseJsonApiData"
              [ it "extracts the resource from a JSON:API single-resource document" (fun () ->
                    let json = """{"data":{"id":"w-42","name":"Widget Mk42"}}"""

                    let widget = JsonApiHelpers.parseJsonApiData<Widget> json

                    expectStr(widget.id).toEqual ("w-42")
                    expectStr(widget.name).toEqual ("Widget Mk42"))
                it "throws when the document omits the data member" (fun () ->
                    let json = """{"meta":{"count":1}}"""

                    expectThrows<JsonException> (fun () -> JsonApiHelpers.parseJsonApiData<Widget> json |> ignore)) ]
          context
              "parseJsonApiCollection"
              [ it "extracts the resource array from a JSON:API collection document" (fun () ->
                    let json =
                        """{
  "data": [
    {"id":"w-1","name":"Widget Mk1"},
    {"id":"w-2","name":"Widget Mk2"}
  ]
}"""

                    let widgets = JsonApiHelpers.parseJsonApiCollection<Widget> json

                    expectSeq(widgets).toHaveLength (2)
                    expectStr(widgets.[0].id).toEqual ("w-1")
                    expectStr(widgets.[1].name).toEqual ("Widget Mk2")) ]
          context
              "parseJsonApiWith"
              [ it "allows parsing of custom envelopes" (fun () ->
                    let json =
                        """{
  "data": [
    {"id":"w-1","name":"Widget Mk1"},
    {"id":"w-2","name":"Widget Mk2"}
  ],
  "meta": {"count": 2}
}"""

                    let envelope = JsonApiHelpers.parseJsonApiWith<WidgetEnvelope> json

                    expectSeq(envelope.data).toHaveLength (2)
                    expectNum(envelope.meta.count).toEqual (2)) ]
          context
              "toJsonApiData"
              [ it "wraps a resource inside a JSON:API data envelope" (fun () ->
                    let widget = { id = "w-7"; name = "Widget Mk7" }

                    let json = JsonApiHelpers.toJsonApiData widget
                    let document = JsonHelpers.parseJson<WidgetDocument> json

                    expectStr(document.data.id).toEqual ("w-7")
                    expectStr(document.data.name).toEqual ("Widget Mk7")
                    expectStr(json).toContain ("\"data\"")) ]
          context
              "toJsonApiCollection"
              [ it "wraps the resources array inside a JSON:API collection envelope" (fun () ->
                    let widgets =
                        [| { id = "w-1"; name = "Widget Mk1" }; { id = "w-2"; name = "Widget Mk2" } |]

                    let json = JsonApiHelpers.toJsonApiCollection widgets
                    let envelope = JsonHelpers.parseJson<WidgetEnvelope> json

                    expectSeq(envelope.data).toHaveLength (2)
                    expectStr(envelope.data.[0].name).toEqual ("Widget Mk1")
                    expectStr(envelope.data.[1].id).toEqual ("w-2")) ]
          context
              "jsonApiMediaType"
              [ it "exposes the official JSON:API media type" (fun () ->
                    expectStr(JsonApiHelpers.jsonApiMediaType).toEqual ("application/vnd.api+json")) ] ]
