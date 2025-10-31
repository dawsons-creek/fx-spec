namespace FxSpec.Core

open System

/// The computation expression builder for creating test specifications.
/// This builder constructs an immutable TestNode tree from the DSL syntax.
type SpecBuilder() =

    /// Yields a single test example node.
    member _.Yield(node: TestNode) : TestNode list =
        [node]

    /// Yields a list of test nodes.
    member _.YieldFrom(nodes: TestNode list) : TestNode list =
        nodes

    /// Combines two lists of test nodes.
    /// This enables sequential test definitions within a block.
    member _.Combine(a: TestNode list, b: TestNode list) : TestNode list =
        a @ b

    /// Delays the evaluation of a computation.
    /// This is necessary for proper lazy evaluation in the CE.
    member _.Delay(f: unit -> TestNode list) : TestNode list =
        f()

    /// Returns an empty list of test nodes.
    /// This is used for empty blocks.
    member _.Zero() : TestNode list =
        []

    /// Runs the computation and returns the final list of test nodes.
    /// This is the entry point that the compiler calls.
    member _.Run(nodes: TestNode list) : TestNode list =
        nodes

/// Helper functions for creating test nodes.
/// These are used directly in the DSL.
[<AutoOpen>]
module SpecHelpers =

    /// Creates a test example node.
    /// Usage: it "description" (fun () -> test code)
    let it description (test: unit -> unit) : TestNode =
        let execution () =
            try
                test()
                Pass
            with ex ->
                Fail(Some ex)
        Example(description, execution)

    /// Creates a group node with nested tests.
    /// Usage: describe "description" [ ... tests ... ]
    let describe description (tests: TestNode list) : TestNode =
        Group(description, tests)

    /// Creates a context node (alias for describe).
    /// Usage: context "description" [ ... tests ... ]
    let context description (tests: TestNode list) : TestNode =
        Group(description, tests)

/// The global instance of the SpecBuilder.
/// Users will write: spec { ... }
[<AutoOpen>]
module SpecBuilderInstance =
    let spec = SpecBuilder()

