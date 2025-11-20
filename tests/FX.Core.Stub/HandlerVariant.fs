namespace FX.Core

open System
open System.Threading.Tasks

open Microsoft.AspNetCore.Http

/// Represents the different handler styles supported by FX routes.
type HandlerVariant =
    | Standard of (HttpContext -> ValueTask<Result<unit, HttpError>>)
    | Raw of (HttpContext -> Task)

[<RequireQualifiedAccess>]
module HandlerVariant =

    /// Create a Standard handler variant from a callback.
    let standard handler = HandlerVariant.Standard handler

    /// Create a Raw handler variant from a callback.
    let raw handler = HandlerVariant.Raw handler

    /// Execute the handler variant, returning a Task that completes when the handler has finished.
    let execute (ctx: HttpContext) (variant: HandlerVariant) : Task<Result<unit, HttpError>> =
        match variant with
        | HandlerVariant.Standard handler -> (handler ctx).AsTask()
        | HandlerVariant.Raw handler ->
            let tcs =
                TaskCompletionSource<Result<unit, HttpError>>(TaskCreationOptions.RunContinuationsAsynchronously)

            let pending = handler ctx

            pending.ContinueWith(fun (completed: Task) ->
                if completed.IsCanceled then
                    tcs.TrySetCanceled() |> ignore
                elif completed.IsFaulted then
                    tcs.TrySetException(completed.Exception.InnerExceptions) |> ignore
                else
                    tcs.TrySetResult(Ok()) |> ignore)
            |> ignore

            tcs.Task
