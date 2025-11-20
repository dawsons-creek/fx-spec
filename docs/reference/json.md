# JSON Testing

FX.Spec provides a fluent API for testing JSON content with path-based navigation, structural matching, and type-safe assertions.

## Overview

The `FX.Spec.Json` package allows you to assert on JSON structures using dot notation without creating temporary types for every test.

```fsharp
open FX.Spec.Json

let json = """
{
  "user": {
    "name": "John Doe",
    "age": 30,
    "roles": ["admin", "editor"]
  }
}
"""

it "validates user json" (fun () ->
    expectJson(json).toHaveProperty("user.name", "John Doe")
    expectJson(json).toHaveProperty("user.roles[0]", "admin")
)
```

## Installation

Add the JSON package to your test project:

```bash
dotnet add package FX.Spec.Json
```

## Core Matchers

### `expectJson(json: string)`

Creates an expectation wrapper for a JSON string string.

### Property Assertions

#### `toHaveProperty(path: string)`

Asserts that a property exists at the specified path, regardless of its value.

```fsharp
expectJson(json).toHaveProperty("meta.pagination")
expectJson(json).toHaveProperty("data[0].id")
```

#### `toHaveProperty<'T>(path: string, expected: 'T)`

Asserts that the property at the path exists and matches the expected value. The value is deserialized to type `'T` before comparison.

```fsharp
// Primitives
expectJson(json).toHaveProperty("count", 42)
expectJson(json).toHaveProperty("isActive", true)
expectJson(json).toHaveProperty("status", "active")

// Complex objects
expectJson(json).toHaveProperty("config", {| timeout = 1000 |})
```

### Path Navigation

Paths support standard dot notation and array indexing:

*   `field` - Top-level property
*   `parent.child` - Nested property
*   `items[0]` - Array index
*   `data.users[1].profile.email` - Deeply nested path

### Structural Matching

#### `toMatchPartial(expected: 'T)`

Asserts that the JSON object matches the shape and values of the expected object. This is a "partial match" meaning:
1.  All properties in `expected` must exist in the JSON.
2.  Values must match.
3.  Extra properties in the JSON are ignored.

This is perfect for testing large API responses where you only care about specific fields.

```fsharp
let json = """
{
  "id": 123,
  "name": "John",
  "email": "john@example.com",
  "metadata": { "created": "2023-01-01", "ip": "127.0.0.1" }
}
"""

// Verify only the fields we care about
expectJson(json).toMatchPartial({| 
    id = 123
    name = "John" 
|})
```

## Array Matchers

### `expectJsonArray(json: string, path: string)`

Creates a specialized expectation for asserting on JSON arrays.

*   `json`: The full JSON string
*   `path`: Path to the array (use `""` for root array)

```fsharp
let json = """{"users": [{"id": 1}, {"id": 2}]}"""
let users = expectJsonArray(json)("users")
```

### Array Assertions

#### `toHaveLength(count: int)`

Asserts the exact length of the array.

```fsharp
expectJsonArray(json)("items").toHaveLength(5)
```

#### `toAllHaveProperty(name: string)`

Asserts that **every** object in the array contains the specified property.

```fsharp
// Ensure all users have an ID
expectJsonArray(json)("users").toAllHaveProperty("id")
```

#### `toContain(item: 'T)`

Asserts that the array contains at least one item matching the expected value.

```fsharp
expectJsonArray(json)("tags").toContain("urgent")
```

#### `toAllSatisfy(predicate: 'T -> bool)`

Asserts that **all** items in the array satisfy the provided predicate function.

```fsharp
// Ensure all scores are positive
expectJsonArray(json)("scores").toAllSatisfy(fun (score: int) -> score > 0)
```

## Helper Functions

The `JsonHelpers` module provides utilities for working with JSON, automatically configured with F# support (e.g., for Options and Unions).

```fsharp
open FX.Spec.Json

// Parse JSON to a type
let user = parseJson<User>(jsonString)

// Serialize value to JSON
let json = toJson(user)
```
