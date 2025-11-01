namespace FxSpec.Matchers

open System.Collections.Generic

/// Matchers for collections and sequences.
[<AutoOpen>]
module CollectionMatchers =

    /// Maximum number of collection items to show in error messages
    let private maxItemsInErrorMessage = 10

    /// Maximum number of failing items to show when predicate fails
    let private maxFailingItemsToShow = 5

    /// Matches if the collection contains the expected item.
    /// Usage: expect [1; 2; 3] |> to' (contain 2)
    let contain (expected: 'a) : Matcher<'a seq> =
        fun actual ->
            if Seq.contains expected actual then
                Pass
            else
                let items = actual |> Seq.truncate maxItemsInErrorMessage |> Seq.toList
                let msg = sprintf "Expected collection to contain %A, but it did not. Collection: %A" expected items
                Fail (msg, Some (box expected), Some (box items))
    
    /// Matches if the collection is empty.
    /// Usage: expect [] |> to' beEmpty
    let beEmpty<'a> : Matcher<'a seq> =
        fun actual ->
            if Seq.isEmpty actual then
                Pass
            else
                let count = Seq.length actual
                let msg = sprintf "Expected empty collection, but found %d items" count
                Fail (msg, Some (box 0), Some (box count))
    
    /// Matches if the collection has the expected length.
    /// Usage: expect [1; 2; 3] |> to' (haveLength 3)
    let haveLength (expected: int) : Matcher<'a seq> =
        if expected < 0 then
            invalidArg (nameof expected) "Expected length must be non-negative"
        fun actual ->
            let actualLength = Seq.length actual
            if actualLength = expected then
                Pass
            else
                let msg = sprintf "Expected collection to have length %d, but found length %d" expected actualLength
                Fail (msg, Some (box expected), Some (box actualLength))
    
    /// Matches if the collection has at least the expected count.
    /// Usage: expect [1; 2; 3] |> to' (haveCountAtLeast 2)
    let haveCountAtLeast (expected: int) : Matcher<'a seq> =
        if expected < 0 then
            invalidArg (nameof expected) "Expected count must be non-negative"
        fun actual ->
            let actualLength = Seq.length actual
            if actualLength >= expected then
                Pass
            else
                let msg = sprintf "Expected collection to have at least %d items, but found %d" expected actualLength
                Fail (msg, Some (box expected), Some (box actualLength))
    
    /// Matches if the collection has at most the expected count.
    /// Usage: expect [1; 2] |> to' (haveCountAtMost 3)
    let haveCountAtMost (expected: int) : Matcher<'a seq> =
        if expected < 0 then
            invalidArg (nameof expected) "Expected count must be non-negative"
        fun actual ->
            let actualLength = Seq.length actual
            if actualLength <= expected then
                Pass
            else
                let msg = sprintf "Expected collection to have at most %d items, but found %d" expected actualLength
                Fail (msg, Some (box expected), Some (box actualLength))
    
    /// Matches if all items in the collection satisfy the predicate.
    /// Usage: expect [2; 4; 6] |> to' (allSatisfy (fun x -> x % 2 = 0))
    let allSatisfy (predicate: 'a -> bool) (description: string) : Matcher<'a seq> =
        fun actual ->
            if Seq.forall predicate actual then
                Pass
            else
                let failing = actual |> Seq.filter (predicate >> not) |> Seq.truncate maxFailingItemsToShow |> Seq.toList
                let msg = sprintf "Expected all items to satisfy '%s', but these did not: %A" description failing
                Fail (msg, None, Some (box failing))
    
    /// Matches if any item in the collection satisfies the predicate.
    /// Usage: expect [1; 2; 3] |> to' (anySatisfy (fun x -> x > 2))
    let anySatisfy (predicate: 'a -> bool) (description: string) : Matcher<'a seq> =
        fun actual ->
            if Seq.exists predicate actual then
                Pass
            else
                let items = actual |> Seq.truncate maxItemsInErrorMessage |> Seq.toList
                let msg = sprintf "Expected at least one item to satisfy '%s', but none did. Collection: %A" description items
                Fail (msg, None, Some (box items))
    
    /// Matches if the collection contains all expected items (in any order).
    /// Usage: expect [1; 2; 3; 4] |> to' (containAll [2; 4])
    let containAll (expected: 'a seq) : Matcher<'a seq> =
        fun actual ->
            let actualSet = Set.ofSeq actual
            let expectedList = Seq.toList expected
            let missing = expectedList |> List.filter (fun x -> not (Set.contains x actualSet))
            
            if List.isEmpty missing then
                Pass
            else
                let msg = sprintf "Expected collection to contain all of %A, but missing: %A" expectedList missing
                Fail (msg, Some (box expectedList), Some (box missing))
    
    /// Matches if the collection equals the expected sequence (same order).
    /// Usage: expect [1; 2; 3] |> to' (equalSeq [1; 2; 3])
    let equalSeq (expected: 'a seq) : Matcher<'a seq> =
        fun actual ->
            let expectedList = Seq.toList expected
            let actualList = Seq.toList actual
            
            if expectedList = actualList then
                Pass
            else
                let msg = sprintf "Expected sequence %A, but found %A" expectedList actualList
                Fail (msg, Some (box expectedList), Some (box actualList))
    
    /// Matches if the collection starts with the expected sequence.
    /// Usage: expect [1; 2; 3; 4] |> to' (startWithSeq [1; 2])
    let startWithSeq (expected: 'a seq) : Matcher<'a seq> =
        fun actual ->
            let expectedList = Seq.toList expected
            let actualPrefix = actual |> Seq.truncate (List.length expectedList) |> Seq.toList
            
            if expectedList = actualPrefix then
                Pass
            else
                let msg = sprintf "Expected sequence to start with %A, but found %A" expectedList actualPrefix
                Fail (msg, Some (box expectedList), Some (box actualPrefix))
    
    /// Matches if the collection ends with the expected sequence.
    /// Usage: expect [1; 2; 3; 4] |> to' (endWithSeq [3; 4])
    let endWithSeq (expected: 'a seq) : Matcher<'a seq> =
        fun actual ->
            let expectedList = Seq.toList expected
            let actualList = Seq.toList actual
            let actualSuffix = actualList |> List.skip (max 0 (List.length actualList - List.length expectedList))
            
            if expectedList = actualSuffix then
                Pass
            else
                let msg = sprintf "Expected sequence to end with %A, but found %A" expectedList actualSuffix
                Fail (msg, Some (box expectedList), Some (box actualSuffix))

