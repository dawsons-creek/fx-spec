# JSON:API Testing

FX.Spec provides dedicated support for testing APIs that adhere to the [JSON:API specification](https://jsonapi.org/).

## Overview

The `FX.Spec.JsonApi` package extends the core JSON testing capabilities with semantic matchers for resources, relationships, includes, and sorting. It abstracts away the complex structure of JSON:API documents (data, included, relationships) so you can test the *content* of your API.

```fsharp
open FX.Spec.JsonApi

it "returns the article with author" (fun () ->
    expectJsonApi(json)
        .toHaveRelationship("author", "people", "9")
        .toHaveIncludedResource("people", "9")
)
```

## Installation

Add the JSON:API package to your test project:

```bash
dotnet add package FX.Spec.JsonApi
```

## Core Matchers

### `expectJsonApi(json: string)`

Creates an expectation wrapper specifically for JSON:API documents.

### Resource Identifiers

#### `toBeResourceIdentifier(path: string, type: string, id: string)`

Asserts that the object at the specified path is a valid Resource Identifier Object containing the correct `type` and `id`.

```fsharp
// Check the primary data
expectJsonApi(json).toBeResourceIdentifier("data", "articles", "1")

// Check nested data
expectJsonApi(json).toBeResourceIdentifier("data.relationships.author.data", "people", "9")
```

### Relationships

#### `toHaveRelationship(name: string, type: string, id: string)`

Asserts that the document's primary data has a relationship with the specified name, and that relationship links to a resource with the given type and ID.

This shorthand validates the structure `data.relationships.{name}.data`.

```fsharp
// Verifies data.relationships.author.data has type="people" and id="9"
expectJsonApi(json).toHaveRelationship("author", "people", "9")
```

### Includes

#### `toHaveIncludedResource(type: string, id: string)`

Asserts that the `included` array contains a resource matching the specified type and ID. This is essential for testing compound documents.

```fsharp
expectJsonApi(json).toHaveIncludedResource("comments", "5")
expectJsonApi(json).toHaveIncludedResource("people", "9")
```

## Collection Matchers

### `expectJsonApiArray(json: string, path: string)`

Creates an expectation for asserting on arrays within a JSON:API document (typically the `data` array for collection responses).

### Sorting

#### `toBeOrderedBy(selector, direction)`

Asserts that the collection is sorted by a specific attribute.

```fsharp
open FX.Spec.JsonApi

// Ascending order
expectJsonApiArray(json)("data")
    .toBeOrderedBy((fun item -> item.attributes.created), Ascending)

// Descending order
expectJsonApiArray(json)("data")
    .toBeOrderedBy((fun item -> item.attributes.title), Descending)
```

## Helper Functions

The `JsonApiHelpers` module provides strongly-typed parsing and serialization helpers for JSON:API envelopes.

### Parsing

```fsharp
open FX.Spec.JsonApi

// Parse single resource response -> returns 'T
let article = parseJsonApiData<Article>(json)

// Parse collection response -> returns 'T array
let articles = parseJsonApiCollection<Article>(json)
```

### Serialization

Useful for setting up test data or mocking responses.

```fsharp
// Wrap item in { "data": ... }
let json = toJsonApiData(myArticle)

// Wrap items in { "data": [...] }
let json = toJsonApiCollection(myArticles)
```
