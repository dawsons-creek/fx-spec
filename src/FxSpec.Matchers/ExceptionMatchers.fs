namespace FxSpec.Matchers

open System

/// Matchers for exception testing.
[<AutoOpen>]
module ExceptionMatchers =
    
    /// Matches if the function raises an exception of the specified type.
    /// Usage: expect (fun () -> failwith "error") |> to' raiseException<Exception>
    let raiseException<'T when 'T :> exn> : Matcher<unit -> unit> =
        fun f ->
            try
                f()
                let expectedType = typeof<'T>.Name
                let msg = sprintf "Expected an exception of type %s to be thrown, but nothing was thrown" expectedType
                Fail (msg, Some (box expectedType), None)
            with
            | :? 'T as ex ->
                Pass
            | ex ->
                let expectedType = typeof<'T>.Name
                let actualType = ex.GetType().Name
                let msg = sprintf "Expected an exception of type %s, but an exception of type %s was thrown: %s" expectedType actualType ex.Message
                Fail (msg, Some (box expectedType), Some (box actualType))
    
    /// Matches if the function raises an exception with the specified message.
    /// Usage: expect (fun () -> failwith "error") |> to' (raiseExceptionWithMessage "error")
    let raiseExceptionWithMessage (expectedMessage: string) : Matcher<unit -> unit> =
        fun f ->
            try
                f()
                let msg = sprintf "Expected an exception with message '%s' to be thrown, but nothing was thrown" expectedMessage
                Fail (msg, Some (box expectedMessage), None)
            with ex ->
                if ex.Message = expectedMessage then
                    Pass
                else
                    let msg = sprintf "Expected exception message '%s', but found '%s'" expectedMessage ex.Message
                    Fail (msg, Some (box expectedMessage), Some (box ex.Message))
    
    /// Matches if the function raises an exception containing the specified message.
    /// Usage: expect (fun () -> failwith "error occurred") |> to' (raiseExceptionContaining "error")
    let raiseExceptionContaining (expectedSubstring: string) : Matcher<unit -> unit> =
        fun f ->
            try
                f()
                let msg = sprintf "Expected an exception containing '%s' to be thrown, but nothing was thrown" expectedSubstring
                Fail (msg, Some (box expectedSubstring), None)
            with ex ->
                if ex.Message.Contains(expectedSubstring) then
                    Pass
                else
                    let msg = sprintf "Expected exception message to contain '%s', but found '%s'" expectedSubstring ex.Message
                    Fail (msg, Some (box expectedSubstring), Some (box ex.Message))
    
    /// Matches if the function raises a specific exception instance.
    /// Usage: expect (fun () -> raise myException) |> to' (raiseExceptionMatching (fun ex -> ex.Message = "specific"))
    let raiseExceptionMatching<'T when 'T :> exn> (predicate: 'T -> bool) (description: string) : Matcher<unit -> unit> =
        fun f ->
            try
                f()
                let expectedType = typeof<'T>.Name
                let msg = sprintf "Expected an exception of type %s matching '%s' to be thrown, but nothing was thrown" expectedType description
                Fail (msg, Some (box description), None)
            with
            | :? 'T as ex when predicate ex ->
                Pass
            | :? 'T as ex ->
                let msg = sprintf "Expected exception to match '%s', but it did not: %s" description ex.Message
                Fail (msg, Some (box description), Some (box ex.Message))
            | ex ->
                let expectedType = typeof<'T>.Name
                let actualType = ex.GetType().Name
                let msg = sprintf "Expected an exception of type %s, but an exception of type %s was thrown" expectedType actualType
                Fail (msg, Some (box expectedType), Some (box actualType))
    
    /// Matches if the function does not raise any exception.
    /// Usage: expect (fun () -> 1 + 1) |> to' notRaiseException
    let notRaiseException : Matcher<unit -> unit> =
        fun f ->
            try
                f()
                Pass
            with ex ->
                let msg = sprintf "Expected no exception to be thrown, but %s was thrown: %s" (ex.GetType().Name) ex.Message
                Fail (msg, None, Some (box ex))

