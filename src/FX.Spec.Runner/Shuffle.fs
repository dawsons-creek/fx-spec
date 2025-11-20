namespace FX.Spec.Runner

module Shuffle =

    open System
    open FX.Spec.Core

    /// Shuffles a list recursively, including nested groups.
    /// Uses the provided Random instance to ensure deterministic shuffling with a seed.
    let rec shuffleRec (random: Random) (nodes: TestNode list) : TestNode list =
        // Shuffle the current level
        let shuffledNodes =
            nodes
            |> List.map (fun node -> (random.Next(), node))
            |> List.sortBy fst
            |> List.map snd

        // Recursively shuffle children in groups
        shuffledNodes
        |> List.map (function
            | Group(desc, hooks, children, metadata) -> Group(desc, hooks, shuffleRec random children, metadata)
            | FocusedGroup(desc, hooks, children, metadata) ->
                FocusedGroup(desc, hooks, shuffleRec random children, metadata)
            | other -> other)

    /// Shuffles a list of test nodes using the provided seed.
    /// Shuffles both top-level nodes and nested examples recursively.
    let shuffle (seed: int) (nodes: TestNode list) : TestNode list =
        let random = Random(seed)
        shuffleRec random nodes
