namespace FX.Core

open System

/// Minimal discriminated union representing supported HTTP verbs.
/// This mirrors the semantics relied on by the Fx Spec helpers without
/// pulling in the full FX.Core dependency graph.
type HttpMethod =
    | GET
    | POST
    | PUT
    | PATCH
    | DELETE
    | OPTIONS
    | HEAD

[<RequireQualifiedAccess>]
module HttpMethod =

    let private allMethods =
        [ GET; POST; PUT; PATCH; DELETE; OPTIONS; HEAD ]

    /// Convert a discriminated union case to its canonical uppercase verb string.
    let toString (method: HttpMethod) =
        match method with
        | GET -> "GET"
        | POST -> "POST"
        | PUT -> "PUT"
        | PATCH -> "PATCH"
        | DELETE -> "DELETE"
        | OPTIONS -> "OPTIONS"
        | HEAD -> "HEAD"

    /// Attempt to parse a string into a known HTTP method.
    let tryParse (value: string) =
        if String.IsNullOrWhiteSpace value then
            None
        else
            allMethods
            |> List.tryFind (fun m -> String.Equals(toString m, value, StringComparison.OrdinalIgnoreCase))

    /// Parse a string into an HTTP method or raise if the verb is unsupported.
    let parse (value: string) =
        match tryParse value with
        | Some method -> method
        | None -> invalidArg (nameof value) $"Unsupported HTTP method: '%s{value}'"

    /// Predefined list of all supported methods.
    let all = allMethods
