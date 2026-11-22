#ifndef CVIL_RUNTIME_H
#define CVIL_RUNTIME_H

#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <stdbool.h>
#include <string.h>
#include <setjmp.h>
#include "cvil_value.h"

#ifdef __cplusplus
extern "C" {
#endif

/* ========================================================================= */
/* Memory Allocation (Boehm GC with standard malloc fallback)                */
/* ========================================================================= */
void evil_runtime_init_gc(void);
void* evil_alloc(size_t size);
void* evil_alloc_atomic(size_t size);
void* evil_realloc(void* ptr, size_t new_size);
void evil_free(void* ptr);

/* ========================================================================= */
/* Upvalue (Shared Closure Cell)                                             */
/* ========================================================================= */
typedef struct evil_upvalue {
    evil_value_t* val_ptr;
    evil_value_t closed;
} evil_upvalue_t;

evil_upvalue_t* evil_upvalue_create(evil_value_t initial_val);

/* ========================================================================= */
/* Strings                                                                   */
/* ========================================================================= */
typedef struct evil_string {
    uint32_t length;
    uint32_t hash;
    char chars[1]; /* flexible array */
} evil_string_t;

evil_value_t evil_str_create(const char* str, size_t len);
evil_value_t evil_str_from_cstr(const char* str);
evil_value_t evil_str_intern(const char* str);
const char* evil_str_chars(evil_value_t v);
size_t evil_str_len(evil_value_t v);

/* Dynamic String Builder for Interpolation */
typedef struct evil_strbuilder {
    char* data;
    size_t length;
    size_t capacity;
} evil_strbuilder_t;

evil_strbuilder_t* evil_strbuilder_create(void);
void evil_strbuilder_append_str(evil_strbuilder_t* sb, const char* str);
void evil_strbuilder_append_val(evil_strbuilder_t* sb, evil_value_t val);
evil_value_t evil_strbuilder_finish(evil_strbuilder_t* sb);

/* ========================================================================= */
/* Tables & Metamethods                                                      */
/* ========================================================================= */
typedef struct evil_table_entry {
    evil_value_t key;
    evil_value_t value;
    uint32_t hash;
    bool occupied;
} evil_table_entry_t;

enum {
    META_ADD      = (1 << 0),
    META_SUB      = (1 << 1),
    META_MUL      = (1 << 2),
    META_DIV      = (1 << 3),
    META_MOD      = (1 << 4),
    META_SHL      = (1 << 5),
    META_SHR      = (1 << 6),
    META_ANEG     = (1 << 7),
    META_INC      = (1 << 8),
    META_DEC      = (1 << 9),
    META_LNOT     = (1 << 10),
    META_LOR      = (1 << 11),
    META_LAND     = (1 << 12),
    META_BOR      = (1 << 13),
    META_BXOR     = (1 << 14),
    META_BAND     = (1 << 15),
    META_BNOT     = (1 << 16),
    META_DEQ      = (1 << 17),
    META_DNE      = (1 << 18),
    META_EQ       = (1 << 19),
    META_NE       = (1 << 20),
    META_GT       = (1 << 21),
    META_LT       = (1 << 22),
    META_GTE      = (1 << 23),
    META_LTE      = (1 << 24),
    META_LEN      = (1 << 25),
    META_TONUM    = (1 << 26),
    META_TOSTR    = (1 << 27),
    META_INVOKE   = (1 << 28),
    META_SET      = (1 << 29),
    META_GET      = (1 << 30),
    META_EXISTS   = (1U << 31)
};

typedef struct evil_table {
    evil_value_t* array_part;
    uint32_t array_count;
    uint32_t array_capacity;

    evil_table_entry_t* hash_entries;
    uint32_t hash_count;
    uint32_t hash_capacity;

    struct evil_table* metatable;
    uint32_t metamethod_flags;
    bool is_frozen;
} evil_table_t;

evil_value_t evil_table_create(uint32_t arr_cap, uint32_t hash_cap);
evil_value_t evil_table_get(evil_value_t tbl, evil_value_t key);
evil_value_t evil_table_get_str(evil_value_t tbl, const char* key);
evil_value_t evil_table_set(evil_value_t tbl, evil_value_t key, evil_value_t val);
evil_value_t evil_table_set_str(evil_value_t tbl, const char* key, evil_value_t val);
evil_value_t evil_table_rawget(evil_value_t tbl, evil_value_t key);
evil_value_t evil_table_rawset(evil_value_t tbl, evil_value_t key, evil_value_t val);
evil_value_t evil_table_set_meta(evil_value_t tbl, evil_value_t meta);
evil_value_t evil_table_get_meta(evil_value_t tbl);
bool evil_table_exists(evil_value_t tbl, evil_value_t key);
size_t evil_table_len(evil_value_t tbl);
evil_value_t evil_table_clone_with(evil_value_t orig, int num_entries, ...);
evil_value_t evil_table_clone(evil_value_t orig, bool deep);

/* Table Iterator for `each` */
typedef struct evil_table_iter {
    evil_table_t* table;
    uint32_t arr_idx;
    uint32_t hash_idx;
} evil_table_iter_t;

evil_table_iter_t evil_table_iter_begin(evil_table_t* t);
bool evil_table_iter_next(evil_table_iter_t* it, evil_value_t* out_key, evil_value_t* out_val);

/* ========================================================================= */
/* Arrays                                                                    */
/* ========================================================================= */
typedef struct evil_array {
    evil_value_t* items;
    uint32_t length;
    uint32_t capacity;
} evil_array_t;

evil_value_t evil_array_create(uint32_t capacity);
evil_value_t evil_array_create_sized(uint32_t size, evil_value_t initial_val);
size_t evil_array_len(evil_value_t arr);
evil_value_t evil_array_get(evil_value_t arr, int64_t index);
evil_value_t evil_array_set(evil_value_t arr, int64_t index, evil_value_t val);
void evil_array_push(evil_value_t arr, evil_value_t val);
evil_value_t evil_array_pop(evil_value_t arr);
evil_value_t evil_array_shift(evil_value_t arr);
void evil_array_unshift(evil_value_t arr, evil_value_t val);
evil_value_t evil_array_slice(evil_value_t arr, int64_t start, int64_t end);
evil_value_t evil_array_clone(evil_value_t orig, bool deep);

/* ========================================================================= */
/* Closures & Functions                                                      */
/* ========================================================================= */
typedef evil_value_t (*evil_fn_ptr_t)(struct evil_fiber* fiber, evil_value_t self, int argc, evil_value_t* argv);

typedef struct evil_closure {
    evil_fn_ptr_t fn_ptr;
    evil_value_t self;
    uint32_t upvalue_count;
    evil_upvalue_t* upvalues[1]; /* flexible array */
} evil_closure_t;

evil_value_t evil_closure_create(evil_fn_ptr_t fn_ptr, evil_value_t self, uint32_t upvalue_count, ...);
evil_value_t evil_closure_create_arr(evil_fn_ptr_t fn_ptr, evil_value_t self, uint32_t upvalue_count, evil_upvalue_t** upvalues);
evil_value_t evil_call(struct evil_fiber* fiber, evil_value_t callee, evil_value_t self, int argc, evil_value_t* argv);
evil_value_t evil_call_method(struct evil_fiber* fiber, evil_value_t obj, const char* method_name, int argc, evil_value_t* argv);

static inline evil_value_t evil_val_fn(evil_fn_ptr_t fn) {
    return evil_closure_create(fn, evil_val_nil(), 0);
}

/* ========================================================================= */
/* Exceptions & Retry                                                        */
/* ========================================================================= */
#define EVIL_JMP_INIT   0
#define EVIL_JMP_THROW  1
#define EVIL_JMP_RETRY  2

typedef struct evil_try_frame {
    jmp_buf jmp;
    struct evil_try_frame* prev;
} evil_try_frame_t;

typedef struct evil_error {
    evil_value_t message;
    evil_value_t data_table;
} evil_error_t;

evil_value_t evil_error_create(evil_value_t message, evil_value_t data);
void evil_throw(struct evil_fiber* fiber, evil_value_t ex);
void evil_retry(struct evil_fiber* fiber);

/* ========================================================================= */
/* Fibers / Coroutines                                                       */
/* ========================================================================= */
typedef enum {
    FIBER_RUNNING,
    FIBER_PAUSED,
    FIBER_DEAD
} evil_fiber_state_t;

typedef struct evil_fiber {
    evil_fiber_state_t state;
    struct evil_fiber* parent;
    evil_value_t target;
    evil_value_t return_value;
    evil_try_frame_t* try_top;
    evil_value_t current_exception;
    evil_value_t self;
    void* native_coro; /* platform context if needed */
    evil_closure_t* current_closure;
} evil_fiber_t;

evil_fiber_t* evil_fiber_create(void);
evil_value_t evil_fiber_yield_call(evil_fiber_t* current, evil_value_t target, int argc, evil_value_t* argv);
evil_value_t evil_yield(evil_fiber_t* fiber, evil_value_t val);

/* ========================================================================= */
/* Native Object & FFI                                                       */
/* ========================================================================= */
typedef struct evil_native_type {
    const char* type_name;
    void (*destructor)(void* data);
} evil_native_type_t;

typedef struct evil_native_obj {
    const evil_native_type_t* type_desc;
    void* payload;
} evil_native_obj_t;

evil_value_t evil_native_obj_create(const evil_native_type_t* type_desc, void* payload);
bool evil_val_is_native_obj(evil_value_t v, const char* type_name);

/* ========================================================================= */
/* Arithmetic, Bitwise, Comparison, Unary, and Pattern Helpers               */
/* ========================================================================= */
evil_value_t evil_val_add(evil_fiber_t* f, evil_value_t a, evil_value_t b);
evil_value_t evil_val_sub(evil_fiber_t* f, evil_value_t a, evil_value_t b);
evil_value_t evil_val_mul(evil_fiber_t* f, evil_value_t a, evil_value_t b);
evil_value_t evil_val_div(evil_fiber_t* f, evil_value_t a, evil_value_t b);
evil_value_t evil_val_mod(evil_fiber_t* f, evil_value_t a, evil_value_t b);
evil_value_t evil_val_neg(evil_fiber_t* f, evil_value_t a);
evil_value_t evil_val_lnot(evil_fiber_t* f, evil_value_t a);
evil_value_t evil_val_land(evil_fiber_t* f, evil_value_t a, evil_value_t b);
evil_value_t evil_val_lor(evil_fiber_t* f, evil_value_t a, evil_value_t b);

evil_value_t evil_val_band(evil_fiber_t* f, evil_value_t a, evil_value_t b);
evil_value_t evil_val_bor(evil_fiber_t* f, evil_value_t a, evil_value_t b);
evil_value_t evil_val_bxor(evil_fiber_t* f, evil_value_t a, evil_value_t b);
evil_value_t evil_val_bnot(evil_fiber_t* f, evil_value_t a);
evil_value_t evil_val_shl(evil_fiber_t* f, evil_value_t a, evil_value_t b);
evil_value_t evil_val_shr(evil_fiber_t* f, evil_value_t a, evil_value_t b);

evil_value_t evil_val_inc(evil_fiber_t* f, evil_value_t a);
evil_value_t evil_val_dec(evil_fiber_t* f, evil_value_t a);

evil_value_t evil_val_eq(evil_fiber_t* f, evil_value_t a, evil_value_t b);
evil_value_t evil_val_ne(evil_fiber_t* f, evil_value_t a, evil_value_t b);
evil_value_t evil_val_deq(evil_fiber_t* f, evil_value_t a, evil_value_t b);
evil_value_t evil_val_dne(evil_fiber_t* f, evil_value_t a, evil_value_t b);

evil_value_t evil_val_lt(evil_fiber_t* f, evil_value_t a, evil_value_t b);
evil_value_t evil_val_lte(evil_fiber_t* f, evil_value_t a, evil_value_t b);
evil_value_t evil_val_gt(evil_fiber_t* f, evil_value_t a, evil_value_t b);
evil_value_t evil_val_gte(evil_fiber_t* f, evil_value_t a, evil_value_t b);
evil_value_t evil_val_in(evil_fiber_t* f, evil_value_t item, evil_value_t coll);

evil_value_t evil_val_len(evil_fiber_t* f, evil_value_t a);
evil_value_t evil_val_tostr(evil_fiber_t* f, evil_value_t a);
evil_value_t evil_val_tonum(evil_fiber_t* f, evil_value_t a);
evil_value_t evil_val_typeof(evil_value_t a);
evil_value_t evil_val_native_typeof(evil_value_t a);
bool evil_val_is_type(evil_value_t a, evil_type_t tc, const char* native_type_opt);

evil_value_t evil_index_get(evil_fiber_t* f, evil_value_t coll, evil_value_t idx);
evil_value_t evil_index_set(evil_fiber_t* f, evil_value_t coll, evil_value_t idx, evil_value_t val);

/* ========================================================================= */
/* Standard Library & Runtime Environment                                    */
/* ========================================================================= */
typedef struct evil_runtime {
    evil_value_t global;
    evil_fiber_t* main_fiber;
} evil_runtime_t;

evil_runtime_t* evil_runtime_create(void);
void evil_runtime_init_globals(evil_runtime_t* rt, int argc, char** argv);
evil_value_t evil_runtime_get_global(evil_runtime_t* rt, const char* name);
void evil_runtime_set_global(evil_runtime_t* rt, const char* name, evil_value_t val);

#ifdef __cplusplus
}
#endif

#endif /* CVIL_RUNTIME_H */
