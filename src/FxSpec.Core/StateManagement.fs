namespace FxSpec.Core

open System.Collections.Concurrent

/// Represents a scope in the execution context.
/// Each describe/context block creates a new scope that is pushed onto the stack.
type ExecutionScope = {
    /// Lazy-loaded variable factories registered with let'
    LetBindings: Map<string, unit -> obj>
    
    /// Cache for memoized values within this test execution
    Cache: ConcurrentDictionary<string, obj>
    
    /// Setup hooks to run before each test
    BeforeHooks: (unit -> unit) list
    
    /// Teardown hooks to run after each test
    AfterHooks: (unit -> unit) list
}

/// Module for working with execution scopes.
module ExecutionScope =
    /// Creates an empty execution scope.
    let empty = {
        LetBindings = Map.empty
        Cache = ConcurrentDictionary<string, obj>()
        BeforeHooks = []
        AfterHooks = []
    }
    
    /// Adds a let binding to the scope.
    let addLetBinding name factory scope =
        { scope with LetBindings = scope.LetBindings |> Map.add name factory }
    
    /// Adds a before hook to the scope.
    let addBeforeHook hook scope =
        { scope with BeforeHooks = scope.BeforeHooks @ [hook] }
    
    /// Adds an after hook to the scope.
    let addAfterHook hook scope =
        { scope with AfterHooks = scope.AfterHooks @ [hook] }

/// Represents the execution context with a stack of scopes.
/// The stack grows as we enter nested describe/context blocks.
type ScopeStack = ExecutionScope list

/// Module for working with scope stacks.
module ScopeStack =
    /// Creates an empty scope stack.
    let empty : ScopeStack = []
    
    /// Pushes a new scope onto the stack.
    let push scope (stack: ScopeStack) : ScopeStack =
        scope :: stack
    
    /// Pops a scope from the stack.
    let pop (stack: ScopeStack) : ScopeStack =
        match stack with
        | [] -> []
        | _ :: rest -> rest
    
    /// Looks up a let binding in the scope stack.
    /// Searches from the innermost (top) scope to the outermost (bottom).
    let rec tryFindBinding name (stack: ScopeStack) : (unit -> obj) option =
        match stack with
        | [] -> None
        | scope :: rest ->
            match scope.LetBindings |> Map.tryFind name with
            | Some factory -> Some factory
            | None -> tryFindBinding name rest
    
    /// Gets a let binding value, using memoization.
    /// If the value has been computed in the current scope, return the cached value.
    /// Otherwise, execute the factory and cache the result.
    let getBinding name (stack: ScopeStack) : obj option =
        match tryFindBinding name stack with
        | None -> None
        | Some factory ->
            // Try to find cached value in any scope
            let cachedValue =
                stack
                |> List.tryPick (fun scope ->
                    match scope.Cache.TryGetValue(name) with
                    | true, value -> Some value
                    | false, _ -> None)
            
            match cachedValue with
            | Some value -> Some value
            | None ->
                // Compute and cache in the innermost scope
                let value = factory()
                match stack with
                | scope :: _ ->
                    scope.Cache.[name] <- value
                    Some value
                | [] -> Some value
    
    /// Collects all before hooks from the stack (outer to inner).
    let collectBeforeHooks (stack: ScopeStack) : (unit -> unit) list =
        stack
        |> List.rev  // Reverse to get outer-to-inner order
        |> List.collect (fun scope -> scope.BeforeHooks)
    
    /// Collects all after hooks from the stack (inner to outer).
    let collectAfterHooks (stack: ScopeStack) : (unit -> unit) list =
        stack
        |> List.collect (fun scope -> scope.AfterHooks)

/// Helper functions for use in test code.
/// These will be used by the DSL to register state and hooks.
module StateHelpers =
    // Note: These are placeholders. The actual implementation will need
    // to interact with the execution context, which is managed by the runner.
    // For now, we define the API that users will call.
    
    /// Thread-local storage for the current scope stack during test execution.
    let private currentScopeStack = new System.Threading.ThreadLocal<ScopeStack>(fun () -> ScopeStack.empty)
    
    /// Registers a lazy-loaded variable.
    /// Usage: let' "myVar" (fun () -> expensiveComputation())
    let let' name (factory: unit -> 'a) : unit =
        let objFactory () = box (factory())
        let currentScope = 
            match currentScopeStack.Value with
            | scope :: _ -> scope
            | [] -> ExecutionScope.empty
        
        let updatedScope = ExecutionScope.addLetBinding name objFactory currentScope
        
        // Update the stack (this is a simplified version; the runner will manage this properly)
        currentScopeStack.Value <- updatedScope :: (ScopeStack.pop currentScopeStack.Value)
    
    /// Gets a let-bound value from the current scope stack.
    /// Usage: let myVar = get "myVar" :?> MyType
    let get name : obj option =
        ScopeStack.getBinding name currentScopeStack.Value
    
    /// Registers a subject (the primary object under test).
    /// This is syntactic sugar over let' "subject" factory.
    let subject (factory: unit -> 'a) : unit =
        let' "subject" factory
    
    /// Gets the subject from the current scope.
    let getSubject() : obj option =
        get "subject"
    
    /// Registers a before hook.
    let before (hook: unit -> unit) : unit =
        let currentScope = 
            match currentScopeStack.Value with
            | scope :: _ -> scope
            | [] -> ExecutionScope.empty
        
        let updatedScope = ExecutionScope.addBeforeHook hook currentScope
        currentScopeStack.Value <- updatedScope :: (ScopeStack.pop currentScopeStack.Value)
    
    /// Registers an after hook.
    let after (hook: unit -> unit) : unit =
        let currentScope = 
            match currentScopeStack.Value with
            | scope :: _ -> scope
            | [] -> ExecutionScope.empty
        
        let updatedScope = ExecutionScope.addAfterHook hook currentScope
        currentScopeStack.Value <- updatedScope :: (ScopeStack.pop currentScopeStack.Value)

