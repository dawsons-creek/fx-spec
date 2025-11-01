/// Comprehensive tests for Result matchers
module FxSpec.Matchers.Tests.ResultMatcherSpecs

open FxSpec.Core
open FxSpec.Matchers

// Test domain types
type AuthError = Unauthorized | Forbidden
type ValidationError = { Field: string; Message: string }
type DbError = NotFound | ConnectionError

[<Tests>]
let resultMatcherSpecs =
    describe "Result Matchers" [
        context "toBeOk with expected value" [
            it "passes when Result is Ok with matching value" (fun () ->
                let result: Result<int, string> = Ok 42
                expectResult(result).toBeOk(42)
            )
            
            it "fails when Result is Ok with different value" (fun () ->
                let result: Result<int, string> = Ok 42
                expectThrows<AssertionException>(fun () ->
                    expectResult(result).toBeOk(100)
                )
            )
            
            it "fails when Result is Error" (fun () ->
                let result: Result<int, string> = Error "failed"
                expectThrows<AssertionException>(fun () ->
                    expectResult(result).toBeOk(42)
                )
            )
        ]
        
        context "toBeOk without value (any Ok)" [
            it "passes when Result is Ok with any value" (fun () ->
                let result1: Result<int, string> = Ok 42
                let result2: Result<int, string> = Ok 100
                let result3: Result<string, string> = Ok "success"
                expectResult(result1).toBeOk()
                expectResult(result2).toBeOk()
                expectResult(result3).toBeOk()
            )
            
            it "fails when Result is Error" (fun () ->
                let result: Result<int, string> = Error "failed"
                expectThrows<AssertionException>(fun () ->
                    expectResult(result).toBeOk()
                )
            )
            
            it "provides helpful error message" (fun () ->
                let result: Result<int, string> = Error "something went wrong"
                try
                    expectResult(result).toBeOk()
                    failwith "Should have thrown"
                with
                | :? AssertionException as ex ->
                    expectStr(ex.Message).toContain("Expected Ok")
                    expectStr(ex.Message).toContain("Error")
            )
        ]
        
        context "toBeError with expected value" [
            it "passes when Result is Error with matching value" (fun () ->
                let result: Result<int, string> = Error "failed"
                expectResult(result).toBeError("failed")
            )
            
            it "fails when Result is Error with different value" (fun () ->
                let result: Result<int, string> = Error "failed"
                expectThrows<AssertionException>(fun () ->
                    expectResult(result).toBeError("other error")
                )
            )
            
            it "fails when Result is Ok" (fun () ->
                let result: Result<int, string> = Ok 42
                expectThrows<AssertionException>(fun () ->
                    expectResult(result).toBeError("failed")
                )
            )
        ]
        
        context "toBeError without value (any Error)" [
            it "passes when Result is Error with any value" (fun () ->
                let result1: Result<int, string> = Error "error1"
                let result2: Result<int, string> = Error "error2"
                let result3: Result<int, int> = Error 404
                expectResult(result1).toBeError()
                expectResult(result2).toBeError()
                expectResult(result3).toBeError()
            )
            
            it "fails when Result is Ok" (fun () ->
                let result: Result<int, string> = Ok 42
                expectThrows<AssertionException>(fun () ->
                    expectResult(result).toBeError()
                )
            )
            
            it "provides helpful error message" (fun () ->
                let result: Result<int, string> = Ok 42
                try
                    expectResult(result).toBeError()
                    failwith "Should have thrown"
                with
                | :? AssertionException as ex ->
                    expectStr(ex.Message).toContain("Expected Error")
                    expectStr(ex.Message).toContain("Ok")
            )
        ]
        
        context "async Result patterns" [
            itAsync "works with async Result<T,E> returning Ok" (async {
                let asyncResult = async { return Ok 42 }
                let! result = asyncResult
                expectResult(result).toBeOk()
                expectResult(result).toBeOk(42)
            })
            
            itAsync "works with async Result<T,E> returning Error" (async {
                let asyncResult = async { return Error "failed" }
                let! result = asyncResult
                expectResult(result).toBeError()
                expectResult(result).toBeError("failed")
            })
            
            itAsync "handles Task<Result<T,E>> patterns" (async {
                let taskResult = System.Threading.Tasks.Task.FromResult(Ok "success")
                let! result = taskResult |> Async.AwaitTask
                expectResult(result).toBeOk()
                expectResult(result).toBeOk("success")
            })
        ]
        
        context "web framework patterns" [
            it "validates authorization results" (fun () ->
                let authorizedResult: Result<string, AuthError> = Ok "user123"
                let unauthorizedResult: Result<string, AuthError> = Error Unauthorized
                
                expectResult(authorizedResult).toBeOk()
                expectResult(unauthorizedResult).toBeError()
                expectResult(unauthorizedResult).toBeError(Unauthorized)
            )
            
            it "validates validation results" (fun () ->
                let validResult: Result<int, ValidationError list> = Ok 42
                let invalidResult: Result<int, ValidationError list> = 
                    Error [{ Field = "email"; Message = "Invalid format" }]
                
                expectResult(validResult).toBeOk(42)
                expectResult(invalidResult).toBeError()
            )
            
            itAsync "validates async database operations" (async {
                let fetchUser userId = async {
                    if userId > 0 then
                        return Ok {| Id = userId; Name = "User" |}
                    else
                        return Error NotFound
                }
                
                let! successResult = fetchUser 1
                let! errorResult = fetchUser -1
                
                expectResult(successResult).toBeOk()
                expectResult(errorResult).toBeError()
                expectResult(errorResult).toBeError(NotFound)
            })
        ]
    ]
