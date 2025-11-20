namespace FX.Spec.JsonApi

open System
open System.Text.Json
open FX.Spec.Json

/// Exception thrown when a JSON:API assertion fails.
type JsonApiAssertionException(message: string, expected: obj option, actual: obj option) =
    inherit Exception(message)
    member _.Expected = expected
    member _.Actual = actual

[<AutoOpen>]
module JsonApiMatchers =

    /// Resource identifier record for JSON:API
    [<CLIMutable>]
    type ResourceIdentifier = { ``type``: string; id: string }

    /// Wrapper type for JSON:API-specific expectations
    type JsonApiExpectation(json: string) =
        member _.Json = json

        /// Asserts that the included array contains a resource with the specified type and id.
        /// Usage: expectJsonApi(json).toHaveIncludedResource("people", "9")
        member _.toHaveIncludedResource(resourceType: string, resourceId: string) =
            use doc = JsonDocument.Parse(json)

            match doc.RootElement.TryGetProperty("included") with
            | true, includedElement when includedElement.ValueKind = JsonValueKind.Array ->
                let found =
                    includedElement.EnumerateArray()
                    |> Seq.exists (fun resource ->
                        match resource.TryGetProperty("type"), resource.TryGetProperty("id") with
                        | (true, typeElem), (true, idElem) ->
                            typeElem.GetString() = resourceType && idElem.GetString() = resourceId
                        | _ -> false)

                if found then ()
                else
                    let msg = sprintf "Expected included array to contain resource with type '%s' and id '%s', but it was not found" resourceType resourceId
                    raise (JsonApiAssertionException(msg, Some (box (resourceType, resourceId)), Some (box json)))
            | true, includedElement ->
                let msg = sprintf "Expected 'included' to be an array, but found %A" includedElement.ValueKind
                raise (JsonApiAssertionException(msg, Some (box "Array"), Some (box includedElement.ValueKind)))
            | false, _ ->
                let msg = "Expected JSON to have 'included' array, but it was not found"
                raise (JsonApiAssertionException(msg, Some (box "included"), Some (box json)))

        /// Asserts that a JSON object at the specified path is a valid resource identifier with the expected type and id.
        /// Usage: expectJsonApi(json).toBeResourceIdentifier("data.relationships.author.data", "people", "9")
        member _.toBeResourceIdentifier(path: string, expectedType: string, expectedId: string) =
            use doc = JsonDocument.Parse(json)

            let parts = path.Split([| '.'; '['; ']' |], StringSplitOptions.RemoveEmptyEntries)
            let rec navigate (current: JsonElement) (remainingParts: string list) =
                match remainingParts with
                | [] -> Some current
                | part :: rest ->
                    match Int32.TryParse(part) with
                    | true, index when current.ValueKind = JsonValueKind.Array ->
                        let array = current.EnumerateArray() |> Seq.toArray
                        if index >= 0 && index < array.Length then
                            navigate array.[index] rest
                        else None
                    | _ ->
                        if current.ValueKind = JsonValueKind.Object then
                            match current.TryGetProperty(part) with
                            | true, element -> navigate element rest
                            | false, _ -> None
                        else None

            match navigate doc.RootElement (parts |> Array.toList) with
            | Some element when element.ValueKind = JsonValueKind.Object ->
                match element.TryGetProperty("type"), element.TryGetProperty("id") with
                | (true, typeElem), (true, idElem) ->
                    let actualType = typeElem.GetString()
                    let actualId = idElem.GetString()

                    if actualType = expectedType && actualId = expectedId then ()
                    else
                        let msg = sprintf "Expected resource identifier at '%s' to have type '%s' and id '%s', but found type '%s' and id '%s'" path expectedType expectedId actualType actualId
                        raise (JsonApiAssertionException(msg, Some (box (expectedType, expectedId)), Some (box (actualType, actualId))))
                | _ ->
                    let rawText = element.GetRawText()
                    let msg = sprintf "Expected object at '%s' to have both 'type' and 'id' properties" path
                    raise (JsonApiAssertionException(msg, Some (box "type and id"), Some (box rawText)))
            | Some element ->
                let msg = sprintf "Expected property at '%s' to be an object, but found %A" path element.ValueKind
                raise (JsonApiAssertionException(msg, Some (box "Object"), Some (box element.ValueKind)))
            | None ->
                let msg = sprintf "Expected JSON to have property at path '%s', but it was not found" path
                raise (JsonApiAssertionException(msg, Some (box path), Some (box json)))

        /// Asserts that the JSON:API document has a relationship with the specified name pointing to a resource of the given type and id.
        /// Usage: expectJsonApi(json).toHaveRelationship("author", "people", "9")
        /// This checks data.relationships.{name}.data for type and id
        member _.toHaveRelationship(relationshipName: string, resourceType: string, resourceId: string) =
            let path = sprintf "data.relationships.%s.data" relationshipName
            use doc = JsonDocument.Parse(json)

            let parts = path.Split([| '.' |], StringSplitOptions.RemoveEmptyEntries)
            let rec navigate (current: JsonElement) (remainingParts: string list) =
                match remainingParts with
                | [] -> Some current
                | part :: rest ->
                    if current.ValueKind = JsonValueKind.Object then
                        match current.TryGetProperty(part) with
                        | true, element -> navigate element rest
                        | false, _ -> None
                    else None

            match navigate doc.RootElement (parts |> Array.toList) with
            | Some element when element.ValueKind = JsonValueKind.Object ->
                match element.TryGetProperty("type"), element.TryGetProperty("id") with
                | (true, typeElem), (true, idElem) ->
                    let actualType = typeElem.GetString()
                    let actualId = idElem.GetString()

                    if actualType = resourceType && actualId = resourceId then ()
                    else
                        let msg = sprintf "Expected relationship '%s' to point to resource type '%s' with id '%s', but found type '%s' and id '%s'" relationshipName resourceType resourceId actualType actualId
                        raise (JsonApiAssertionException(msg, Some (box (resourceType, resourceId)), Some (box (actualType, actualId))))
                | _ ->
                    let rawText = element.GetRawText()
                    let msg = sprintf "Expected relationship '%s' data to have both 'type' and 'id' properties" relationshipName
                    raise (JsonApiAssertionException(msg, Some (box "type and id"), Some (box rawText)))
            | Some element when element.ValueKind = JsonValueKind.Null ->
                let msg = sprintf "Expected relationship '%s' to point to resource type '%s' with id '%s', but found null" relationshipName resourceType resourceId
                raise (JsonApiAssertionException(msg, Some (box (resourceType, resourceId)), Some (box "null")))
            | Some element ->
                let msg = sprintf "Expected relationship '%s' data to be an object, but found %A" relationshipName element.ValueKind
                raise (JsonApiAssertionException(msg, Some (box "Object"), Some (box element.ValueKind)))
            | None ->
                let msg = sprintf "Expected JSON to have relationship '%s' at path '%s', but it was not found" relationshipName path
                raise (JsonApiAssertionException(msg, Some (box relationshipName), Some (box json)))

    /// Wrapper type for JSON:API array expectations with ordering support
    type JsonApiArrayExpectation(json: string, path: string) =
        member _.Json = json
        member _.Path = path

        /// Asserts that an array is ordered by the specified selector in the given direction.
        /// Usage: (expectJsonApiArray json "data").toBeOrderedBy((fun item -> item.attributes.createdAt), Ascending)
        member _.toBeOrderedBy(selector: 'T -> 'TKey, direction: SortDirection) =
            use doc = JsonDocument.Parse(json)

            let parts = if String.IsNullOrEmpty(path) then [||] else path.Split([| '.'; '['; ']' |], StringSplitOptions.RemoveEmptyEntries)
            let rec navigate (current: JsonElement) (remainingParts: string list) =
                match remainingParts with
                | [] -> Some current
                | part :: rest ->
                    match Int32.TryParse(part) with
                    | true, index when current.ValueKind = JsonValueKind.Array ->
                        let array = current.EnumerateArray() |> Seq.toArray
                        if index >= 0 && index < array.Length then
                            navigate array.[index] rest
                        else None
                    | _ ->
                        if current.ValueKind = JsonValueKind.Object then
                            match current.TryGetProperty(part) with
                            | true, element -> navigate element rest
                            | false, _ -> None
                        else None

            let arrayElement =
                if parts.Length = 0 then
                    Some doc.RootElement
                else
                    navigate doc.RootElement (parts |> Array.toList)

            match arrayElement with
            | Some element when element.ValueKind = JsonValueKind.Array ->
                let items =
                    element.EnumerateArray()
                    |> Seq.map (fun item ->
                        try
                            JsonSerializer.Deserialize<'T>(item.GetRawText(), JsonHelpers.defaultJsonOptions)
                        with ex ->
                            let rawText = item.GetRawText()
                            let msg = sprintf "Failed to deserialize item in array at '%s': %s" path ex.Message
                            raise (JsonApiAssertionException(msg, Some (box typeof<'T>), Some (box rawText))))
                    |> Seq.toList

                if items.Length <= 1 then ()
                else
                    let keys = items |> List.map selector
                    let sorted =
                        match direction with
                        | Ascending -> keys |> List.sort
                        | Descending -> keys |> List.sortDescending

                    if keys = sorted then ()
                    else
                        let msg =
                            match direction with
                            | Ascending -> sprintf "Expected array at '%s' to be ordered ascending, but it was not" path
                            | Descending -> sprintf "Expected array at '%s' to be ordered descending, but it was not" path
                        raise (JsonApiAssertionException(msg, Some (box sorted), Some (box keys)))
            | Some element ->
                let msg = sprintf "Expected property at path '%s' to be an array, but found %A" path element.ValueKind
                raise (JsonApiAssertionException(msg, Some (box "Array"), Some (box element.ValueKind)))
            | None ->
                let msg = sprintf "Expected JSON to have array at path '%s', but it was not found" path
                raise (JsonApiAssertionException(msg, Some (box path), Some (box json)))

    and SortDirection =
        | Ascending
        | Descending

    /// Create a JSON:API expectation from a JSON string.
    let expectJsonApi (json: string) = JsonApiExpectation(json)

    /// Create a JSON:API array expectation from a JSON string and path.
    let expectJsonApiArray (json: string) (path: string) = JsonApiArrayExpectation(json, path)
