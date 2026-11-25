# EVIL Grammar Specification (Flex & Bison)

This directory contains the formal lexical and syntactic grammar specifications for the **EVIL** programming language.

The grammar is validated against the test suite in `VirtualMachine/Tests/EVIL.Ceres.LanguageTests/tests/*.vil` as well as the runtime built-ins in `VirtualMachine/EVIL.Ceres.Runtime/ScriptBuiltins/*.vil`.

---

## Files

| File | Description |
| :--- | :--- |
| [`evil.l`](evil.l) | Flex lexical scanner specification |
| [`evil.y`](evil.y) | Bison LALR(1) / GLR grammar specification |
| [`evil.ebnf`](evil.ebnf) | ISO/IEC 14977 style Extended Backus-Naur Form grammar |
| [`main.c`](main.c) | CLI test runner and parser driver |
| [`Makefile`](Makefile) | Build and test automation |
| [`README.md`](README.md) | Grammar documentation and reference |

---

## Building and Testing

### Prerequisites

- `flex` (>= 2.6)
- `bison` (>= 3.8)
- `gcc` or `clang`
- `make`

### Build

```bash
make
```

This compiles `evil.l` and `evil.y` with `gcc -O2` into the `evil_parser` executable.

### Test

```bash
make test
```

This validates `evil_parser` against the test files in `VirtualMachine/Tests/EVIL.Ceres.LanguageTests/tests` and the 2 runtime builtins (`dofile.vil`, `require.vil`).

To run manually on individual files:

```bash
./evil_parser path/to/script.vil
./evil_parser -q path/to/script.vil   # Quiet mode
```

Or read from standard input:

```bash
echo 'val x = 42;' | ./evil_parser
```

---

## Lexical Structure (`evil.l`)

### Whitespace and Comments

- **Whitespace**: Spaces, tabs (`\t`), carriage returns (`\r`), vertical tabs (`\v`), and form feeds (`\f`) are discarded.
- **UTF-8 BOM**: Byte order mark `0xEF 0xBB 0xBF` is ignored at the start of input.
- **Line Comments**: `// ...` to end of line.
- **Block Comments**: `/* ... */` with arbitrary nesting and newlines.

### Literals

- **Numbers**: Decimal integers (`42`), floating-point numbers (`3.14`, `.5`, `10.`), scientific notation (`1e-5`, `2.5E+3`).
- **Hex Integers**: `0x...` / `0X...` (e.g. `0xFF`, `0x1a2b`).
- **Plain Strings**: Single-quoted string literals `'...'` with escape sequences `\'`, `\\`, etc.
- **Interpolated Strings**: Double-quoted strings `"..."` with embedded expressions `"${...}"` or `"$ident"`. The scanner tracks brace nesting, nested quotes, and comments inside `${...}` blocks so that complex interpolated expressions are consumed as a single token.
- **Special Values / Constants**: `true`, `false`, `nil`, `NaN`, `Infinity`.

### Type Identifiers

`Nil`, `Number`, `String`, `Boolean`, `Table`, `Array`, `Fiber`, `Function` (alias `Fn`), `Error`, `Type`, `NativeFunction`, `NativeObject`.

### Composite Tokens & Operators

- **Attributes**: `#[`
- **Equality**: `==`, `!=`, `<==>` (deep equal), `<!=>` (deep not equal)
- **Relational**: `<`, `>`, `<=`, `>=`, `in`, `!in`, `is`, `!is`
  - Note: `!in` and `!is` followed by identifier characters (e.g., `!inside`, `!island`) are disambiguated by trailing context (`yyless(1)`) to yield `!` followed by an identifier.
- **Logical**: `&&`, `||`
- **Bitwise**: `&`, `|`, `^`, `~`, `<<`, `>>`
- **Assignment**: `=`, `+=`, `-=`, `*=`, `/=`, `%=`, `&=`, `|=`, `^=`, `<<=`, `>>=`, `??=`
- **Navigation & Access**: `.`, `?.` (elvis property), `?[` (elvis indexer), `..` (half-open collection range inside indexers), `:` (method call), `?:` (safe method call), `?(` (safe invocation), `::` (targeted fn / self::fn), `!` (postfix non-nil assertion), `...?` (nil-tolerant table spread)
- **Collection from-end bounds**: `#` before an index or range bound (e.g. `values[#-1]`); parenthesize a length expression when using it as a bound (e.g. `values[(#values - 1)]`)
- **Functions, Closures & Spreads**: `=>` (association / arm), `->` (expression body / arm), `...` (extra arguments / varargs / table spread), `*` (variadic forwarding)
- **Nil Coalescing**: `??`

---

## Grammar Hierarchy (`evil.y`)

The grammar reflects the recursive descent precedence climbing parser in `Core/EVIL.Grammar/`:

### 1. Program & Statements

```
program
    : statement_list_opt
    ;

statement
    : fn_statement
    | if_statement
    | for_statement
    | while_statement
    | do_while_statement
    | each_statement
    | block_statement
    | val_statement
    | try_statement
    | retry_statement
    | throw_statement
    | ret_statement
    | skip_statement
    | break_statement
    | expression_statement
    ;
```

### 2. Declarations and Functions

- **Global & Local Functions**:
  - `fn name(params) { ... }`
  - `loc fn name(params) -> expr;`
  - `loc fn target::method(params) { ... }` (targeted definitions)
  - `loc fn table.sub.method(params) { ... }` (indexed definitions)
- **Attributes**:
  - `#[test]`
  - `#[attribute(key = value, "arg")]`
- **Variable Declarations**:
  - `val x = 1, y = 2;`
  - `rw val x = 1;` (read-write mutable local)

### 3. Control Flow

- **If / Elif / Else**:
  - `if (cond) stmt elif (cond2) stmt else stmt`
  - Expression branch shorthand: `if (cond) -> expr;`
- **Loops**:
  - `while (cond) stmt`
  - `do stmt while (cond);`
  - `for (init; cond; step) stmt`
  - `each (rw val item : collection) stmt`
  - `each (rw val key, val : collection) stmt`
- **Exceptions**:
  - `try stmt catch { ... }`
  - `try stmt catch(err) { ... }`
  - `throw expr;`
  - `retry;`

### 4. Expression Precedence Table

From lowest to highest precedence:

| Level | Expression Type             | Operators / Constructs                                                                                                                                                       |  Associativity   |
|:-----:|:----------------------------|:-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------|:----------------:|
|   1   | Assignment & Arrow Function | `=`, `+=`, `-=`, `*=`, `/=`, `%=`, `&=`, `\|=`, `^=`, `<<=`, `>>=`, `??=`, `fn -> expr`                                                                                      |      Right       |
|   2   | Conditional (Ternary)       | `? :`                                                                                                                                                                        |      Right       |
|   3   | Nil-Coalescing             | `??`, `?? throw expr`                                                                                                                                                        |      Right       |
|   4   | Logical OR                  | `\|\|`                                                                                                                                                                       |       Left       |
|   5   | Logical AND                 | `&&`                                                                                                                                                                         |       Left       |
|   6   | Bitwise OR                  | `\|`                                                                                                                                                                         |       Left       |
|   7   | Bitwise XOR                 | `^`                                                                                                                                                                          |       Left       |
|   8   | Bitwise AND                 | `&`                                                                                                                                                                          |       Left       |
|   9   | Equality                    | `==`, `!=`, `<==>`, `<!=>`                                                                                                                                                   |       Left       |
|  10   | Relational & Type Check     | `<`, `>`, `<=`, `>=`, `in`, `!in`, `is`, `!is`                                                                                                                               |       Left       |
|  11   | Bitwise Shift               | `<<`, `>>`                                                                                                                                                                   |       Left       |
|  12   | Additive                    | `+`, `-`                                                                                                                                                                     |       Left       |
|  13   | Multiplicative              | `*`, `/`, `%`                                                                                                                                                                |       Left       |
|  14   | Pattern                     | `by (expr) { ... }`, `expr with { ... }`                                                                                                                                     | Non-assoc / Left |
|  15   | Prefix                      | `++expr`, `--expr`                                                                                                                                                           |      Right       |
|  16   | Unary                       | `+`, `-`, `!`, `~`, `@`, `#`, `$`                                                                                                                                            |      Right       |
|  17   | Postfix                     | `expr()`, `expr[]`, `expr!.id`, `expr!!`, `expr.id`, `expr?.id`, `expr?[idx]`, `expr:method()`, `expor?:method()` expr++`, `expr--`                                                                                 |       Left       |
|  18   | Primary                     | Identifiers, literals, tables `{...}`, arrays `[...]` / `array(...)`, parenthesized `(...)`, block functions `fn(...) {...}`, `typeof(...)`, `yield expr(...)`, `error(...)` |      Atomic      |

---

## Design and Disambiguation Notes

1. **Arrow Function Precedence (`fn ... -> expr`)**:
   In the C# reference parser, `fn -> expr` greedily parses an `AssignmentExpression()` as its body until terminated by statement or container delimiters (`;`, `,`, `)`, `]`, `}`). Placing `fn_arrow_expression` at the `assignment_expression` level (and reserving `primary_expression` for block functions `fn { ... }` and parenthesized arrow functions `(fn -> ...)`) prevents ambiguities where binary operators (such as `+`) or invocations (`(...)`) could attach outside an unparenthesized arrow function.

2. **Block Statement vs Empty Table (`{}` Ambiguity)**:
   A bare block `{}` at statement level is parsed as an empty block statement (`block_statement`), exactly matching EVIL's statement parser which immediately dispatches `TokenType.LBrace` to `BlockStatement()`. A table literal alone is not a valid expression statement under `Expression.IsValidExpressionStatement`.

3. **`is NativeObject` with Class Specifier**:
   EVIL supports checking native object subtypes using `expr is NativeObject "Namespace.ClassName"`. The grammar models this through the optional `is_native_opt` string literal following the `type_code`.

4. **Argument List with Variadic Forwarding (`*`)**:
   EVIL allows forwarding extra variadic arguments directly to a callee via `f(*)`. This is explicitly modeled in `argument_list` alongside standard `assignment_expression` arguments.

5. **Collection Ranges (`..`)**:
   A range is recognized only inside normal or conditional indexers (`items[1..4]`, `items[..3]`, `items[3..]`). Bounds are half-open, may use a leading `#` for signed from-end offsets, and are not assignable. `#` remains the length operator outside index bounds, so parenthesize a bound expression that starts with length; `^` remains binary XOR.
