namespace FX.Spec.Performance.Tests

open System
open FX.Spec.Core
open FX.Spec.Performance

module PerformanceDslSpecs =

    [<Tests>]
    let performanceDslSpecs =
        describe
            "Performance DSL"
            [ describe
                  "describePerf"
                  [ it "produces a group node compatible with the core describe helper" (fun () ->
                        let node = PerformanceDsl.describePerf "sample suite" []

                        match node with
                        | Group(description, _, children, _) ->
                            if description <> "sample suite" then
                                failwith $"Expected description to be 'sample suite' but it was '{description}'"

                            if not (List.isEmpty children) then
                                failwith "Expected children to be empty for the placeholder group"
                        | _ -> failwith "describePerf should yield a Group node") ]

              describe
                  "itPerf"
                  [ it "invokes the supplied scenario with a perf builder" (fun () ->
                        let invoked = ref false
                        let builderObserved = ref false

                        let node =
                            PerformanceDsl.itPerf "captures invocation" (fun perf ->
                                invoked := true

                                if perf |> box |> isNull |> not then
                                    builderObserved := true)

                        match node with
                        | Example(_, execute, _) ->
                            match execute () |> Async.RunSynchronously with
                            | Pass ->
                                if not !invoked then
                                    failwith "Expected the scenario to be invoked"

                                if not !builderObserved then
                                    failwith "Expected to observe the provided PerfSpecBuilder instance"
                            | Fail _ -> failwith "Expected the placeholder scenario to succeed"
                            | Skipped reason -> failwith $"Execution was unexpectedly skipped: {reason}"
                        | _ -> failwith "itPerf should yield an Example node") ]

              describe
                  "PerfSpecBuilder placeholders"
                  [ it "marks loadTest as not supported" (fun () ->
                        let config =
                            { Target = Some(Uri("http://localhost:8080/plaintext"))
                              Duration = TimeSpan.FromSeconds(5.)
                              Connections = Some 64
                              Warmup = Some(TimeSpan.FromSeconds(1.))
                              Notes = Some "placeholder" }

                        let node =
                            PerformanceDsl.itPerf "loadTest placeholder" (fun perf -> perf.loadTest config |> ignore)

                        match node with
                        | Example(_, execute, _) ->
                            match execute () |> Async.RunSynchronously with
                            | Fail(Some(:? NotSupportedException)) -> ()
                            | Fail(Some ex) -> failwith $"Unexpected exception type: {ex.GetType().FullName}"
                            | Fail None -> failwith "Expected a NotSupportedException instance, got none"
                            | Pass -> failwith "loadTest placeholder should not pass yet"
                            | Skipped reason -> failwith $"loadTest placeholder should not skip: {reason}"
                        | _ -> failwith "Expected Example node from itPerf")

                    it "marks benchmark as not supported" (fun () ->
                        let config =
                            { Iterations = 10
                              WarmupIterations = 3
                              Measure = (fun () -> ())
                              BenchmarkName = Some "noop" }

                        let node =
                            PerformanceDsl.itPerf "benchmark placeholder" (fun perf -> perf.benchmark config |> ignore)

                        match node with
                        | Example(_, execute, _) ->
                            match execute () |> Async.RunSynchronously with
                            | Fail(Some(:? NotSupportedException)) -> ()
                            | Fail(Some ex) -> failwith $"Unexpected exception type: {ex.GetType().FullName}"
                            | Fail None -> failwith "Expected a NotSupportedException instance, got none"
                            | Pass -> failwith "benchmark placeholder should not pass yet"
                            | Skipped reason -> failwith $"benchmark placeholder should not skip: {reason}"
                        | _ -> failwith "Expected Example node from itPerf")

                    it "marks profiled scenarios as not supported" (fun () ->
                        let node =
                            PerformanceDsl.itPerf "profiled placeholder" (fun perf ->
                                perf.profiled "baseline" (fun _ -> ()))

                        match node with
                        | Example(_, execute, _) ->
                            match execute () |> Async.RunSynchronously with
                            | Fail(Some(:? NotSupportedException)) -> ()
                            | Fail(Some ex) -> failwith $"Unexpected exception type: {ex.GetType().FullName}"
                            | Fail None -> failwith "Expected a NotSupportedException instance, got none"
                            | Pass -> failwith "profiled placeholder should not pass yet"
                            | Skipped reason -> failwith $"profiled placeholder should not skip: {reason}"
                        | _ -> failwith "Expected Example node from itPerf") ]

              describe
                  "expectPerf pipeline"
                  [ it "converts pending measurements into pending evaluations" (fun () ->
                        let measurement = PerformanceDsl.pendingMeasurement "deferred implementation"
                        let evaluation = PerformanceDsl.expectPerf measurement

                        match evaluation with
                        | PendingEvaluation note ->
                            if note <> "deferred implementation" then
                                failwith $"Expected pending note to be 'deferred implementation' but was '{note}'")

                    it "signals that throughput profile evaluation is not supported yet" (fun () ->
                        let state =
                            PerformanceDsl.pendingMeasurement "not ready" |> PerformanceDsl.expectPerf

                        let captured =
                            try
                                PerformanceDsl.toMeetThroughputProfile "plaintext" state |> ignore
                                None
                            with ex ->
                                Some ex

                        match captured with
                        | Some(:? NotSupportedException) -> ()
                        | Some ex -> failwith $"Expected NotSupportedException but received {ex.GetType().FullName}"
                        | None -> failwith "Expected to raise NotSupportedException, but no exception was thrown")

                    it "signals that tolerance adjustment is not supported yet" (fun () ->
                        let state =
                            PerformanceDsl.pendingMeasurement "not ready" |> PerformanceDsl.expectPerf

                        let captured =
                            try
                                PerformanceDsl.withTolerancePercent 5.0 state |> ignore
                                None
                            with ex ->
                                Some ex

                        match captured with
                        | Some(:? NotSupportedException) -> ()
                        | Some ex -> failwith $"Expected NotSupportedException but received {ex.GetType().FullName}"
                        | None -> failwith "Expected to raise NotSupportedException, but no exception was thrown") ] ]
