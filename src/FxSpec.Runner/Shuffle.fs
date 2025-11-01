namespace FxSpec.Runner

module Shuffle =

    open System
    open FxSpec.Core

    /// Shuffles a list of test nodes using the provided seed.
    let shuffle (seed: int) (nodes: TestNode list) : TestNode list =
        let random = Random(seed)
        nodes
        |> List.map (fun node -> (random.Next(), node))
        |> List.sortBy fst
        |> List.map snd
