namespace FX.Spec.JsonApi

open FX.Spec.Json

[<AutoOpen>]
module JsonApiHelpers =

    /// JSON:API media type identifier.
    let jsonApiMediaType = "application/vnd.api+json"

    /// JSON:API envelope for single resources.
    type JsonApiEnvelope<'T> = { data: 'T }

    /// JSON:API envelope for resource collections.
    type JsonApiCollectionEnvelope<'T> = { data: 'T array }

    /// Parse a JSON:API single-resource envelope from a JSON string.
    let parseJsonApiData<'T> (json: string) : 'T =
        let envelope = parseJson<JsonApiEnvelope<'T>> json
        envelope.data

    /// Parse a JSON:API collection envelope from a JSON string.
    let parseJsonApiCollection<'T> (json: string) : 'T array =
        let envelope = parseJson<JsonApiCollectionEnvelope<'T>> json
        envelope.data

    /// Parse a JSON:API document with a custom envelope shape.
    let parseJsonApiWith<'TEnvelope> (json: string) : 'TEnvelope =
        parseJson<'TEnvelope> json

    /// Wrap a resource in a JSON:API single-resource envelope and serialize to JSON.
    let toJsonApiData<'T> (resource: 'T) : string =
        let envelope: JsonApiEnvelope<'T> = { data = resource }
        toJson envelope

    /// Wrap a resource array in a JSON:API collection envelope and serialize to JSON.
    let toJsonApiCollection<'T> (resources: 'T array) : string =
        let envelope: JsonApiCollectionEnvelope<'T> = { data = resources }
        toJson envelope
