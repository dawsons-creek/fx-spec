/// BDD Specs for async test support in FxSpec
/// Demonstrates and validates itAsync, fitAsync, xitAsync functionality
module FxSpec.Core.Tests.AsyncSupportSpecs

open System
open System.Threading.Tasks
open FxSpec.Core
open FxSpec.Matchers

[<Tests>]
let asyncSupportSpecs =
    describe "Async Test Support" [
        
        context "itAsync" [
            itAsync "runs async test successfully" (async {
                let! result = async { return 42 }
                expect(result).toEqual(42)
            })

            itAsync "handles async computation expressions" (async {
                let! x = async { return 10 }
                let! y = async { return 20 }
                let sum = x + y
                expect(sum).toEqual(30)
            })

            itAsync "works with Task interop" (async {
                let task = Task.Run(fun () -> 42)
                let! result = Async.AwaitTask task
                expect(result).toEqual(42)
            })
        ]

        context "fitAsync (focused async tests)" [
            it "creates a FocusedExample node" (fun () ->
                let node = fitAsync "focused test" (async { return () })
                match node with
                | FocusedExample (desc, _) ->
                    expect(desc).toEqual("focused test")
                | _ ->
                    failwith "Expected FocusedExample node"
            )
        ]

        context "xitAsync (skipped async tests)" [
            it "creates a skipped Example node" (fun () ->
                let node = xitAsync "skipped test" (async { return () })
                match node with
                | Example (desc, execution) ->
                    expect(desc).toEqual("skipped test")
                    let result = execution()
                    match result with
                    | Skipped reason -> 
                        expectStr(reason).toContain("pending")
                    | _ -> failwith "Expected Skipped result"
                | _ ->
                    failwith "Expected Example node"
            )
        ]

        context "async tests with HTTP scenarios" [
            itAsync "simulates async HTTP request" (async {
                let! response = async {
                    do! Async.Sleep 10
                    return "OK"
                }
                expect(response).toEqual("OK")
            })

            itAsync "handles multiple async operations" (async {
                let operations = [
                    async { return 1 }
                    async { return 2 }
                    async { return 3 }
                ]
                
                let! results = Async.Parallel operations
                expectSeq(results |> Array.toList).toContainAll([1; 2; 3])
            })
        ]

        context "async Result patterns" [
            itAsync "works with async Result workflows" (async {
                let fetchData () = async {
                    do! Async.Sleep 5
                    return Ok 42
                }
                
                let! result = fetchData()
                expectResult(result).toBeOk(42)
            })

            itAsync "handles async Error results" (async {
                let failingOperation () = async {
                    do! Async.Sleep 5
                    return Error "Something went wrong"
                }
                
                let! result = failingOperation()
                expectResult(result).toBeError("Something went wrong")
            })
        ]

        context "backwards compatibility" [
            it "regular it still works for sync tests" (fun () ->
                let x = 42
                expect(x).toEqual(42)
            )

            it "can mix sync and async tests" (fun () ->
                expectBool(true).toBeTrue()
            )
        ]
    ]
