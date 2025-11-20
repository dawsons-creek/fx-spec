module FX.Spec.JsonApi.Tests.JsonApiMatchersSpecs

open FX.Spec.Core
open FX.Spec.Matchers
open FX.Spec.JsonApi

[<CLIMutable>]
type private Article = { id: string; title: string; createdAt: string }

[<CLIMutable>]
type private Person = { id: string; name: string }

[<CLIMutable>]
type private ArticleResource =
    { ``type``: string
      id: string
      attributes: Article }

[<Tests>]
let specs =
    describe
        "JSON:API Matchers"
        [ context
              "expectJsonApi().toHaveIncludedResource(type, id)"
              [ it "passes when included array contains resource with matching type and id" (fun () ->
                    let json =
                        """{
  "data": {"type": "articles", "id": "1"},
  "included": [
    {"type": "people", "id": "9", "attributes": {"name": "Alice"}},
    {"type": "people", "id": "10", "attributes": {"name": "Bob"}}
  ]
}"""
                    expectJsonApi(json).toHaveIncludedResource("people", "9")
                    expectJsonApi(json).toHaveIncludedResource("people", "10"))

                it "throws when included array does not contain matching resource" (fun () ->
                    let json =
                        """{
  "data": {"type": "articles", "id": "1"},
  "included": [
    {"type": "people", "id": "9"}
  ]
}"""
                    expectThrows<JsonApiAssertionException> (fun () ->
                        expectJsonApi(json).toHaveIncludedResource("people", "99")))

                it "throws when included is not an array" (fun () ->
                    let json = """{"data": {}, "included": "not-an-array"}"""
                    expectThrows<JsonApiAssertionException> (fun () ->
                        expectJsonApi(json).toHaveIncludedResource("people", "9")))

                it "throws when included is missing" (fun () ->
                    let json = """{"data": {"type": "articles", "id": "1"}}"""
                    expectThrows<JsonApiAssertionException> (fun () ->
                        expectJsonApi(json).toHaveIncludedResource("people", "9"))) ]

          context
              "expectJsonApi().toBeResourceIdentifier(path, type, id)"
              [ it "passes when object at path has matching type and id" (fun () ->
                    let json =
                        """{
  "data": {
    "type": "articles",
    "id": "1",
    "relationships": {
      "author": {
        "data": {"type": "people", "id": "9"}
      }
    }
  }
}"""
                    expectJsonApi(json).toBeResourceIdentifier("data.relationships.author.data", "people", "9"))

                it "throws when type does not match" (fun () ->
                    let json =
                        """{"data": {"relationships": {"author": {"data": {"type": "users", "id": "9"}}}}}"""
                    expectThrows<JsonApiAssertionException> (fun () ->
                        expectJsonApi(json).toBeResourceIdentifier("data.relationships.author.data", "people", "9")))

                it "throws when id does not match" (fun () ->
                    let json =
                        """{"data": {"relationships": {"author": {"data": {"type": "people", "id": "10"}}}}}"""
                    expectThrows<JsonApiAssertionException> (fun () ->
                        expectJsonApi(json).toBeResourceIdentifier("data.relationships.author.data", "people", "9")))

                it "throws when object is missing type or id" (fun () ->
                    let json = """{"data": {"relationships": {"author": {"data": {"type": "people"}}}}}"""
                    expectThrows<JsonApiAssertionException> (fun () ->
                        expectJsonApi(json).toBeResourceIdentifier("data.relationships.author.data", "people", "9")))

                it "throws when path does not exist" (fun () ->
                    let json = """{"data": {}}"""
                    expectThrows<JsonApiAssertionException> (fun () ->
                        expectJsonApi(json).toBeResourceIdentifier("data.relationships.author.data", "people", "9")))

                it "throws when property at path is not an object" (fun () ->
                    let json = """{"data": {"relationships": {"author": {"data": "not-an-object"}}}}"""
                    expectThrows<JsonApiAssertionException> (fun () ->
                        expectJsonApi(json).toBeResourceIdentifier("data.relationships.author.data", "people", "9"))) ]

          context
              "expectJsonApi().toHaveRelationship(name, type, id)"
              [ it "passes when relationship points to resource with matching type and id" (fun () ->
                    let json =
                        """{
  "data": {
    "type": "articles",
    "id": "1",
    "relationships": {
      "author": {
        "data": {"type": "people", "id": "9"}
      }
    }
  }
}"""
                    expectJsonApi(json).toHaveRelationship("author", "people", "9"))

                it "passes with multiple relationships" (fun () ->
                    let json =
                        """{
  "data": {
    "type": "articles",
    "id": "1",
    "relationships": {
      "author": {"data": {"type": "people", "id": "9"}},
      "comments": {"data": [{"type": "comments", "id": "5"}]}
    }
  }
}"""
                    expectJsonApi(json).toHaveRelationship("author", "people", "9"))

                it "throws when type does not match" (fun () ->
                    let json =
                        """{
  "data": {
    "relationships": {
      "author": {"data": {"type": "users", "id": "9"}}
    }
  }
}"""
                    expectThrows<JsonApiAssertionException> (fun () ->
                        expectJsonApi(json).toHaveRelationship("author", "people", "9")))

                it "throws when id does not match" (fun () ->
                    let json =
                        """{
  "data": {
    "relationships": {
      "author": {"data": {"type": "people", "id": "10"}}
    }
  }
}"""
                    expectThrows<JsonApiAssertionException> (fun () ->
                        expectJsonApi(json).toHaveRelationship("author", "people", "9")))

                it "throws when relationship is null" (fun () ->
                    let json =
                        """{
  "data": {
    "relationships": {
      "author": {"data": null}
    }
  }
}"""
                    expectThrows<JsonApiAssertionException> (fun () ->
                        expectJsonApi(json).toHaveRelationship("author", "people", "9")))

                it "throws when relationship does not exist" (fun () ->
                    let json = """{"data": {"relationships": {}}}"""
                    expectThrows<JsonApiAssertionException> (fun () ->
                        expectJsonApi(json).toHaveRelationship("author", "people", "9"))) ]

          context
              "expectJsonApiArray().toBeOrderedBy(selector, direction)"
              [ it "passes when array is ordered ascending by selector" (fun () ->
                    let json =
                        """[
  {"type": "articles", "id": "1", "attributes": {"id": "1", "title": "A", "createdAt": "2024-01-01"}},
  {"type": "articles", "id": "2", "attributes": {"id": "2", "title": "B", "createdAt": "2024-01-02"}},
  {"type": "articles", "id": "3", "attributes": {"id": "3", "title": "C", "createdAt": "2024-01-03"}}
]"""
                    (expectJsonApiArray json "").toBeOrderedBy((fun (r: ArticleResource) -> r.attributes.createdAt), Ascending))

                it "passes when array is ordered descending by selector" (fun () ->
                    let json =
                        """[
  {"type": "articles", "id": "3", "attributes": {"id": "3", "title": "C", "createdAt": "2024-01-03"}},
  {"type": "articles", "id": "2", "attributes": {"id": "2", "title": "B", "createdAt": "2024-01-02"}},
  {"type": "articles", "id": "1", "attributes": {"id": "1", "title": "A", "createdAt": "2024-01-01"}}
]"""
                    (expectJsonApiArray json "").toBeOrderedBy((fun (r: ArticleResource) -> r.attributes.createdAt), Descending))

                it "passes when array has single element" (fun () ->
                    let json =
                        """[{"type": "articles", "id": "1", "attributes": {"id": "1", "title": "A", "createdAt": "2024-01-01"}}]"""
                    (expectJsonApiArray json "").toBeOrderedBy((fun (r: ArticleResource) -> r.attributes.createdAt), Ascending))

                it "passes when array is empty" (fun () ->
                    let json = """[]"""
                    (expectJsonApiArray json "").toBeOrderedBy((fun (r: ArticleResource) -> r.attributes.createdAt), Ascending))

                it "throws when array is not ordered correctly ascending" (fun () ->
                    let json =
                        """[
  {"type": "articles", "id": "1", "attributes": {"id": "1", "title": "A", "createdAt": "2024-01-03"}},
  {"type": "articles", "id": "2", "attributes": {"id": "2", "title": "B", "createdAt": "2024-01-01"}}
]"""
                    expectThrows<JsonApiAssertionException> (fun () ->
                        (expectJsonApiArray json "").toBeOrderedBy((fun (r: ArticleResource) -> r.attributes.createdAt), Ascending)))

                it "throws when array is not ordered correctly descending" (fun () ->
                    let json =
                        """[
  {"type": "articles", "id": "1", "attributes": {"id": "1", "title": "A", "createdAt": "2024-01-01"}},
  {"type": "articles", "id": "2", "attributes": {"id": "2", "title": "B", "createdAt": "2024-01-03"}}
]"""
                    expectThrows<JsonApiAssertionException> (fun () ->
                        (expectJsonApiArray json "").toBeOrderedBy((fun (r: ArticleResource) -> r.attributes.createdAt), Descending)))

                it "works with nested arrays" (fun () ->
                    let json =
                        """{
  "data": [
    {"type": "articles", "id": "1", "attributes": {"id": "1", "title": "A", "createdAt": "2024-01-01"}},
    {"type": "articles", "id": "2", "attributes": {"id": "2", "title": "B", "createdAt": "2024-01-02"}}
  ]
}"""
                    (expectJsonApiArray json "data").toBeOrderedBy((fun (r: ArticleResource) -> r.attributes.title), Ascending))

                it "throws when property is not an array" (fun () ->
                    let json = """{"data": "not-an-array"}"""
                    expectThrows<JsonApiAssertionException> (fun () ->
                        (expectJsonApiArray json "data").toBeOrderedBy((fun (r: ArticleResource) -> r.attributes.title), Ascending))) ] ]
