# CVIL Technical Specification: The `libcvil` Runtime Library

This document specifies the internal architecture, memory layout, algorithms, and data structures of the native C runtime library (`libcvil`).

---

## 1. Value Representation (`DynamicValue`)

EVIL is dynamically typed with 12 distinct value types: `Nil`, `Number`, `String`, `Boolean`, `Table`, `Array`, `Fiber`, `Function` (`Chunk`), `Error`, `Type`, `NativeFunction`, and `NativeObject`.

### 1.1 NaN-Boxing (64-bit Systems - Default Engine)

On 64-bit architectures, standard IEEE-754 double-precision floating-point numbers occupy 64 bits. A NaN (Not-a-Number) is defined as any value where all 11 exponent bits are set to 1 and the mantissa is non-zero:

```
[Sign 1 bit][Exponent 11 bits (all 1s)][Quiet NaN bit 1][Tag 3 bits][Payload 48 bits]
```

- Any normal number is stored directly as its double-precision representation.
- Pointers on x86_64 and AArch64 use at most 48 bits of virtual address space, leaving the upper 16 bits free to encode type tags.

```c
typedef uint64_t evil_value_t;

#define QNAN_MASK         ((uint64_t)0x7FFC000000000000ULL)
#define SIGN_BIT_MASK     ((uint64_t)0x8000000000000000ULL)
#define TAG_MASK          ((uint64_t)0x0003000000000000ULL)
#define PAYLOAD_MASK      ((uint64_t)0x0000FFFFFFFFFFFFULL)

#define TAG_NIL           ((uint64_t)0x0000000000000000ULL)
#define TAG_BOOLEAN       ((uint64_t)0x0001000000000000ULL)
#define TAG_TYPE_CODE     ((uint64_t)0x0002000000000000ULL)
#define TAG_OBJECT_PTR    ((uint64_t)0x0003000000000000ULL)

/* Constant Constructors */
#define EVIL_NIL          (QNAN_MASK | TAG_NIL)
#define EVIL_TRUE         (QNAN_MASK | TAG_BOOLEAN | 1ULL)
#define EVIL_FALSE        (QNAN_MASK | TAG_BOOLEAN | 0ULL)

static inline bool evil_is_number(evil_value_t v) {
    return (v & QNAN_MASK) != QNAN_MASK;
}

static inline double evil_as_number(evil_value_t v) {
    union { uint64_t u; double d; } cast = { v };
    return cast.d;
}

static inline evil_value_t evil_number(double d) {
    union { double d; uint64_t u; } cast = { d };
    /* Canonicalize standard NaNs */
    if ((cast.u & QNAN_MASK) == QNAN_MASK) return (QNAN_MASK | 0x1ULL);
    return cast.u;
}
```

### 1.2 Tagged Union (32-bit & Fallback Engine)

For 32-bit platforms or microcontrollers where NaN-boxing is non-trivial:

```c
typedef enum {
    EVIL_TYPE_NIL = 0,
    EVIL_TYPE_NUMBER,
    EVIL_TYPE_STRING,
    EVIL_TYPE_BOOLEAN,
    EVIL_TYPE_TABLE,
    EVIL_TYPE_ARRAY,
    EVIL_TYPE_FIBER,
    EVIL_TYPE_CHUNK,
    EVIL_TYPE_ERROR,
    EVIL_TYPE_TYPE,
    EVIL_TYPE_NATIVE_FUNCTION,
    EVIL_TYPE_NATIVE_OBJECT
} evil_type_t;

typedef struct evil_value {
    evil_type_t type;
    uint32_t flags;
    union {
        double number;
        bool boolean;
        evil_type_t type_code;
        struct evil_string* string;
        struct evil_table* table;
        struct evil_array* array;
        struct evil_fiber* fiber;
        struct evil_closure* closure;
        struct evil_error* error;
        evil_native_fn_t native_fn;
        struct evil_native_obj* native_obj;
    } as;
} evil_value_t;
```

---

## 2. Memory Management: Garbage Collection

Dynamic tables and closures in EVIL can contain cyclic references (`a.parent = b; b.child = a;`). Reference counting alone is insufficient without complex cycle detection.

### 2.1 Primary Allocator: Boehm-Demers-Weiser Collector (`libgc`)

CVIL standardizes on `libgc`:
1. **Zero Stack Map Overhead:** Runs conservatively across the stack, examining machine registers and local stack frames for pointer references.
2. **Deterministic Destructors:** When native objects (`evil_native_obj_t`) are collected, `GC_register_finalizer` executes their C cleanup callbacks.
3. **Atomic Allocations:** Non-pointer buffers (such as raw string text or numeric array memory) use `GC_MALLOC_ATOMIC` to skip scanning, maximizing collection throughput.

```c
#include <gc.h>

void* evil_alloc(size_t size) {
    void* ptr = GC_MALLOC(size);
    if (!ptr) {
        fprintf(stderr, "Out of memory in EVIL runtime allocation (%zu bytes)\n", size);
        abort();
    }
    return ptr;
}

void* evil_alloc_atomic(size_t size) {
    return GC_MALLOC_ATOMIC(size);
}

void evil_register_destructor(void* obj, void (*destructor)(void*)) {
    GC_register_finalizer(obj, (GC_finalization_proc)destructor, NULL, NULL, NULL);
}
```

---

## 3. Core Data Structures

### 3.1 String System (`evil_string_t`)

Strings in EVIL are immutable and UTF-8 encoded:

```c
typedef struct evil_string {
    uint32_t length;     /* Byte length */
    uint32_t hash;       /* Precomputed FNV-1a or MurmurHash3 */
    char chars[];        /* Flexible array member */
} evil_string_t;
```

- **Interning:** Identifier keys and constant string literals are interned into a global string table for $O(1)$ equality checks via pointer comparisons.
- **Dynamic Concatenation:** Concatenation with `+` uses pre-allocated buffers.

---

### 3.2 Hybrid Tables (`evil_table_t`)

EVIL tables serve as both associative dictionaries and sequential arrays. The Ceres reference uses an array part (capacity up to 1024) and a hash dictionary.

```c
typedef struct evil_table_entry {
    evil_value_t key;
    evil_value_t value;
    uint32_t hash;
} evil_table_entry_t;

typedef struct evil_table {
    /* Array Part (0-based sequential integers) */
    evil_value_t* array_part;
    uint32_t array_count;
    uint32_t array_capacity;

    /* Hash Part (arbitrary keys via Robin Hood hashing) */
    evil_table_entry_t* hash_entries;
    uint32_t hash_count;
    uint32_t hash_capacity;

    /* Metatable & Quick Dispatch Flags */
    struct evil_table* metatable;
    uint32_t metamethod_bitmask;
    bool is_frozen;
} evil_table_t;
```

#### Metamethod Fast Bitmask
When indexing or performing operations, checking whether a table overrides an operator is an $O(1)$ bit test:
```c
enum {
    META_ADD      = (1 << 0),
    META_SUB      = (1 << 1),
    META_MUL      = (1 << 2),
    META_DIV      = (1 << 3),
    META_INVOKE   = (1 << 4),
    META_INDEX    = (1 << 5),
    META_SET      = (1 << 6),
    /* ... covers all 32 metamethods */
};

static inline evil_value_t evil_table_get_meta(evil_table_t* t, uint32_t meta_flag, const char* name) {
    if (!t->metatable || !(t->metatable->metamethod_bitmask & meta_flag)) {
        return EVIL_NIL;
    }
    return evil_table_get_str(t->metatable, name);
}
```

---

### 3.3 Dynamic Arrays (`evil_array_t`)

Arrays created via `[...]` or `array(size) { ... }`:

```c
typedef struct evil_array {
    evil_value_t* items;
    uint32_t length;
    uint32_t capacity;
} evil_array_t;

void evil_array_push(evil_array_t* arr, evil_value_t val) {
    if (arr->length >= arr->capacity) {
        uint32_t new_cap = arr->capacity < 8 ? 8 : arr->capacity * 2;
        arr->items = (evil_value_t*)GC_REALLOC(arr->items, new_cap * sizeof(evil_value_t));
        arr->capacity = new_cap;
    }
    arr->items[arr->length++] = val;
}
```

---

## 4. Cooperative Fibers & Coroutines

EVIL concurrency operates through cooperative fibers scheduled via `yield<target>(args)`:
1. When `yield<target>(args)` is executed, the calling fiber is suspended.
2. A child fiber executing `target` is allocated its own native execution stack.
3. Execution transfers to the child fiber.
4. When the child fiber yields or terminates, context switches back to the parent fiber.

### 4.1 Native Context Switching

CVIL implements stackful coroutines via platform context primitives:

```c
#ifdef _WIN32
  #include <windows.h>
  typedef LPVOID evil_coro_context_t;
#else
  #include <ucontext.h>
  typedef struct {
      ucontext_t uctx;
      void* stack_mem;
      size_t stack_size;
  } evil_coro_context_t;
#endif

typedef enum {
    FIBER_CREATED,
    FIBER_RUNNING,
    FIBER_PAUSED,
    FIBER_COMPLETED,
    FIBER_DEAD
} evil_fiber_state_t;

typedef struct evil_fiber {
    evil_coro_context_t coro;
    evil_fiber_state_t state;
    
    struct evil_fiber* parent;
    struct evil_fiber* waiting_for;
    
    evil_value_t target_fn;
    int argc;
    evil_value_t* argv;
    evil_value_t return_value;
    
    /* Exception handler stack */
    struct evil_try_frame* try_stack_top;
    evil_value_t current_exception;
} evil_fiber_t;
```

#### Fiber Switching Logic:
- **On Linux / macOS:** Contexts are initialized via `getcontext()` + `makecontext()`. Switching is executed via `swapcontext(&from->coro.uctx, &to->coro.uctx)`.
- **On Windows:** Initialized via `CreateFiber()`. Switching is executed via `SwitchToFiber(to->coro)`.
- **Performance:** Native context switches cost less than 30 nanoseconds (saving only callee-saved registers: `rsp`, `rbp`, `rbx`, `r12-r15`).

---

## 5. Exception Architecture & The `retry` Statement

The EVIL exception model includes `throw expr;`, `try { ... } catch (e) { ... }`, and the unique `retry;` statement.

### 5.1 The `retry` Mechanism

When `retry;` is executed inside a catch block, execution must jump back to the start of the `try` block and re-attempt execution.

```c
#include <setjmp.h>

#define EVIL_JMP_INIT   0
#define EVIL_JMP_THROW  1
#define EVIL_JMP_RETRY  2

typedef struct evil_try_frame {
    jmp_buf jump_buffer;
    struct evil_try_frame* prev;
} evil_try_frame_t;

void evil_throw(evil_fiber_t* fiber, evil_value_t exception_val) {
    if (!fiber->try_stack_top) {
        fprintf(stderr, "Unhandled EVIL Exception: ");
        evil_val_print(stderr, exception_val);
        fprintf(stderr, "\n");
        abort();
    }
    
    fiber->current_exception = exception_val;
    longjmp(fiber->try_stack_top->jump_buffer, EVIL_JMP_THROW);
}

void evil_retry(evil_fiber_t* fiber) {
    if (!fiber->try_stack_top) {
        fprintf(stderr, "Error: 'retry;' outside of try block context\n");
        abort();
    }
    longjmp(fiber->try_stack_top->jump_buffer, EVIL_JMP_RETRY);
}
```

#### Codegen Pattern for `try` / `catch` / `retry`:

```c
/* Generated C Code for:
 * try {
 *     risky_operation();
 * } catch (e) {
 *     if (can_recover) retry;
 * }
 */
{
    evil_try_frame_t __try_frame;
    __try_frame.prev = fiber->try_stack_top;
    fiber->try_stack_top = &__try_frame;

    int __jmp_res = setjmp(__try_frame.jump_buffer);
    if (__jmp_res == EVIL_JMP_INIT || __jmp_res == EVIL_JMP_RETRY) {
        /* ----- TRY BODY ----- */
        risky_operation(fiber, 0, NULL);
        
        /* Pop try frame on normal completion */
        fiber->try_stack_top = __try_frame.prev;
    } else if (__jmp_res == EVIL_JMP_THROW) {
        /* ----- CATCH BODY ----- */
        evil_value_t e = fiber->current_exception;
        
        if (evil_to_bool(can_recover)) {
            /* retry: re-execute try block */
            evil_retry(fiber);
        }
        
        fiber->try_stack_top = __try_frame.prev;
    }
}
```
This pattern provides exact semantics for both standard errors and `retry;` cycles.
