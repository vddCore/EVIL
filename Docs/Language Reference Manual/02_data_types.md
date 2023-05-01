# EVIL Programming Language & Syntax Reference

## 2. Data types

### 2.1. `Nil`
>#### Synopsis
> The [`Nil`](#21-nil) data type represents the absence of a useable value. If a global value or a table key is set to 
> [`Nil`](#21-nil) it's assumed to not exist. Likewise - if the user no longer wants a variable to exist, they can set 
> it to [`Nil`](#21-nil) and effectively _undefine_ a global variable or a table entry.

>#### Language tokens
> `nil` **Keyword**

>#### Comparison characteristics
> Attempting to compare  [`Nil`](#21-nil) values using operators other than `==`, `!=`, `<==>` and `<!=>` with any other
> value is defined as a runtime error.
> 
> A [`Nil`](#21-nil) value will never be equal to any other value that itself is not a [`Nil`](#21-nil).

>#### Truth characteristics
> A [`Nil`](#21-nil) value will always evaluate to [`False`](#24-boolean) when performing logical operations.

>#### Conversions
> **Boolean coalescence**  
> It is possible to coalesce a [`Nil`](#21-nil) value to a [`Boolean`](#24-boolean) value by stacking logical negation
> operator, as pictured by the following:
> 
>```
> var a = !!nil;        // `a' is now [False <Boolean>]
> var type = typeof(a); // `type' is now ["Boolean" <String>]
>```
> 
> **String conversion**  
> Any `Nil` value is a subject to [`String`](#23-string) type conversion. All string conversion considerations
> have been described in section [4.1.5.1](04_expressions.md#to-string-operator).
> 
> **Conversion to data types other than specified**  
> Attempting to convert a [`Nil`](#21-nil) value to an incompatible data type is defined as a runtime error.

### 2.2 `Number`
>#### Synopsis
> The [`Number`](#22-number) data type represents a double-precision floating-point number compliant with the IEEE-754 
> standard.
> 