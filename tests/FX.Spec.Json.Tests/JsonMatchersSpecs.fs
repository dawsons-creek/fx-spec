module FX.Spec.Json.Tests.JsonMatchersSpecs

open FX.Spec.Core
open FX.Spec.Matchers
open FX.Spec.Json

[<CLIMutable>]
type private Widget = { id: string; name: string; price: decimal }

[<CLIMutable>]
type private Customer = { id: int; email: string; active: bool }

[<Tests>]
let specs =
    describe
        "JSON Matchers"
        [ context
              "expectJson().toHaveProperty(path)"
              [ it "passes when property exists at simple path" (fun () ->
                    let json = """{"name": "Widget"}"""
                    expectJson(json).toHaveProperty("name"))

                it "passes when property exists at nested path" (fun () ->
                    let json = """{"data": {"user": {"name": "Alice"}}}"""
                    expectJson(json).toHaveProperty("data.user.name"))

                it "passes when property exists at array index path" (fun () ->
                    let json = """{"items": [{"id": "w-1"}, {"id": "w-2"}]}"""
                    expectJson(json).toHaveProperty("items[0].id"))

                it "throws when property does not exist" (fun () ->
                    let json = """{"name": "Widget"}"""
                    expectThrows<JsonAssertionException> (fun () -> expectJson(json).toHaveProperty("missing")))

                it "throws when nested property does not exist" (fun () ->
                    let json = """{"data": {}}"""
                    expectThrows<JsonAssertionException> (fun () ->
                        expectJson(json).toHaveProperty("data.missing")))

                it "throws when array index is out of bounds" (fun () ->
                    let json = """{"items": [{"id": "w-1"}]}"""
                    expectThrows<JsonAssertionException> (fun () -> expectJson(json).toHaveProperty("items[5].id"))) ]

          context
              "expectJson().toHaveProperty(path, value)"
              [ it "passes when property has expected value" (fun () ->
                    let json = """{"name": "Widget", "price": 42}"""
                    expectJson(json).toHaveProperty("name", "Widget")
                    expectJson(json).toHaveProperty("price", 42))

                it "passes when nested property has expected value" (fun () ->
                    let json = """{"data": {"count": 5}}"""
                    expectJson(json).toHaveProperty("data.count", 5))

                it "passes when array item property has expected value" (fun () ->
                    let json = """{"items": [{"id": "w-1", "name": "Widget"}]}"""
                    expectJson(json).toHaveProperty("items[0].name", "Widget"))

                it "throws when property has different value" (fun () ->
                    let json = """{"name": "Widget"}"""
                    expectThrows<JsonAssertionException> (fun () ->
                        expectJson(json).toHaveProperty("name", "Gadget")))

                it "throws when property does not exist" (fun () ->
                    let json = """{"name": "Widget"}"""
                    expectThrows<JsonAssertionException> (fun () -> expectJson(json).toHaveProperty("missing", "value")))

                it "works with complex types" (fun () ->
                    let json = """{"id": "w-1", "name": "Widget", "price": 99.99}"""
                    expectJson(json).toHaveProperty("price", 99.99m)) ]

          context
              "expectJson().toMatchPartial(expected)"
              [ it "passes when JSON matches all fields in anonymous record" (fun () ->
                    let json = """{"id": "w-1", "name": "Widget", "price": 42.5, "inStock": true}"""
                    expectJson(json).toMatchPartial({| name = "Widget"; price = 42.5M |}))

                it "passes when JSON has exact match" (fun () ->
                    let json = """{"id": "w-1", "name": "Widget", "price": 10.5}"""
                    expectJson(json).toMatchPartial({| id = "w-1"; name = "Widget"; price = 10.5M |}))

                it "passes when nested objects match" (fun () ->
                    let json = """{"data": {"user": {"name": "Alice", "age": 30}}, "meta": {"count": 1}}"""
                    expectJson(json).toMatchPartial({| data = {| user = {| name = "Alice" |} |} |}))

                it "passes when arrays match exactly" (fun () ->
                    let json = """{"items": [1, 2, 3]}"""
                    expectJson(json).toMatchPartial({| items = [| 1; 2; 3 |] |}))

                it "throws when property value differs" (fun () ->
                    let json = """{"name": "Widget", "price": 42}"""
                    expectThrows<JsonAssertionException> (fun () ->
                        expectJson(json).toMatchPartial({| name = "Gadget" |})))

                it "throws when expected property is missing" (fun () ->
                    let json = """{"name": "Widget"}"""
                    expectThrows<JsonAssertionException> (fun () -> expectJson(json).toMatchPartial({| price = 42 |})))

                it "throws when array lengths differ" (fun () ->
                    let json = """{"items": [1, 2]}"""
                    expectThrows<JsonAssertionException> (fun () ->
                        expectJson(json).toMatchPartial({| items = [| 1; 2; 3 |] |}))) ]

          context
              "expectJsonArray(json, path).toHaveLength(n)"
              [ it "passes when root array has expected length" (fun () ->
                    let json = """[1, 2, 3]"""
                    (expectJsonArray json "").toHaveLength(3))

                it "passes when nested array has expected length" (fun () ->
                    let json = """{"items": [{"id": 1}, {"id": 2}]}"""
                    (expectJsonArray json "items").toHaveLength(2))

                it "passes when empty array has length zero" (fun () ->
                    let json = """{"items": []}"""
                    (expectJsonArray json "items").toHaveLength(0))

                it "throws when length does not match" (fun () ->
                    let json = """[1, 2, 3]"""
                    expectThrows<JsonAssertionException> (fun () -> (expectJsonArray json "").toHaveLength(5)))

                it "throws when property is not an array" (fun () ->
                    let json = """{"items": "not-an-array"}"""
                    expectThrows<JsonAssertionException> (fun () ->
                        (expectJsonArray json "items").toHaveLength(0)))

                it "throws when property does not exist" (fun () ->
                    let json = """{"items": []}"""
                    expectThrows<JsonAssertionException> (fun () ->
                        (expectJsonArray json "missing").toHaveLength(0))) ]

          context
              "expectJsonArray(json, path).toAllHaveProperty(name)"
              [ it "passes when all items have the property" (fun () ->
                    let json = """[{"id": 1, "name": "A"}, {"id": 2, "name": "B"}]"""
                    (expectJsonArray json "").toAllHaveProperty("id")
                    (expectJsonArray json "").toAllHaveProperty("name"))

                it "passes when nested array items all have property" (fun () ->
                    let json = """{"widgets": [{"id": "w-1"}, {"id": "w-2"}]}"""
                    (expectJsonArray json "widgets").toAllHaveProperty("id"))

                it "passes when array is empty" (fun () ->
                    let json = """[]"""
                    (expectJsonArray json "").toAllHaveProperty("anyProperty"))

                it "throws when one item is missing the property" (fun () ->
                    let json = """[{"id": 1}, {"name": "B"}]"""
                    expectThrows<JsonAssertionException> (fun () -> (expectJsonArray json "").toAllHaveProperty("id")))

                it "throws when item is not an object" (fun () ->
                    let json = """[1, 2, 3]"""
                    expectThrows<JsonAssertionException> (fun () -> (expectJsonArray json "").toAllHaveProperty("id"))) ]

          context
              "expectJsonArray(json, path).toAllSatisfy(predicate)"
              [ it "passes when all items satisfy predicate" (fun () ->
                    let json = """[{"id": "w-1", "name": "A", "price": 10}, {"id": "w-2", "name": "B", "price": 20}]"""
                    (expectJsonArray json "").toAllSatisfy(fun (w: Widget) -> w.price > 5m))

                it "passes when empty array satisfies predicate" (fun () ->
                    let json = """[]"""
                    (expectJsonArray json "").toAllSatisfy(fun (w: Widget) -> false))

                it "throws when one item does not satisfy predicate" (fun () ->
                    let json = """[{"id": "w-1", "name": "A", "price": 10}, {"id": "w-2", "name": "B", "price": 3}]"""
                    expectThrows<JsonAssertionException> (fun () ->
                        (expectJsonArray json "").toAllSatisfy(fun (w: Widget) -> w.price > 5m)))

                it "works with nested arrays" (fun () ->
                    let json = """{"customers": [{"id": 1, "email": "a@example.com", "active": true}]}"""
                    (expectJsonArray json "customers").toAllSatisfy(fun (c: Customer) -> c.active)) ]

          context
              "expectJsonArray(json, path).toContain(item)"
              [ it "passes when array contains expected item" (fun () ->
                    let json = """[{"id": "w-1", "name": "Widget", "price": 10}, {"id": "w-2", "name": "Gadget", "price": 20}]"""
                    (expectJsonArray json "").toContain({ id = "w-1"; name = "Widget"; price = 10m }))

                it "passes when array contains primitive" (fun () ->
                    let json = """[1, 2, 3, 4, 5]"""
                    (expectJsonArray json "").toContain(3))

                it "throws when array does not contain item" (fun () ->
                    let json = """[{"id": "w-1", "name": "Widget", "price": 10}]"""
                    expectThrows<JsonAssertionException> (fun () ->
                        (expectJsonArray json "").toContain({ id = "w-99"; name = "Missing"; price = 0m })))

                it "throws when empty array does not contain item" (fun () ->
                    let json = """[]"""
                    expectThrows<JsonAssertionException> (fun () -> (expectJsonArray json "").toContain(42))) ]

          context
              "JSON path navigation"
              [ it "handles deeply nested paths" (fun () ->
                    let json = """{"a": {"b": {"c": {"d": {"value": 42}}}}}"""
                    expectJson(json).toHaveProperty("a.b.c.d.value", 42))

                it "handles mixed object and array paths" (fun () ->
                    let json = """{"users": [{"friends": [{"name": "Alice"}]}]}"""
                    expectJson(json).toHaveProperty("users[0].friends[0].name", "Alice"))

                it "handles multiple array indices" (fun () ->
                    let json = """[[1, 2], [3, 4], [5, 6]]"""
                    expectJson(json).toHaveProperty("[1][1]", 4)) ] ]
