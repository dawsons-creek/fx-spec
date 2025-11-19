namespace FX.Spec.Performance

open System
open FX.Spec.Core

/// Configuration for load-test style scenarios.
type PerfLoadTestConfig =
    { Target: Uri option
      Duration: TimeSpan
      Connections: int option
      Warmup: TimeSpan option
      Notes: string option }

/// Configuration for microbenchmark style scenarios.
type PerfBenchmarkConfig =
    { Iterations: int
      WarmupIterations: int
      Measure: unit -> unit
      BenchmarkName: string option }

/// Placeholder representation of a measurement result.
type PerfMeasurement =
    | PendingMeasurement of string

/// Placeholder representation of an evaluation pipeline state.
type PerfEvaluationState =
    | PendingEvaluation of string

/// Builder passed to performance specs. All operations currently throw
/// to make it clear the implementation is pending.
type PerfSpecBuilder internal () =
    member _.loadTest (config: PerfLoadTestConfig) : PerfMeasurement =
        let target =
            config.Target
            |> Option.map (fun uri -> uri.ToString())
            |> Option.defaultValue "<unspecified>"
        let message =
            $"perf.loadTest is not implemented yet. Requested target: {target}"
        raise (NotSupportedException(message))

    member _.benchmark (config: PerfBenchmarkConfig) : PerfMeasurement =
        let message =
            $"perf.benchmark is not implemented yet. Requested iterations: {config.Iterations}"
        raise (NotSupportedException(message))

    member _.profiled (profileKey: string) (scenario: PerfSpecBuilder -> unit) : unit =
        let message =
            $"perf.profiled is not implemented yet. Profile requested: {profileKey}"
        raise (NotSupportedException(message))

[<RequireQualifiedAccess>]
module PerformanceDsl =
    /// Top-level container for performance scenarios (alias for describe).
    let describePerf description (tests: TestNode list) : TestNode =
        describe description tests

    /// Context helper mirroring the behavior DSL.
    let contextPerf description (tests: TestNode list) : TestNode =
        describe description tests

    /// Defines a performance scenario. Currently executes the supplied block
    /// with a fresh PerfSpecBuilder and relies on the core DSL for wrapping.
    let itPerf description (scenario: PerfSpecBuilder -> unit) : TestNode =
        it description (fun () ->
            let builder = PerfSpecBuilder()
            scenario builder)

    /// Begins an expectation pipeline for a measurement.
    let expectPerf (measurement: PerfMeasurement) : PerfEvaluationState =
        match measurement with
        | PendingMeasurement note -> PendingEvaluation note

    /// Placeholder expectation helper that currently throws to indicate
    /// the feature is not yet available.
    let toMeetThroughputProfile (profileKey: string) (state: PerfEvaluationState) : PerfEvaluationState =
        match state with
        | PendingEvaluation note ->
            let message =
                $"toMeetThroughputProfile is not implemented yet. Profile '{profileKey}'. Details: {note}"
            raise (NotSupportedException(message))

    /// Placeholder tolerance helper mirroring the eventual fluent API.
    let withTolerancePercent (tolerance: float) (state: PerfEvaluationState) : PerfEvaluationState =
        match state with
        | PendingEvaluation note ->
            let message =
                $"withTolerancePercent is not implemented yet. Requested tolerance: {tolerance}. Details: {note}"
            raise (NotSupportedException(message))

    /// Convenience helper for marking future expectation steps as pending.
    let pendingExpectation (message: string) : PerfEvaluationState =
        PendingEvaluation message

    /// Convenience helper for creating a placeholder measurement.
    let pendingMeasurement (message: string) : PerfMeasurement =
        PendingMeasurement message
