# EVIL Programming Language & Syntax Reference

## 2. Data types

### 2.1. `Nil`
>#### Synopsis
> The `Nil` data type represents the absence of a useable value.  If a global value or a table key is 
> set to `Nil` it's assumed to not exist.  Likewise - if the language user no longer wants a variable
> to exist, they can set it to `Nil` and effectively erase the variable.

>#### Language tokens
> `nil`

>#### Comparison characteristics
> Attempting to compare `Nil` values using operators other than `==`, `!=`, `<==>` or `<!=>` is defined
> as a runtime error.
> 
> A `Nil` value will never be equal to any other value that itself isn't a `Nil` value.

>#### Truth characteristics
> A `Nil` value will always evaluate to [`False`](#24-boolean)  when performing logical operations.

>#### Conversions
> **Boolean coalescence**  
> It is possible to coalesce a `Nil` data type to a [`Boolean`](#24-boolean) data type by stacking logical
> negation operator, as described by the following
> ```EVIL
> var bool = !!nil;
> ```
> 
> **String conversion**  
> Any `Nil` value is a subject to [`String`](#23-string) type conversion. All string conversion considerations
> have been described in section [4.1.5.1](04_expressions.md#to-string-operator).
> 
> **Conversion to data types other than described**  
> Attempting to convert a `Nil` value to an incompatible data type is defined as a runtime error.
