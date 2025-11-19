namespace FX.Spec.Json

open System
open System.Text.Json

[<AutoOpen>]
module JsonHelpers =

    /// Shared JSON serialization options with case-insensitive property matching and F# type support.
    let defaultJsonOptions =
        let options = JsonSerializerOptions(PropertyNameCaseInsensitive = true)
        options.Converters.Add(System.Text.Json.Serialization.JsonFSharpConverter())
        options

    /// Parse a JSON string into the requested type using default options.
    let parseJson<'T> (json: string) : 'T =
        if String.IsNullOrWhiteSpace(json) then
            invalidArg (nameof json) "JSON string must not be null or whitespace."

        JsonSerializer.Deserialize<'T>(json, defaultJsonOptions)

    /// Parse a JSON string into the requested type using custom options.
    let parseJsonWith<'T> (options: JsonSerializerOptions) (json: string) : 'T =
        if isNull options then
            invalidArg (nameof options) "JsonSerializerOptions must not be null."

        if String.IsNullOrWhiteSpace(json) then
            invalidArg (nameof json) "JSON string must not be null or whitespace."

        JsonSerializer.Deserialize<'T>(json, options)

    /// Serialize a value to JSON string using default options.
    let toJson<'T> (value: 'T) : string =
        JsonSerializer.Serialize<'T>(value, defaultJsonOptions)

    /// Serialize a value to JSON string using custom options.
    let toJsonWith<'T> (options: JsonSerializerOptions) (value: 'T) : string =
        if isNull options then
            invalidArg (nameof options) "JsonSerializerOptions must not be null."

        JsonSerializer.Serialize<'T>(value, options)
