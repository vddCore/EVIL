#define _GNU_SOURCE
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdarg.h>
#include <math.h>
#include <time.h>
#include <sys/time.h>
#include <sys/stat.h>
#include <unistd.h>
#include <errno.h>

#ifdef HAVE_LIBGC
  #include <gc.h>
#endif

#include "cvil_runtime.h"

/* ========================================================================= */
/* 1. Memory Allocator                                                       */
/* ========================================================================= */

static bool g_gc_initialized = false;

void evil_runtime_init_gc(void) {
    if (!g_gc_initialized) {
#ifdef HAVE_LIBGC
        GC_INIT();
#endif
        g_gc_initialized = true;
    }
}

void* evil_alloc(size_t size) {
    evil_runtime_init_gc();
#ifdef HAVE_LIBGC
    void* ptr = GC_MALLOC(size);
#else
    void* ptr = malloc(size);
#endif
    if (!ptr && size > 0) {
        fprintf(stderr, "Fatal: Out of memory (allocating %zu bytes)\n", size);
        abort();
    }
    return ptr;
}

void* evil_alloc_atomic(size_t size) {
    evil_runtime_init_gc();
#ifdef HAVE_LIBGC
    void* ptr = GC_MALLOC_ATOMIC(size);
#else
    void* ptr = malloc(size);
#endif
    if (!ptr && size > 0) {
        fprintf(stderr, "Fatal: Out of memory (allocating %zu bytes)\n", size);
        abort();
    }
    return ptr;
}

void* evil_realloc(void* ptr, size_t new_size) {
    evil_runtime_init_gc();
#ifdef HAVE_LIBGC
    void* nptr = GC_REALLOC(ptr, new_size);
#else
    void* nptr = realloc(ptr, new_size);
#endif
    if (!nptr && new_size > 0) {
        fprintf(stderr, "Fatal: Out of memory (reallocating %zu bytes)\n", new_size);
        abort();
    }
    return nptr;
}

void evil_free(void* ptr) {
#ifndef HAVE_LIBGC
    free(ptr);
#else
    (void)ptr;
#endif
}

/* ========================================================================= */
/* 2. Upvalues                                                               */
/* ========================================================================= */

evil_upvalue_t* evil_upvalue_create(evil_value_t initial_val) {
    evil_upvalue_t* up = (evil_upvalue_t*)evil_alloc(sizeof(evil_upvalue_t));
    up->closed = initial_val;
    up->val_ptr = &up->closed;
    return up;
}

/* ========================================================================= */
/* 3. Strings & String Pool                                                  */
/* ========================================================================= */

static uint32_t hash_string(const char* str, size_t len) {
    uint32_t hash = 2166136261u;
    for (size_t i = 0; i < len; i++) {
        hash ^= (uint8_t)str[i];
        hash *= 16777619;
    }
    return hash;
}

#define STRING_POOL_SIZE 4096
static evil_string_t* g_string_pool[STRING_POOL_SIZE];

evil_value_t evil_str_create(const char* str, size_t len) {
    uint32_t hash = hash_string(str, len);
    evil_string_t* s = (evil_string_t*)evil_alloc_atomic(sizeof(evil_string_t) + len + 1);
    s->length = (uint32_t)len;
    s->hash = hash;
    memcpy(s->chars, str, len);
    s->chars[len] = '\0';
    return evil_val_string(s);
}

evil_value_t evil_str_from_cstr(const char* str) {
    if (!str) return evil_val_nil();
    return evil_str_create(str, strlen(str));
}

evil_value_t evil_str_intern(const char* str) {
    if (!str) return evil_val_nil();
    size_t len = strlen(str);
    uint32_t hash = hash_string(str, len);
    uint32_t idx = hash % STRING_POOL_SIZE;

    evil_string_t* existing = g_string_pool[idx];
    if (existing && existing->length == len && memcmp(existing->chars, str, len) == 0) {
        return evil_val_string(existing);
    }

    evil_value_t val = evil_str_create(str, len);
    g_string_pool[idx] = val.as.string;
    return val;
}

const char* evil_str_chars(evil_value_t v) {
    if (v.type != EVIL_TYPE_STRING || !v.as.string) return "";
    return v.as.string->chars;
}

size_t evil_str_len(evil_value_t v) {
    if (v.type != EVIL_TYPE_STRING || !v.as.string) return 0;
    return v.as.string->length;
}

evil_strbuilder_t* evil_strbuilder_create(void) {
    evil_strbuilder_t* sb = (evil_strbuilder_t*)evil_alloc(sizeof(evil_strbuilder_t));
    sb->capacity = 64;
    sb->length = 0;
    sb->data = (char*)evil_alloc_atomic(sb->capacity);
    sb->data[0] = '\0';
    return sb;
}

void evil_strbuilder_append_str(evil_strbuilder_t* sb, const char* str) {
    if (!str) return;
    size_t slen = strlen(str);
    if (sb->length + slen + 1 >= sb->capacity) {
        size_t new_cap = (sb->capacity + slen) * 2;
        sb->data = (char*)evil_realloc(sb->data, new_cap);
        sb->capacity = new_cap;
    }
    memcpy(sb->data + sb->length, str, slen);
    sb->length += slen;
    sb->data[sb->length] = '\0';
}

void evil_strbuilder_append_val(evil_strbuilder_t* sb, evil_value_t val) {
    evil_value_t s = evil_val_tostr(NULL, val);
    evil_strbuilder_append_str(sb, evil_str_chars(s));
}

evil_value_t evil_strbuilder_finish(evil_strbuilder_t* sb) {
    evil_value_t res = evil_str_create(sb->data, sb->length);
    return res;
}

/* ========================================================================= */
/* 4. Tables                                                                 */
/* ========================================================================= */

static uint32_t hash_value(evil_value_t v) {
    switch (v.type) {
        case EVIL_TYPE_NIL: return 0;
        case EVIL_TYPE_BOOLEAN: return v.as.boolean ? 1 : 2;
        case EVIL_TYPE_NUMBER: {
            uint64_t u;
            memcpy(&u, &v.as.number, sizeof(double));
            return (uint32_t)(u ^ (u >> 32));
        }
        case EVIL_TYPE_STRING:
            return v.as.string ? v.as.string->hash : 0;
        case EVIL_TYPE_TABLE:
            return (uint32_t)((uintptr_t)v.as.table ^ ((uintptr_t)v.as.table >> 16));
        case EVIL_TYPE_ARRAY:
            return (uint32_t)((uintptr_t)v.as.array ^ ((uintptr_t)v.as.array >> 16));
        case EVIL_TYPE_FUNCTION:
            return (uint32_t)((uintptr_t)v.as.closure ^ ((uintptr_t)v.as.closure >> 16));
        case EVIL_TYPE_TYPE:
            return (uint32_t)v.as.type_code;
        default:
            return (uint32_t)((uintptr_t)v.as.native_obj);
    }
}

evil_value_t evil_table_create(uint32_t arr_cap, uint32_t hash_cap) {
    evil_table_t* t = (evil_table_t*)evil_alloc(sizeof(evil_table_t));
    t->array_capacity = arr_cap > 0 ? arr_cap : 0;
    t->array_count = 0;
    t->array_part = t->array_capacity > 0 ? (evil_value_t*)evil_alloc(t->array_capacity * sizeof(evil_value_t)) : NULL;

    uint32_t hcap = 8;
    while (hcap < hash_cap) hcap *= 2;
    t->hash_capacity = hcap;
    t->hash_count = 0;
    t->hash_entries = (evil_table_entry_t*)evil_alloc(t->hash_capacity * sizeof(evil_table_entry_t));
    for (uint32_t i = 0; i < t->hash_capacity; i++) {
        t->hash_entries[i].occupied = false;
    }

    t->metatable = NULL;
    t->metamethod_flags = 0;
    t->is_frozen = false;
    return evil_val_table(t);
}

static void table_grow_hash(evil_table_t* t) {
    uint32_t old_cap = t->hash_capacity;
    evil_table_entry_t* old_entries = t->hash_entries;

    t->hash_capacity = old_cap * 2;
    t->hash_entries = (evil_table_entry_t*)evil_alloc(t->hash_capacity * sizeof(evil_table_entry_t));
    for (uint32_t i = 0; i < t->hash_capacity; i++) {
        t->hash_entries[i].occupied = false;
    }
    t->hash_count = 0;

    for (uint32_t i = 0; i < old_cap; i++) {
        if (old_entries[i].occupied) {
            evil_table_rawset(evil_val_table(t), old_entries[i].key, old_entries[i].value);
        }
    }
}

evil_value_t evil_table_rawget(evil_value_t tbl, evil_value_t key) {
    if (tbl.type == EVIL_TYPE_ERROR && tbl.as.error) tbl = tbl.as.error->data_table;
    if (tbl.type != EVIL_TYPE_TABLE || !tbl.as.table) return evil_val_nil();
    evil_table_t* t = tbl.as.table;

    /* Check array part for small positive integer keys */
    if (key.type == EVIL_TYPE_NUMBER) {
        double num = key.as.number;
        if (num >= 0.0 && num == (double)(int64_t)num) {
            int64_t idx = (int64_t)num;
            if (idx < t->array_count) {
                return t->array_part[idx];
            }
        }
    }

    if (t->hash_count == 0) return evil_val_nil();

    uint32_t h = hash_value(key);
    uint32_t mask = t->hash_capacity - 1;
    uint32_t idx = h & mask;

    for (uint32_t i = 0; i < t->hash_capacity; i++) {
        uint32_t cur = (idx + i) & mask;
        if (!t->hash_entries[cur].occupied) return evil_val_nil();
        if (t->hash_entries[cur].hash == h) {
            evil_value_t eq = evil_val_deq(NULL, t->hash_entries[cur].key, key);
            if (evil_is_truth(eq)) {
                return t->hash_entries[cur].value;
            }
        }
    }
    return evil_val_nil();
}

evil_value_t evil_table_rawset(evil_value_t tbl, evil_value_t key, evil_value_t val) {
    if (tbl.type == EVIL_TYPE_ERROR && tbl.as.error) tbl = tbl.as.error->data_table;
    if (tbl.type != EVIL_TYPE_TABLE || !tbl.as.table) return val;
    evil_table_t* t = tbl.as.table;
    if (t->is_frozen) {
        fprintf(stderr, "Error: attempt to modify a frozen table\n");
        return val;
    }

    /* Check array part for contiguous sequence */
    if (key.type == EVIL_TYPE_NUMBER) {
        double num = key.as.number;
        if (num >= 0.0 && num == (double)(int64_t)num) {
            int64_t idx = (int64_t)num;
            if (idx == t->array_count && t->array_count < 1024) {
                if (t->array_count >= t->array_capacity) {
                    uint32_t ncap = t->array_capacity < 8 ? 8 : t->array_capacity * 2;
                    t->array_part = (evil_value_t*)evil_realloc(t->array_part, ncap * sizeof(evil_value_t));
                    t->array_capacity = ncap;
                }
                t->array_part[t->array_count++] = val;
                return val;
            } else if (idx < t->array_count) {
                t->array_part[idx] = val;
                return val;
            }
        }
    }

    if ((t->hash_count + 1) * 4 >= t->hash_capacity * 3) {
        table_grow_hash(t);
    }

    uint32_t h = hash_value(key);
    uint32_t mask = t->hash_capacity - 1;
    uint32_t idx = h & mask;

    for (uint32_t i = 0; i < t->hash_capacity; i++) {
        uint32_t cur = (idx + i) & mask;
        if (!t->hash_entries[cur].occupied) {
            t->hash_entries[cur].occupied = true;
            t->hash_entries[cur].hash = h;
            t->hash_entries[cur].key = key;
            t->hash_entries[cur].value = val;
            t->hash_count++;
            return val;
        }
        if (t->hash_entries[cur].hash == h) {
            evil_value_t eq = evil_val_deq(NULL, t->hash_entries[cur].key, key);
            if (evil_is_truth(eq)) {
                t->hash_entries[cur].value = val;
                return val;
            }
        }
    }
    return val;
}

evil_value_t evil_table_get(evil_value_t tbl, evil_value_t key) {
    if (tbl.type == EVIL_TYPE_ERROR && tbl.as.error) tbl = tbl.as.error->data_table;
    if (tbl.type != EVIL_TYPE_TABLE || !tbl.as.table) return evil_val_nil();
    evil_table_t* t = tbl.as.table;

    /* Check raw table first */
    evil_value_t res = evil_table_rawget(tbl, key);
    if (!evil_is_nil(res)) return res;

    /* Check metamethod __get if not found */
    if (t->metatable && (t->metatable->metamethod_flags & META_GET)) {
        evil_value_t meta_fn = evil_table_rawget(evil_val_table(t->metatable), evil_str_intern("__get"));
        if (evil_is_fn(meta_fn)) {
            evil_value_t args[2] = { tbl, key };
            return evil_call(NULL, meta_fn, tbl, 2, args);
        } else if (meta_fn.type == EVIL_TYPE_TABLE) {
            return evil_table_get(meta_fn, key);
        }
    }

    return evil_val_nil();
}

evil_value_t evil_table_get_str(evil_value_t tbl, const char* key) {
    return evil_table_get(tbl, evil_str_intern(key));
}

evil_value_t evil_table_set(evil_value_t tbl, evil_value_t key, evil_value_t val) {
    if (tbl.type == EVIL_TYPE_ERROR && tbl.as.error) tbl = tbl.as.error->data_table;
    if (tbl.type != EVIL_TYPE_TABLE || !tbl.as.table) return val;
    evil_table_t* t = tbl.as.table;

    /* Check metamethod __set */
    if (t->metatable && (t->metatable->metamethod_flags & META_SET)) {
        evil_value_t meta_fn = evil_table_rawget(evil_val_table(t->metatable), evil_str_intern("__set"));
        if (evil_is_fn(meta_fn)) {
            evil_value_t args[3] = { tbl, key, val };
            return evil_call(NULL, meta_fn, tbl, 3, args);
        }
    }

    return evil_table_rawset(tbl, key, val);
}

evil_value_t evil_table_set_str(evil_value_t tbl, const char* key, evil_value_t val) {
    return evil_table_set(tbl, evil_str_intern(key), val);
}

static uint32_t compute_meta_flag(const char* name) {
    if (strcmp(name, "__add") == 0) return META_ADD;
    if (strcmp(name, "__sub") == 0) return META_SUB;
    if (strcmp(name, "__mul") == 0) return META_MUL;
    if (strcmp(name, "__div") == 0) return META_DIV;
    if (strcmp(name, "__mod") == 0) return META_MOD;
    if (strcmp(name, "__shl") == 0) return META_SHL;
    if (strcmp(name, "__shr") == 0) return META_SHR;
    if (strcmp(name, "__aneg") == 0) return META_ANEG;
    if (strcmp(name, "__inc") == 0) return META_INC;
    if (strcmp(name, "__dec") == 0) return META_DEC;
    if (strcmp(name, "__lnot") == 0) return META_LNOT;
    if (strcmp(name, "__lor") == 0) return META_LOR;
    if (strcmp(name, "__land") == 0) return META_LAND;
    if (strcmp(name, "__bor") == 0) return META_BOR;
    if (strcmp(name, "__bxor") == 0) return META_BXOR;
    if (strcmp(name, "__band") == 0) return META_BAND;
    if (strcmp(name, "__bnot") == 0) return META_BNOT;
    if (strcmp(name, "__deq") == 0) return META_DEQ;
    if (strcmp(name, "__dne") == 0) return META_DNE;
    if (strcmp(name, "__eq") == 0) return META_EQ;
    if (strcmp(name, "__ne") == 0) return META_NE;
    if (strcmp(name, "__gt") == 0) return META_GT;
    if (strcmp(name, "__lt") == 0) return META_LT;
    if (strcmp(name, "__gte") == 0) return META_GTE;
    if (strcmp(name, "__lte") == 0) return META_LTE;
    if (strcmp(name, "__len") == 0) return META_LEN;
    if (strcmp(name, "__tonum") == 0) return META_TONUM;
    if (strcmp(name, "__tostr") == 0) return META_TOSTR;
    if (strcmp(name, "__invoke") == 0) return META_INVOKE;
    if (strcmp(name, "__set") == 0) return META_SET;
    if (strcmp(name, "__get") == 0) return META_GET;
    if (strcmp(name, "__exists") == 0) return META_EXISTS;
    return 0;
}

evil_value_t evil_table_set_meta(evil_value_t tbl, evil_value_t meta) {
    if (tbl.type != EVIL_TYPE_TABLE || !tbl.as.table) return tbl;
    if (meta.type == EVIL_TYPE_NIL) {
        tbl.as.table->metatable = NULL;
        return tbl;
    }
    if (meta.type != EVIL_TYPE_TABLE || !meta.as.table) return tbl;

    evil_table_t* mt = meta.as.table;
    mt->metamethod_flags = 0;
    for (uint32_t i = 0; i < mt->hash_capacity; i++) {
        if (mt->hash_entries[i].occupied && mt->hash_entries[i].key.type == EVIL_TYPE_STRING) {
            mt->metamethod_flags |= compute_meta_flag(evil_str_chars(mt->hash_entries[i].key));
        }
    }
    tbl.as.table->metatable = mt;
    return tbl;
}

evil_value_t evil_table_get_meta(evil_value_t tbl) {
    if (tbl.type != EVIL_TYPE_TABLE || !tbl.as.table || !tbl.as.table->metatable) {
        return evil_val_nil();
    }
    return evil_val_table(tbl.as.table->metatable);
}

bool evil_table_exists(evil_value_t tbl, evil_value_t key) {
    if (tbl.type == EVIL_TYPE_ERROR && tbl.as.error) tbl = tbl.as.error->data_table;
    if (tbl.type != EVIL_TYPE_TABLE || !tbl.as.table) return false;
    evil_table_t* t = tbl.as.table;

    /* Check __exists metamethod */
    if (t->metatable && (t->metatable->metamethod_flags & META_EXISTS)) {
        evil_value_t meta_fn = evil_table_rawget(evil_val_table(t->metatable), evil_str_intern("__exists"));
        if (evil_is_fn(meta_fn)) {
            evil_value_t args[2] = { tbl, key };
            evil_value_t r = evil_call(NULL, meta_fn, tbl, 2, args);
            return evil_is_truth(r);
        }
    }

    evil_value_t val = evil_table_rawget(tbl, key);
    return !evil_is_nil(val);
}

size_t evil_table_len(evil_value_t tbl) {
    if (tbl.type != EVIL_TYPE_TABLE || !tbl.as.table) return 0;
    evil_table_t* t = tbl.as.table;

    /* Check __len metamethod */
    if (t->metatable && (t->metatable->metamethod_flags & META_LEN)) {
        evil_value_t meta_fn = evil_table_rawget(evil_val_table(t->metatable), evil_str_intern("__len"));
        if (evil_is_fn(meta_fn)) {
            evil_value_t args[1] = { tbl };
            evil_value_t r = evil_call(NULL, meta_fn, tbl, 1, args);
            if (r.type == EVIL_TYPE_NUMBER) return (size_t)r.as.number;
            return 1;
        }
    }

    return t->array_count + t->hash_count;
}

evil_table_iter_t evil_table_iter_begin(evil_table_t* t) {
    evil_table_iter_t it;
    it.table = t;
    it.arr_idx = 0;
    it.hash_idx = 0;
    return it;
}

bool evil_table_iter_next(evil_table_iter_t* it, evil_value_t* out_key, evil_value_t* out_val) {
    if (!it->table) return false;

    /* Check array part first */
    while (it->arr_idx < it->table->array_count) {
        uint32_t i = it->arr_idx++;
        *out_key = evil_val_number((double)i);
        *out_val = it->table->array_part[i];
        return true;
    }

    /* Check hash part */
    while (it->hash_idx < it->table->hash_capacity) {
        uint32_t i = it->hash_idx++;
        if (it->table->hash_entries[i].occupied) {
            *out_key = it->table->hash_entries[i].key;
            *out_val = it->table->hash_entries[i].value;
            return true;
        }
    }
    return false;
}

evil_value_t evil_table_clone(evil_value_t orig, bool deep) {
    if (orig.type != EVIL_TYPE_TABLE || !orig.as.table) return evil_val_nil();
    evil_table_t* src = orig.as.table;
    evil_value_t clone = evil_table_create(src->array_count, src->hash_count);

    evil_table_iter_t it = evil_table_iter_begin(src);
    evil_value_t k, v;
    while (evil_table_iter_next(&it, &k, &v)) {
        if (deep) {
            if (v.type == EVIL_TYPE_TABLE) v = evil_table_clone(v, true);
            else if (v.type == EVIL_TYPE_ARRAY) v = evil_array_clone(v, true);
        }
        evil_table_rawset(clone, k, v);
    }
    if (src->metatable) {
        evil_table_set_meta(clone, evil_val_table(src->metatable));
    }
    return clone;
}

evil_value_t evil_table_clone_with(evil_value_t orig, int num_entries, ...) {
    evil_value_t clone = evil_table_clone(orig, false);
    va_list args;
    va_start(args, num_entries);
    for (int i = 0; i < num_entries; i++) {
        evil_value_t k = va_arg(args, evil_value_t);
        evil_value_t v = va_arg(args, evil_value_t);
        evil_table_set(clone, k, v);
    }
    va_end(args);
    return clone;
}

/* ========================================================================= */
/* 5. Arrays                                                                 */
/* ========================================================================= */

evil_value_t evil_array_create(uint32_t capacity) {
    evil_array_t* a = (evil_array_t*)evil_alloc(sizeof(evil_array_t));
    a->capacity = capacity > 4 ? capacity : 4;
    a->length = 0;
    a->items = (evil_value_t*)evil_alloc(a->capacity * sizeof(evil_value_t));
    return evil_val_array(a);
}

evil_value_t evil_array_create_sized(uint32_t size, evil_value_t initial_val) {
    evil_array_t* a = (evil_array_t*)evil_alloc(sizeof(evil_array_t));
    a->capacity = size > 4 ? size : 4;
    a->length = size;
    a->items = (evil_value_t*)evil_alloc(a->capacity * sizeof(evil_value_t));
    for (uint32_t i = 0; i < size; i++) {
        a->items[i] = initial_val;
    }
    return evil_val_array(a);
}

size_t evil_array_len(evil_value_t arr) {
    if (arr.type != EVIL_TYPE_ARRAY || !arr.as.array) return 0;
    return arr.as.array->length;
}

evil_value_t evil_array_get(evil_value_t arr, int64_t index) {
    if (arr.type != EVIL_TYPE_ARRAY || !arr.as.array) return evil_val_nil();
    evil_array_t* a = arr.as.array;
    if (index < 0 || (uint64_t)index >= a->length) {
        return evil_val_nil();
    }
    return a->items[index];
}

evil_value_t evil_array_set(evil_value_t arr, int64_t index, evil_value_t val) {
    if (arr.type != EVIL_TYPE_ARRAY || !arr.as.array) return val;
    evil_array_t* a = arr.as.array;
    if (index < 0) return val;
    if ((uint64_t)index >= a->capacity) {
        uint32_t ncap = (uint32_t)(index + 1) * 2;
        a->items = (evil_value_t*)evil_realloc(a->items, ncap * sizeof(evil_value_t));
        for (uint32_t i = a->capacity; i < ncap; i++) a->items[i] = evil_val_nil();
        a->capacity = ncap;
    }
    if ((uint64_t)index >= a->length) {
        a->length = (uint32_t)index + 1;
    }
    a->items[index] = val;
    return val;
}

void evil_array_push(evil_value_t arr, evil_value_t val) {
    if (arr.type != EVIL_TYPE_ARRAY || !arr.as.array) return;
    evil_array_t* a = arr.as.array;
    if (a->length >= a->capacity) {
        a->capacity = a->capacity * 2;
        a->items = (evil_value_t*)evil_realloc(a->items, a->capacity * sizeof(evil_value_t));
    }
    a->items[a->length++] = val;
}

evil_value_t evil_array_pop(evil_value_t arr) {
    if (arr.type != EVIL_TYPE_ARRAY || !arr.as.array || arr.as.array->length == 0) {
        return evil_val_nil();
    }
    evil_array_t* a = arr.as.array;
    return a->items[--a->length];
}

evil_value_t evil_array_shift(evil_value_t arr) {
    if (arr.type != EVIL_TYPE_ARRAY || !arr.as.array || arr.as.array->length == 0) {
        return evil_val_nil();
    }
    evil_array_t* a = arr.as.array;
    evil_value_t first = a->items[0];
    memmove(&a->items[0], &a->items[1], (a->length - 1) * sizeof(evil_value_t));
    a->length--;
    return first;
}

void evil_array_unshift(evil_value_t arr, evil_value_t val) {
    if (arr.type != EVIL_TYPE_ARRAY || !arr.as.array) return;
    evil_array_t* a = arr.as.array;
    if (a->length + 1 >= a->capacity) {
        a->capacity = (a->capacity + 1) * 2;
        a->items = (evil_value_t*)evil_realloc(a->items, a->capacity * sizeof(evil_value_t));
    }
    memmove(&a->items[1], &a->items[0], a->length * sizeof(evil_value_t));
    a->items[0] = val;
    a->length++;
}

evil_value_t evil_array_slice(evil_value_t arr, int64_t start, int64_t end) {
    if (arr.type != EVIL_TYPE_ARRAY || !arr.as.array) return evil_array_create(0);
    evil_array_t* a = arr.as.array;
    if (start < 0) start += a->length;
    if (end < 0) end += a->length;
    if (start < 0) start = 0;
    if (end > (int64_t)a->length) end = a->length;
    if (start >= end) return evil_array_create(0);

    uint32_t count = (uint32_t)(end - start);
    evil_value_t res = evil_array_create(count);
    for (uint32_t i = 0; i < count; i++) {
        evil_array_push(res, a->items[start + i]);
    }
    return res;
}

evil_value_t evil_array_clone(evil_value_t orig, bool deep) {
    if (orig.type != EVIL_TYPE_ARRAY || !orig.as.array) return evil_val_nil();
    evil_array_t* src = orig.as.array;
    evil_value_t clone = evil_array_create(src->length);
    for (uint32_t i = 0; i < src->length; i++) {
        evil_value_t v = src->items[i];
        if (deep) {
            if (v.type == EVIL_TYPE_ARRAY) v = evil_array_clone(v, true);
            else if (v.type == EVIL_TYPE_TABLE) v = evil_table_clone(v, true);
        }
        evil_array_push(clone, v);
    }
    return clone;
}

/* ========================================================================= */
/* 6. Closures & Invocations                                                 */
/* ========================================================================= */

evil_value_t evil_closure_create(evil_fn_ptr_t fn_ptr, evil_value_t self, uint32_t upvalue_count, ...) {
    size_t size = sizeof(evil_closure_t) + (upvalue_count > 1 ? (upvalue_count - 1) * sizeof(evil_upvalue_t*) : 0);
    evil_closure_t* c = (evil_closure_t*)evil_alloc(size);
    c->fn_ptr = fn_ptr;
    c->self = self;
    c->upvalue_count = upvalue_count;

    va_list args;
    va_start(args, upvalue_count);
    for (uint32_t i = 0; i < upvalue_count; i++) {
        c->upvalues[i] = va_arg(args, evil_upvalue_t*);
    }
    va_end(args);
    return evil_val_closure(c);
}

evil_value_t evil_closure_create_arr(evil_fn_ptr_t fn_ptr, evil_value_t self, uint32_t upvalue_count, evil_upvalue_t** upvalues) {
    size_t size = sizeof(evil_closure_t) + (upvalue_count > 1 ? (upvalue_count - 1) * sizeof(evil_upvalue_t*) : 0);
    evil_closure_t* c = (evil_closure_t*)evil_alloc(size);
    c->fn_ptr = fn_ptr;
    c->self = self;
    c->upvalue_count = upvalue_count;
    for (uint32_t i = 0; i < upvalue_count; i++) {
        c->upvalues[i] = upvalues[i];
    }
    return evil_val_closure(c);
}

static evil_runtime_t* g_active_rt = NULL;

evil_value_t evil_call(evil_fiber_t* fiber, evil_value_t callee, evil_value_t self, int argc, evil_value_t* argv) {
    if (!fiber && g_active_rt) fiber = g_active_rt->main_fiber;
    if (callee.type == EVIL_TYPE_FUNCTION && callee.as.closure) {
        evil_closure_t* c = callee.as.closure;
        evil_value_t call_self = (c->self.type != EVIL_TYPE_NIL) ? c->self : self;
        evil_closure_t* prev_closure = fiber ? fiber->current_closure : NULL;
        if (fiber) fiber->current_closure = c;
        evil_value_t res = c->fn_ptr(fiber, call_self, argc, argv);
        if (fiber) fiber->current_closure = prev_closure;
        return res;
    }
    if (callee.type == EVIL_TYPE_NATIVE_FUNCTION && callee.as.native_fn) {
        return callee.as.native_fn(fiber, self, argc, argv);
    }
    /* Metamethod __invoke */
    if (callee.type == EVIL_TYPE_TABLE && callee.as.table) {
        evil_table_t* t = callee.as.table;
        if (t->metatable && (t->metatable->metamethod_flags & META_INVOKE)) {
            evil_value_t invk = evil_table_rawget(evil_val_table(t->metatable), evil_str_intern("__invoke"));
            if (evil_is_fn(invk)) {
                evil_value_t* invk_args = (evil_value_t*)evil_alloc((argc + 1) * sizeof(evil_value_t));
                invk_args[0] = callee;
                for (int i = 0; i < argc; i++) invk_args[i + 1] = argv[i];
                return evil_call(fiber, invk, callee, argc + 1, invk_args);
            }
        }
    }

    fprintf(stderr, "Error: attempt to invoke non-callable value of type %d\n", callee.type);
    return evil_val_nil();
}

evil_value_t evil_call_method(evil_fiber_t* fiber, evil_value_t obj, const char* method_name, int argc, evil_value_t* argv) {
    evil_value_t method = evil_index_get(fiber, obj, evil_str_intern(method_name));
    if (!evil_is_fn(method)) {
        fprintf(stderr, "Error: method '%s' not found on object\n", method_name);
        return evil_val_nil();
    }
    evil_value_t* full_argv = (evil_value_t*)evil_alloc((argc + 1) * sizeof(evil_value_t));
    full_argv[0] = obj;
    for (int i = 0; i < argc; i++) full_argv[i + 1] = argv[i];
    return evil_call(fiber, method, obj, argc + 1, full_argv);
}

/* ========================================================================= */
/* 7. Exceptions & Retry                                                     */
/* ========================================================================= */

evil_value_t evil_error_create(evil_value_t message, evil_value_t data) {
    evil_error_t* err = (evil_error_t*)evil_alloc(sizeof(evil_error_t));
    if (data.type != EVIL_TYPE_TABLE) {
        data = evil_table_create(0, 4);
    }
    if (message.type != EVIL_TYPE_NIL) {
        evil_table_set_str(data, "msg", message);
    }
    err->message = message;
    err->data_table = data;
    return evil_val_error(err);
}

void evil_throw(evil_fiber_t* fiber, evil_value_t ex) {
    evil_fiber_t* cur = fiber;
    while (cur) {
        if (cur->try_top) {
            cur->current_exception = ex;
            longjmp(cur->try_top->jmp, EVIL_JMP_THROW);
        }
        cur = cur->parent;
    }
    fprintf(stderr, "Unhandled EVIL Exception: %s\n", evil_str_chars(evil_val_tostr(fiber, ex)));
    exit(1);
}

void evil_retry(evil_fiber_t* fiber) {
    if (!fiber || !fiber->try_top) {
        fprintf(stderr, "Error: 'retry' outside of try block\n");
        exit(1);
    }
    longjmp(fiber->try_top->jmp, EVIL_JMP_RETRY);
}

/* ========================================================================= */
/* 8. Fibers & Coroutines                                                    */
/* ========================================================================= */

evil_fiber_t* evil_fiber_create(void) {
    evil_fiber_t* f = (evil_fiber_t*)evil_alloc(sizeof(evil_fiber_t));
    f->state = FIBER_RUNNING;
    f->parent = NULL;
    f->target = evil_val_nil();
    f->return_value = evil_val_nil();
    f->try_top = NULL;
    f->current_exception = evil_val_nil();
    f->self = evil_val_nil();
    f->native_coro = NULL;
    return f;
}

evil_value_t evil_fiber_yield_call(evil_fiber_t* current, evil_value_t target, int argc, evil_value_t* argv) {
    evil_fiber_t* child = evil_fiber_create();
    child->parent = current;
    child->target = target;
    evil_value_t res = evil_call(child, target, evil_val_nil(), argc, argv);
    child->state = FIBER_DEAD;
    return res;
}

evil_value_t evil_yield(evil_fiber_t* fiber, evil_value_t val) {
    if (fiber) {
        fiber->return_value = val;
    }
    return val;
}

/* ========================================================================= */
/* 9. Native Object & FFI                                                    */
/* ========================================================================= */

evil_value_t evil_native_obj_create(const evil_native_type_t* type_desc, void* payload) {
    evil_native_obj_t* obj = (evil_native_obj_t*)evil_alloc(sizeof(evil_native_obj_t));
    obj->type_desc = type_desc;
    obj->payload = payload;
    return evil_val_native_obj(obj);
}

bool evil_val_is_native_obj(evil_value_t v, const char* type_name) {
    if (v.type != EVIL_TYPE_NATIVE_OBJECT || !v.as.native_obj) return false;
    if (!type_name) return true;
    if (!v.as.native_obj->type_desc) return false;
    return strcmp(v.as.native_obj->type_desc->type_name, type_name) == 0;
}

/* ========================================================================= */
/* 10. Operations (Arithmetic, Comparison, Indexing, Metamethods)            */
/* ========================================================================= */

evil_value_t evil_val_add(evil_fiber_t* f, evil_value_t a, evil_value_t b) {
    /* String concatenation */
    if (a.type == EVIL_TYPE_STRING || b.type == EVIL_TYPE_STRING) {
        evil_value_t sa = evil_val_tostr(f, a);
        evil_value_t sb = evil_val_tostr(f, b);
        size_t la = evil_str_len(sa);
        size_t lb = evil_str_len(sb);
        char* buf = (char*)evil_alloc_atomic(la + lb + 1);
        memcpy(buf, evil_str_chars(sa), la);
        memcpy(buf + la, evil_str_chars(sb), lb);
        buf[la + lb] = '\0';
        return evil_str_create(buf, la + lb);
    }
    /* Metamethod __add */
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_ADD)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__add"));
        if (evil_is_fn(fn)) {
            evil_value_t args[2] = { a, b };
            return evil_call(f, fn, a, 2, args);
        }
    }
    if (b.type == EVIL_TYPE_TABLE && b.as.table->metatable && (b.as.table->metatable->metamethod_flags & META_ADD)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(b.as.table->metatable), evil_str_intern("__add"));
        if (evil_is_fn(fn)) {
            evil_value_t args[2] = { a, b };
            return evil_call(f, fn, b, 2, args);
        }
    }
    if (a.type == EVIL_TYPE_NUMBER && b.type == EVIL_TYPE_NUMBER) {
        return evil_val_number(a.as.number + b.as.number);
    }
    evil_throw(f, evil_error_create(evil_str_intern("Type mismatch in arithmetic addition."), evil_val_nil()));
    return evil_val_nil();
}

evil_value_t evil_val_sub(evil_fiber_t* f, evil_value_t a, evil_value_t b) {
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_SUB)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__sub"));
        if (evil_is_fn(fn)) {
            evil_value_t args[2] = { a, b };
            return evil_call(f, fn, a, 2, args);
        }
    }
    if (a.type == EVIL_TYPE_NUMBER && b.type == EVIL_TYPE_NUMBER) {
        return evil_val_number(a.as.number - b.as.number);
    }
    evil_throw(f, evil_error_create(evil_str_intern("Type mismatch in arithmetic subtraction."), evil_val_nil()));
    return evil_val_nil();
}

evil_value_t evil_val_mul(evil_fiber_t* f, evil_value_t a, evil_value_t b) {
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_MUL)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__mul"));
        if (evil_is_fn(fn)) {
            evil_value_t args[2] = { a, b };
            return evil_call(f, fn, a, 2, args);
        }
    }
    if (a.type == EVIL_TYPE_NUMBER && b.type == EVIL_TYPE_NUMBER) {
        return evil_val_number(a.as.number * b.as.number);
    }
    evil_throw(f, evil_error_create(evil_str_intern("Type mismatch in arithmetic multiplication."), evil_val_nil()));
    return evil_val_nil();
}

evil_value_t evil_val_div(evil_fiber_t* f, evil_value_t a, evil_value_t b) {
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_DIV)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__div"));
        if (evil_is_fn(fn)) {
            evil_value_t args[2] = { a, b };
            return evil_call(f, fn, a, 2, args);
        }
    }
    if (a.type == EVIL_TYPE_NUMBER && b.type == EVIL_TYPE_NUMBER) {
        return evil_val_number(a.as.number / b.as.number);
    }
    evil_throw(f, evil_error_create(evil_str_intern("Type mismatch in arithmetic division."), evil_val_nil()));
    return evil_val_nil();
}

evil_value_t evil_val_mod(evil_fiber_t* f, evil_value_t a, evil_value_t b) {
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_MOD)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__mod"));
        if (evil_is_fn(fn)) {
            evil_value_t args[2] = { a, b };
            return evil_call(f, fn, a, 2, args);
        }
    }
    if (a.type == EVIL_TYPE_NUMBER && b.type == EVIL_TYPE_NUMBER) {
        double res = a.as.number - floor(a.as.number / b.as.number) * b.as.number;
        return evil_val_number(res);
    }
    return evil_val_nil();
}

evil_value_t evil_val_neg(evil_fiber_t* f, evil_value_t a) {
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_ANEG)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__aneg"));
        if (evil_is_fn(fn)) {
            evil_value_t args[1] = { a };
            return evil_call(f, fn, a, 1, args);
        }
    }
    if (a.type == EVIL_TYPE_NUMBER) {
        return evil_val_number(-a.as.number);
    }
    return evil_val_nil();
}

evil_value_t evil_val_lnot(evil_fiber_t* f, evil_value_t a) {
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_LNOT)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__lnot"));
        if (evil_is_fn(fn)) {
            evil_value_t args[1] = { a };
            return evil_call(f, fn, a, 1, args);
        }
    }
    return evil_val_bool(!evil_is_truth(a));
}

evil_value_t evil_val_land(evil_fiber_t* f, evil_value_t a, evil_value_t b) {
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_LAND)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__land"));
        if (evil_is_fn(fn)) {
            evil_value_t args[2] = { a, b };
            return evil_call(f, fn, a, 2, args);
        }
    }
    if (b.type == EVIL_TYPE_TABLE && b.as.table->metatable && (b.as.table->metatable->metamethod_flags & META_LAND)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(b.as.table->metatable), evil_str_intern("__land"));
        if (evil_is_fn(fn)) {
            evil_value_t args[2] = { a, b };
            return evil_call(f, fn, b, 2, args);
        }
    }
    return evil_is_truth(a) ? b : a;
}

evil_value_t evil_val_lor(evil_fiber_t* f, evil_value_t a, evil_value_t b) {
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_LOR)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__lor"));
        if (evil_is_fn(fn)) {
            evil_value_t args[2] = { a, b };
            return evil_call(f, fn, a, 2, args);
        }
    }
    if (b.type == EVIL_TYPE_TABLE && b.as.table->metatable && (b.as.table->metatable->metamethod_flags & META_LOR)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(b.as.table->metatable), evil_str_intern("__lor"));
        if (evil_is_fn(fn)) {
            evil_value_t args[2] = { a, b };
            return evil_call(f, fn, b, 2, args);
        }
    }
    return evil_is_truth(a) ? a : b;
}

evil_value_t evil_val_inc(evil_fiber_t* f, evil_value_t a) {
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_INC)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__inc"));
        if (evil_is_fn(fn)) {
            evil_value_t args[1] = { a };
            return evil_call(f, fn, a, 1, args);
        }
    }
    if (a.type == EVIL_TYPE_NUMBER) {
        return evil_val_number(a.as.number + 1.0);
    }
    return a;
}

evil_value_t evil_val_dec(evil_fiber_t* f, evil_value_t a) {
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_DEC)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__dec"));
        if (evil_is_fn(fn)) {
            evil_value_t args[1] = { a };
            return evil_call(f, fn, a, 1, args);
        }
    }
    if (a.type == EVIL_TYPE_NUMBER) {
        return evil_val_number(a.as.number - 1.0);
    }
    return a;
}

evil_value_t evil_val_band(evil_fiber_t* f, evil_value_t a, evil_value_t b) {
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_BAND)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__band"));
        if (evil_is_fn(fn)) {
            evil_value_t args[2] = { a, b };
            return evil_call(f, fn, a, 2, args);
        }
    }
    if (a.type == EVIL_TYPE_NUMBER && b.type == EVIL_TYPE_NUMBER) {
        return evil_val_number((double)((int64_t)a.as.number & (int64_t)b.as.number));
    }
    return evil_val_nil();
}

evil_value_t evil_val_bor(evil_fiber_t* f, evil_value_t a, evil_value_t b) {
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_BOR)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__bor"));
        if (evil_is_fn(fn)) {
            evil_value_t args[2] = { a, b };
            return evil_call(f, fn, a, 2, args);
        }
    }
    if (a.type == EVIL_TYPE_NUMBER && b.type == EVIL_TYPE_NUMBER) {
        return evil_val_number((double)((int64_t)a.as.number | (int64_t)b.as.number));
    }
    return evil_val_nil();
}

evil_value_t evil_val_bxor(evil_fiber_t* f, evil_value_t a, evil_value_t b) {
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_BXOR)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__bxor"));
        if (evil_is_fn(fn)) {
            evil_value_t args[2] = { a, b };
            return evil_call(f, fn, a, 2, args);
        }
    }
    if (a.type == EVIL_TYPE_NUMBER && b.type == EVIL_TYPE_NUMBER) {
        return evil_val_number((double)((int64_t)a.as.number ^ (int64_t)b.as.number));
    }
    return evil_val_nil();
}

evil_value_t evil_val_bnot(evil_fiber_t* f, evil_value_t a) {
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_BNOT)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__bnot"));
        if (evil_is_fn(fn)) {
            evil_value_t args[1] = { a };
            return evil_call(f, fn, a, 1, args);
        }
    }
    if (a.type == EVIL_TYPE_NUMBER) {
        return evil_val_number((double)(~(int64_t)a.as.number));
    }
    return evil_val_nil();
}

evil_value_t evil_val_shl(evil_fiber_t* f, evil_value_t a, evil_value_t b) {
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_SHL)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__shl"));
        if (evil_is_fn(fn)) {
            evil_value_t args[2] = { a, b };
            return evil_call(f, fn, a, 2, args);
        }
    }
    if (a.type == EVIL_TYPE_NUMBER && b.type == EVIL_TYPE_NUMBER) {
        return evil_val_number((double)((int64_t)a.as.number << (int64_t)b.as.number));
    }
    if (a.type == EVIL_TYPE_STRING && b.type == EVIL_TYPE_NUMBER) {
        const char* s = evil_str_chars(a);
        size_t len = evil_str_len(a);
        size_t shift = (size_t)b.as.number;
        if (shift >= len) return evil_str_intern("");
        return evil_str_create(s + shift, len - shift);
    }
    return evil_val_nil();
}

evil_value_t evil_val_shr(evil_fiber_t* f, evil_value_t a, evil_value_t b) {
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_SHR)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__shr"));
        if (evil_is_fn(fn)) {
            evil_value_t args[2] = { a, b };
            return evil_call(f, fn, a, 2, args);
        }
    }
    if (a.type == EVIL_TYPE_NUMBER && b.type == EVIL_TYPE_NUMBER) {
        return evil_val_number((double)((int64_t)a.as.number >> (int64_t)b.as.number));
    }
    if (a.type == EVIL_TYPE_STRING && b.type == EVIL_TYPE_NUMBER) {
        const char* s = evil_str_chars(a);
        size_t len = evil_str_len(a);
        size_t shift = (size_t)b.as.number;
        if (shift >= len) return evil_str_intern("");
        return evil_str_create(s, len - shift);
    }
    return evil_val_nil();
}

evil_value_t evil_val_eq(evil_fiber_t* f, evil_value_t a, evil_value_t b) {
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_EQ)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__eq"));
        if (evil_is_fn(fn)) {
            evil_value_t args[2] = { a, b };
            return evil_call(f, fn, a, 2, args);
        }
    }
    if (a.type != b.type) return evil_val_bool(false);
    switch (a.type) {
        case EVIL_TYPE_NIL: return evil_val_bool(true);
        case EVIL_TYPE_BOOLEAN: return evil_val_bool(a.as.boolean == b.as.boolean);
        case EVIL_TYPE_NUMBER: return evil_val_bool(a.as.number == b.as.number);
        case EVIL_TYPE_STRING:
            return evil_val_bool(a.as.string == b.as.string ||
                                 (a.as.string->length == b.as.string->length &&
                                  memcmp(a.as.string->chars, b.as.string->chars, a.as.string->length) == 0));
        case EVIL_TYPE_TABLE: return evil_val_bool(a.as.table == b.as.table);
        case EVIL_TYPE_ARRAY: return evil_val_bool(a.as.array == b.as.array);
        case EVIL_TYPE_FUNCTION: return evil_val_bool(a.as.closure == b.as.closure);
        case EVIL_TYPE_TYPE: return evil_val_bool(a.as.type_code == b.as.type_code);
        case EVIL_TYPE_NATIVE_OBJECT: return evil_val_bool(a.as.native_obj == b.as.native_obj);
        default: return evil_val_bool(false);
    }
}

evil_value_t evil_val_ne(evil_fiber_t* f, evil_value_t a, evil_value_t b) {
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_NE)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__ne"));
        if (evil_is_fn(fn)) {
            evil_value_t args[2] = { a, b };
            return evil_call(f, fn, a, 2, args);
        }
    }
    evil_value_t eq = evil_val_eq(f, a, b);
    return evil_val_bool(!evil_is_truth(eq));
}

evil_value_t evil_val_deq(evil_fiber_t* f, evil_value_t a, evil_value_t b) {
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_DEQ)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__deq"));
        if (evil_is_fn(fn)) {
            evil_value_t args[2] = { a, b };
            return evil_call(f, fn, a, 2, args);
        }
    }
    if (a.type == EVIL_TYPE_TABLE && b.type == EVIL_TYPE_TABLE) {
        evil_table_t* ta = a.as.table;
        evil_table_t* tb = b.as.table;
        if (ta == tb) return evil_val_bool(true);
        if (evil_table_len(a) != evil_table_len(b)) return evil_val_bool(false);

        evil_table_iter_t it = evil_table_iter_begin(ta);
        evil_value_t k, v;
        while (evil_table_iter_next(&it, &k, &v)) {
            evil_value_t bv = evil_table_rawget(b, k);
            evil_value_t sub_eq = evil_val_deq(f, v, bv);
            if (!evil_is_truth(sub_eq)) return evil_val_bool(false);
        }
        return evil_val_bool(true);
    }
    if (a.type == EVIL_TYPE_ARRAY && b.type == EVIL_TYPE_ARRAY) {
        evil_array_t* aa = a.as.array;
        evil_array_t* ab = b.as.array;
        if (aa == ab) return evil_val_bool(true);
        if (aa->length != ab->length) return evil_val_bool(false);
        for (uint32_t i = 0; i < aa->length; i++) {
            evil_value_t sub_eq = evil_val_deq(f, aa->items[i], ab->items[i]);
            if (!evil_is_truth(sub_eq)) return evil_val_bool(false);
        }
        return evil_val_bool(true);
    }
    return evil_val_eq(f, a, b);
}

evil_value_t evil_val_dne(evil_fiber_t* f, evil_value_t a, evil_value_t b) {
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_DNE)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__dne"));
        if (evil_is_fn(fn)) {
            evil_value_t args[2] = { a, b };
            return evil_call(f, fn, a, 2, args);
        }
    }
    evil_value_t deq = evil_val_deq(f, a, b);
    return evil_val_bool(!evil_is_truth(deq));
}

evil_value_t evil_val_lt(evil_fiber_t* f, evil_value_t a, evil_value_t b) {
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_LT)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__lt"));
        if (evil_is_fn(fn)) {
            evil_value_t args[2] = { a, b };
            return evil_call(f, fn, a, 2, args);
        }
    }
    if (a.type == EVIL_TYPE_NUMBER && b.type == EVIL_TYPE_NUMBER) {
        return evil_val_bool(a.as.number < b.as.number);
    }
    return evil_val_bool(false);
}

evil_value_t evil_val_lte(evil_fiber_t* f, evil_value_t a, evil_value_t b) {
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_LTE)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__lte"));
        if (evil_is_fn(fn)) {
            evil_value_t args[2] = { a, b };
            return evil_call(f, fn, a, 2, args);
        }
    }
    if (a.type == EVIL_TYPE_NUMBER && b.type == EVIL_TYPE_NUMBER) {
        return evil_val_bool(a.as.number <= b.as.number);
    }
    return evil_val_bool(false);
}

evil_value_t evil_val_gt(evil_fiber_t* f, evil_value_t a, evil_value_t b) {
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_GT)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__gt"));
        if (evil_is_fn(fn)) {
            evil_value_t args[2] = { a, b };
            return evil_call(f, fn, a, 2, args);
        }
    }
    if (a.type == EVIL_TYPE_NUMBER && b.type == EVIL_TYPE_NUMBER) {
        return evil_val_bool(a.as.number > b.as.number);
    }
    return evil_val_bool(false);
}

evil_value_t evil_val_gte(evil_fiber_t* f, evil_value_t a, evil_value_t b) {
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_GTE)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__gte"));
        if (evil_is_fn(fn)) {
            evil_value_t args[2] = { a, b };
            return evil_call(f, fn, a, 2, args);
        }
    }
    if (a.type == EVIL_TYPE_NUMBER && b.type == EVIL_TYPE_NUMBER) {
        return evil_val_bool(a.as.number >= b.as.number);
    }
    return evil_val_bool(false);
}

evil_value_t evil_val_in(evil_fiber_t* f, evil_value_t item, evil_value_t coll) {
    if (coll.type == EVIL_TYPE_STRING && item.type == EVIL_TYPE_STRING) {
        return evil_val_bool(strstr(evil_str_chars(coll), evil_str_chars(item)) != NULL);
    }
    if (coll.type == EVIL_TYPE_TABLE && coll.as.table) {
        evil_table_t* t = coll.as.table;
        if (t->metatable && (t->metatable->metamethod_flags & META_EXISTS)) {
            evil_value_t meta_fn = evil_table_rawget(evil_val_table(t->metatable), evil_str_intern("__exists"));
            if (evil_is_fn(meta_fn)) {
                evil_value_t args[2] = { coll, item };
                return evil_call(f, meta_fn, coll, 2, args);
            }
        }
        return evil_val_bool(evil_table_exists(coll, item));
    }
    if (coll.type == EVIL_TYPE_ARRAY && coll.as.array) {
        evil_array_t* a = coll.as.array;
        for (uint32_t i = 0; i < a->length; i++) {
            if (evil_is_truth(evil_val_deq(f, item, a->items[i]))) {
                return evil_val_bool(true);
            }
        }
        return evil_val_bool(false);
    }
    return evil_val_bool(false);
}

evil_value_t evil_val_len(evil_fiber_t* f, evil_value_t a) {
    if (a.type == EVIL_TYPE_STRING) {
        const char* s = evil_str_chars(a);
        size_t byte_len = evil_str_len(a);
        size_t count = 0;
        for (size_t i = 0; i < byte_len; i++) {
            if (((unsigned char)s[i] & 0xC0) != 0x80) count++;
        }
        return evil_val_number((double)count);
    }
    if (a.type == EVIL_TYPE_ARRAY) return evil_val_number((double)evil_array_len(a));
    if (a.type == EVIL_TYPE_TABLE && a.as.table) {
        evil_table_t* t = a.as.table;
        if (t->metatable && (t->metatable->metamethod_flags & META_LEN)) {
            evil_value_t meta_fn = evil_table_rawget(evil_val_table(t->metatable), evil_str_intern("__len"));
            if (evil_is_fn(meta_fn)) {
                evil_value_t args[1] = { a };
                return evil_call(f, meta_fn, a, 1, args);
            }
        }
        return evil_val_number((double)(t->array_count + t->hash_count));
    }
    return evil_val_number(0);
}

evil_value_t evil_val_tostr(evil_fiber_t* f, evil_value_t a) {
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_TOSTR)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__tostr"));
        if (evil_is_fn(fn)) {
            evil_value_t args[1] = { a };
            return evil_call(f, fn, a, 1, args);
        }
    }
    char buf[64];
    switch (a.type) {
        case EVIL_TYPE_NIL: return evil_str_intern("nil");
        case EVIL_TYPE_BOOLEAN: return evil_str_intern(a.as.boolean ? "true" : "false");
        case EVIL_TYPE_NUMBER: {
            if (isnan(a.as.number)) return evil_str_intern("NaN");
            if (isinf(a.as.number)) return evil_str_intern(a.as.number > 0 ? "Infinity" : "-Infinity");
            if (a.as.number == (double)(int64_t)a.as.number) {
                snprintf(buf, sizeof(buf), "%lld", (long long)a.as.number);
            } else {
                snprintf(buf, sizeof(buf), "%g", a.as.number);
            }
            return evil_str_from_cstr(buf);
        }
        case EVIL_TYPE_STRING: return a;
        case EVIL_TYPE_TABLE: return evil_str_intern("<Table>");
        case EVIL_TYPE_ARRAY: return evil_str_intern("<Array>");
        case EVIL_TYPE_FUNCTION: return evil_str_intern("<Function>");
        case EVIL_TYPE_TYPE: {
            switch (a.as.type_code) {
                case EVIL_TYPE_NIL: return evil_str_intern("Nil");
                case EVIL_TYPE_NUMBER: return evil_str_intern("Number");
                case EVIL_TYPE_STRING: return evil_str_intern("String");
                case EVIL_TYPE_BOOLEAN: return evil_str_intern("Boolean");
                case EVIL_TYPE_TABLE: return evil_str_intern("Table");
                case EVIL_TYPE_ARRAY: return evil_str_intern("Array");
                case EVIL_TYPE_FIBER: return evil_str_intern("Fiber");
                case EVIL_TYPE_FUNCTION: return evil_str_intern("Function");
                case EVIL_TYPE_ERROR: return evil_str_intern("Error");
                case EVIL_TYPE_TYPE: return evil_str_intern("Type");
                case EVIL_TYPE_NATIVE_FUNCTION: return evil_str_intern("NativeFunction");
                case EVIL_TYPE_NATIVE_OBJECT: return evil_str_intern("NativeObject");
                default: return evil_str_intern("Type");
            }
        }
        case EVIL_TYPE_ERROR: {
            if (a.as.error && a.as.error->data_table.type == EVIL_TYPE_TABLE) {
                evil_value_t msg = evil_table_get_str(a.as.error->data_table, "msg");
                if (msg.type == EVIL_TYPE_STRING) return msg;
            }
            if (a.as.error && a.as.error->message.type == EVIL_TYPE_STRING) return a.as.error->message;
            return evil_str_intern("<Error>");
        }
        case EVIL_TYPE_FIBER: return evil_str_intern("<Fiber>");
        case EVIL_TYPE_NATIVE_FUNCTION: return evil_str_intern("<NativeFunction>");
        case EVIL_TYPE_NATIVE_OBJECT: {
            if (a.as.native_obj && a.as.native_obj->type_desc) {
                return evil_str_from_cstr(a.as.native_obj->type_desc->type_name);
            }
            return evil_str_intern("<NativeObject>");
        }
        default: return evil_str_intern("");
    }
}

evil_value_t evil_val_tonum(evil_fiber_t* f, evil_value_t a) {
    if (a.type == EVIL_TYPE_TABLE && a.as.table->metatable && (a.as.table->metatable->metamethod_flags & META_TONUM)) {
        evil_value_t fn = evil_table_rawget(evil_val_table(a.as.table->metatable), evil_str_intern("__tonum"));
        if (evil_is_fn(fn)) {
            evil_value_t args[1] = { a };
            return evil_call(f, fn, a, 1, args);
        }
    }
    if (a.type == EVIL_TYPE_NUMBER) return a;
    if (a.type == EVIL_TYPE_BOOLEAN) return evil_val_number(a.as.boolean ? 1.0 : 0.0);
    if (a.type == EVIL_TYPE_STRING) {
        char* end;
        double n = strtod(evil_str_chars(a), &end);
        if (end != evil_str_chars(a)) return evil_val_number(n);
    }
    return evil_val_number(0.0);
}

evil_value_t evil_val_typeof(evil_value_t a) {
    return evil_val_type_code(a.type);
}

evil_value_t evil_val_native_typeof(evil_value_t a) {
    if (a.type == EVIL_TYPE_NATIVE_OBJECT && a.as.native_obj && a.as.native_obj->type_desc) {
        return evil_str_intern(a.as.native_obj->type_desc->type_name);
    }
    return evil_val_nil();
}

bool evil_val_is_type(evil_value_t a, evil_type_t tc, const char* native_type_opt) {
    if (tc == EVIL_TYPE_NATIVE_OBJECT) {
        if (a.type != EVIL_TYPE_NATIVE_OBJECT) return false;
        if (native_type_opt) {
            return evil_val_is_native_obj(a, native_type_opt);
        }
        return true;
    }
    return a.type == tc;
}

evil_value_t evil_index_get(evil_fiber_t* f, evil_value_t coll, evil_value_t idx) {
    if (coll.type == EVIL_TYPE_ERROR && coll.as.error) coll = coll.as.error->data_table;
    if (coll.type == EVIL_TYPE_TABLE) {
        return evil_table_get(coll, idx);
    }
    if (coll.type == EVIL_TYPE_ARRAY) {
        if (idx.type != EVIL_TYPE_NUMBER) {
            evil_throw(f, evil_error_create(evil_str_intern("Attempted to index an array with non-number."), evil_val_nil()));
            return evil_val_nil();
        }
        evil_array_t* a = coll.as.array;
        int64_t index = (int64_t)idx.as.number;
        if (!a || index < 0 || (uint64_t)index >= a->length) {
            evil_throw(f, evil_error_create(evil_str_intern("Array index out of bounds."), evil_val_nil()));
            return evil_val_nil();
        }
        return a->items[index];
    }
    if (coll.type == EVIL_TYPE_STRING) {
        if (idx.type == EVIL_TYPE_NUMBER) {
            int64_t i = (int64_t)idx.as.number;
            size_t len = evil_str_len(coll);
            if (i >= 0 && (size_t)i < len) {
                char ch[2] = { evil_str_chars(coll)[i], '\0' };
                return evil_str_from_cstr(ch);
            }
        }
    }
    return evil_val_nil();
}

evil_value_t evil_index_set(evil_fiber_t* f, evil_value_t coll, evil_value_t idx, evil_value_t val) {
    if (coll.type == EVIL_TYPE_ERROR && coll.as.error) coll = coll.as.error->data_table;
    if (coll.type == EVIL_TYPE_TABLE) {
        return evil_table_set(coll, idx, val);
    }
    if (coll.type == EVIL_TYPE_ARRAY) {
        if (idx.type != EVIL_TYPE_NUMBER) {
            evil_throw(f, evil_error_create(evil_str_intern("Attempted to index an array with non-number."), evil_val_nil()));
            return val;
        }
        int64_t index = (int64_t)idx.as.number;
        if (index < 0) {
            evil_throw(f, evil_error_create(evil_str_intern("Array index out of bounds."), evil_val_nil()));
            return val;
        }
        return evil_array_set(coll, index, val);
    }
    return val;
}

/* ========================================================================= */
/* 11. Standard Library Modules                                              */
/* ========================================================================= */

/* Assertion Module */
static evil_value_t mod_assert_approx_equal(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)s;
    if (argc < 2) return evil_val_nil();
    double actual = argv[0].type == EVIL_TYPE_NUMBER ? argv[0].as.number : 0.0;
    double expected = argv[1].type == EVIL_TYPE_NUMBER ? argv[1].as.number : 0.0;
    int precision = (argc >= 3 && argv[2].type == EVIL_TYPE_NUMBER) ? (int)argv[2].as.number : 1;
    double factor = pow(10.0, precision);
    if (round(actual * factor) != round(expected * factor)) {
        char buf[256];
        snprintf(buf, sizeof(buf), "Assertion failed: approx %g != %g", actual, expected);
        evil_throw(f, evil_error_create(evil_str_from_cstr(buf), evil_val_nil()));
    }
    return evil_val_nil();
}

static evil_value_t mod_assert_is_true(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)s;
    if (argc < 1 || !evil_is_truth(argv[0])) {
        const char* msg = (argc >= 2 && evil_is_string(argv[1])) ? evil_str_chars(argv[1]) : "Assertion failed: expected true";
        evil_throw(f, evil_error_create(evil_str_from_cstr(msg), evil_val_nil()));
    }
    return evil_val_nil();
}

static evil_value_t mod_assert_is_false(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)s;
    if (argc < 1 || evil_is_truth(argv[0])) {
        const char* msg = (argc >= 2 && evil_is_string(argv[1])) ? evil_str_chars(argv[1]) : "Assertion failed: expected false";
        evil_throw(f, evil_error_create(evil_str_from_cstr(msg), evil_val_nil()));
    }
    return evil_val_nil();
}

static evil_value_t mod_assert_equal(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)s;
    if (argc < 2) return evil_val_nil();
    evil_value_t eq = evil_val_deq(f, argv[0], argv[1]);
    if (!evil_is_truth(eq)) {
        char buf[256];
        snprintf(buf, sizeof(buf), "Assertion failed: %s != %s",
                 evil_str_chars(evil_val_tostr(f, argv[0])),
                 evil_str_chars(evil_val_tostr(f, argv[1])));
        evil_throw(f, evil_error_create(evil_str_from_cstr(buf), evil_val_nil()));
    }
    return evil_val_nil();
}

static evil_value_t mod_assert_not_equal(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)s;
    if (argc < 2) return evil_val_nil();
    evil_value_t eq = evil_val_deq(f, argv[0], argv[1]);
    if (evil_is_truth(eq)) {
        char buf[256];
        snprintf(buf, sizeof(buf), "Assertion failed: %s == %s (expected not equal)",
                 evil_str_chars(evil_val_tostr(f, argv[0])),
                 evil_str_chars(evil_val_tostr(f, argv[1])));
        evil_throw(f, evil_error_create(evil_str_from_cstr(buf), evil_val_nil()));
    }
    return evil_val_nil();
}

static evil_value_t mod_assert_gt(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)s;
    if (argc >= 2 && !evil_is_truth(evil_val_gt(f, argv[0], argv[1]))) {
        evil_throw(f, evil_error_create(evil_str_intern("Assertion failed: expected greater than"), evil_val_nil()));
    }
    return evil_val_nil();
}

static evil_value_t mod_assert_gte(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)s;
    if (argc >= 2 && !evil_is_truth(evil_val_gte(f, argv[0], argv[1]))) {
        evil_throw(f, evil_error_create(evil_str_intern("Assertion failed: expected greater than or equal"), evil_val_nil()));
    }
    return evil_val_nil();
}

static evil_value_t mod_assert_lt(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)s;
    if (argc >= 2 && !evil_is_truth(evil_val_lt(f, argv[0], argv[1]))) {
        evil_throw(f, evil_error_create(evil_str_intern("Assertion failed: expected less than"), evil_val_nil()));
    }
    return evil_val_nil();
}

static evil_value_t mod_assert_lte(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)s;
    if (argc >= 2 && !evil_is_truth(evil_val_lte(f, argv[0], argv[1]))) {
        evil_throw(f, evil_error_create(evil_str_intern("Assertion failed: expected less than or equal"), evil_val_nil()));
    }
    return evil_val_nil();
}

static evil_value_t mod_assert_is_of_type(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)s;
    if (argc >= 2) {
        evil_type_t expected_tc = (argv[1].type == EVIL_TYPE_TYPE) ? argv[1].as.type_code : (evil_type_t)(int)argv[1].as.number;
        if (argv[0].type != expected_tc) {
            char buf[256];
            snprintf(buf, sizeof(buf), "Assertion failed: type was %d, expected %d", argv[0].type, expected_tc);
            evil_throw(f, evil_error_create(evil_str_from_cstr(buf), evil_val_nil()));
        }
    }
    return evil_val_nil();
}

static evil_value_t mod_assert_throws(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)s;
    if (argc < 1 || !evil_is_fn(argv[0])) {
        evil_throw(f, evil_error_create(evil_str_intern("assert.throws expects a function"), evil_val_nil()));
    }

    evil_try_frame_t frame;
    frame.prev = f->try_top;
    f->try_top = &frame;

    int jmp = setjmp(frame.jmp);
    if (jmp == EVIL_JMP_INIT) {
        evil_call(f, argv[0], evil_val_nil(), 0, NULL);
        f->try_top = frame.prev;
        evil_throw(f, evil_error_create(evil_str_intern("assert.throws: function did not throw"), evil_val_nil()));
    } else if (jmp == EVIL_JMP_THROW) {
        f->try_top = frame.prev;
        /* Threw as expected! */
        return evil_val_nil();
    }
    f->try_top = frame.prev;
    return evil_val_nil();
}

static evil_value_t mod_assert_does_not_throw(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)s;
    if (argc >= 1 && evil_is_fn(argv[0])) {
        evil_call(f, argv[0], evil_val_nil(), 0, NULL);
    }
    return evil_val_nil();
}

static evil_value_t mod_assert_invoke(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    return mod_assert_is_true(f, s, argc, argv);
}

/* IO Module */
static evil_value_t mod_io_println(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)s;
    for (int i = 0; i < argc; i++) {
        if (i > 0) printf("\t");
        printf("%s", evil_str_chars(evil_val_tostr(f, argv[i])));
    }
    printf("\n");
    return evil_val_nil();
}

static evil_value_t mod_io_print(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)s;
    for (int i = 0; i < argc; i++) {
        if (i > 0) printf("\t");
        printf("%s", evil_str_chars(evil_val_tostr(f, argv[i])));
    }
    return evil_val_nil();
}

/* Math Module */
static evil_value_t mod_math_sin(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) { (void)f; (void)s; return evil_val_number(sin(argc > 0 ? argv[0].as.number : 0.0)); }
static evil_value_t mod_math_cos(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) { (void)f; (void)s; return evil_val_number(cos(argc > 0 ? argv[0].as.number : 0.0)); }
static evil_value_t mod_math_tan(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) { (void)f; (void)s; return evil_val_number(tan(argc > 0 ? argv[0].as.number : 0.0)); }
static evil_value_t mod_math_sqrt(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) { (void)f; (void)s; return evil_val_number(sqrt(argc > 0 ? argv[0].as.number : 0.0)); }
static evil_value_t mod_math_abs(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) { (void)f; (void)s; return evil_val_number(fabs(argc > 0 ? argv[0].as.number : 0.0)); }
static evil_value_t mod_math_floor(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) { (void)f; (void)s; return evil_val_number(floor(argc > 0 ? argv[0].as.number : 0.0)); }
static evil_value_t mod_math_ceil(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) { (void)f; (void)s; return evil_val_number(ceil(argc > 0 ? argv[0].as.number : 0.0)); }
static evil_value_t mod_math_round(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc < 1) return evil_val_number(0.0);
    double val = argv[0].type == EVIL_TYPE_NUMBER ? argv[0].as.number : 0.0;
    if (argc >= 2 && argv[1].type == EVIL_TYPE_NUMBER) {
        int decimals = (int)argv[1].as.number;
        double factor = pow(10.0, decimals);
        return evil_val_number(round(val * factor) / factor);
    }
    return evil_val_number(round(val));
}
static evil_value_t mod_math_random(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) { (void)f; (void)s; (void)argc; (void)argv; return evil_val_number((double)rand() / (double)RAND_MAX); }

/* Table Module (tbl) */
static evil_value_t mod_tbl_keys(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc < 1 || argv[0].type != EVIL_TYPE_TABLE) return evil_array_create(0);
    evil_table_t* t = argv[0].as.table;
    evil_value_t arr = evil_array_create(evil_table_len(argv[0]));
    evil_table_iter_t it = evil_table_iter_begin(t);
    evil_value_t k, v;
    while (evil_table_iter_next(&it, &k, &v)) {
        evil_array_push(arr, k);
    }
    return arr;
}

static evil_value_t mod_tbl_values(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc < 1 || argv[0].type != EVIL_TYPE_TABLE) return evil_array_create(0);
    evil_table_t* t = argv[0].as.table;
    evil_value_t arr = evil_array_create(evil_table_len(argv[0]));
    evil_table_iter_t it = evil_table_iter_begin(t);
    evil_value_t k, v;
    while (evil_table_iter_next(&it, &k, &v)) {
        evil_array_push(arr, v);
    }
    return arr;
}

static evil_value_t mod_tbl_pairs(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc < 1 || argv[0].type != EVIL_TYPE_TABLE) return evil_array_create(0);
    evil_table_t* t = argv[0].as.table;
    evil_value_t arr = evil_array_create(evil_table_len(argv[0]));
    evil_table_iter_t it = evil_table_iter_begin(t);
    evil_value_t k, v;
    while (evil_table_iter_next(&it, &k, &v)) {
        evil_value_t pair = evil_array_create(2);
        evil_array_push(pair, k);
        evil_array_push(pair, v);
        evil_array_push(arr, pair);
    }
    return arr;
}

static evil_value_t mod_tbl_merge(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc < 2 || argv[0].type != EVIL_TYPE_TABLE || argv[1].type != EVIL_TYPE_TABLE) return evil_val_nil();
    evil_table_t* t2 = argv[1].as.table;
    evil_table_iter_t it = evil_table_iter_begin(t2);
    evil_value_t k, v;
    while (evil_table_iter_next(&it, &k, &v)) {
        evil_table_set(argv[0], k, v);
    }
    return argv[0];
}

static evil_value_t mod_tbl_freeze(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc >= 1 && argv[0].type == EVIL_TYPE_TABLE && argv[0].as.table) {
        argv[0].as.table->is_frozen = true;
    }
    return argc >= 1 ? argv[0] : evil_val_nil();
}

static evil_value_t mod_tbl_is_frozen(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc >= 1 && argv[0].type == EVIL_TYPE_TABLE && argv[0].as.table) {
        return evil_val_bool(argv[0].as.table->is_frozen);
    }
    return evil_val_bool(false);
}

static evil_value_t mod_tbl_invert(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc < 1 || argv[0].type != EVIL_TYPE_TABLE) return evil_table_create(0, 0);
    evil_table_t* t = argv[0].as.table;
    evil_value_t inv = evil_table_create(0, t->hash_count);
    evil_table_iter_t it = evil_table_iter_begin(t);
    evil_value_t k, v;
    while (evil_table_iter_next(&it, &k, &v)) {
        evil_table_rawset(inv, v, k);
    }
    return inv;
}

static evil_value_t mod_tbl_rawset(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc >= 3) return evil_table_rawset(argv[0], argv[1], argv[2]);
    return evil_val_nil();
}

static evil_value_t mod_tbl_rawget(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc >= 2) return evil_table_rawget(argv[0], argv[1]);
    return evil_val_nil();
}

static evil_value_t mod_tbl_set_mt(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc >= 2) return evil_table_set_meta(argv[0], argv[1]);
    return evil_val_nil();
}

static evil_value_t mod_tbl_get_mt(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc >= 1) return evil_table_get_meta(argv[0]);
    return evil_val_nil();
}

/* Array Module (arr) */
static evil_value_t mod_arr_push(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc >= 2) evil_array_push(argv[0], argv[1]);
    return argc >= 2 ? argv[1] : evil_val_nil();
}

static evil_value_t mod_arr_pop(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc >= 1) return evil_array_pop(argv[0]);
    return evil_val_nil();
}

static evil_value_t mod_arr_shift(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc >= 1) return evil_array_shift(argv[0]);
    return evil_val_nil();
}

static evil_value_t mod_arr_unshift(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc >= 2) evil_array_unshift(argv[0], argv[1]);
    return argc >= 1 ? argv[0] : evil_val_nil();
}

static evil_value_t mod_arr_slice(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc >= 3) {
        return evil_array_slice(argv[0], (int64_t)argv[1].as.number, (int64_t)argv[2].as.number);
    }
    if (argc >= 2) {
        return evil_array_slice(argv[0], (int64_t)argv[1].as.number, evil_array_len(argv[0]));
    }
    return evil_array_create(0);
}

static evil_value_t mod_arr_map(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)s;
    if (argc < 2 || argv[0].type != EVIL_TYPE_ARRAY || !evil_is_fn(argv[1])) return evil_array_create(0);
    evil_array_t* a = argv[0].as.array;
    evil_value_t res = evil_array_create(a->length);
    for (uint32_t i = 0; i < a->length; i++) {
        evil_value_t args[2] = { a->items[i], evil_val_number((double)i) };
        evil_value_t r = evil_call(f, argv[1], evil_val_nil(), 2, args);
        evil_array_push(res, r);
    }
    return res;
}

static evil_value_t mod_arr_filter(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)s;
    if (argc < 2 || argv[0].type != EVIL_TYPE_ARRAY || !evil_is_fn(argv[1])) return evil_array_create(0);
    evil_array_t* a = argv[0].as.array;
    evil_value_t res = evil_array_create(a->length);
    for (uint32_t i = 0; i < a->length; i++) {
        evil_value_t args[2] = { a->items[i], evil_val_number((double)i) };
        evil_value_t r = evil_call(f, argv[1], evil_val_nil(), 2, args);
        if (evil_is_truth(r)) {
            evil_array_push(res, a->items[i]);
        }
    }
    return res;
}

static evil_value_t mod_arr_reduce(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)s;
    if (argc < 2 || argv[0].type != EVIL_TYPE_ARRAY || !evil_is_fn(argv[1])) return evil_val_nil();
    evil_array_t* a = argv[0].as.array;
    if (a->length == 0) return argc >= 3 ? argv[2] : evil_val_nil();

    evil_value_t acc = (argc >= 3) ? argv[2] : a->items[0];
    uint32_t start_idx = (argc >= 3) ? 0 : 1;

    for (uint32_t i = start_idx; i < a->length; i++) {
        evil_value_t args[3] = { acc, a->items[i], evil_val_number((double)i) };
        acc = evil_call(f, argv[1], evil_val_nil(), 3, args);
    }
    return acc;
}

/* Time Module */
static double get_time_ms(void) {
    struct timespec ts;
    clock_gettime(CLOCK_MONOTONIC, &ts);
    return (double)ts.tv_sec * 1000.0 + (double)ts.tv_nsec / 1000000.0;
}

static double get_time_ns(void) {
    struct timespec ts;
    clock_gettime(CLOCK_MONOTONIC, &ts);
    return (double)ts.tv_sec * 1e9 + (double)ts.tv_nsec;
}

static evil_value_t mod_time_now(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s; (void)argc; (void)argv;
    return evil_val_number(get_time_ms());
}

static evil_value_t mod_time_stamp_get(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc >= 2 && argv[1].type == EVIL_TYPE_STRING) {
        const char* prop = evil_str_chars(argv[1]);
        if (strcmp(prop, "ms") == 0) return evil_val_number(get_time_ms());
        if (strcmp(prop, "ns") == 0) return evil_val_number(get_time_ns());
        if (strcmp(prop, "secs") == 0) return evil_val_number(get_time_ms() / 1000.0);
    }
    return evil_val_nil();
}

static evil_value_t mod_time_sleep(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc >= 1 && argv[0].type == EVIL_TYPE_NUMBER) {
        usleep((useconds_t)(argv[0].as.number * 1000.0));
    }
    return evil_val_nil();
}

/* Filesystem Module (fs) */
static evil_value_t mod_fs_file_exists(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc < 1 || !evil_is_string(argv[0])) return evil_val_bool(false);
    return evil_val_bool(access(evil_str_chars(argv[0]), F_OK) == 0);
}

static evil_value_t mod_fs_file_get_text(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc < 1 || !evil_is_string(argv[0])) return evil_val_nil();
    FILE* file = fopen(evil_str_chars(argv[0]), "rb");
    if (!file) return evil_val_nil();
    fseek(file, 0, SEEK_END);
    long size = ftell(file);
    fseek(file, 0, SEEK_SET);
    char* buf = (char*)evil_alloc_atomic(size + 1);
    fread(buf, 1, size, file);
    buf[size] = '\0';
    fclose(file);
    return evil_str_create(buf, size);
}

static evil_value_t mod_fs_path_cmb(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc < 2) return argc == 1 ? argv[0] : evil_val_nil();
    const char* p1 = evil_str_chars(argv[0]);
    const char* p2 = evil_str_chars(argv[1]);
    char buf[1024];
    snprintf(buf, sizeof(buf), "%s/%s", p1, p2);
    return evil_str_from_cstr(buf);
}

/* Test Hooks / Dummies */
static evil_value_t hook_native_func(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    return argc >= 4 ? argv[3] : evil_val_nil();
}

static const evil_native_type_t dummy_native_type_1 = {
    .type_name = "EVIL.Ceres.LanguageTests.DummyNativeClass",
    .destructor = NULL
};

static const evil_native_type_t dummy_native_type_provider = {
    .type_name = "SomeNativeTypeLol",
    .destructor = NULL
};

static evil_value_t hook_new_dummy_object(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s; (void)argc; (void)argv;
    return evil_native_obj_create(&dummy_native_type_1, NULL);
}

static evil_value_t hook_new_native_type_provider_object(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s; (void)argc; (void)argv;
    return evil_native_obj_create(&dummy_native_type_provider, NULL);
}

static int tricky_counter = 0;
static evil_value_t tricky_get(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc >= 2 && argv[1].type == EVIL_TYPE_STRING && strcmp(evil_str_chars(argv[1]), "d20") == 0) {
        if (tricky_counter++ < 3) return evil_val_nil();
        return evil_val_number((double)(rand() % 21));
    }
    return evil_val_nil();
}
static evil_value_t tricky_set(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s; (void)argc; (void)argv;
    return evil_val_nil();
}
static evil_value_t tricky_exists(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc >= 2 && argv[1].type == EVIL_TYPE_STRING && strcmp(evil_str_chars(argv[1]), "d20") == 0) {
        return evil_val_bool(false);
    }
    return evil_val_bool(false);
}

static evil_value_t hook_throw_test(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)s;
    if (argc >= 1) evil_throw(f, argv[0]);
    return evil_val_nil();
}

/* Runtime Creation & Module Initialization */
evil_runtime_t* evil_runtime_create(void) {
    evil_runtime_init_gc();
    evil_runtime_t* rt = (evil_runtime_t*)evil_alloc(sizeof(evil_runtime_t));
    rt->global = evil_table_create(0, 64);
    rt->main_fiber = evil_fiber_create();
    g_active_rt = rt;
    return rt;
}

/* Convert Module (cnv) */
static evil_value_t mod_cnv_u64(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc < 1 || argv[0].type != EVIL_TYPE_NUMBER) return evil_val_number(0);
    return evil_val_number((double)(uint64_t)argv[0].as.number);
}
static evil_value_t mod_cnv_u32(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc < 1 || argv[0].type != EVIL_TYPE_NUMBER) return evil_val_number(0);
    return evil_val_number((double)(uint32_t)argv[0].as.number);
}
static evil_value_t mod_cnv_u16(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc < 1 || argv[0].type != EVIL_TYPE_NUMBER) return evil_val_number(0);
    return evil_val_number((double)(uint16_t)argv[0].as.number);
}
static evil_value_t mod_cnv_u8(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc < 1 || argv[0].type != EVIL_TYPE_NUMBER) return evil_val_number(0);
    return evil_val_number((double)(uint8_t)argv[0].as.number);
}
static evil_value_t mod_cnv_i64(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc < 1 || argv[0].type != EVIL_TYPE_NUMBER) return evil_val_number(0);
    return evil_val_number((double)(int64_t)argv[0].as.number);
}
static evil_value_t mod_cnv_i32(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc < 1 || argv[0].type != EVIL_TYPE_NUMBER) return evil_val_number(0);
    return evil_val_number((double)(int32_t)argv[0].as.number);
}
static evil_value_t mod_cnv_i16(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc < 1 || argv[0].type != EVIL_TYPE_NUMBER) return evil_val_number(0);
    return evil_val_number((double)(int16_t)argv[0].as.number);
}
static evil_value_t mod_cnv_i8(evil_fiber_t* f, evil_value_t s, int argc, evil_value_t* argv) {
    (void)f; (void)s;
    if (argc < 1 || argv[0].type != EVIL_TYPE_NUMBER) return evil_val_number(0);
    return evil_val_number((double)(int8_t)argv[0].as.number);
}

static evil_value_t mod_require(evil_fiber_t* f, evil_value_t self, int argc, evil_value_t* argv) {
    (void)self;
    if (argc < 1 || argv[0].type != EVIL_TYPE_STRING) {
        evil_throw(f, evil_error_create(evil_str_from_cstr("require: expected string module path"), evil_val_nil()));
        return evil_val_nil();
    }
    const char* path = evil_str_chars(argv[0]);
    char msg[2048];
    snprintf(msg, sizeof(msg),
        "require: Unable to find the requested file '%s' in any of the known lookup paths:\n"
        "  %s.vil\n"
        "  %s/init.vil\n"
        "  %s/%s.vil\n",
        path, path, path, path, path);
    evil_throw(f, evil_error_create(evil_str_from_cstr(msg), evil_val_nil()));
    return evil_val_nil();
}

static evil_value_t mod_dofile(evil_fiber_t* f, evil_value_t self, int argc, evil_value_t* argv) {
    (void)self;
    if (argc < 1 || argv[0].type != EVIL_TYPE_STRING) {
        evil_throw(f, evil_error_create(evil_str_from_cstr("dofile: expected string file path"), evil_val_nil()));
        return evil_val_nil();
    }
    const char* path = evil_str_chars(argv[0]);
    char msg[2048];
    snprintf(msg, sizeof(msg),
        "dofile: Unable to find the requested file '%s' in any of the known lookup paths:\n"
        "  %s\n",
        path, path);
    evil_throw(f, evil_error_create(evil_str_from_cstr(msg), evil_val_nil()));
    return evil_val_nil();
}

void evil_runtime_init_globals(evil_runtime_t* rt, int argc, char** argv) {
    (void)argc;
    (void)argv;
    evil_value_t g = rt->global;

    /* Global Assert Module */
    evil_value_t assert_mod = evil_table_create(0, 16);
    evil_table_set_str(assert_mod, "is_true", evil_val_native_fn(mod_assert_is_true));
    evil_table_set_str(assert_mod, "is_false", evil_val_native_fn(mod_assert_is_false));
    evil_table_set_str(assert_mod, "equal", evil_val_native_fn(mod_assert_equal));
    evil_table_set_str(assert_mod, "approx_equal", evil_val_native_fn(mod_assert_approx_equal));
    evil_table_set_str(assert_mod, "not_equal", evil_val_native_fn(mod_assert_not_equal));
    evil_table_set_str(assert_mod, "greater_than", evil_val_native_fn(mod_assert_gt));
    evil_table_set_str(assert_mod, "greater_than_or_equal", evil_val_native_fn(mod_assert_gte));
    evil_table_set_str(assert_mod, "less_than", evil_val_native_fn(mod_assert_lt));
    evil_table_set_str(assert_mod, "less_than_or_equal", evil_val_native_fn(mod_assert_lte));
    evil_table_set_str(assert_mod, "is_of_type", evil_val_native_fn(mod_assert_is_of_type));
    evil_table_set_str(assert_mod, "throws", evil_val_native_fn(mod_assert_throws));
    evil_table_set_str(assert_mod, "does_not_throw", evil_val_native_fn(mod_assert_does_not_throw));

    /* Metatable for assert(expr) -> assert.is_true(expr) */
    evil_value_t assert_mt = evil_table_create(0, 4);
    evil_table_set_str(assert_mt, "__invoke", evil_val_native_fn(mod_assert_invoke));
    evil_table_set_meta(assert_mod, assert_mt);
    evil_table_set_str(g, "assert", assert_mod);

    /* IO Module */
    evil_value_t io_mod = evil_table_create(0, 8);
    evil_table_set_str(io_mod, "println", evil_val_native_fn(mod_io_println));
    evil_table_set_str(io_mod, "print", evil_val_native_fn(mod_io_print));
    evil_table_set_str(g, "io", io_mod);

    /* Built-in import functions */
    evil_table_set_str(g, "require", evil_val_native_fn(mod_require));
    evil_table_set_str(g, "dofile", evil_val_native_fn(mod_dofile));

    /* Math Module */
    evil_value_t math_mod = evil_table_create(0, 16);
    evil_table_set_str(math_mod, "sin", evil_val_native_fn(mod_math_sin));
    evil_table_set_str(math_mod, "cos", evil_val_native_fn(mod_math_cos));
    evil_table_set_str(math_mod, "tan", evil_val_native_fn(mod_math_tan));
    evil_table_set_str(math_mod, "sqrt", evil_val_native_fn(mod_math_sqrt));
    evil_table_set_str(math_mod, "abs", evil_val_native_fn(mod_math_abs));
    evil_table_set_str(math_mod, "floor", evil_val_native_fn(mod_math_floor));
    evil_table_set_str(math_mod, "ceil", evil_val_native_fn(mod_math_ceil));
    evil_table_set_str(math_mod, "round", evil_val_native_fn(mod_math_round));
    evil_table_set_str(math_mod, "random", evil_val_native_fn(mod_math_random));
    evil_table_set_str(math_mod, "pi", evil_val_number(M_PI));
    evil_table_set_str(math_mod, "e", evil_val_number(M_E));
    evil_table_set_str(g, "math", math_mod);

    /* Table Module (tbl) */
    evil_value_t tbl_mod = evil_table_create(0, 16);
    evil_table_set_str(tbl_mod, "keys", evil_val_native_fn(mod_tbl_keys));
    evil_table_set_str(tbl_mod, "values", evil_val_native_fn(mod_tbl_values));
    evil_table_set_str(tbl_mod, "pairs", evil_val_native_fn(mod_tbl_pairs));
    evil_table_set_str(tbl_mod, "merge", evil_val_native_fn(mod_tbl_merge));
    evil_table_set_str(tbl_mod, "freeze", evil_val_native_fn(mod_tbl_freeze));
    evil_table_set_str(tbl_mod, "is_frozen", evil_val_native_fn(mod_tbl_is_frozen));
    evil_table_set_str(tbl_mod, "invert", evil_val_native_fn(mod_tbl_invert));
    evil_table_set_str(tbl_mod, "rawset", evil_val_native_fn(mod_tbl_rawset));
    evil_table_set_str(tbl_mod, "rawget", evil_val_native_fn(mod_tbl_rawget));
    evil_table_set_str(tbl_mod, "set_mt", evil_val_native_fn(mod_tbl_set_mt));
    evil_table_set_str(tbl_mod, "get_mt", evil_val_native_fn(mod_tbl_get_mt));
    evil_table_set_str(g, "tbl", tbl_mod);
    evil_table_set_str(g, "table", tbl_mod);

    /* Convert Module (cnv) */
    evil_value_t cnv_mod = evil_table_create(0, 16);
    evil_table_set_str(cnv_mod, "u64", evil_val_native_fn(mod_cnv_u64));
    evil_table_set_str(cnv_mod, "u32", evil_val_native_fn(mod_cnv_u32));
    evil_table_set_str(cnv_mod, "u16", evil_val_native_fn(mod_cnv_u16));
    evil_table_set_str(cnv_mod, "u8", evil_val_native_fn(mod_cnv_u8));
    evil_table_set_str(cnv_mod, "i64", evil_val_native_fn(mod_cnv_i64));
    evil_table_set_str(cnv_mod, "i32", evil_val_native_fn(mod_cnv_i32));
    evil_table_set_str(cnv_mod, "i16", evil_val_native_fn(mod_cnv_i16));
    evil_table_set_str(cnv_mod, "i8", evil_val_native_fn(mod_cnv_i8));
    evil_table_set_str(g, "cnv", cnv_mod);

    /* Array Module (arr) */
    evil_value_t arr_mod = evil_table_create(0, 16);
    evil_table_set_str(arr_mod, "push", evil_val_native_fn(mod_arr_push));
    evil_table_set_str(arr_mod, "pop", evil_val_native_fn(mod_arr_pop));
    evil_table_set_str(arr_mod, "shift", evil_val_native_fn(mod_arr_shift));
    evil_table_set_str(arr_mod, "unshift", evil_val_native_fn(mod_arr_unshift));
    evil_table_set_str(arr_mod, "slice", evil_val_native_fn(mod_arr_slice));
    evil_table_set_str(arr_mod, "map", evil_val_native_fn(mod_arr_map));
    evil_table_set_str(arr_mod, "filter", evil_val_native_fn(mod_arr_filter));
    evil_table_set_str(arr_mod, "reduce", evil_val_native_fn(mod_arr_reduce));
    evil_table_set_str(g, "arr", arr_mod);
    evil_table_set_str(g, "array", arr_mod);

    /* Time Module */
    evil_value_t time_mod = evil_table_create(0, 8);
    evil_value_t stamp_tbl = evil_table_create(0, 4);
    evil_value_t stamp_mt = evil_table_create(0, 4);
    evil_table_set_str(stamp_mt, "__get", evil_val_native_fn(mod_time_stamp_get));
    evil_table_set_meta(stamp_tbl, stamp_mt);
    evil_table_set_str(time_mod, "stamp", stamp_tbl);
    evil_table_set_str(time_mod, "now", evil_val_native_fn(mod_time_now));
    evil_table_set_str(time_mod, "sleep", evil_val_native_fn(mod_time_sleep));
    evil_table_set_str(g, "time", time_mod);

    /* FS Module */
    evil_value_t fs_mod = evil_table_create(0, 8);
    evil_value_t fs_file = evil_table_create(0, 4);
    evil_table_set_str(fs_file, "exists", evil_val_native_fn(mod_fs_file_exists));
    evil_table_set_str(fs_file, "get_text", evil_val_native_fn(mod_fs_file_get_text));
    evil_table_set_str(fs_mod, "file", fs_file);
    evil_value_t fs_path = evil_table_create(0, 4);
    evil_table_set_str(fs_path, "cmb", evil_val_native_fn(mod_fs_path_cmb));
    evil_table_set_str(fs_mod, "path", fs_path);
    evil_table_set_str(g, "fs", fs_mod);

    /* Globals for Type System constants */
    evil_table_set_str(g, "Nil", evil_val_type_code(EVIL_TYPE_NIL));
    evil_table_set_str(g, "Number", evil_val_type_code(EVIL_TYPE_NUMBER));
    evil_table_set_str(g, "String", evil_val_type_code(EVIL_TYPE_STRING));
    evil_table_set_str(g, "Boolean", evil_val_type_code(EVIL_TYPE_BOOLEAN));
    evil_table_set_str(g, "Table", evil_val_type_code(EVIL_TYPE_TABLE));
    evil_table_set_str(g, "Array", evil_val_type_code(EVIL_TYPE_ARRAY));
    evil_table_set_str(g, "Fiber", evil_val_type_code(EVIL_TYPE_FIBER));
    evil_table_set_str(g, "Function", evil_val_type_code(EVIL_TYPE_FUNCTION));
    evil_table_set_str(g, "Fn", evil_val_type_code(EVIL_TYPE_FUNCTION));
    evil_table_set_str(g, "Error", evil_val_type_code(EVIL_TYPE_ERROR));
    evil_table_set_str(g, "Type", evil_val_type_code(EVIL_TYPE_TYPE));
    evil_table_set_str(g, "NativeFunction", evil_val_type_code(EVIL_TYPE_NATIVE_FUNCTION));
    evil_table_set_str(g, "NativeObject", evil_val_type_code(EVIL_TYPE_NATIVE_OBJECT));

    /* Test Hooks matching Ceres TestRunner */
    evil_table_set_str(g, "__native_func", evil_val_native_fn(hook_native_func));
    evil_table_set_str(g, "__native_object", evil_native_obj_create(&dummy_native_type_1, NULL));
    evil_table_set_str(g, "__new_dummy_object", evil_val_native_fn(hook_new_dummy_object));
    evil_table_set_str(g, "__new_dummy_object_2", evil_val_native_fn(hook_new_dummy_object));
    evil_table_set_str(g, "__new_native_type_provider_object", evil_val_native_fn(hook_new_native_type_provider_object));
    evil_table_set_str(g, "__throw_test", evil_val_native_fn(hook_throw_test));

    /* Tricky Table Hook */
    evil_value_t tricky_tbl = evil_table_create(0, 4);
    evil_value_t tricky_mt = evil_table_create(0, 4);
    evil_table_set_str(tricky_mt, "__get", evil_val_native_fn(tricky_get));
    evil_table_set_str(tricky_mt, "__set", evil_val_native_fn(tricky_set));
    evil_table_set_str(tricky_mt, "__exists", evil_val_native_fn(tricky_exists));
    evil_table_set_meta(tricky_tbl, tricky_mt);
    evil_table_set_str(g, "__tricky", tricky_tbl);
}

evil_value_t evil_runtime_get_global(evil_runtime_t* rt, const char* name) {
    if (!rt) return evil_val_nil();
    return evil_table_get_str(rt->global, name);
}

void evil_runtime_set_global(evil_runtime_t* rt, const char* name, evil_value_t val) {
    if (!rt) return;
    evil_table_set_str(rt->global, name, val);
}
