namespace FxSpec.Core

open System

// The SpecBuilder computation expression has been removed.
// It is no longer needed with the simplified DSL.
//
// Old style: spec { yield describe "..." [...] }
// New style: describe "..." [...]
//
// Just use describe/context directly to create test trees.

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
        // Separate hooks from other nodes
        let hooks, nonHooks =
            tests |> List.partition (function
                | BeforeAllHook _ | BeforeEachHook _ | AfterEachHook _ | AfterAllHook _ -> true
                | _ -> false)

        // Collect hooks into GroupHooks
        let groupHooks =
            hooks |> List.fold (fun acc node ->
                match node with
                | BeforeAllHook h -> GroupHooks.addBeforeAll h acc
                | BeforeEachHook h -> GroupHooks.addBeforeEach h acc
                | AfterEachHook h -> GroupHooks.addAfterEach h acc
                | AfterAllHook h -> GroupHooks.addAfterAll h acc
                | _ -> acc
            ) GroupHooks.empty

        Group(description, groupHooks, nonHooks)

    /// Creates a context node (alias for describe).
    /// Usage: context "description" [ ... tests ... ]
    let context description (tests: TestNode list) : TestNode =
        describe description tests

    /// Creates a skipped test example (xit = "exclude it").
    /// Usage: xit "description" (fun () -> test code)
    let xit description (test: unit -> unit) : TestNode =
        let execution () = Skipped "Test marked as pending with xit"
        Example(description, execution)

    /// Creates a skipped test example (alias for xit).
    /// Usage: pending "description" (fun () -> test code)
    let pending description (test: unit -> unit) : TestNode =
        xit description test

    /// Creates a focused test example (fit = "focused it").
    /// When any fit is present, only fit tests will run.
    /// Usage: fit "description" (fun () -> test code)
    let fit description (test: unit -> unit) : TestNode =
        let execution () =
            try
                test()
                Pass
            with ex ->
                Fail(Some ex)
        FocusedExample(description, execution)

    /// Creates a focused group (fdescribe = "focused describe").
    /// When any fdescribe is present, only tests in fdescribe blocks will run.
    /// Usage: fdescribe "description" [ ... tests ... ]
    let fdescribe description (tests: TestNode list) : TestNode =
        // Separate hooks from other nodes
        let hooks, nonHooks =
            tests |> List.partition (function
                | BeforeAllHook _ | BeforeEachHook _ | AfterEachHook _ | AfterAllHook _ -> true
                | _ -> false)

        // Collect hooks into GroupHooks
        let groupHooks =
            hooks |> List.fold (fun acc node ->
                match node with
                | BeforeAllHook h -> GroupHooks.addBeforeAll h acc
                | BeforeEachHook h -> GroupHooks.addBeforeEach h acc
                | AfterEachHook h -> GroupHooks.addAfterEach h acc
                | AfterAllHook h -> GroupHooks.addAfterAll h acc
                | _ -> acc
            ) GroupHooks.empty

        FocusedGroup(description, groupHooks, nonHooks)

    /// Creates a focused context (alias for fdescribe).
    /// Usage: fcontext "description" [ ... tests ... ]
    let fcontext description (tests: TestNode list) : TestNode =
        fdescribe description tests

    /// Registers a beforeAll hook.
    /// Usage: beforeAll (fun () -> setup code)
    let beforeAll (hook: unit -> unit) : TestNode =
        BeforeAllHook hook

    /// Registers a beforeEach hook.
    /// Usage: beforeEach (fun () -> setup code)
    let beforeEach (hook: unit -> unit) : TestNode =
        BeforeEachHook hook

    /// Registers an afterEach hook.
    /// Usage: afterEach (fun () -> teardown code)
    let afterEach (hook: unit -> unit) : TestNode =
        AfterEachHook hook

    /// Registers an afterAll hook.
    /// Usage: afterAll (fun () -> teardown code)
    let afterAll (hook: unit -> unit) : TestNode =
        AfterAllHook hook

