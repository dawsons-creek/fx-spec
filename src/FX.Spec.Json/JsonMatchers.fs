namespace FX.Spec.Json

open System
open System.Text.Json

/// Exception thrown when a JSON assertion fails.
type JsonAssertionException(message: string, expected: obj option, actual: obj option) =
    inherit Exception(message)
    member _.Expected = expected
    member _.Actual = actual

[<AutoOpen>]
module JsonMatchers =

    /// Helper to navigate JSON using dot notation and array indexing
    /// Examples: "data", "data.items", "data.items[0].name"
    let private navigateJsonPath (json: JsonElement) (path: string) : JsonElement option =
        let parts = path.Split([| '.'; '['; ']' |], StringSplitOptions.RemoveEmptyEntries)

        let rec navigate (current: JsonElement) (remainingParts: string list) =
            match remainingParts with
            | [] -> Some current
            | part :: rest ->
                // Try to parse as array index
                match Int32.TryParse(part) with
                | true, index when current.ValueKind = JsonValueKind.Array ->
                    let array = current.EnumerateArray() |> Seq.toArray
                    if index >= 0 && index < array.Length then
                        navigate array.[index] rest
                    else
                        None
                | _ ->
                    // Try as property name
                    if current.ValueKind = JsonValueKind.Object then
                        match current.TryGetProperty(part) with
                        | true, element -> navigate element rest
                        | false, _ -> None
                    else
                        None

        navigate json (parts |> Array.toList)

    /// Parse JSON string to JsonDocument
    let private parseJsonDocument (json: string) : JsonDocument =
        if String.IsNullOrWhiteSpace(json) then
            invalidArg (nameof json) "JSON string must not be null or whitespace."
        JsonDocument.Parse(json)

    /// Wrapper type for JSON expectations
    type JsonExpectation(json: string) =
        member _.Json = json

        /// Asserts that the JSON contains the specified property (path).
        /// Supports dot notation and array indexing: "data.items[0].name"
        member _.toHaveProperty(path: string) =
            use doc = parseJsonDocument json
            match navigateJsonPath doc.RootElement path with
            | Some _ -> ()
            | None ->
                let msg = sprintf "Expected JSON to have property at path '%s', but it was not found" path
                raise (JsonAssertionException(msg, Some (box path), Some (box json)))

        /// Asserts that the JSON property at the specified path has the expected value.
        member _.toHaveProperty(path: string, expectedValue: 'T) =
            use doc = parseJsonDocument json
            match navigateJsonPath doc.RootElement path with
            | Some element ->
                try
                    let actualValue = JsonSerializer.Deserialize<'T>(element.GetRawText(), JsonHelpers.defaultJsonOptions)
                    if actualValue = expectedValue then ()
                    else
                        let msg = sprintf "Expected property '%s' to be %A, but found %A" path expectedValue actualValue
                        raise (JsonAssertionException(msg, Some (box expectedValue), Some (box actualValue)))
                with ex ->
                    let rawText = element.GetRawText()
                    let msg = sprintf "Expected property '%s' to be %A, but failed to deserialize: %s" path expectedValue ex.Message
                    raise (JsonAssertionException(msg, Some (box expectedValue), Some (box rawText)))
            | None ->
                let msg = sprintf "Expected JSON to have property at path '%s', but it was not found" path
                raise (JsonAssertionException(msg, Some (box path), Some (box json)))

        /// Asserts that the JSON matches the partial object (subset of fields).
        /// Uses anonymous records or regular records to specify expected fields.
        member _.toMatchPartial(expected: 'T) =
            try
                // Serialize the expected partial object to JSON
                let expectedJson = JsonHelpers.toJson expected
                use expectedDoc = parseJsonDocument expectedJson
                use actualDoc = parseJsonDocument json

                // Check that all properties in expected exist in actual with same values
                let rec checkProperties (expectedElem: JsonElement) (actualElem: JsonElement) (path: string) =
                    match expectedElem.ValueKind with
                    | JsonValueKind.Object ->
                        expectedElem.EnumerateObject()
                        |> Seq.iter (fun prop ->
                            match actualElem.TryGetProperty(prop.Name) with
                            | true, actualProp ->
                                let newPath = if String.IsNullOrEmpty(path) then prop.Name else sprintf "%s.%s" path prop.Name
                                checkProperties prop.Value actualProp newPath
                            | false, _ ->
                                let msg = sprintf "Expected JSON to have property '%s.%s', but it was not found" path prop.Name
                                raise (JsonAssertionException(msg, Some (box prop.Name), Some (box json))))
                    | JsonValueKind.Array ->
                        let expectedArray = expectedElem.EnumerateArray() |> Seq.toArray
                        let actualArray = actualElem.EnumerateArray() |> Seq.toArray
                        if expectedArray.Length <> actualArray.Length then
                            let msg = sprintf "Expected array at '%s' to have length %d, but found length %d" path expectedArray.Length actualArray.Length
                            raise (JsonAssertionException(msg, Some (box expectedArray.Length), Some (box actualArray.Length)))
                        Array.zip expectedArray actualArray
                        |> Array.iteri (fun i (exp, act) ->
                            checkProperties exp act (sprintf "%s[%d]" path i))
                    | _ ->
                        // Compare values
                        let expectedText = expectedElem.GetRawText()
                        let actualText = actualElem.GetRawText()
                        if expectedText <> actualText then
                            let msg = sprintf "Expected property '%s' to be %s, but found %s" path expectedText actualText
                            raise (JsonAssertionException(msg, Some (box expectedText), Some (box actualText)))

                checkProperties expectedDoc.RootElement actualDoc.RootElement ""
            with
            | :? JsonAssertionException as ex -> raise ex
            | ex ->
                let msg = sprintf "Failed to match partial JSON: %s" ex.Message
                raise (JsonAssertionException(msg, Some (box expected), Some (box json)))

    /// Wrapper type for JSON array expectations
    type JsonArrayExpectation(json: string, path: string) =
        member _.Json = json
        member _.Path = path

        /// Asserts that the array has the expected length.
        member _.toHaveLength(expectedLength: int) =
            use doc = parseJsonDocument json
            match navigateJsonPath doc.RootElement path with
            | Some element when element.ValueKind = JsonValueKind.Array ->
                let array = element.EnumerateArray() |> Seq.toArray
                if array.Length = expectedLength then ()
                else
                    let msg = sprintf "Expected array at '%s' to have length %d, but found length %d" path expectedLength array.Length
                    raise (JsonAssertionException(msg, Some (box expectedLength), Some (box array.Length)))
            | Some element ->
                let msg = sprintf "Expected property at path '%s' to be an array, but found %A" path element.ValueKind
                raise (JsonAssertionException(msg, Some (box "Array"), Some (box element.ValueKind)))
            | None ->
                let msg = sprintf "Expected JSON to have array at path '%s', but it was not found" path
                raise (JsonAssertionException(msg, Some (box path), Some (box json)))

        /// Asserts that all items in the array have the specified property.
        member _.toAllHaveProperty(propertyName: string) =
            use doc = parseJsonDocument json
            match navigateJsonPath doc.RootElement path with
            | Some element when element.ValueKind = JsonValueKind.Array ->
                element.EnumerateArray()
                |> Seq.iteri (fun index item ->
                    if item.ValueKind <> JsonValueKind.Object then
                        let msg = sprintf "Expected item at '%s[%d]' to be an object, but found %A" path index item.ValueKind
                        raise (JsonAssertionException(msg, Some (box "Object"), Some (box item.ValueKind)))

                    match item.TryGetProperty(propertyName) with
                    | true, _ -> ()
                    | false, _ ->
                        let rawText = item.GetRawText()
                        let msg = sprintf "Expected item at '%s[%d]' to have property '%s', but it was not found" path index propertyName
                        raise (JsonAssertionException(msg, Some (box propertyName), Some (box rawText))))
            | Some element ->
                let msg = sprintf "Expected property at path '%s' to be an array, but found %A" path element.ValueKind
                raise (JsonAssertionException(msg, Some (box "Array"), Some (box element.ValueKind)))
            | None ->
                let msg = sprintf "Expected JSON to have array at path '%s', but it was not found" path
                raise (JsonAssertionException(msg, Some (box path), Some (box json)))

        /// Asserts that all items in the array satisfy the predicate.
        member _.toAllSatisfy(predicate: 'T -> bool) =
            use doc = parseJsonDocument json
            match navigateJsonPath doc.RootElement path with
            | Some element when element.ValueKind = JsonValueKind.Array ->
                element.EnumerateArray()
                |> Seq.iteri (fun index item ->
                    try
                        let deserialized = JsonSerializer.Deserialize<'T>(item.GetRawText(), JsonHelpers.defaultJsonOptions)
                        if not (predicate deserialized) then
                            let msg = sprintf "Expected item at '%s[%d]' to satisfy predicate, but it did not: %A" path index deserialized
                            raise (JsonAssertionException(msg, Some (box "predicate"), Some (box deserialized)))
                    with
                    | :? JsonAssertionException as ex -> raise ex
                    | ex ->
                        let rawText = item.GetRawText()
                        let msg = sprintf "Failed to deserialize item at '%s[%d]': %s" path index ex.Message
                        raise (JsonAssertionException(msg, Some (box typeof<'T>), Some (box rawText))))
            | Some element ->
                let msg = sprintf "Expected property at path '%s' to be an array, but found %A" path element.ValueKind
                raise (JsonAssertionException(msg, Some (box "Array"), Some (box element.ValueKind)))
            | None ->
                let msg = sprintf "Expected JSON to have array at path '%s', but it was not found" path
                raise (JsonAssertionException(msg, Some (box path), Some (box json)))

        /// Asserts that the array contains an item matching the expected value.
        member _.toContain(expectedItem: 'T) =
            use doc = parseJsonDocument json
            match navigateJsonPath doc.RootElement path with
            | Some element when element.ValueKind = JsonValueKind.Array ->
                let found =
                    element.EnumerateArray()
                    |> Seq.exists (fun item ->
                        try
                            let deserialized = JsonSerializer.Deserialize<'T>(item.GetRawText(), JsonHelpers.defaultJsonOptions)
                            deserialized = expectedItem
                        with _ -> false)

                if found then ()
                else
                    let msg = sprintf "Expected array at '%s' to contain %A, but it did not" path expectedItem
                    raise (JsonAssertionException(msg, Some (box expectedItem), Some (box json)))
            | Some element ->
                let msg = sprintf "Expected property at path '%s' to be an array, but found %A" path element.ValueKind
                raise (JsonAssertionException(msg, Some (box "Array"), Some (box element.ValueKind)))
            | None ->
                let msg = sprintf "Expected JSON to have array at path '%s', but it was not found" path
                raise (JsonAssertionException(msg, Some (box path), Some (box json)))

    /// Create a JSON expectation from a JSON string.
    let expectJson (json: string) = JsonExpectation(json)

    /// Create a JSON array expectation from a JSON string and path.
    /// Path can be empty string for root array, or dot notation for nested arrays.
    let expectJsonArray (json: string) (path: string) = JsonArrayExpectation(json, path)
