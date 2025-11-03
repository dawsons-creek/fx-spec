#r "nuget: FxSpec.Core"
#load "../src/FxSpec.Core/Types.fs"
#load "../src/FxSpec.Core/SpecBuilder.fs"

open FxSpec.Core

// Helper function that throws an exception
let divideByZero x =
    x / 0

let nestedFunction x =
    let innerFunction y =
        divideByZero y
    innerFunction x

let callAnotherFunction x =
    nestedFunction x

[<Tests>]
let errorTests = 
    describe "Error handling" [
        it "should show exception from code under test" (fun () ->
            let result = divideByZero 10
            result == 0
        )
        
        it "should show nested call stack" (fun () ->
            let result = callAnotherFunction 5
            result == 0
        )
        
        it "should show null reference exception" (fun () ->
            let nullString: string = null
            let length = nullString.Length
            length == 0
        )
        
        it "should show index out of range" (fun () ->
            let arr = [| 1; 2; 3 |]
            let item = arr.[10]
            item == 0
        )
    ]
