/// This example demonstrates how to use hooks (beforeEach, afterEach, beforeAll, afterAll)
/// for test setup and teardown.

#load "../src/FxSpec.Core/bin/Debug/net9.0/FxSpec.Core.dll"
#load "../src/FxSpec.Matchers/bin/Debug/net9.0/FxSpec.Matchers.dll"

open FxSpec.Core
open FxSpec.Matchers

/// A simple mutable stack for demonstration purposes.
/// In a real functional application, you would use an immutable data structure.
type Stack<'a>() =
    let mutable items = []
    member _.Push(item: 'a) = items <- item :: items
    member _.Pop() =
        match items with
        | [] -> failwith "Stack is empty"
        | head :: tail ->
            items <- tail
            head
    member _.Peek() = List.head items
    member _.Count = List.length items
    member _.Clear() = items <- []

/// Example showing hooks for setup and teardown
[<Tests>]
let hooksExample =
    describe "A Stack" [
            beforeAll (fun () ->
                printfn "-- Setting up test suite (runs once) --"
            )

            afterAll (fun () ->
                printfn "-- Tearing down test suite (runs once) --"
            )

            context "when newly created" [
                it "is empty" (fun () ->
                    let stack = Stack<int>()
                    expect(stack.Count).toEqual(0)
                )

                it "can have items pushed to it" (fun () ->
                    let stack = Stack<int>()
                    stack.Push(42)
                    expect(stack.Count).toEqual(1)
                    expect(stack.Peek()).toEqual(42)
                )
            ]

            context "with beforeEach and afterEach hooks" [
                // Mutable reference to demonstrate hook execution
                let mutable sharedStack = Stack<int>()

                beforeEach (fun () ->
                    sharedStack <- Stack<int>()
                    sharedStack.Push(10)
                    sharedStack.Push(20)
                    printfn "-- beforeEach: Stack initialized with 2 items --"
                )

                afterEach (fun () ->
                    sharedStack.Clear()
                    printfn "-- afterEach: Stack cleared --"
                )

                it "has items from beforeEach hook" (fun () ->
                    expect(sharedStack.Count).toEqual(2)
                )

                it "pops items in LIFO order" (fun () ->
                    expect(sharedStack.Pop()).toEqual(20)
                    expect(sharedStack.Pop()).toEqual(10)
                )

                it "can push additional items" (fun () ->
                    sharedStack.Push(30)
                    expect(sharedStack.Count).toEqual(3)
                    expect(sharedStack.Peek()).toEqual(30)
                )
            ]

            context "nested contexts with hooks" [
                let mutable outerValue = 0

                beforeEach (fun () ->
                    outerValue <- 1
                    printfn "-- Outer beforeEach: Set to 1 --"
                )

                it "has value from outer hook" (fun () ->
                    expect(outerValue).toEqual(1)
                )

                context "inner context" [
                    beforeEach (fun () ->
                        outerValue <- outerValue + 10
                        printfn "-- Inner beforeEach: Added 10 --"
                    )

                    it "has value from both outer and inner hooks" (fun () ->
                        // Outer hook sets to 1, inner hook adds 10
                        expect(outerValue).toEqual(11)
                    )

                    it "hooks run for each test" (fun () ->
                        // Fresh execution: outer sets 1, inner adds 10
                        expect(outerValue).toEqual(11)
                    )
                ]
            ]
        ]
    ]

printfn "\nThis example demonstrates:"
printfn "1. beforeAll/afterAll - Run once per describe/context block"
printfn "2. beforeEach/afterEach - Run before/after each test"
printfn "3. Hook composition - Inner contexts inherit outer hooks"
printfn "4. Hooks execute in order: outer-to-inner for before, inner-to-outer for after"
printfn "\nRun with: dotnet run --project src/FxSpec.Runner -- examples/StateManagementExample.fsx"
