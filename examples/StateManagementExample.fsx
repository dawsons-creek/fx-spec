/// This example demonstrates how to use state management features
/// like `let'`, `subject`, and hooks (`before`/`after`).

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

let statefulSpec =
    spec {
        describe "A Stack" {
            // Use `let'` to define a lazy-loaded, memoized variable for the stack.
            // This factory function is executed only once per test example.
            let' "stack" (fun () -> Stack<int>())

            // `subject` is syntactic sugar for `let' "subject" ...`
            // It defines the primary object under test.
            subject (fun () -> 
                let stack = StateHelpers.get "stack" |> Option.get :?> Stack<int>
                stack.Push(1)
                stack
            )

            context "when newly created" {
                it "is empty" {
                    let stack = StateHelpers.get "stack" |> Option.get :?> Stack<int>
                    expect stack.Count |> to' (equal 0)
                }

                it "can have items pushed to it" {
                    let stack = StateHelpers.get "stack" |> Option.get :?> Stack<int>
                    stack.Push(42)
                    expect stack.Count |> to' (equal 1)
                    expect stack.Peek() |> to' (equal 42)
                }
            }

            context "with a subject" {
                it "has one item" {
                    let stack = StateHelpers.getSubject() |> Option.get :?> Stack<int>
                    expect stack.Count |> to' (equal 1)
                }
            }

            context "with hooks" {
                // `before` runs before each test in this context.
                before (fun () ->
                    let stack = StateHelpers.get "stack" |> Option.get :?> Stack<int>
                    stack.Push(10)
                    stack.Push(20)
                    printfn "-- Before hook executed --"
                )

                // `after` runs after each test in this context.
                after (fun () ->
                    let stack = StateHelpers.get "stack" |> Option.get :?> Stack<int>
                    stack.Clear()
                    printfn "-- After hook executed --"
                )

                it "has the correct number of items" {
                    let stack = StateHelpers.get "stack" |> Option.get :?> Stack<int>
                    // The stack has 2 items from the `before` hook
                    expect stack.Count |> to' (equal 2)
                }

                it "pops items in LIFO order" {
                    let stack = StateHelpers.get "stack" |> Option.get :?> Stack<int>
                    // The stack has 2 items from the `before` hook
                    expect (stack.Pop()) |> to' (equal 20)
                    expect (stack.Pop()) |> to' (equal 10)
                }
            }
        }
    }

// To run this example, you would typically use the FxSpec runner.
// For demonstration, we can manually execute parts of it.

printfn "Running stateful spec..."

// In a real scenario, the runner would handle this.
// This is a simplified execution for demonstration.
let execute (node: TestNode) =
    match node with
    | Example (desc, test) -> 
        printfn "- %s" desc
        match test() with
        | Pass -> printfn "  ✓ Passed"
        | Fail ex -> printfn "  ✗ Failed: %A" ex
        | Skipped reason -> printfn "  - Skipped: %s" reason
    | Group (desc, children) ->
        printfn "%s" desc
        children |> List.iter execute
    | _ -> ()

statefulSpec |> List.iter execute
