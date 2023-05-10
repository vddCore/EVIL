# EVIL Programming Language & Syntax Reference

## 2. Data types

### 2.1. `Nil`

> #### Synopsis
> The [`Nil`](#21-nil) data type represents the absence of a useable value. If a global value or a table key is set to
> [`Nil`](#21-nil) it's assumed to not exist. Likewise - if the user no longer wants a variable to exist, they can set
> it to [`Nil`](#21-nil) and effectively _undefine_ a global variable or a table entry.

> #### Truth characteristics
> A [`Nil`](#21-nil) value will always evaluate to [`false`](#24-boolean) when performing logical operations.

### 2.2. `Number`

> #### Synopsis
> The [`Number`](#22-number) data type represents an IEEE-754-compatible floating-point number The actual precision may
> vary between language implementations but it must not be lower than 64 bits. It is the only numeric type available in
> EVIL.

> #### Truth characteristics
> A [`Number`](#22-number) value will evaluate to [`true`](#24-boolean) when performing logical operations if and only
> if it is not equal to zero.

> #### Important notes
> Being an IEEE-754-compliant data type, [`Number`](#22-number) is vulnerable to floating-point rounding errors.

### 2.3. `String`

> #### Synopsis
> The [`String`](#23-string) data type represents a sequence of UTF-8 characters.

> #### Truth characteristics
> A [`String`](#23-string) value will always evaluate to [`true`](#24-boolean) when performing logical operations,
> regardless of the value it may currently be holding.

### 2.4 `Boolean`
> #### Synopsis
> The [`Boolean`](#24-boolean) data type represents a binary state - either `true` or `false`.

### 2.5 `Table`
> #### Synopsis
> The [`Table`](#25-table) data type represents an associative array of values of any other type, including the
> [`Table`](#25-table) type itself. Each [`Table`](#25-table) entry has a corresponding key.