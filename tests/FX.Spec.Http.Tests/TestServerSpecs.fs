/// BDD specifications for TestServer lifecycle
module FxSpec.Http.Tests.TestServerSpecs

open System
open System.Net.Http
open System.Threading
open System.Threading.Tasks
open FxSpec.Core
open FxSpec.Matchers
open FxSpec.Http

[<Tests>]
let testServerLifecycleSpecs =
    describe "TestServer Lifecycle" [

        context "HttpClient management" [
            it "returns the same HttpClient instance on multiple accesses" (fun () ->
                let port = Random().Next(8000, 9000)
                use server = createTestServerWithPort port (fun ct -> task {
                    // Simple server that just runs
                    while not ct.IsCancellationRequested do
                        do! Task.Delay(100)
                })
                server.Start()

                let client1 = server.Client
                let client2 = server.Client
                let client3 = server.Client

                // All accesses should return the same instance
                expectBool(Object.ReferenceEquals(client1, client2)).toBeTrue()
                expectBool(Object.ReferenceEquals(client2, client3)).toBeTrue()
            )

            it "disposes HttpClient when server is disposed" (fun () ->
                let port = Random().Next(8000, 9000)
                let mutable clientReference: HttpClient option = None

                do
                    use server = createTestServerWithPort port (fun ct -> task {
                        while not ct.IsCancellationRequested do
                            do! Task.Delay(100)
                    })
                    server.Start()
                    clientReference <- Some server.Client
                // Server disposed here

                // Client should be disposed
                match clientReference with
                | Some client ->
                    expectThrows<ObjectDisposedException>(fun () ->
                        client.GetAsync("http://example.com") |> ignore
                    )
                | None -> failwith "Client reference was not set"
            )
        ]

        context "startup configuration" [
            it "respects custom startup delay when provided" (fun () ->
                let port = Random().Next(8000, 9000)
                let mutable serverStartTime = DateTime.MinValue

                use server = TestServer.Create(
                    (fun ct -> task {
                        serverStartTime <- DateTime.UtcNow
                        while not ct.IsCancellationRequested do
                            do! Task.Delay(100)
                    }),
                    port,
                    startupDelay = 200
                )

                let beforeStart = DateTime.UtcNow
                server.Start()
                let afterStart = DateTime.UtcNow

                // Startup should have waited at least ~200ms
                let elapsedMs = (afterStart - beforeStart).TotalMilliseconds
                expectBool(elapsedMs >= 180.0).toBeTrue()
            )

            it "uses default startup delay when not specified" (fun () ->
                let port = Random().Next(8000, 9000)
                use server = TestServer.Create(
                    (fun ct -> task {
                        while not ct.IsCancellationRequested do
                            do! Task.Delay(100)
                    }),
                    port
                )

                let beforeStart = DateTime.UtcNow
                server.Start()
                let afterStart = DateTime.UtcNow

                // Should use default delay (100ms)
                let elapsedMs = (afterStart - beforeStart).TotalMilliseconds
                expectBool(elapsedMs >= 80.0 && elapsedMs < 500.0).toBeTrue()
            )
        ]

        context "shutdown" [
            it "properly cancels server task on Stop" (fun () ->
                let port = Random().Next(8000, 9000)
                let mutable wasCancelled = false

                use server = createTestServerWithPort port (fun ct -> task {
                    try
                        while not ct.IsCancellationRequested do
                            do! Task.Delay(50)
                        wasCancelled <- true
                    with
                    | :? OperationCanceledException -> wasCancelled <- true
                })

                server.Start()
                Threading.Thread.Sleep(100)
                server.Stop()
                Threading.Thread.Sleep(100)

                expectBool(wasCancelled).toBeTrue()
            )

            it "can safely call Stop multiple times" (fun () ->
                let port = Random().Next(8000, 9000)
                use server = createTestServerWithPort port (fun ct -> task {
                    while not ct.IsCancellationRequested do
                        do! Task.Delay(100)
                })

                server.Start()
                server.Stop()
                server.Stop()  // Should not throw
                server.Stop()  // Should not throw
            )

            it "properly disposes on Dispose call" (fun () ->
                let port = Random().Next(8000, 9000)
                let mutable wasCancelled = false

                let server = createTestServerWithPort port (fun ct -> task {
                    try
                        while not ct.IsCancellationRequested do
                            do! Task.Delay(50, ct)
                    with
                    | :? OperationCanceledException -> wasCancelled <- true
                })

                server.Start()
                Threading.Thread.Sleep(150)
                (server :> IDisposable).Dispose()
                Threading.Thread.Sleep(300)

                expectBool(wasCancelled).toBeTrue()
            )
        ]

        context "server lifecycle states" [
            it "can restart server after Stop" (fun () ->
                let port = Random().Next(8000, 9000)
                use server = createTestServerWithPort port (fun ct -> task {
                    while not ct.IsCancellationRequested do
                        do! Task.Delay(100)
                })

                server.Start()
                server.Stop()
                server.Start()  // Should work
            )

            it "does not start twice if called multiple times" (fun () ->
                let port = Random().Next(8000, 9000)
                let mutable startCount = 0

                use server = createTestServerWithPort port (fun ct -> task {
                    Interlocked.Increment(&startCount) |> ignore
                    while not ct.IsCancellationRequested do
                        do! Task.Delay(100)
                })

                server.Start()
                server.Start()  // Should be idempotent
                server.Start()  // Should be idempotent

                Threading.Thread.Sleep(200)
                expectInt(startCount).toEqual(1)
            )
        ]
    ]
