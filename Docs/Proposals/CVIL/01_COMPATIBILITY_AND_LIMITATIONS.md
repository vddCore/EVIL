# CVIL Technical Analysis: Compatibility & Native Boundaries

This document provides an exhaustive assessment of EVIL language features when translating from the C# / Ceres virtual machine to native C (ANSI C99 / C11).

---

## 1. What is Strictly Impossible in Native C

The EVIL reference implementation (`Ceres`) runs on the .NET 9 Common Language Runtime (CLR). Certain features rely directly on the managed CLR substrate and cannot exist in a standalone native C executable without embedding a managed runtime:

### 1.1 CLR Dynamic Reflection & Type Loading

- **Reference Implementation:** The C# runtime can perform dynamic type lookups via `System.Type.GetType()`, load external `.dll` assemblies at runtime, and dynamically instantiate arbitrary CLR types.
- **Native C Reality:** C has no runtime metadata, no reflection tables, no virtual execution environment, and no class loaders. C types are erased at compile-time.
- **Conclusion:** **Impossible.** Programs expecting to query .NET type definitions or inspect arbitrary C# assembly classes cannot do so in native C.

### 1.2 Direct Managed Object Interoperability

- **Reference Implementation:** `DynamicValueType.NativeObject` wraps any arbitrary `System.Object`, and `DynamicValueType.NativeFunction` wraps a .NET delegate `Func<DynamicValue[], DynamicValue>`.
- **Native C Reality:** Native C pointers (`void*`) cannot point to managed garbage-collected objects on a CLR heap without pinning and active CLR hosting.
- **Conclusion:** **Impossible directly.** Replaced with a native C Foreign Function Interface (FFI) using opaque pointers and function pointer callbacks (detailed in Section 2).

### 1.3 Unconstrained In-Memory `evil.compile` without Compiler

- **Reference Implementation:** EVIL code can call `evil.compile(code_string)` at runtime, generating an in-memory `Chunk` and immediately executing it.
- **Native C Reality:** Once C code is compiled into native machine instructions (`ELF` or `PE`), a standalone binary cannot compile new source strings into machine code unless:
  1. A C compiler toolchain (e.g. `gcc`, `clang`, or embedded `libtcc`) is present on the host system to compile and dynamically link a `.so`/`.dll` via `dlopen`.
  2. An embedded bytecode interpreter or AST evaluator is bundled alongside the compiled native binary.
- **Conclusion:** Pure AOT compiled binaries cannot execute arbitrary newly generated source strings without bundling an evaluation engine.

---

## 2. Adapted Native Equivalents

For every boundary where native C differs from the CLR, CVIL specifies a native C architecture that preserves language semantics:

### 2.1 Native Objects & Native Functions (The CVIL FFI)

In Ceres, `NativeObject` and `NativeFunction` represent host extensions. In CVIL, these are cleanly adapted to standard C ABIs:

```c
/* Native function pointer matching EVIL calling conventions */
typedef evil_value_t (*evil_native_fn_t)(
    struct evil_fiber* fiber, 
    int argc, 
    evil_value_t* argv
);

/* Type descriptor for native objects */
typedef struct evil_native_type {
    const char* type_name;                    /* e.g., "PhysicsEngine.Body" */
    uint32_t type_id;                         /* Fast unique numeric ID */
    void (*destructor)(void* payload);        /* Finalizer callback */
    evil_value_t (*to_string)(void* payload); /* Custom stringifier */
} evil_native_type_t;

/* Native object wrapper */
typedef struct evil_native_obj {
    const evil_native_type_t* descriptor;
    void* payload;                            /* User data pointer */
} evil_native_obj_t;
```

#### Language Level Compatibility:
The EVIL expression:
```evil
assert(my_body is NativeObject "PhysicsEngine.Body");
```
Is evaluated in C as:
```c
evil_bool(evil_val_is_native_obj(my_body, "PhysicsEngine.Body"));
```
This preserves the exact syntax and behavior tested in `012_type.vil`.

---

### 2.2 Module Loading: Static Linking vs Dynamic Shared Objects

In the Ceres VM, `require("module.vil")` and `dofile("module.vil")` read file text and dynamically invoke `evil.compile`.

CVIL provides two compilation strategies:

#### Strategy A: Whole-Program Static Compilation (Default & Recommended)
During AOT compilation, the CVIL transpiler analyzes static calls to `require()` and `dofile()`. All referenced `.vil` files are parsed, validated, and transpiled into the unified C translation unit:
- Modules become static initializers.
- Re-requiring returns the cached module table from the global module registry.
- Zero runtime disk I/O, zero compilation overhead, maximum optimization and dead-code elimination.

#### Strategy B: Dynamic Native Modules (`.so` / `.dll`)
If dynamic runtime loading is explicitly needed:
- External modules are pre-compiled into native shared objects (`.so` on Linux, `.dylib` on macOS, `.dll` on Windows).
- At runtime, `require("plugin")` uses `dlopen()` / `LoadLibrary()` and resolves the module entry point `evil_module_init()`.

---

## 3. Preserved Language Capabilities

All standard EVIL constructs defined in `Core/EVIL.Grammar` and validated in `VirtualMachine/Tests/EVIL.Ceres.LanguageTests/tests/*.vil` are fully preserved:

| EVIL Language Feature | Ceres Implementation | CVIL Native C Implementation | Preserved? |
| :--- | :--- | :--- | :---: |
| **Read-Only / Read-Write Binding** (`val` vs `rw val`) | VM opcode checks | Compile-time semantic analyzer enforcement | **YES** |
| **Lexical Closures & Upvalues** | Runtime `ClosureContext` & `Upvalue` | Heap-allocated `evil_upvalue_t` cells | **YES** |
| **Tables (Array + Hash Parts)** | `Table` class with array and dictionary | C hybrid struct with Robin Hood hash table | **YES** |
| **All 32 Metamethods** | Metamethod table lookups | C fast-flag bitmasks and function dispatch | **YES** |
| **Pattern Matching** (`by`) | Bytecode jumps | Lowered C conditional decision trees | **YES** |
| **Table Expansion** (`with`) | VM shallow copy + merge | `evil_table_clone_with()` C runtime | **YES** |
| **Cooperative Coroutines** (`yield<T>(args)`) | VM fibers | Stackful native coroutines (`ucontext_t` / Win32) | **YES** |
| **Exceptions & `retry`** | Managed exception unwind | `setjmp` / `longjmp` scope stack | **YES** |
| **String Interpolation** | AST interpolation builder | Dynamic C string builder | **YES** |
| **Safe Navigation** (`?.`, `?[`) | Null-conditional opcodes | Short-circuiting C ternary checks | **YES** |
| **Variadic Forwarding** (`*`) | Stack frame unwrapping | Argument slice passing | **YES** |
