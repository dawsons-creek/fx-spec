namespace FX.Core

open System
open Microsoft.AspNetCore.Http

[<Struct>]
type HttpError =
    { StatusCode: int
      Title: string option
      Detail: string option
      Instance: string option }

[<RequireQualifiedAccess>]
module HttpError =

    let private normalize =
        Option.bind (fun value -> if String.IsNullOrWhiteSpace value then None else Some value)

    let create statusCode title detail instance =
        { StatusCode = statusCode
          Title = normalize title
          Detail = normalize detail
          Instance = normalize instance }

    let ofStatusCode statusCode = create statusCode None None None

    let withTitle (title: string) error =
        { error with
            Title = normalize (Some title) }

    let withDetail (detail: string) error =
        { error with
            Detail = normalize (Some detail) }

    let withInstance (instance: string) error =
        { error with
            Instance = normalize (Some instance) }

    let toStatusCode (error: HttpError) = error.StatusCode

    let message (error: HttpError) =
        error.Detail |> Option.orElse error.Title |> Option.defaultValue String.Empty

    let notFound detail =
        create StatusCodes.Status404NotFound None detail None

    let badRequest detail =
        create StatusCodes.Status400BadRequest None detail None

    let internalError detail =
        create StatusCodes.Status500InternalServerError None detail None
