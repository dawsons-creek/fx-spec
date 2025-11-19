# String Matchers

String-specific matchers for prefixes, suffixes, patterns, and more.

---

## Prefixes & Suffixes

### startWith

**Type:** `string -> Matcher<string>`

Matches if the string starts with the expected prefix.

**Usage:**

```fsharp
expectStr(string).toStartWith(prefix)
```

**Examples:**

```fsharp
expectStr("hello world").toStartWith("hello")
expectStr("FxSpec").toStartWith("Fx")
expectStr("https://example.com").toStartWith("https://")

// Case-sensitive
expectStr("Hello").notToStartWith("hello")
```

**Failure Message:**

```fsharp
expectStr("hello world").toStartWith("goodbye")
// => Expected string to start with 'goodbye', but found 'hello world'
```

**Notes:**

- Case-sensitive by default
- Returns error for null strings
- For case-insensitive matching, use custom logic or convert to lowercase first

---

### endWith

**Type:** `string -> Matcher<string>`

Matches if the string ends with the expected suffix.

**Usage:**

```fsharp
expectStr(string).toEndWith(suffix)
```

**Examples:**

```fsharp
expectStr("hello world").toEndWith("world")
expectStr("test.txt").toEndWith(".txt")
expectStr("FxSpec").toEndWith("Spec")

// File extensions
let filename = "document.pdf"
expectStr(filename).toEndWith(".pdf")
```

**Failure Message:**

```fsharp
expectStr("hello world").toEndWith(".txt")
// => Expected string to end with '.txt', but found 'hello world'
```

---

## Substrings

### containSubstring

**Type:** `string -> Matcher<string>`

Matches if the string contains the expected substring.

**Usage:**

```fsharp
expectStr(string).toContainSubstring(substring)
```

**Examples:**

```fsharp
expectStr("hello world").toContainSubstring("lo wo")
expectStr("FX.Spec is great").toContainSubstring("Spec")
expectStr("error: file not found").toContainSubstring("error:")

// Search in logs
let logMessage = "2025-01-01 10:00:00 INFO User logged in"
expectStr(logMessage).toContainSubstring("INFO")
expectStr(logMessage).toContainSubstring("logged in")
```

**Failure Message:**

```fsharp
expectStr("hello world").toContainSubstring("goodbye")
// => Expected string to contain 'goodbye', but found 'hello world'
```

---

## Patterns

### matchRegex

**Type:** `string -> Matcher<string>`

Matches if the string matches the regular expression pattern.

**Usage:**

```fsharp
expectStr(string).toMatchRegex(pattern)
```

**Examples:**

```fsharp
// Email validation
expectStr("test@example.com").toMatchRegex(@"^\w+@\w+\.\w+$")

// Phone numbers
expectStr("555-1234").toMatchRegex(@"^\d{3}-\d{4}$")

// Dates
expectStr("2025-01-01").toMatchRegex(@"^\d{4}-\d{2}-\d{2}$")

// Contains digits
expectStr("hello123").toMatchRegex(@"\d+")

// Starts with uppercase
expectStr("Hello").toMatchRegex(@"^[A-Z]")
```

**Failure Message:**

```fsharp
expectStr("hello").toMatchRegex(@"^\d+$")
// => Expected string to match pattern '^\d+$', but found 'hello'
```

**Notes:**

- Uses .NET `Regex.IsMatch`
- Patterns are case-sensitive by default
- Use `(?i)` flag for case-insensitive matching: `matchRegex @"(?i)hello"`

---

## Empty Checks

### beEmptyString

**Type:** `Matcher<string>`

Matches if the string is empty (`""`).

**Usage:**

```fsharp
expectStr(string).toBeEmpty()
```

**Examples:**

```fsharp
expectStr("").toBeEmpty()
expectStr(String.Empty).toBeEmpty()

// After operations
let trimmed = "   ".Trim()
expectStr(trimmed).toBeEmpty()
```

**Failure Message:**

```fsharp
expectStr("hello").toBeEmpty()
// => Expected empty string, but found 'hello'

expectStr(null).toBeEmpty()
// => Expected empty string, but found null
```

---

### beNullOrEmpty

**Type:** `Matcher<string>`

Matches if the string is null or empty.

**Usage:**

```fsharp
expect string |> should beNullOrEmpty
```

**Examples:**

```fsharp
expect null |> should beNullOrEmpty
expect "" |> should beNullOrEmpty
expect String.Empty |> should beNullOrEmpty

// Validation
let userInput = getUserInput()
if String.IsNullOrEmpty(userInput) then
    expect userInput |> should beNullOrEmpty
```

**Failure Message:**

```fsharp
expect "hello" |> should beNullOrEmpty
// => Expected null or empty string, but found 'hello'
```

---

### beNullOrWhitespace

**Type:** `Matcher<string>`

Matches if the string is null, empty, or contains only whitespace.

**Usage:**

```fsharp
expect string |> should beNullOrWhitespace
```

**Examples:**

```fsharp
expect null |> should beNullOrWhitespace
expect "" |> should beNullOrWhitespace
expect "   " |> should beNullOrWhitespace
expect "\t\n" |> should beNullOrWhitespace

// User input validation
let comment = getCommentText()
if String.IsNullOrWhiteSpace(comment) then
    expect comment |> should beNullOrWhitespace
```

**Failure Message:**

```fsharp
expect "hello" |> should beNullOrWhitespace
// => Expected null, empty, or whitespace string, but found 'hello'
```

---

## Length

### haveStringLength

**Type:** `int -> Matcher<string>`

Matches if the string has the expected length.

**Usage:**

```fsharp
expectStr(string).toHaveLength(length)
```

**Examples:**

```fsharp
expectStr("hello").toHaveLength(5)
expectStr("").toHaveLength(0)
expectStr("FxSpec").toHaveLength(6)

// Password validation
let password = "secret123"
expectStr(password).toHaveLength(9)
```

**Failure Message:**

```fsharp
expectStr("hello").toHaveLength(10)
// => Expected string of length 10, but found length 5 ('hello')

expectStr(null).toHaveLength(5)
// => Expected string of length 5, but found null
```

---

## Case Insensitive

### equalIgnoreCase

**Type:** `string -> Matcher<string>`

Matches if the string equals the expected value (case-insensitive).

**Usage:**

```fsharp
expect string |> should (equalIgnoreCase expected)
```

**Examples:**

```fsharp
expect "HELLO" |> should (equalIgnoreCase "hello")
expect "FxSpec" |> should (equalIgnoreCase "fxspec")
expect "TeSt" |> should (equalIgnoreCase "test")

// Configuration keys
let configKey = "DatabaseConnectionString"
expect configKey |> should (equalIgnoreCase "databaseconnectionstring")
```

**Failure Message:**

```fsharp
expect "HELLO" |> should (equalIgnoreCase "goodbye")
// => Expected 'goodbye' (case-insensitive), but found 'HELLO'
```

**Notes:**

- Uses `StringComparison.OrdinalIgnoreCase`
- Handles null correctly
- For exact case match, use `equal` from core matchers

---

## Character Types

### beAlphabetic

**Type:** `Matcher<string>`

Matches if the string contains only letters.

**Usage:**

```fsharp
expect string |> should beAlphabetic
```

**Examples:**

```fsharp
expect "hello" |> should beAlphabetic
expect "FxSpec" |> should beAlphabetic
expect "ABC" |> should beAlphabetic

// Fails
expect "hello123" |> shouldNot beAlphabetic  // Contains digits
expect "hello world" |> shouldNot beAlphabetic  // Contains space
expect "" |> shouldNot beAlphabetic  // Empty string
```

**Failure Message:**

```fsharp
expect "hello123" |> should beAlphabetic
// => Expected alphabetic string, but found 'hello123'

expect "" |> should beAlphabetic
// => Expected alphabetic string, but found empty string
```

---

### beNumeric

**Type:** `Matcher<string>`

Matches if the string contains only digits.

**Usage:**

```fsharp
expect string |> should beNumeric
```

**Examples:**

```fsharp
expect "12345" |> should beNumeric
expect "0" |> should beNumeric
expect "999" |> should beNumeric

// Fails
expect "123.45" |> shouldNot beNumeric  // Contains decimal point
expect "12 34" |> shouldNot beNumeric  // Contains space
expect "" |> shouldNot beNumeric  // Empty string
```

**Failure Message:**

```fsharp
expect "12.34" |> should beNumeric
// => Expected numeric string, but found '12.34'
```

**Notes:**

- Only accepts digits 0-9
- Does not accept decimal points, minus signs, or spaces
- For parsing numbers, use `Int32.TryParse` or similar

---

## Complete Examples

### Testing URL Validation

```fsharp
module UrlValidationSpecs

open FX.Spec.Core
open FX.Spec.Matchers

[<Tests>]
let urlValidationSpecs =
    spec {
        describe "URL Validator" [
            describe "validateUrl" [
                context "when URL is valid" [
                    it "accepts https URLs" (fun () ->
                        let url = "https://example.com"
                        expectStr(url).toStartWith("https://")
                        expectStr(url).toMatchRegex(@"^https?://[\w\-]+(\.[\w\-]+)+[/#?]?.*$")
                    )

                    it "accepts http URLs" (fun () ->
                        let url = "http://example.com/path?query=value"
                        expectStr(url).toStartWith("http://")
                        expectStr(url).toContainSubstring("example.com")
                    )

                    it "accepts URLs with paths" (fun () ->
                        let url = "https://example.com/api/users"
                        expectStr(url).toContainSubstring("/api/users")
                        expectStr(url).toEndWith("/users")
                    )
                ]

                context "when URL is invalid" [
                    it "rejects empty strings" (fun () ->
                        expectStr("").toBeEmpty()
                        expectStr("").notToStartWith("http")
                    )

                    it "rejects non-URL strings" (fun () ->
                        expectStr("not a url").notToMatchRegex(@"^https?://")
                    )
                ]
            ]
        ]
    }
```

### Testing String Transformations

```fsharp
module StringTransformationSpecs

open FX.Spec.Core
open FX.Spec.Matchers

[<Tests>]
let stringTransformationSpecs =
    spec {
        describe "StringHelpers" [
            describe "slugify" [
                it "converts to lowercase" (fun () ->
                    let result = StringHelpers.slugify "Hello World"
                    expect result |> should (equalIgnoreCase "hello-world")
                    expect result |> should (equal "hello-world")
                )

                it "replaces spaces with hyphens" (fun () ->
                    let result = StringHelpers.slugify "My Blog Post"
                    expect result |> should (equal "my-blog-post")
                    expectStr(result).notToContainSubstring(" ")
                )

                it "removes special characters" (fun () ->
                    let result = StringHelpers.slugify "Hello, World!"
                    expect result |> should (equal "hello-world")
                    expect result |> should beAlphabetic  // False, contains hyphen
                    expectStr(result).toMatchRegex(@"^[a-z\-]+$")
                )
            ]

            describe "sanitize" [
                it "trims whitespace" (fun () ->
                    let result = StringHelpers.sanitize "  hello  "
                    expect result |> should (equal "hello")
                    expectStr(result).notToStartWith(" ")
                    expectStr(result).notToEndWith(" ")
                )

                it "returns empty string for whitespace-only input" (fun () ->
                    let result = StringHelpers.sanitize "   "
                    expectStr(result).toBeEmpty()
                )
            ]
        ]
    }
```

---

## See Also

- **[Core Matchers](core.md)** - Basic equality matchers
- **[Collection Matchers](collections.md)** - Collection matchers (strings are sequences of chars)
- **[Numeric Matchers](numeric.md)** - Numeric comparisons
- **[Quick Start](../../quick-start.md)** - Get started with FX.Spec
