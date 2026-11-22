# CVIL Technical Specification: Implementation Roadmap & Milestones

This document defines the phased implementation strategy, engineering tasks, verification criteria, and milestone delivery schedule for building the CVIL transpiler.

---

## 1. Project Organization & Repository Layout

The transpiler project will be structured in `Transpiler/CVIL/`:

```
Transpiler/CVIL/
├── CMakeLists.txt                # Root build system
├── Makefile                      # Quick build & test driver
├── include/                      # Public compiler and runtime headers
│   ├── cvil.h                    # Master include
│   ├── cvil_ast.h                # AST data structures
│   ├── cvil_diag.h               # Diagnostic reporting engine
│   ├── cvil_runtime.h            # Runtime library API
│   └── cvil_value.h              # DynamicValue representation
├── src/
│   ├── driver/                   # CLI front-end (cvil main)
│   │   └── main.c
│   ├── frontend/                 # Parser & AST construction
│   │   ├── evil.l                # (Symlinked or copied from Docs/Syntax/evil.l)
│   │   ├── evil.y                # (Symlinked or copied from Docs/Syntax/evil.y)
│   │   ├── ast.c
│   │   └── scope.c
│   ├── analysis/                 # Semantic validation
│   │   ├── immutability.c
│   │   ├── closures.c
│   │   └── diagnostics.c
│   ├── codegen/                  # C code emission
│   │   ├── emit_c.c
│   │   ├── emit_expr.c
│   │   ├── emit_stmt.c
│   │   └── emit_pattern.c
│   └── runtime/                  # libcvil C runtime library
│       ├── alloc.c               # Boehm GC / Memory wrappers
│       ├── value.c               # Value operations & type checks
│       ├── table.c               # Hybrid table & metamethod dispatch
│       ├── array.c               # Dynamic arrays
│       ├── string.c              # Strings & builder
│       ├── fiber.c               # Stackful coroutine context switching
│       ├── exception.c           # setjmp / longjmp / retry frames
│       └── modules/              # Ported standard library modules
│           ├── mod_core.c
│           ├── mod_io.c
│           ├── mod_math.c
│           ├── mod_string.c
│           ├── mod_table.c
│           ├── mod_array.c
│           ├── mod_time.c
│           └── mod_fs.c
└── tests/                        # Verification runner
    ├── runner.c                  # Test harness executing all 36 .vil tests
    └── test_helpers.c
```

---

## 2. Milestone Execution Plan

### Milestone 1: Runtime Foundation (`libcvil`)
**Goal:** Build and verify all core data structures, memory allocators, and value operations in pure C.

- [ ] Task 1.1: Implement `evil_value_t` NaN-boxing macros and fallback tagged-union.
- [ ] Task 1.2: Implement `evil_string_t` with FNV-1a hashing and string interning pool.
- [ ] Task 1.3: Implement `evil_array_t` with geometric expansion.
- [ ] Task 1.4: Implement `evil_table_t` with sequential array part and Robin Hood hash part.
- [ ] Task 1.5: Implement all 32 metamethod dispatch hooks (`__add`, `__invoke`, `__index`, etc.).
- [ ] Task 1.6: Integrate Boehm GC (`libgc`) allocator and finalizer registry.
- [ ] Task 1.7: Author C unit tests verifying equality, hashing, metamethods, and memory stability.

**Exit Criteria:** All C runtime unit tests pass with zero memory leaks under Valgrind / ASan.

---

### Milestone 2: Transpiler Frontend & Diagnostic Engine
**Goal:** Ingest `.vil` source, build typed AST, perform scope analysis, and report user-friendly errors.

- [ ] Task 2.1: Integrate `evil.l` and `evil.y` to produce AST structures.
- [ ] Task 2.2: Implement lexical scope graph (`cvil_scope_t`) and symbol resolution.
- [ ] Task 2.3: Implement immutability check (`val` vs `rw val`).
- [ ] Task 2.4: Implement precision diagnostic engine with ANSI colored gutters, caret indicators, and error codes.
- [ ] Task 2.5: Add fuzzy identifier suggestion (Levenshtein distance).
- [ ] Task 2.6: Add `--format=json` diagnostic output flag.

**Exit Criteria:** The frontend parses all 38 `.vil` files; detects invalid variable mutations with precise diagnostic errors.

---

### Milestone 3: Statements, Functions, Closures & Initial Codegen
**Goal:** Transpile basic EVIL scripts into runnable C and native executables.

- [ ] Task 3.1: Implement C code generator for variables (`val`, `rw val`), control flow (`if`, `while`, `do..while`, `for`, `each`).
- [ ] Task 3.2: Implement global and local function declarations.
- [ ] Task 3.3: Implement closure analysis, lambda lifting, and heap upvalue cell allocation (`evil_upvalue_t`).
- [ ] Task 3.4: Emit `#line` directives mapping C lines back to `.vil` source lines.
- [ ] Task 3.5: Build `cvil` CLI driver invoking host compiler (`gcc` or `clang`).
- [ ] Task 3.6: Validate against:
  - `000_regression.vil`
  - `001_truth.vil`
  - `002_constants.vil`
  - `005_assignment_operators.vil`
  - `006_while_loop.vil`
  - `007_do_while_loop.vil`
  - `008_for_loop.vil`
  - `017_scope.vil`
  - `019_closures.vil`

**Exit Criteria:** Basic loops, scopes, arithmetic, and closures compile to native binaries and pass assertions.

---

### Milestone 4: Advanced Expressions, Tables & Metamethods
**Goal:** Complete support for EVIL's table system, metamethods, pattern matching, and syntax extensions.

- [ ] Task 4.1: Lower `by` pattern-matching expressions into optimized C decision trees.
- [ ] Task 4.2: Lower `with` table expansion into `evil_table_clone_with()`.
- [ ] Task 4.3: Lower safe navigation (`?.`, `?[`) and null-coalescing (`??`).
- [ ] Task 4.4: Lower string interpolation chains (`"${...}"`).
- [ ] Task 4.5: Complete table literal construction (`{ [key] => val, method: fn ... }`).
- [ ] Task 4.6: Validate against:
  - `003_basic_operators.vil`
  - `004_ternary_operator.vil`
  - `009_tables.vil`
  - `010_invoke.vil`
  - `011_array.vil`
  - `014_extra_arguments.vil`
  - `016_parameter_initializers.vil`
  - `022_self.vil`
  - `023_by_expr.vil`
  - `024_override.vil`
  - `028_conditional_indexer.vil`
  - `029_with_expr.vil`

**Exit Criteria:** All complex expression, table metamethod, and pattern tests pass natively.

---

### Milestone 5: Concurrency, Exceptions & Standard Library
**Goal:** Deliver fiber coroutines, exceptions (`try`/`catch`/`throw`/`retry`), and native standard library modules.

- [ ] Task 5.1: Implement native stackful context switching (`ucontext_t` / Win32 Fibers).
- [ ] Task 5.2: Implement `yield<target>(args)` scheduler coordination and return value retrieval.
- [ ] Task 5.3: Implement `setjmp` / `longjmp` exception frames with `retry` loop.
- [ ] Task 5.4: Port runtime modules to C: `core`, `io`, `math`, `string`, `table`, `array`, `time`, `fs`, `convert`.
- [ ] Task 5.5: Validate against:
  - `012_type.vil`
  - `013_if_statement.vil`
  - `015_nontrivial_tests.vil`
  - `018_each_loop.vil`
  - `025_try.vil`
  - `026_fn_targeted.vil`
  - `027_fn_indexed.vil`
  - `099_yield.vil`

**Exit Criteria:** Cooperative fibers and retry-enabled exceptions pass test assertions identically to Ceres.

---

### Milestone 6: Verification, Optimization & Packaging
**Goal:** Attain 100% test suite parity, optimize binary size, and package release.

- [ ] Task 6.1: Run full verification suite across all 36 test files in `VirtualMachine/Tests/EVIL.Ceres.LanguageTests/tests/*.vil` + runtime builtins.
- [ ] Task 6.2: Achieve **38 / 38 (100%) test pass rate**.
- [ ] Task 6.3: Implement `--strip` and `-O3` optimizations (measuring binary footprint, execution speed vs Ceres VM).
- [ ] Task 6.4: Add static AOT module bundling for `require()` and `dofile()`.
- [ ] Task 6.5: Document CLI usage, C FFI integration guide, and publish final release notes.

**Exit Criteria:** All tests pass; standalone executable generation verified on Linux, macOS, and Windows.
