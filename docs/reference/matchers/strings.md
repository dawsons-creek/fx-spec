# String Matchers

String-specific matchers for prefixes, suffixes, patterns, and more.

---

## Prefixes & Suffixes

### startWith

**Type:** `string -> Matcher<string>`

Matches if the string starts with the expected prefix.

**Usage:**

```fsharp
expect string |> should (startWith prefix)
```

**Examples:**

```fsharp
expect "hello world" |> should (startWith "hello")
expect "FxSpec" |> should (startWith "Fx")
expect "https://example.com" |> should (startWith "https://")

// Case-sensitive
expect "Hello" |> shouldNot (startWith "hello")
```

**Failure Message:**

```fsharp
expect "hello world" |> should (startWith "goodbye")
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
expect string |> should (endWith suffix)
```

**Examples:**

```fsharp
expect "hello world" |> should (endWith "world")
expect "test.txt" |> should (endWith ".txt")
expect "FxSpec" |> should (endWith "Spec")

// File extensions
let filename = "document.pdf"
expect filename |> should (endWith ".pdf")
```

**Failure Message:**

```fsharp
expect "hello world" |> should (endWith ".txt")
// => Expected string to end with '.txt', but found 'hello world'
```

---

## Substrings

### containSubstring

**Type:** `string -> Matcher<string>`

Matches if the string contains the expected substring.

**Usage:**

```fsharp
expect string |> should (containSubstring substring)
```

**Examples:**

```fsharp
expect "hello world" |> should (containSubstring "lo wo")
expect "FxSpec is great" |> should (containSubstring "Spec")
expect "error: file not found" |> should (containSubstring "error:")

// Search in logs
let logMessage = "2025-01-01 10:00:00 INFO User logged in"
expect logMessage |> should (containSubstring "INFO")
expect logMessage |> should (containSubstring "logged in")
```

**Failure Message:**

```fsharp
expect "hello world" |> should (containSubstring "goodbye")
// => Expected string to contain 'goodbye', but found 'hello world'
```

---

## Patterns

### matchRegex

**Type:** `string -> Matcher<string>`

Matches if the string matches the regular expression pattern.

**Usage:**

```fsharp
expect string |> should (matchRegex pattern)
```

**Examples:**

```fsharp
// Email validation
expect "test@example.com" |> should (matchRegex @"^\w+@\w+\.\w+$")

// Phone numbers
expect "555-1234" |> should (matchRegex @"^\d{3}-\d{4}$")

// Dates
expect "2025-01-01" |> should (matchRegex @"^\d{4}-\d{2}-\d{2}$")

// Contains digits
expect "hello123" |> should (matchRegex @"\d+")

// Starts with uppercase
expect "Hello" |> should (matchRegex @"^[A-Z]")
```

**Failure Message:**

```fsharp
expect "hello" |> should (matchRegex @"^\d+$")
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
expect string |> should beEmptyString
```

**Examples:**

```fsharp
expect "" |> should beEmptyString
expect String.Empty |> should beEmptyString

// After operations
let trimmed = "   ".Trim()
expect trimmed |> should beEmptyString
```

**Failure Message:**

```fsharp
expect "hello" |> should beEmptyString
// => Expected empty string, but found 'hello'

expect null |> should beEmptyString
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
expect string |> should (haveStringLength length)
```

**Examples:**

```fsharp
expect "hello" |> should (haveStringLength 5)
expect "" |> should (haveStringLength 0)
expect "FxSpec" |> should (haveStringLength 6)

// Password validation
let password = "secret123"
expect password |> should (haveStringLength 9)
```

**Failure Message:**

```fsharp
expect "hello" |> should (haveStringLength 10)
// => Expected string of length 10, but found length 5 ('hello')

expect null |> should (haveStringLength 5)
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

open FxSpec.Core
open FxSpec.Matchers

[<Tests>]
let urlValidationSpecs =
    spec {
        describe "URL Validator" [
            describe "validateUrl" [
                context "when URL is valid" [
                    it "accepts https URLs" (fun () ->
                        let url = "https://example.com"
                        expect url |> should (startWith "https://")
                        expect url |> should (matchRegex @"^https?://[\w\-]+(\.[\w\-]+)+[/#?]?.*$")
                    )

                    it "accepts http URLs" (fun () ->
                        let url = "http://example.com/path?query=value"
                        expect url |> should (startWith "http://")
                        expect url |> should (containSubstring "example.com")
                    )

                    it "accepts URLs with paths" (fun () ->
                        let url = "https://example.com/api/users"
                        expect url |> should (containSubstring "/api/users")
                        expect url |> should (endWith "/users")
                    )
                ]

                context "when URL is invalid" [
                    it "rejects empty strings" (fun () ->
                        expect "" |> should beEmptyString
                        expect "" |> shouldNot (startWith "http")
                    )

                    it "rejects non-URL strings" (fun () ->
                        expect "not a url" |> shouldNot (matchRegex @"^https?://")
                    )
                ]
            ]
        ]
    }
```

### Testing String Transformations

```fsharp
module StringTransformationSpecs

open FxSpec.Core
open FxSpec.Matchers

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
                    expect result |> shouldNot (containSubstring " ")
                )

                it "removes special characters" (fun () ->
                    let result = StringHelpers.slugify "Hello, World!"
                    expect result |> should (equal "hello-world")
                    expect result |> should beAlphabetic  // False, contains hyphen
                    expect result |> should (matchRegex @"^[a-z\-]+$")
                )
            ]

            describe "sanitize" [
                it "trims whitespace" (fun () ->
                    let result = StringHelpers.sanitize "  hello  "
                    expect result |> should (equal "hello")
                    expect result |> shouldNot (startWith " ")
                    expect result |> shouldNot (endWith " ")
                )

                it "returns empty string for whitespace-only input" (fun () ->
                    let result = StringHelpers.sanitize "   "
                    expect result |> should beEmptyString
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
- **[Quick Start](../../quick-start.md)** - Get started with FxSpec
