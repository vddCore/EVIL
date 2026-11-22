# CVIL: C Transpiler & Native AOT Toolchain for EVIL

> **Status:** Proposal & Architectural Specification  
> **Target Language:** EVIL (Ensign Virtual Interpreted Language)  
> **Output Language:** ANSI C (C99 / C11)  
> **Target Binary:** Standalone Native Executables (`ELF`, `Mach-O`, `PE/COFF`) and Shared Libraries (`.so`, `.dylib`, `.dll`)  
> **Location:** `Docs/Proposals/CVIL/`

---

## Table of Contents

1. [Executive Summary & Motivation](#1-executive-summary--motivation)
2. [Language Feature Compatibility & Native Boundaries](#2-language-feature-compatibility--native-boundaries)
   - [2.1 Strictly Impossible Features (CLR Specifics)](#21-strictly-impossible-features-clr-specifics)
   - [2.2 Adapted Native Equivalents](#22-adapted-native-equivalents)
   - [2.3 Preserved EVIL Language Features](#23-preserved-evil-language-features)
3. [Architecture Overview & Pipeline](#3-architecture-overview--pipeline)
4. [User-Friendly Diagnostic & Error Reporting System](#4-user-friendly-diagnostic--error-reporting-system)
5. [AST Lowering & Code Generation Strategy](#5-ast-lowering--code-generation-strategy)
   - [5.1 Dynamic Value Model: Tagged Union vs NaN-Boxing](#51-dynamic-value-model-tagged-union-vs-nan-boxing)
   - [5.2 Scoping, Immutability & Closure Conversion](#52-scoping-immutability--closure-conversion)
   - [5.3 Hybrid Tables & Metamethod Dispatch](#53-hybrid-tables--metamethod-dispatch)
   - [5.4 Pattern Matching (`by`) and Table Expansion (`with`)](#54-pattern-matching-by-and-table-expansion-with)
   - [5.5 Cooperative Concurrency & Fibers (`yield`)](#55-cooperative-concurrency--fibers-yield)
   - [5.6 Exception Handling & `retry` Statement](#56-exception-handling--retry-statement)
6. [The `libcvil` Native Runtime Library](#6-the-libcvil-native-runtime-library)
   - [6.1 Memory Management & Garbage Collection](#61-memory-management--garbage-collection)
   - [6.2 Standard Library C Implementations](#62-standard-library-c-implementations)
   - [6.3 Native C FFI (Foreign Function Interface)](#63-native-c-ffi-foreign-function-interface)
7. [C Compiler Driver & Executable Generation](#7-c-compiler-driver--executable-generation)
8. [Implementation Roadmap & Milestones](#8-implementation-roadmap--milestones)
9. [Risk Assessment & Mitigation Matrix](#9-risk-assessment--mitigation-matrix)

---

## 1. Executive Summary & Motivation

The **EVIL** programming language reference implementation (`Ceres`) is an interpreted bytecode system hosted entirely on the .NET 9 CLR. While Ceres provides a clean embeddable scripting environment and dynamic sandbox, running EVIL via a managed virtual machine introduces runtime overhead:
- Mandatory dependency on the .NET runtime.
- Bytecode interpretation loop and dispatch overhead.
- Memory overhead from CLR object headers and boxing.
- Non-deterministic startup latency unsuitable for small CLI utilities or embedded game engines.

**CVIL** is an Ahead-Of-Time (AOT) transpiler that translates EVIL source code into portable, human-readable, highly optimizable **C99/C11** source code, which is then compiled using standard host compilers (`gcc`, `clang`, `msvc`, or `tcc`) into standalone native machine-code executables or dynamic libraries.

### Core Objectives
1. **Zero Runtime Dependency:** Emit standalone native executables that run directly on bare OS kernels with no .NET dependency.
2. **Preserve Complete Language Semantics:** Full support for tables, metatables, closures, upvalues, string interpolation, pattern matching (`by`), table expansion (`with`), exceptions (`try`/`catch`/`throw`/`retry`), and cooperative fibers (`yield`).
3. **High-Performance Native Execution:** Leverage modern C optimizing compilers for register allocation, inlining, loop unrolling, and vectorization.
4. **Precision Diagnostics:** Industrial-grade, user-friendly diagnostic reporting featuring colored source spans, caret indicators, error codes, and contextual fix hints.
5. **Debuggability:** Generate clean C with `#line` directives mapping compiled machine instructions directly back to original `.vil` source files in GDB, LLDB, and Valgrind.

---

## 2. Language Feature Compatibility & Native Boundaries

Because EVIL was originally authored in C# on .NET, transitioning to native C requires defining the boundary between language semantics and runtime host environments.

```
+-------------------------------------------------------------------------------+
|                             EVIL LANGUAGE CORE                                |
|  Statements, Val/Rw, Closures, Metatables, Fibers, Patterns, Interpolation     |
+---------------------------------------+---------------------------------------+
                                        |
                 +----------------------+----------------------+
                 |                                             |
                 v                                             v
+---------------------------------+           +---------------------------------+
|      C# / CERES RUNTIME         |           |       CVIL NATIVE RUNTIME       |
| - .NET CLR Garbage Collector    |           | - Boehm GC / Tracing Mark-Sweep |
| - System.Reflection Type System |  ====>>   | - Typed C FFI & Native Registry |
| - Dynamic Assembly Loading      |           | - AOT Whole-Program Linking     |
| - Managed ThreadPool Fibers     |           | - Native Stackful Coroutines    |
| - Managed Exceptions            |           | - setjmp/longjmp Structured Try |
+---------------------------------+           +---------------------------------+
```

### 2.1 Strictly Impossible Features (CLR Specifics)

The following features belong strictly to the managed .NET execution environment and **cannot** be ported to native C without embedding a full .NET CLR runtime (which defeats the purpose of an AOT C transpiler):

| Feature / Pattern | Description in .NET Reference | Native C Reality & Limitation |
| :--- | :--- | :--- |
| **CLR Dynamic Reflection** | `Type.GetType("System.Text.StringBuilder")` or querying arbitrary .NET assembly metadata at runtime. | **Impossible.** Native C binaries have no managed metadata tables, CLR AppDomains, or JIT assembly loaders. |
| **Arbitrary Managed Object Wrapping** | Storing managed C# instances (`System.IO.Stream`, `System.Net.Sockets.Socket`) inside `DynamicValueType.NativeObject`. | **Impossible directly.** Native C has no knowledge of managed object pointers or CLR heap layout. |
| **Unconstrained Runtime `evil.compile`** | Compiling arbitrary arbitrary dynamically-generated strings into machine code at runtime inside a static binary without a compiler present. | **Impossible without bundled compiler.** A static binary cannot compile new source code unless an embedded compiler (e.g., `libtcc`) or an embedded AST evaluator is linked into the binary. |

### 2.2 Adapted Native Equivalents

For every CLR-specific feature that cannot be directly mapped, CVIL provides a clean, native C architecture:

| C# / Ceres Construct | CVIL Native C Equivalent | Adaptation Details |
| :--- | :--- | :--- |
| `DynamicValueType.NativeObject` | `evil_native_obj_t` (`void* data` + `evil_native_type_t* descriptor`) | Explicit C struct containing an opaque payload pointer, type name string, and custom destructor/finalizer callback function. |
| `DynamicValueType.NativeFunction` | `evil_native_fn_t` function pointer | C ABI function pointer: `evil_value_t (*)(evil_fiber_t* fiber, int argc, evil_value_t* argv)`. |
| `Type` Code Checking (`is NativeObject "Type"`) | Native Type Registry with string hashes | Opaque objects register their native type descriptor in a global type registry table; `x is NativeObject "ClassName"` checks the descriptor's string identifier. |
| Dynamic `dofile` / `require` | Static AOT Linking + Dynamic Shared Object (`dlopen`) Loading | 1. **Static Mode (Default):** The transpiler resolves module paths at compile-time and links them into the single binary.<br>2. **Dynamic Mode:** If a file is required dynamically at runtime, CVIL loads pre-compiled native shared libraries (`.so` / `.dll`). |
| Host Interop | C Foreign Function Interface (FFI) | Clean, macro-assisted C binding API allowing C developers to bind native structs, libraries, and functions to EVIL tables. |

### 2.3 Preserved EVIL Language Features

All pure language semantics defined in the EVIL specification are **100% preserved**:
- **Lexical and Syntax:** Every keyword, operator (`<==>`, `<!=>`, `!in`, `!is`, `?.`, `?[`, `::`, `??=`, etc.), and literal.
- **Variables & Scoping:** Lexical blocks, `val` (immutable by default), `rw val` (mutable), global scope table.
- **Closures & Upvalues:** Full lexical scoping with shared mutable upvalue cells (transparent across nested functions).
- **Tables & Metamethods:** All 32 metamethods (`__add`, `__invoke`, `__index`/`__get`, `__set`, `__tostr`, `__len`, etc.), freezing, shallow/deep copies.
- **Arrays:** Resizable dynamic array semantics with 0-based indexing and built-in methods.
- **String Interpolation:** Embedded expressions `"${x + 1}"` and identifier shortcuts `"$var"`.
- **Pattern Matching (`by`):** Shallow (`->`) and deep (`=>`) arms, default (`else:`) fallback, expression bodies.
- **Table Expansion (`with`):** Non-destructive table cloning and property augmentation.
- **Error Handling:** `try`, `catch (err)`, `throw expr`, and the unique EVIL `retry` statement.
- **Cooperative Fibers:** `yield<target>(args)` cooperative coroutines with argument passing and return value retrieval.

---

## 3. Architecture Overview & Pipeline

CVIL operates in four distinct phases:

```
  +--------------------------------------------------------------------------+
  |                               PHASE 1: FRONTEND                          |
  |  evil.l (Flex) -> evil.y (Bison) -> Concrete AST -> Scope/Symbol Graph   |
  +-------------------------------------+------------------------------------+
                                        |
                                        v
  +--------------------------------------------------------------------------+
  |                           PHASE 2: SEMANTIC & ANALYSIS                   |
  |  - Val/Rw Immutability Validation                                        |
  |  - Closure & Upvalue Analysis (Escape Analysis)                         |
  |  - Precision Diagnostic Engine (Errors, Warnings, Hints)                 |
  +-------------------------------------+------------------------------------+
                                        |
                                        v
  +--------------------------------------------------------------------------+
  |                           PHASE 3: LOWERING & CODEGEN                    |
  |  - Desugar 'by', 'with', '?.', '?[', string interpolation               |
  |  - Lift Closures into C Functions + Environment Structs                  |
  |  - Emit Formatted C99 Code with #line Source Mappings                    |
  +-------------------------------------+------------------------------------+
                                        |
                                        v
  +--------------------------------------------------------------------------+
  |                           PHASE 4: NATIVE COMPILATION                    |
  |  Host Compiler (GCC / Clang / MSVC / TCC) + libcvil.a -> Native Binary   |
  +--------------------------------------------------------------------------+
```

### Detailed Pipeline Flow

1. **Frontend (Lexing & Parsing):**
   - The Flex scanner ([evil.l](file:///workbench/code/dotnet/EVIL/Docs/Syntax/evil.l)) and Bison parser ([evil.y](file:///workbench/code/dotnet/EVIL/Docs/Syntax/evil.y)) construct an Abstract Syntax Tree (AST).
   - Source spans (`Line`, `Column`, `Length`, `SourceFile`) are attached to every AST node.

2. **Semantic Analysis & Scope Resolution:**
   - Constructs a hierarchical Scope Tree.
   - Detects illegal reassignment to immutable `val` bindings.
   - Identifies free variables captured across closure boundaries and marks them as **Upvalues**.

3. **AST Transformation & Lowering:**
   - Desugars complex EVIL features into foundational runtime operations:
     - `by` expressions become multi-branch condition trees with deep equality testing.
     - `with` expressions become calls to `evil_table_clone_with()`.
     - String interpolations become dynamic string builder chains.
     - Function declarations become static C functions taking an explicit `evil_fiber_t*` and argument vector.

4. **C Code Generation:**
   - Emits self-contained C files.
   - Generates constant pools (strings, numbers, symbols) in the data segment.
   - Generates `#line` directives matching original `.vil` line numbers for native debugger support.

5. **Host Toolchain Invocation:**
   - Drives `gcc`, `clang`, or `tcc` with optimal compiler flags (`-O2`, `-fno-strict-aliasing`, `-Wall`).
   - Statically links `libcvil.a` (the native runtime) and the garbage collector (`libgc`).
   - Produces the final native executable.

---

## 4. User-Friendly Diagnostic & Error Reporting System

A core requirement is that CVIL must provide **compiler-grade, user-friendly diagnostics** matching the quality of Rust, Clang, and the EVIL reference analyzer.

### Diagnostic Visual Design

Diagnostics feature:
1. **Source Context:** File name, line number, column number.
2. **Diagnostic Severity & Unique Code:** `error[CVIL0104]`, `warning[CVIL0201]`, `note[CVIL0002]`.
3. **Colored Visual Gutter:** Highlighting the exact source line.
4. **Caret & Squiggly Underlines:** Pinpointing the token or expression span.
5. **Contextual Help & Suggestions:** Suggesting the fix (e.g. `help: declare with 'rw val' to allow mutation`).

#### Example 1: Illegal Mutation of Immutable `val`
```text
error[CVIL0102]: cannot assign to immutable variable 'player_health'
  --> tests/game_logic.vil:14:3
   |
13 |   val player_health = 100;
   |       ------------- 'player_health' was originally declared immutable here
14 |   player_health -= 25;
   |   ^^^^^^^^^^^^^ cannot assign to immutable 'val'
   |
   = help: declare variable as read-write using 'rw val':
13 |   rw val player_health = 100;
   |   ++++++
```

#### Example 2: Invalid Metamethod Syntax
```text
warning[CVIL0205]: unrecognized metamethod key '__ad'
  --> entities/vector.vil:8:5
   |
 8 |     __ad: fn(a, b) -> Vector::new(a.x + b.x, a.y + b.y),
   |     ^^^^ did you mean '__add'?
   |
   = note: table metamethods must match known operators (e.g., '__add', '__sub', '__invoke')
```

#### Example 3: Undefined Symbol with Fuzzy Matching
```text
error[CVIL0140]: cannot find symbol 'assert_eq' in current scope
  --> tests/math_test.vil:22:3
   |
22 |   assert_eq(result, 42);
   |   ^^^^^^^^^ unknown identifier
   |
   = help: a function with a similar name exists: 'assert.equal'
```

### Diagnostic Architecture in Code

```c
typedef enum {
    SEVERITY_NOTE,
    SEVERITY_WARNING,
    SEVERITY_ERROR,
    SEVERITY_FATAL
} cvil_severity_t;

typedef struct cvil_span {
    const char* filename;
    int first_line;
    int first_column;
    int last_line;
    int last_column;
} cvil_span_t;

typedef struct cvil_diagnostic {
    cvil_severity_t severity;
    const char* code;         /* e.g., "CVIL0102" */
    const char* message;      /* e.g., "cannot assign to immutable variable" */
    cvil_span_t span;         /* primary span */
    cvil_span_t label_span;   /* secondary reference span */
    const char* label_msg;    /* e.g., "originally declared here" */
    const char* help_msg;     /* suggestion string */
    const char* suggestion;   /* replacement code */
} cvil_diagnostic_t;

void cvil_emit_diagnostic(const cvil_diagnostic_t* diag, FILE* out);
```

The diagnostics engine will also support `--format=json` for IDE integration, language servers (LSP), and CI pipelines.

---

## 5. AST Lowering & Code Generation Strategy

### 5.1 Dynamic Value Model: Tagged Union vs NaN-Boxing

EVIL values are dynamic and can hold any of 12 distinct types. In C, two representations are viable:

#### Comparison Matrix

| Criteria | Tagged Union (`struct evil_value`) | NaN-Boxing (`uint64_t`) |
| :--- | :--- | :--- |
| **Size** | 16 bytes (8-byte tag/padding + 8-byte payload) | **8 bytes** (entirely fits in 64-bit IEEE-754 NaN space) |
| **Memory Footprint**| Standard | **50% smaller arrays and hash tables** |
| **CPU Cache Efficiency**| Good | **Outstanding** |
| **Portability** | Universal (C89/C99 on 32-bit & 64-bit) | Requires 64-bit architecture with IEEE-754 floats |
| **Implementation Complexity** | Simple, clear C code | Moderate (bitwise bit-masking macros) |

#### Recommendation: Dual-Engine Architecture
1. **Production Mode (Default on x86_64 and AArch64):** **NaN-Boxing**.
   An IEEE-754 double has 52 mantissa bits. If the exponent is all 1s (NaN), the upper mantissa bits encode the `evil_type_t`, and the lower 48 bits encode a pointer or integer payload:
   ```
   [1 sign bit][11 bits exponent (all 1s)][4 bits tag][48 bits pointer / integer]
   ```
2. **Fallback Mode (32-bit or embedded targets):** **16-byte Tagged Union**:
   ```c
   typedef struct evil_value {
       uint32_t type;
       uint32_t flags;
       union {
           double number;
           bool boolean;
           void* reference;
           evil_native_fn_t native_fn;
       } as;
   } evil_value_t;
   ```

---

### 5.2 Scoping, Immutability & Closure Conversion

#### Immutability Enforcement
In EVIL, `val x = 10;` is immutable by default. Any assignment `x = 20;` must be caught at compile-time:
```evil
val x = 10;
x = 20; // CVIL Error: Cannot mutate immutable binding 'x'
```
The transpiler tracks the `is_mutable` attribute on every symbol. If reassignment is attempted, compilation halts with an error. In C code, immutable values that do not escape can be emitted directly as `const evil_value_t x = ...;`.

#### Closures & Upvalue Lifting
EVIL functions can capture local variables from enclosing parent scopes:
```evil
fn make_counter(start) {
    rw val c = start;
    ret fn() -> ++c;
}
```
If a variable is captured by a closure and is mutated (`rw val`), it cannot live on the C stack because the stack frame vanishes upon return.

**CVIL Solution: Heap Upvalue Allocation**
Captured mutable variables are promoted to a heap-allocated `evil_upvalue_t` cell:
```c
typedef struct evil_upvalue {
    evil_value_t* value_ptr; /* points to stack while open, or to 'closed' when closed */
    evil_value_t closed;     /* storage once stack frame exits */
    struct evil_upvalue* next;
} evil_upvalue_t;
```
When `make_counter` runs in generated C:
```c
evil_value_t evil_fn_make_counter(evil_fiber_t* fiber, int argc, evil_value_t* argv) {
    evil_value_t start = (argc > 0) ? argv[0] : EVIL_NIL;
    
    /* Variable 'c' escapes into a closure, allocate Upvalue */
    evil_upvalue_t* upval_c = evil_upvalue_create(start);
    
    /* Create closure with 1 captured upvalue */
    evil_closure_t* closure = evil_closure_create(evil_fn_lambda_1, 1);
    closure->upvalues[0] = upval_c;
    
    return evil_value_closure(closure);
}

evil_value_t evil_fn_lambda_1(evil_fiber_t* fiber, int argc, evil_value_t* argv) {
    evil_closure_t* self_closure = fiber->current_frame->closure;
    evil_upvalue_t* upval_c = self_closure->upvalues[0];
    
    /* ++c */
    evil_value_t current_val = *upval_c->value_ptr;
    evil_value_t incremented = evil_val_add(current_val, EVIL_NUM(1));
    *upval_c->value_ptr = incremented;
    
    return incremented;
}
```

---

### 5.3 Hybrid Tables & Metamethod Dispatch

EVIL tables operate identically to Lua tables: they contain an **array part** (for sequential 0-indexed integer keys) and a **hash part** (for string keys and arbitrary dynamic keys).

```c
typedef struct evil_table_entry {
    evil_value_t key;
    evil_value_t value;
    uint32_t hash;
} evil_table_entry_t;

typedef struct evil_table {
    /* Array part for contiguous numeric indices */
    evil_value_t* array_part;
    uint32_t array_count;
    uint32_t array_capacity;

    /* Hash part for associative keys */
    evil_table_entry_t* hash_entries;
    uint32_t hash_count;
    uint32_t hash_capacity;

    /* Metamethod fast-flags and dispatch */
    uint32_t metamethod_flags;
    struct evil_table* metatable;
    bool is_frozen;
} evil_table_t;
```

#### Metamethod Fast Dispatch
When evaluating an operator (e.g. `a + b`), if either operand is a table:
1. Fast-path check: Check `metamethod_flags & (1 << META_ADD)`.
2. If set, retrieve `__add` from table or its metatable.
3. Invoke the closure with `a` and `b`.

---

### 5.4 Pattern Matching (`by`) and Table Expansion (`with`)

#### Lowering `by` Expressions
An EVIL pattern matching expression:
```evil
val category = by code {
    0 -> "Success",
    1, 2 -> "Warning",
    [ 200, "OK" ] => "HTTP Success",
    else: "Unknown"
};
```
Is desugared by the C code generator into an optimized evaluation block:
- Constant literal arms (`0`, `1`, `2`) are checked via fast equality `evil_val_equal()`.
- Deep matching arms (`=>`) are evaluated with `evil_val_deep_equal()`.
- The `else:` arm acts as the default branch.

#### Lowering `with` Expressions
EVIL's non-destructive table expansion:
```evil
val modified = original with { x: 10, y: 20 };
```
Transpiles directly into:
```c
evil_value_t modified = evil_table_clone_with(original, 2, 
    evil_str_intern("x"), EVIL_NUM(10),
    evil_str_intern("y"), EVIL_NUM(20)
);
```

---

### 5.5 Cooperative Concurrency & Fibers (`yield`)

In the Ceres VM, coroutines run via `yield<target>(args)`. The calling fiber pauses, spawns/switches to a child fiber executing `target(args)`, and resumes when the child fiber yields or returns.

#### Native Implementation in C

Fibers require **stackful coroutines** (each fiber possesses its own call stack so it can yield from arbitrary call depth). CVIL implements this using **Platform Context Switching**:

```
+--------------------------------------------------------------------------------+
|                              FIBER SCHEDULER                                   |
|                                                                                |
|  [Main Fiber] <--- swapcontext() ---> [Child Fiber 1] (Target Chunk)          |
|                                             |                                  |
|                                        yield<delay>()                          |
|                                             |                                  |
|                                             v                                  |
|                                       [Child Fiber 2]                          |
+--------------------------------------------------------------------------------+
```

1. **Linux / macOS / POSIX:** Implemented via `makecontext()` / `swapcontext()` or high-performance assembly context switching (e.g., lightweight `libco` or assembly routines saving callee-saved registers: `rsp`, `rbp`, `rbx`, `r12-r15`).
2. **Windows:** Implemented using Win32 Fiber APIs: `ConvertThreadToFiber()`, `CreateFiber()`, and `SwitchToFiber()`.

```c
typedef struct evil_fiber {
    void* native_stack;
    size_t stack_size;
    evil_coro_context_t context;
    evil_fiber_state_t state;
    
    struct evil_fiber* parent;
    struct evil_fiber* waiting_for;
    
    evil_value_t return_value;
} evil_fiber_t;

evil_value_t evil_yield_call(evil_fiber_t* current, evil_value_t target, int argc, evil_value_t* argv) {
    evil_fiber_t* child = evil_fiber_spawn(target, argc, argv);
    child->parent = current;
    current->waiting_for = child;
    current->state = FIBER_PAUSED;
    
    /* Switch execution context to child fiber */
    evil_coro_switch(&current->context, &child->context);
    
    /* Resumed: extract child return value */
    return child->return_value;
}
```

This guarantees **100% semantic identity** with the Ceres VM fiber model without requiring complex CPS (Continuation-Passing Style) whole-program restructuring!

---

### 5.6 Exception Handling & `retry` Statement

EVIL provides `try { ... } catch(e) { ... }`, `throw expr;`, and `retry;`. The `retry` statement has unique semantics: when executed inside a `catch` block or error handler, it jumps execution back to the beginning of the `try` block to re-execute it.

#### Native Implementation via `setjmp` / `longjmp`

CVIL implements structured exception frames with an explicit `retry` loop:

```c
evil_try_frame_t try_frame;
evil_push_try_frame(fiber, &try_frame);

int try_status = setjmp(try_frame.jump_buffer);

if (try_status == 0 || try_status == EVIL_JMP_RETRY) {
    /* ================= TRY BLOCK ================= */
    /* If retry occurs, setjmp returns EVIL_JMP_RETRY and re-runs this block */
    evil_execute_try_body(...);
    evil_pop_try_frame(fiber);
} else if (try_status == EVIL_JMP_THROW) {
    /* ================ CATCH BLOCK ================ */
    evil_value_t error_val = fiber->current_exception;
    evil_pop_try_frame(fiber);
    
    /* If 'retry;' is encountered inside catch: */
    if (should_retry) {
        evil_push_try_frame(fiber, &try_frame);
        longjmp(try_frame.jump_buffer, EVIL_JMP_RETRY);
    }
}
```

---

## 6. The `libcvil` Native Runtime Library

The runtime library `libcvil` is a lightweight, high-performance static library (`libcvil.a`) or single-header unity file linked with every transpiled EVIL program.

### 6.1 Memory Management & Garbage Collection

EVIL is dynamically typed, and tables can form reference cycles (`table.self = table`). Raw reference counting without a cycle detector would leak memory.

#### GC Strategy for CVIL
1. **Primary Engine: Boehm-Demers-Weiser Conservative GC (`libgc`)**
   - Battle-tested, rock-solid, multi-platform garbage collector.
   - Requires zero compiler-generated stack maps or complex shadow stacks.
   - Integrates via `GC_MALLOC`, `GC_MALLOC_ATOMIC`, and `GC_register_finalizer`.
   - Native objects registered with custom finalizers are cleaned up automatically when collected.
2. **Alternative Embedded Engine: Generational Mark-and-Sweep**
   - For environments where `libgc` is unavailable, CVIL includes a self-contained, compact mark-and-sweep collector with shadow-stack root tracking.

---

### 6.2 Standard Library C Implementations

All Ceres standard library modules in `VirtualMachine/EVIL.Ceres.Runtime/Modules/` are implemented in pure native C:

| Module | Native C Implementation Scope |
| :--- | :--- |
| **`core`** | `assert`, `print`, `println`, `type`, error constructors, equality utilities. |
| **`io`** | Fast buffered I/O, `stdin`/`stdout`/`stderr` wrappers, binary read/write. |
| **`math`** | C `math.h` bindings: `sin`, `cos`, `tan`, `sqrt`, `floor`, `ceil`, `round`, `abs`, `random` (xoshiro256** PRNG), `min`, `max`, `pi`, `e`. |
| **`string`** | UTF-8 compliant string operations: `substring`, `split`, `join`, `trim`, `to_lower`, `to_upper`, `replace`, `format`, length calculation. |
| **`table`** | Table manipulation: `keys`, `values`, `pairs`, `merge`, `freeze`, `is_frozen`, `invert`, shallow and deep clone. |
| **`array`** | High-performance dynamic arrays: `push`, `pop`, `shift`, `unshift`, `slice`, `map`, `filter`, `reduce`, `sort`, `reverse`. |
| **`time`** | High-resolution monotonic timers: `stamp.ms`, `stamp.ns`, `now`, `sleep`. |
| **`fs`** | POSIX / Win32 filesystem operations: `file.exists`, `file.read`, `file.write`, `dir.list`, `dir.create`, `path.combine`, `path.get_filename`. |
| **`convert`**| Base conversion, numeric parsing, string-to-number with strict validation. |
| **`debug`** | Native callstack inspection, memory statistics, GC trigger (`debug.gc.collect()`). |

---

### 6.3 Native C FFI (Foreign Function Interface)

To replace CLR type reflection with a superior native integration model, CVIL provides a declarative C API:

```c
/* Example: Exposing a native C physics engine to EVIL scripts */
#include "cvil.h"

static evil_value_t native_raycast(evil_fiber_t* fiber, int argc, evil_value_t* argv) {
    double origin_x = evil_to_number(argv[0]);
    double origin_y = evil_to_number(argv[1]);
    
    HitResult hit = Physics_Raycast(origin_x, origin_y);
    
    evil_table_t* result = evil_table_create(0, 2);
    evil_table_set_str(result, "hit", evil_bool(hit.occurred));
    evil_table_set_str(result, "dist", evil_number(hit.distance));
    return evil_val_table(result);
}

void register_physics_module(evil_runtime_t* rt) {
    evil_table_t* mod = evil_table_create(0, 4);
    evil_table_set_str(mod, "raycast", evil_val_native_fn(native_raycast));
    evil_register_global(rt, "physics", evil_val_table(mod));
}
```

---

## 7. C Compiler Driver & Executable Generation

The CVIL CLI executable (`cvil`) orchestrates transpilation and C compilation:

```bash
# Compile and emit native executable
cvil input.vil -o my_program

# Compile with optimization level 3 and strip symbols
cvil input.vil -O3 --strip -o my_program

# Emit C source code only (transpiled output inspection)
cvil input.vil --emit-c -o output.c

# Run in test runner mode (executes and validates test assertions)
cvil --test tests/000_regression.vil tests/001_truth.vil
```

### Compiler Discovery Matrix
CVIL automatically discovers available host C compilers:
1. `clang` (preferred for diagnostics and optimization)
2. `gcc` (industry standard)
3. `cl.exe` (Microsoft Visual C++ on Windows)
4. `tcc` (Tiny C Compiler for ultra-fast compilation under 50ms)

---

## 8. Implementation Roadmap & Milestones

The project is structured into **6 iterative milestones**, each verifiable against existing Ceres test suites.

```
M1: Runtime Core (libcvil)
    |
    v
M2: Transpiler Frontend & Diagnostic Engine
    |
    v
M3: Statements, Functions, Closures & Codegen
    |
    v
M4: Advanced Expressions & Metatable System
    |
    v
M5: Fiber Concurrency & Exception Handling
    |
    v
M6: Complete Language Test Suite Validation & Release
```

### Milestone Breakdown

#### Milestone 1: Core Runtime Library (`libcvil`)
- [ ] Implement `DynamicValue` representation (NaN-boxing on x86_64, Tagged Union fallback).
- [ ] Implement `evil_string_t` with string interning and SSO.
- [ ] Implement `evil_array_t` with geometric buffer growth.
- [ ] Implement `evil_table_t` with dual array/hash parts and Robin Hood hashing.
- [ ] Integrate Boehm GC (`libgc`) allocator.
- [ ] Unit tests for runtime data structures.

#### Milestone 2: Transpiler Frontend & Diagnostic Engine
- [ ] Connect Flex scanner ([evil.l](file:///workbench/code/dotnet/EVIL/Docs/Syntax/evil.l)) and Bison parser ([evil.y](file:///workbench/code/dotnet/EVIL/Docs/Syntax/evil.y)) to build AST nodes.
- [ ] Build Symbol Table and Lexical Scope Graph.
- [ ] Implement immutable `val` vs mutable `rw val` checking.
- [ ] Build User-Friendly Diagnostic Formatter (ANSI color, caret pointers, error codes, fix suggestions).
- [ ] Add JSON output mode (`--format=json`).

#### Milestone 3: Basic Statements, Functions & C Codegen
- [ ] Implement basic statement code generator (`val`, `if`/`elif`/`else`, `while`, `do..while`, `for`, `each`).
- [ ] Implement global and local function declarations.
- [ ] Implement Closure Analysis and Upvalue lifting into heap cells.
- [ ] Implement C code emitter with `#line` source mapping directives.
- [ ] Compiler driver invoking `gcc`/`clang` to emit working native executables.

#### Milestone 4: Advanced Expressions & Metatable System
- [ ] Lower pattern matching (`by`) expressions with shallow and deep equality arms.
- [ ] Lower table expansion (`with`) expressions.
- [ ] Implement string interpolation codegen with dynamic string builders.
- [ ] Implement safe navigation (`?.`, `?[`) short-circuiting.
- [ ] Implement full metatable operator dispatch (all 32 metamethods).
- [ ] Validate against `009_tables.vil`, `023_by_expr.vil`, `024_override.vil`, `029_with_expr.vil`.

#### Milestone 5: Fibers, Exceptions & Concurrency
- [ ] Implement native stackful coroutine context switching (`swapcontext` / Win32 Fibers).
- [ ] Implement `yield<target>(args)` and scheduler coordination.
- [ ] Implement `try`, `catch`, `throw`, and `retry` via `setjmp`/`longjmp`.
- [ ] Port `CoreModule`, `IoModule`, `MathModule`, `StringModule`, `TimeModule`, and `FsModule` to native C.
- [ ] Validate against `025_try.vil` and `099_yield.vil`.

#### Milestone 6: Complete Test Suite Validation & Optimization
- [ ] Run CVIL against all 36 test files in `VirtualMachine/Tests/EVIL.Ceres.LanguageTests/tests/*.vil`.
- [ ] Achieve **100% test pass rate** matching the C# Ceres engine.
- [ ] Benchmarking against Ceres bytecode VM (latency, memory footprint, binary size).
- [ ] Package standalone `cvil` CLI compiler binary.

---

## 9. Risk Assessment & Mitigation Matrix

| Risk Factor | Impact | Likelihood | Mitigation Strategy |
| :--- | :---: | :---: | :--- |
| **Stackful Fiber Portability Across OSes** | High | Medium | Use conditional compilation: POSIX `ucontext_t` on Linux/macOS, Windows Fibers on Windows, with a fallback to lightweight assembly context switches (`boost.context` assembly snippets). |
| **Recursive Metamethod Stack Overflow** | Medium | Low | Maintain a recursion depth counter in `evil_fiber_t` (default limit: 1024 calls) and raise an EVIL runtime error if exceeded. |
| **`retry` Loop State Corruption** | Medium | Medium | Re-initialize local variables declared within the `try` block scope upon `longjmp(..., EVIL_JMP_RETRY)` so execution re-starts from clean slate. |
| **C Compiler Availability on User Machines** | High | Low | Bundle TinyCC (`libtcc`) as an optional built-in zero-dependency compiler fallback if no host GCC or Clang is detected. |
| **AOT Dynamic Module Loading (`require`)** | Medium | Medium | Implement AOT static bundle resolution: during compilation, scan for static `require("path.vil")` and compile all referenced modules into a single unified binary. |
