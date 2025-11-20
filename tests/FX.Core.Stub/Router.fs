namespace FX.Core

open System
open Microsoft.AspNetCore.Http

/// Minimal router abstraction that mirrors the surface area consumed by the FX.Spec.AspNetCore helpers.
/// Internally it wraps a matcher function that resolves an HttpContext/HttpMethod/path triple
/// into an optional handler variant.
type Router(matchFn: HttpContext -> HttpMethod -> string -> HandlerVariant voption) =

    /// Attempt to match an incoming request to a handler variant.
    member _.Match(ctx: HttpContext, method: HttpMethod, path: string) : HandlerVariant voption =
        matchFn ctx method path

    /// Create a router from a matching function.
    static member Create(matchFn: HttpContext -> HttpMethod -> string -> HandlerVariant voption) =
        Router(matchFn)

/// Convenience helpers for composing routers inside the test harness.
[<RequireQualifiedAccess>]
module Router =

    /// Creates a router from a matcher function.
    let create matchFn = Router.Create matchFn

    /// A router that never matches any routes (always returns ValueNone).
    let empty =
        create (fun _ _ _ -> ValueNone)

    /// Create a router that matches exact method/path combinations using a simple lookup table.
    let ofTable (routes: seq<HttpMethod * string * HandlerVariant>) =
        let normalized =
            routes
            |> Seq.map (fun (method, path, handler) -> ((method, path.Trim()), handler))
            |> dict

        create (fun _ method path ->
            let key = (method, path.Trim())
            if normalized.ContainsKey key then
                ValueSome(normalized[key])
            else
                ValueNone)
