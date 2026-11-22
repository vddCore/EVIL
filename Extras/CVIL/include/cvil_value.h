#ifndef CVIL_VALUE_H
#define CVIL_VALUE_H

#include <stdint.h>
#include <stdbool.h>
#include <stddef.h>

typedef enum {
    EVIL_TYPE_NIL = 0,
    EVIL_TYPE_NUMBER,
    EVIL_TYPE_STRING,
    EVIL_TYPE_BOOLEAN,
    EVIL_TYPE_TABLE,
    EVIL_TYPE_ARRAY,
    EVIL_TYPE_FIBER,
    EVIL_TYPE_FUNCTION,
    EVIL_TYPE_ERROR,
    EVIL_TYPE_TYPE,
    EVIL_TYPE_NATIVE_FUNCTION,
    EVIL_TYPE_NATIVE_OBJECT
} evil_type_t;

struct evil_string;
struct evil_table;
struct evil_array;
struct evil_fiber;
struct evil_closure;
struct evil_error;
struct evil_native_obj;

struct evil_value;
typedef struct evil_value (*evil_native_fn_t)(struct evil_fiber* fiber, struct evil_value self, int argc, struct evil_value* argv);

typedef struct evil_value {
    evil_type_t type;
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

/* Value Constructors */
static inline evil_value_t evil_val_nil(void) {
    evil_value_t v;
    v.type = EVIL_TYPE_NIL;
    v.as.number = 0.0;
    return v;
}

static inline evil_value_t evil_val_number(double n) {
    evil_value_t v;
    v.type = EVIL_TYPE_NUMBER;
    v.as.number = n;
    return v;
}

static inline evil_value_t evil_val_bool(bool b) {
    evil_value_t v;
    v.type = EVIL_TYPE_BOOLEAN;
    v.as.boolean = b;
    return v;
}

static inline evil_value_t evil_val_type_code(evil_type_t tc) {
    evil_value_t v;
    v.type = EVIL_TYPE_TYPE;
    v.as.type_code = tc;
    return v;
}

static inline evil_value_t evil_val_string(struct evil_string* s) {
    evil_value_t v;
    v.type = EVIL_TYPE_STRING;
    v.as.string = s;
    return v;
}

static inline evil_value_t evil_val_table(struct evil_table* t) {
    evil_value_t v;
    v.type = EVIL_TYPE_TABLE;
    v.as.table = t;
    return v;
}

static inline evil_value_t evil_val_array(struct evil_array* a) {
    evil_value_t v;
    v.type = EVIL_TYPE_ARRAY;
    v.as.array = a;
    return v;
}

static inline evil_value_t evil_val_closure(struct evil_closure* c) {
    evil_value_t v;
    v.type = EVIL_TYPE_FUNCTION;
    v.as.closure = c;
    return v;
}

static inline evil_value_t evil_val_fiber(struct evil_fiber* f) {
    evil_value_t v;
    v.type = EVIL_TYPE_FIBER;
    v.as.fiber = f;
    return v;
}

static inline evil_value_t evil_val_error(struct evil_error* e) {
    evil_value_t v;
    v.type = EVIL_TYPE_ERROR;
    v.as.error = e;
    return v;
}

static inline evil_value_t evil_val_native_fn(evil_native_fn_t fn) {
    evil_value_t v;
    v.type = EVIL_TYPE_NATIVE_FUNCTION;
    v.as.native_fn = fn;
    return v;
}

static inline evil_value_t evil_val_native_obj(struct evil_native_obj* obj) {
    evil_value_t v;
    v.type = EVIL_TYPE_NATIVE_OBJECT;
    v.as.native_obj = obj;
    return v;
}

/* Value Checkers */
static inline bool evil_is_nil(evil_value_t v) { return v.type == EVIL_TYPE_NIL; }
static inline bool evil_is_number(evil_value_t v) { return v.type == EVIL_TYPE_NUMBER; }
static inline bool evil_is_bool(evil_value_t v) { return v.type == EVIL_TYPE_BOOLEAN; }
static inline bool evil_is_string(evil_value_t v) { return v.type == EVIL_TYPE_STRING; }
static inline bool evil_is_table(evil_value_t v) { return v.type == EVIL_TYPE_TABLE; }
static inline bool evil_is_array(evil_value_t v) { return v.type == EVIL_TYPE_ARRAY; }
static inline bool evil_is_fn(evil_value_t v) { return v.type == EVIL_TYPE_FUNCTION || v.type == EVIL_TYPE_NATIVE_FUNCTION; }
static inline bool evil_is_fiber(evil_value_t v) { return v.type == EVIL_TYPE_FIBER; }
static inline bool evil_is_error(evil_value_t v) { return v.type == EVIL_TYPE_ERROR; }
static inline bool evil_is_type(evil_value_t v) { return v.type == EVIL_TYPE_TYPE; }

/* EVIL Truthiness: nil, false, and 0 are falsey; all others are truthy */
static inline bool evil_is_truth(evil_value_t v) {
    if (v.type == EVIL_TYPE_NIL) return false;
    if (v.type == EVIL_TYPE_BOOLEAN && !v.as.boolean) return false;
    if (v.type == EVIL_TYPE_NUMBER && v.as.number == 0.0) return false;
    return true;
}

#endif /* CVIL_VALUE_H */
