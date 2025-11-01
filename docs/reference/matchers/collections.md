# Collection Matchers

Matchers for lists, arrays, sequences, and other collections.

---

## Membership

### contain

**Type:** `'a -> Matcher<'a seq>`

Matches if the collection contains the expected item.

**Usage:**

```fsharp
expectSeq(collection).toContain(item)
```

**Examples:**

```fsharp
expectSeq([1; 2; 3]).toContain(2)
expectSeq([|"a"; "b"; "c"|]).toContain("b")
expectSeq((seq { 1..10 })).toContain(5)

// With custom types
type Person = { Name: string }
let people = [{ Name = "Alice" }; { Name = "Bob" }]
expectSeq(people).toContain({ Name = "Alice" })
```

**Failure Message:**

```fsharp
expectSeq([1; 2; 3]).toContain(5)
// => Expected collection to contain 5, but it did not. Collection: [1; 2; 3]
```

---

### containAll

**Type:** `'a seq -> Matcher<'a seq>`

Matches if the collection contains all expected items (in any order).

**Usage:**

```fsharp
expectSeq(collection).toContainAll(expected)
```

**Examples:**

```fsharp
expectSeq([1; 2; 3; 4; 5]).toContainAll([2; 4])
expectSeq([1; 2; 3; 4; 5]).toContainAll([5; 1; 3])  // Order doesn't matter
expectSeq(["a"; "b"; "c"; "d"]).toContainAll(["c"; "a"])

// Empty expected list always passes
expectSeq([1; 2; 3]).toContainAll([])
```

**Failure Message:**

```fsharp
expectSeq([1; 2; 3]).toContainAll([2; 5; 7])
// => Expected collection to contain all of [2; 5; 7], but missing: [5; 7]
```

---

## Size

### beEmpty

**Type:** `Matcher<'a seq>`

Matches if the collection is empty.

**Usage:**

```fsharp
expectSeq(collection).toBeEmpty()
```

**Examples:**

```fsharp
expectSeq([]).toBeEmpty()
expect [||] |> should beEmpty
expectSeq(Seq.empty).toBeEmpty()
expectSeq((List<int>())).toBeEmpty()

// After operations
let filtered = [1; 2; 3] |> List.filter (fun x -> x > 10)
expectSeq(filtered).toBeEmpty()
```

**Failure Message:**

```fsharp
expectSeq([1; 2; 3]).toBeEmpty()
// => Expected empty collection, but found 3 items
```

---

### haveLength

**Type:** `int -> Matcher<'a seq>`

Matches if the collection has the expected length.

**Usage:**

```fsharp
expectSeq(collection).toHaveLength(count)
```

**Examples:**

```fsharp
expectSeq([1; 2; 3]).toHaveLength(3)
expect [|"a"; "b"|] |> should (haveLength 2)
expectSeq("hello").toHaveLength(5)  // Strings are sequences of chars

// With operations
let doubled = [1; 2; 3] |> List.map (fun x -> x * 2)
expectSeq(doubled).toHaveLength(3)
```

**Failure Message:**

```fsharp
expectSeq([1; 2; 3]).toHaveLength(5)
// => Expected collection to have length 5, but found length 3
```

**Notes:**

- Validates that expected length is non-negative
- Works with any `seq<'a>`
- For strings, use `haveStringLength` for better error messages

---

### haveCountAtLeast

**Type:** `int -> Matcher<'a seq>`

Matches if the collection has at least the expected count.

**Usage:**

```fsharp
expect collection |> should (haveCountAtLeast minimum)
```

**Examples:**

```fsharp
expect [1; 2; 3; 4; 5] |> should (haveCountAtLeast 3)  // Passes (5 >= 3)
expect [1; 2] |> should (haveCountAtLeast 2)  // Passes (2 >= 2)
expect [1] |> should (haveCountAtLeast 5)  // Fails (1 < 5)

// Useful for pagination
let page = database.Query().Take(10)
expect page |> should (haveCountAtLeast 1)  // At least one result
```

**Failure Message:**

```fsharp
expect [1; 2] |> should (haveCountAtLeast 5)
// => Expected collection to have at least 5 items, but found 2
```

---

### haveCountAtMost

**Type:** `int -> Matcher<'a seq>`

Matches if the collection has at most the expected count.

**Usage:**

```fsharp
expect collection |> should (haveCountAtMost maximum)
```

**Examples:**

```fsharp
expect [1; 2] |> should (haveCountAtMost 5)  // Passes (2 <= 5)
expect [1; 2; 3] |> should (haveCountAtMost 3)  // Passes (3 <= 3)
expect [1; 2; 3; 4; 5] |> should (haveCountAtMost 3)  // Fails (5 > 3)

// Useful for limiting results
let recent = events |> List.take 100
expect recent |> should (haveCountAtMost 100)
```

**Failure Message:**

```fsharp
expect [1; 2; 3; 4; 5] |> should (haveCountAtMost 3)
// => Expected collection to have at most 3 items, but found 5
```

---

## Sequences

### equalSeq

**Type:** `'a seq -> Matcher<'a seq>`

Matches if the collection equals the expected sequence (same order, same values).

**Usage:**

```fsharp
expect collection |> should (equalSeq expected)
```

**Examples:**

```fsharp
expect [1; 2; 3] |> should (equalSeq [1; 2; 3])
expect [|"a"; "b"|] |> should (equalSeq ["a"; "b"])  // Arrays and lists can be compared

// Order matters!
expect [1; 2; 3] |> shouldNot (equalSeq [3; 2; 1])
expect [1; 2; 3] |> shouldNot (equalSeq [1; 3; 2])
```

**Failure Message:**

```fsharp
expect [1; 2; 3] |> should (equalSeq [1; 2; 4])
// => Expected sequence [1; 2; 4], but found [1; 2; 3]
```

**Notes:**

- Order matters (unlike `containAll`)
- Use `equal` for structural equality (works with lists, arrays, etc.)
- Use `equalSeq` when comparing different collection types

---

### startWithSeq

**Type:** `'a seq -> Matcher<'a seq>`

Matches if the collection starts with the expected sequence.

**Usage:**

```fsharp
expect collection |> should (startWithSeq prefix)
```

**Examples:**

```fsharp
expect [1; 2; 3; 4; 5] |> should (startWithSeq [1; 2])
expect [1; 2; 3; 4; 5] |> should (startWithSeq [1; 2; 3])
expect [1; 2; 3; 4; 5] |> should (startWithSeq [])  // Empty prefix always matches

// Fails
expect [1; 2; 3; 4; 5] |> shouldNot (startWithSeq [2; 3])
```

**Failure Message:**

```fsharp
expect [1; 2; 3] |> should (startWithSeq [2; 3])
// => Expected sequence to start with [2; 3], but found [1; 2]
```

---

### endWithSeq

**Type:** `'a seq -> Matcher<'a seq>`

Matches if the collection ends with the expected sequence.

**Usage:**

```fsharp
expect collection |> should (endWithSeq suffix)
```

**Examples:**

```fsharp
expect [1; 2; 3; 4; 5] |> should (endWithSeq [4; 5])
expect [1; 2; 3; 4; 5] |> should (endWithSeq [3; 4; 5])
expect [1; 2; 3; 4; 5] |> should (endWithSeq [])  // Empty suffix always matches

// Fails
expect [1; 2; 3; 4; 5] |> shouldNot (endWithSeq [3; 4])
```

**Failure Message:**

```fsharp
expect [1; 2; 3] |> should (endWithSeq [2; 4])
// => Expected sequence to end with [2; 4], but found [2; 3]
```

---

## Predicates

### allSatisfy

**Type:** `(('a -> bool) -> string -> Matcher<'a seq>)`

Matches if all items in the collection satisfy the predicate.

**Parameters:**

- `predicate` - Function to test each item
- `description` - Human-readable description

**Usage:**

```fsharp
expect collection |> should (allSatisfy predicate "description")
```

**Examples:**

```fsharp
expect [2; 4; 6; 8] |> should (allSatisfy (fun x -> x % 2 = 0) "be even")
expect ["hello"; "world"] |> should (allSatisfy (fun s -> s.Length > 0) "be non-empty")

// With domain logic
type User = { Name: string; Age: int }
let users = [
    { Name = "Alice"; Age = 25 }
    { Name = "Bob"; Age = 30 }
]
expect users |> should (allSatisfy (fun u -> u.Age >= 18) "be adults")
```

**Failure Message:**

```fsharp
expect [2; 3; 4; 5] |> should (allSatisfy (fun x -> x % 2 = 0) "be even")
// => Expected all items to satisfy 'be even', but these did not: [3; 5]
```

**Notes:**

- Shows up to 5 failing items in error message
- Empty collection always passes (vacuous truth)
- Use descriptive descriptions for clear error messages

---

### anySatisfy

**Type:** `(('a -> bool) -> string -> Matcher<'a seq>)`

Matches if at least one item in the collection satisfies the predicate.

**Parameters:**

- `predicate` - Function to test each item
- `description` - Human-readable description

**Usage:**

```fsharp
expect collection |> should (anySatisfy predicate "description")
```

**Examples:**

```fsharp
expect [1; 2; 3; 4; 5] |> should (anySatisfy (fun x -> x > 3) "be greater than 3")
expect ["hello"; "world"; "!"] |> should (anySatisfy (fun s -> s.Length = 1) "be a single character")

// Searching
type Product = { Name: string; Price: decimal; InStock: bool }
let products = [
    { Name = "Laptop"; Price = 999.99m; InStock = true }
    { Name = "Mouse"; Price = 29.99m; InStock = false }
]
expect products |> should (anySatisfy (fun p -> p.InStock) "be in stock")
```

**Failure Message:**

```fsharp
expect [1; 2; 3] |> should (anySatisfy (fun x -> x > 10) "be greater than 10")
// => Expected at least one item to satisfy 'be greater than 10', but none did. Collection: [1; 2; 3]
```

**Notes:**

- Fails on empty collection (no items to satisfy)
- Short-circuits on first match (efficient for large collections)
- Shows collection contents in error message (up to 10 items)

---

## Complete Examples

### Testing a Shopping Cart

```fsharp
module ShoppingCartSpecs

open FxSpec.Core
open FxSpec.Matchers

type CartItem = { ProductId: int; Quantity: int; Price: decimal }
type Cart = { Items: CartItem list }

[<Tests>]
let shoppingCartSpecs =
    spec {
        describe "ShoppingCart" [
            describe "addItem" [
                it "adds item to empty cart" (fun () ->
                    let cart = Cart.empty
                    let updated = cart.AddItem({ ProductId = 1; Quantity = 2; Price = 10.00m })

                    expectSeq(updated.Items).toHaveLength(1)
                    expectSeq(updated.Items).toContain({ ProductId = 1; Quantity = 2; Price = 10.00m })
                )

                it "increases quantity for existing item" (fun () ->
                    let cart = { Items = [{ ProductId = 1; Quantity = 1; Price = 10.00m }] }
                    let updated = cart.AddItem({ ProductId = 1; Quantity = 2; Price = 10.00m })

                    expectSeq(updated.Items).toHaveLength(1)
                    expect updated.Items.[0].Quantity |> should (equal 3)
                )
            ]

            describe "removeItem" [
                it "removes item from cart" (fun () ->
                    let cart = {
                        Items = [
                            { ProductId = 1; Quantity = 1; Price = 10.00m }
                            { ProductId = 2; Quantity = 1; Price = 20.00m }
                        ]
                    }
                    let updated = cart.RemoveItem(1)

                    expectSeq(updated.Items).toHaveLength(1)
                    expect updated.Items |> should (anySatisfy (fun item -> item.ProductId = 2) "contain product 2")
                    expect updated.Items |> shouldNot (anySatisfy (fun item -> item.ProductId = 1) "contain product 1")
                )
            ]

            describe "total" [
                it "calculates total price" (fun () ->
                    let cart = {
                        Items = [
                            { ProductId = 1; Quantity = 2; Price = 10.00m }  // $20
                            { ProductId = 2; Quantity = 1; Price = 30.00m }  // $30
                        ]
                    }
                    let total = cart.Total()

                    expect total |> should (equal 50.00m)
                )

                it "returns zero for empty cart" (fun () ->
                    let cart = Cart.empty
                    expect cart.Total() |> should (equal 0.00m)
                    expectSeq(cart.Items).toBeEmpty()
                )
            ]
        ]
    }
```

---

## See Also

- **[Core Matchers](core.md)** - Basic equality and type matchers
- **[String Matchers](strings.md)** - String-specific matchers
- **[Numeric Matchers](numeric.md)** - Numeric comparisons
- **[Quick Start](../../quick-start.md)** - Get started with FxSpec
