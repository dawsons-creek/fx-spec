# FxSpec vs RSpec: A Detailed Comparison

## Philosophy

Both frameworks share the same BDD philosophy: tests should read like specifications and serve as living documentation. However, FxSpec brings the additional benefits of static typing and functional programming.

## Syntax Comparison

### Basic Structure

**RSpec (Ruby)**
```ruby
describe "Calculator" do
  context "when adding numbers" do
    it "adds two positive numbers" do
      expect(2 + 2).to eq(4)
    end
  end
end
```

**FxSpec (F#)**
```fsharp
spec {
    describe "Calculator" {
        context "when adding numbers" {
            it "adds two positive numbers" {
                expect (2 + 2) |> should (equal 4)
            }
        }
    }
}
```

**Similarity**: Nearly identical structure and readability  
**Difference**: FxSpec uses computation expressions and pipe operators

### State Management

**RSpec (Ruby)**
```ruby
describe "User" do
  let(:user) { User.new("john@example.com") }
  subject { user }
  
  before do
    user.activate
  end
  
  it "is active" do
    expect(subject.active?).to be true
  end
end
```

**FxSpec (F#)**
```fsharp
spec {
    describe "User" {
        let' "user" (fun () -> User("john@example.com"))
        subject (fun () -> get "user" :?> User)
        
        before (fun () ->
            let user = getSubject() :?> User
            user.Activate()
        )
        
        it "is active" {
            let user = getSubject() :?> User
            expect user.Active |> should (equal true)
        }
    }
}
```

**Similarity**: Same lazy evaluation and scoping semantics  
**Difference**: FxSpec requires explicit type casting (more verbose but safer)

## Key Improvements in FxSpec

### 1. Compile-Time Type Safety

**RSpec (Runtime Error)**
```ruby
describe "Math" do
  it "compares numbers" do
    expect("5").to be > 3  # Runtime error: String doesn't support >
  end
end
```

**FxSpec (Compile Error)**
```fsharp
spec {
    describe "Math" {
        it "compares numbers" {
            expect "5" |> should (beGreaterThan 3)
            // Compile error: Type mismatch
            // Expected: int, Got: string
        }
    }
}
```

**Benefit**: Catch errors before running tests

### 2. Type-Safe Matchers

**RSpec (Dynamic)**
```ruby
# Matcher can be used with any type
expect([1, 2, 3]).to include(2)
expect("hello").to include("ell")
expect({a: 1}).to include(:a)
```

**FxSpec (Type-Constrained)**
```fsharp
// Different matchers for different types
expect [1; 2; 3] |> should (contain 2)        // seq<int>
expect "hello" |> should (containSubstring "ell")  // string
expect map |> should (haveKey "a")            // Map<string, 'a>

// Compile error if you mix them up:
expect [1; 2; 3] |> should (containSubstring "2")  // ERROR!
```

**Benefit**: Impossible to use wrong matcher for a type

### 3. Explicit Result Types

**RSpec (Implicit)**
```ruby
class MyMatcher
  def matches?(actual)
    @actual = actual
    actual == @expected
  end
  
  def failure_message
    "Expected #{@expected}, got #{@actual}"
  end
end
```

**FxSpec (Explicit)**
```fsharp
let myMatcher expected : Matcher<'a> =
    fun actual ->
        if actual = expected then
            Pass
        else
            Fail($"Expected {expected}, got {actual}",
                 Some (box expected),
                 Some (box actual))
```

**Benefit**: 
- Single source of truth
- Impossible to forget failure message
- Compiler ensures all cases handled

### 4. Immutability by Default

**RSpec (Mutable)**
```ruby
describe "Counter" do
  let(:counter) { Counter.new }
  
  it "increments" do
    counter.increment  # Mutates counter
    expect(counter.value).to eq(1)
  end
  
  it "starts at zero" do
    # If tests run in wrong order, this could fail
    expect(counter.value).to eq(0)
  end
end
```

**FxSpec (Immutable)**
```fsharp
spec {
    describe "Counter" {
        let' "counter" (fun () -> Counter.create())
        
        it "increments" {
            let counter = get "counter" :?> Counter
            let newCounter = Counter.increment counter
            expect newCounter.Value |> should (equal 1)
        }
        
        it "starts at zero" {
            // Fresh counter every time - guaranteed
            let counter = get "counter" :?> Counter
            expect counter.Value |> should (equal 0)
        }
    }
}
```

**Benefit**: Tests are truly independent and can run in any order

### 5. Pattern Matching for Complex Assertions

**RSpec (Limited)**
```ruby
describe "Result" do
  it "is successful" do
    result = do_something()
    expect(result.success?).to be true
    expect(result.value).to eq(42)
  end
end
```

**FxSpec (Powerful)**
```fsharp
spec {
    describe "Result" {
        it "is successful" {
            let result = doSomething()
            
            // Pattern match on discriminated union
            match result with
            | Ok value -> expect value |> should (equal 42)
            | Error msg -> failwith $"Expected Ok, got Error: {msg}"
            
            // Or use built-in matcher
            expect result |> should (beOk 42)
        }
    }
}
```

**Benefit**: Leverage F#'s powerful pattern matching in tests

### 6. Function Composition in Matchers

**RSpec (Chaining)**
```ruby
expect(user.name).to start_with("John").and end_with("Doe")
```

**FxSpec (Composition)**
```fsharp
// Compose matchers functionally
let matchBoth m1 m2 : Matcher<'a> =
    fun actual ->
        match m1 actual, m2 actual with
        | Pass, Pass -> Pass
        | Fail(msg1, _, _), _ -> Fail(msg1, None, None)
        | _, Fail(msg2, _, _) -> Fail(msg2, None, None)

expect user.Name 
|> should (matchBoth (startWith "John") (endWith "Doe"))
```

**Benefit**: Matchers are just functions - compose them however you want

## What RSpec Does Better

### 1. Less Verbose Syntax

**RSpec**
```ruby
let(:user) { User.new }
```

**FxSpec**
```fsharp
let' "user" (fun () -> User())
```

**Trade-off**: FxSpec is more explicit but also more verbose

### 2. Dynamic Flexibility

**RSpec**
```ruby
# Can dynamically add matchers at runtime
RSpec::Matchers.define :be_even do
  match { |actual| actual.even? }
end
```

**FxSpec**
```fsharp
// Must define at compile time
let beEven : Matcher<int> =
    fun actual -> if actual % 2 = 0 then Pass else Fail(...)
```

**Trade-off**: Less flexibility but more safety

### 3. Metaprogramming

**RSpec**
```ruby
# Can generate tests dynamically
[1, 2, 3].each do |n|
  it "handles #{n}" do
    expect(process(n)).to be_valid
  end
end
```

**FxSpec**
```fsharp
// Must be more explicit
spec {
    describe "Processing" {
        for n in [1; 2; 3] do
            it $"handles {n}" {
                expect (process n) |> should beValid
            }
    }
}
```

**Trade-off**: Similar capability but less "magical"

## Performance Comparison

| Aspect | RSpec | FxSpec |
|--------|-------|--------|
| **Startup Time** | Fast (interpreted) | Slower (compiled, but one-time) |
| **Execution Speed** | Moderate | Fast (compiled, optimized) |
| **Memory Usage** | Higher (Ruby VM) | Lower (native .NET) |
| **Parallel Execution** | Built-in | Planned (future) |

## Feature Parity Matrix

| Feature | RSpec | FxSpec | Notes |
|---------|-------|--------|-------|
| describe/context | ✅ | ✅ | Identical |
| it/specify | ✅ | ✅ | Identical |
| let/let! | ✅ | ✅ | FxSpec uses `let'` |
| subject | ✅ | ✅ | Similar API |
| before/after | ✅ | ✅ | Identical semantics |
| expect().to | ✅ | ✅ | FxSpec uses `to'` |
| Matchers | ✅ | ✅ | Type-safe in FxSpec |
| Custom matchers | ✅ | ✅ | Simpler in FxSpec |
| Shared examples | ✅ | 🔄 | Planned |
| Metadata/tags | ✅ | 🔄 | Planned |
| Mocking | ✅ (rspec-mocks) | 🔄 | Planned |
| Request specs | ✅ (rspec-rails) | ✅ | Built-in |
| Feature specs | ✅ (Capybara) | 🔄 | Planned (Playwright) |

## When to Choose FxSpec

Choose FxSpec when:
- ✅ You value compile-time safety
- ✅ You're working in the .NET ecosystem
- ✅ You prefer functional programming
- ✅ You want immutability by default
- ✅ You need strong typing for complex domains
- ✅ You want better IDE support (IntelliSense, refactoring)

## When to Choose RSpec

Choose RSpec when:
- ✅ You're working in Ruby/Rails
- ✅ You need maximum flexibility
- ✅ You prefer dynamic typing
- ✅ You want the most mature ecosystem
- ✅ You need extensive third-party integrations
- ✅ You value conciseness over explicitness

## Conclusion

FxSpec is not just an RSpec clone—it's a **conceptual enhancement** that brings:

1. **Type Safety**: Catch errors at compile time
2. **Functional Purity**: Immutable, composable tests
3. **Rich Type System**: Discriminated unions, pattern matching
4. **Performance**: Compiled, optimized execution
5. **Tooling**: Full IDE support with IntelliSense

While RSpec remains the gold standard for Ruby, FxSpec aims to be the definitive BDD framework for F# and .NET, combining RSpec's elegant syntax with F#'s powerful type system and functional programming paradigm.

