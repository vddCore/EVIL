# CVIL Technical Specification: Transpiler Pipeline & Code Generation

This document details the AST transformations, closure conversion algorithms, lowering passes, C code emission rules, and the precision diagnostic reporting engine.

---

## 1. Compiler Pipeline Overview

```
                      +-----------------------------+
                      |      EVIL Source Code       |
                      |          (*.vil)            |
                      +--------------+--------------+
                                     |
                                     v
                      +-----------------------------+
                      |       Flex Lexer (evil.l)   |
                      |      Bison Parser (evil.y)  |
                      +--------------+--------------+
                                     |
                                     v
                      +-----------------------------+
                      |     Concrete AST Generation |
                      +--------------+--------------+
                                     |
                                     v
                      +-----------------------------+
                      |  Scope & Upvalue Analysis   |  <--- Immutability ('val' vs 'rw val')
                      +--------------+--------------+       Free Variable / Escape Analysis
                                     |
                                     v
                      +-----------------------------+
                      |    AST Lowering & Desugar   |  <--- Desugar: 'by', 'with', '?.', '?[',
                      +--------------+--------------+                String Interpolation
                                     |
                                     v
                      +-----------------------------+
                      |    Closure Conversion       |  <--- Lift Lambdas to C Functions
                      +--------------+--------------+       Generate Upvalue Env Structs
                                     |
                                     v
                      +-----------------------------+
                      |      C99 Code Generator     |  <--- Emit #line source mappings,
                      +--------------+--------------+       constant pools, C prototypes
                                     |
                                     v
                      +-----------------------------+
                      |     Transpiled C Source     |
                      |          (*.cvil.c)         |
                      +--------------+--------------+
                                     |
                                     v
                      +-----------------------------+
                      |   C Compiler (GCC / Clang)  |
                      |   + Static libcvil.a + GC   |
                      +--------------+--------------+
                                     |
                                     v
                      +-----------------------------+
                      |  Standalone Native Binary   |
                      +-----------------------------+
```

---

## 2. Semantic Analysis & Scoping

### 2.1 Lexical Scope Hierarchy

Every block statement `{ ... }`, function body, and loop body opens a new `cvil_scope_t`:

```c
typedef struct cvil_symbol {
    const char* name;
    bool is_mutable;       /* True if declared with 'rw val', false for 'val' */
    bool is_captured;      /* True if captured by any nested closure */
    bool is_parameter;     /* True if function parameter */
    int slot_index;        /* Local stack slot index */
    cvil_span_t decl_span; /* Source location of declaration */
} cvil_symbol_t;

typedef struct cvil_scope {
    struct cvil_scope* parent;
    struct cvil_function* owner_fn;
    cvil_symbol_t* symbols;
    uint32_t symbol_count;
    bool is_function_boundary;
} cvil_scope_t;
```

### 2.2 Immutability Check
When an assignment node `target = expr` or `target += expr` is encountered:
1. Look up `target` in the scope hierarchy.
2. If `target` is found and `!symbol->is_mutable`:
3. Immediately emit a compile-time error:
   ```text
   error[CVIL0102]: cannot assign to immutable binding 'x'
     --> script.vil:10:3
      |
    9 |   val x = 42;
      |       - 'x' was declared immutable here
   10 |   x = 50;
      |   ^ cannot assign to immutable binding
      |
      = help: declare variable as mutable using 'rw val x = 42;'
   ```

---

## 3. Closure Conversion & Lambda Lifting

In EVIL, functions can be declared anywhere and can capture local variables from outer scopes. Because ANSI C does not support nested functions with lexical scoping, CVIL performs **Closure Conversion and Lambda Lifting**:

### Step 1: Upvalue Identification (Escape Analysis)
During scope analysis, when an inner function accesses a symbol `x` declared in an ancestor scope:
1. `x` is marked as `is_captured = true`.
2. The inner function adds `x` to its list of captured upvalues.
3. If multiple nested functions capture `x`, they all reference the same shared `evil_upvalue_t` cell.

### Step 2: Lambda Lifting
Every anonymous lambda or nested local function is lifted to a top-level C function with a mangled unique name:
```c
evil_value_t evil_fn_<parent>_lambda_<id>(evil_fiber_t* fiber, int argc, evil_value_t* argv);
```

### Step 3: Closure Instantiation
At the point of definition in the parent function, an `evil_closure_t` object is allocated, storing:
1. The C function pointer.
2. An array of pointers to `evil_upvalue_t` cells.

---

## 4. AST Desugaring Passes

### 4.1 Pattern Matching (`by`) Desugaring

EVIL source:
```evil
val res = by x {
    0 -> "zero",
    1, 2 -> "small",
    [ 10, "ten" ] => "deep match",
    else: "other"
};
```

Lowers into:
```c
evil_value_t __res;
evil_value_t __match_val = x;

if (evil_val_equal(__match_val, EVIL_NUM(0))) {
    __res = evil_str_intern("zero");
} else if (evil_val_equal(__match_val, EVIL_NUM(1)) || 
           evil_val_equal(__match_val, EVIL_NUM(2))) {
    __res = evil_str_intern("small");
} else if (evil_val_deep_equal(__match_val, __const_array_10_ten)) {
    __res = evil_str_intern("deep match");
} else {
    __res = evil_str_intern("other");
}
```

---

### 4.2 Table Expansion (`with`) Desugaring

EVIL source:
```evil
val new_tbl = base with { color: "red", size: 42 };
```

Lowers into:
```c
evil_value_t new_tbl = evil_table_clone_with(
    base, 
    2, 
    evil_str_intern("color"), evil_str_intern("red"),
    evil_str_intern("size"), EVIL_NUM(42)
);
```

---

### 4.3 Safe Navigation (`?.` and `?[`) Desugaring

EVIL source:
```evil
val item = a?.b?.c?[0];
```

Lowers into a short-circuiting temporary evaluation chain:
```c
evil_value_t __t1 = a;
evil_value_t __t2 = evil_is_nil(__t1) ? EVIL_NIL : evil_table_get_prop(__t1, evil_str_intern("b"));
evil_value_t __t3 = evil_is_nil(__t2) ? EVIL_NIL : evil_table_get_prop(__t2, evil_str_intern("c"));
evil_value_t item = evil_is_nil(__t3) ? EVIL_NIL : evil_index_get(__t3, EVIL_NUM(0));
```

---

### 4.4 String Interpolation Desugaring

EVIL source:
```evil
val msg = "User ${user.name} logged in at ${time.now()} with IP $ip";
```

Lowers into:
```c
evil_strbuilder_t* __sb = evil_strbuilder_create();
evil_strbuilder_append_str(__sb, "User ");
evil_strbuilder_append_val(__sb, evil_table_get_prop(user, evil_str_intern("name")));
evil_strbuilder_append_str(__sb, " logged in at ");
evil_strbuilder_append_val(__sb, time_now(fiber, 0, NULL));
evil_strbuilder_append_str(__sb, " with IP ");
evil_strbuilder_append_val(__sb, ip);
evil_value_t msg = evil_strbuilder_finish(__sb);
```

---

## 5. C Code Generation & Source Mapping

### 5.1 Debugger Source Mapping (`#line` Directives)

To allow standard native debuggers (`gdb`, `lldb`) and profiling tools (`valgrind`, `perf`) to reference the original `.vil` source code directly, CVIL emits `#line` directives before every statement:

```c
#line 42 "tests/003_basic_operators.vil"
evil_value_t a = evil_val_add(x, y);

#line 43 "tests/003_basic_operators.vil"
if (!evil_to_bool(evil_val_equal(a, EVIL_NUM(21.37)))) {
    evil_throw_assertion_failure("tests/003_basic_operators.vil", 43);
}
```
When an assertion fails, segmentation fault occurs, or breakpoint is hit, GDB displays `tests/003_basic_operators.vil:42` instead of the generated C file!

---

### 5.2 Emitted Program Structure

Generated C output follows a standardized, readable layout:

```c
/* ========================================================================= */
/* 1. Header Includes & Runtime Definitions                                  */
/* ========================================================================= */
#include "cvil_runtime.h"

/* ========================================================================= */
/* 2. Constant Pool Declarations                                             */
/* ========================================================================= */
static evil_string_t* _str_const_0; /* "Hello, world!" */
static evil_string_t* _str_const_1; /* "name" */

/* ========================================================================= */
/* 3. Forward Declarations of Lifted Functions                               */
/* ========================================================================= */
static evil_value_t _fn_calculate_total(evil_fiber_t* fiber, int argc, evil_value_t* argv);
static evil_value_t _fn_lambda_1(evil_fiber_t* fiber, int argc, evil_value_t* argv);

/* ========================================================================= */
/* 4. Function Implementations                                               */
/* ========================================================================= */
static evil_value_t _fn_calculate_total(evil_fiber_t* fiber, int argc, evil_value_t* argv) {
    /* Generated C body */
}

/* ========================================================================= */
/* 5. Top-Level Entrypoint (main)                                            */
/* ========================================================================= */
int main(int argc, char** argv) {
    /* Initialize Boehm GC */
    GC_INIT();
    
    /* Initialize CVIL Runtime Environment */
    evil_runtime_t* rt = evil_runtime_create();
    evil_runtime_init_modules(rt, argc, argv);
    
    /* Initialize Constant Pool */
    _str_const_0 = evil_str_intern("Hello, world!");
    _str_const_1 = evil_str_intern("name");
    
    /* Run Top-Level Script in Main Fiber */
    evil_fiber_t* main_fiber = rt->main_fiber;
    evil_execute_toplevel(main_fiber);
    
    return 0;
}
```

---

## 6. Precision Diagnostic & Error Reporting Engine

Compiler diagnostics must be clear, precise, and helpful.

### 6.1 Diagnostic Taxonomy

| Error Code Range | Category | Examples |
| :--- | :--- | :--- |
| `CVIL0001` - `CVIL0099` | Lexical Errors | Unterminated string, invalid escape sequence, malformed number |
| `CVIL0100` - `CVIL0199` | Syntax Errors | Unexpected token, missing closing brace, invalid attribute arg |
| `CVIL0200` - `CVIL0299` | Scope & Mutability | Mutating immutable `val`, variable re-declaration, unknown symbol |
| `CVIL0300` - `CVIL0399` | Language Semantics | Invalid metamethod name, yield outside fiber, illegal break/skip |
| `CVIL0400` - `CVIL0499` | Codegen & C Toolchain| Host C compiler not found, linker error, ABI mismatch |

### 6.2 Visual Output Rendering

```text
error[CVIL0201]: unknown identifier 'prntln'
  --> src/main.vil:12:3
   |
11 |   val message = "Done!";
12 |   prntln(message);
   |   ^^^^^^ unknown function or variable
   |
   = help: did you mean 'println'?
   = note: 'println' is part of the standard core library
```

#### Diagnostic Implementation Details:
1. **Source Line Caching:** The compiler caches source lines in memory.
2. **Gutter Formatting:** Dynamic width line numbers with vertical bars `|`.
3. **ANSI Color Support:** Red for errors, yellow for warnings, cyan for notes, green for suggestions. Automatically disabled if output stream is not a TTY.
4. **Fuzzy Name Matching:** Uses Levenshtein distance $\le 2$ against in-scope symbols to suggest typos.
