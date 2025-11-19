// Basic example demonstrating the FX.Spec DSL
#r "../src/FX.Spec.Core/bin/Debug/net8.0/FX.Spec.Core.dll"

open FX.Spec.Core

// Example 1: Simple test structure
let simpleSpec =
    describe "Calculator" [
        it "adds two numbers" (fun () ->
            let result = 2 + 2
            if result <> 4 then
                failwith "Expected 4"
        )

        it "subtracts numbers" (fun () ->
            let result = 5 - 3
            if result <> 2 then
                failwith "Expected 2"
        )
    ]

// Example 2: Nested contexts
let nestedSpec =
    describe "User" [
        context "when newly created" [
            it "has no posts" (fun () ->
                // Test logic here
                ()
            )

            it "has default settings" (fun () ->
                // Test logic here
                ()
            )
        ]

        context "when activated" [
            it "can log in" (fun () ->
                // Test logic here
                ()
            )
        ]
    ]

// Example 3: Multiple describe blocks
let stackSpec =
    describe "Stack" [
        it "starts empty" (fun () ->
            let stack = System.Collections.Generic.Stack<int>()
            if stack.Count <> 0 then
                failwith "Expected empty stack"
        )
    ]

let queueSpec =
    describe "Queue" [
        it "is FIFO" (fun () ->
            let queue = System.Collections.Generic.Queue<int>()
            queue.Enqueue(1)
            queue.Enqueue(2)
            let first = queue.Dequeue()
            if first <> 1 then
                failwith "Expected FIFO order"
        )
    ]

// Print the structure to verify it builds correctly
printfn "Simple spec structure:"
match simpleSpec with
| Group (desc, _, children) ->
    printfn "  Group: %s (%d children)" desc (List.length children)
    children |> List.iter (fun child ->
        match child with
        | Example (desc, _) -> printfn "    Example: %s" desc
        | Group (desc, _, _) -> printfn "    Group: %s" desc
        | _ -> ()
    )
| Example (desc, _) ->
    printfn "  Example: %s" desc
| _ -> ()

printfn "\nNested spec structure:"
match nestedSpec with
| Group (desc, _, children) ->
    printfn "  Group: %s" desc
    children |> List.iter (fun child ->
        match child with
        | Group (desc2, _, children2) ->
            printfn "    Group: %s" desc2
            children2 |> List.iter (fun child2 ->
                match child2 with
                | Example (desc3, _) -> printfn "      Example: %s" desc3
                | _ -> ()
            )
        | Example (desc2, _) -> printfn "    Example: %s" desc2
        | _ -> ()
    )
| Example (desc, _) ->
    printfn "  Example: %s" desc
| _ -> ()

printfn "\nTest counts:"
printfn "Simple spec: %d examples, %d groups" 
    (TestNode.countExamples simpleSpec)
    (TestNode.countGroups simpleSpec)

printfn "Nested spec: %d examples, %d groups"
    (TestNode.countExamples nestedSpec)
    (TestNode.countGroups nestedSpec)

printfn "\n✓ DSL is working correctly!"

