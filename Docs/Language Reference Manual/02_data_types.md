# EVIL Programming Language & Syntax Reference

## 2. Data types

### 2.1. `Nil`

> #### Synopsis
> The [`Nil`](#21-nil) data type represents the absence of a useable value. If a global value or a table key is set to
> [`Nil`](#21-nil) it's assumed to not exist. Likewise - if the user no longer wants a variable to exist, they can set
> it to [`Nil`](#21-nil) and effectively _undefine_ a global variable or a table entry.

> #### Language tokens
> |  Type   | Value |
> |:-------:|:-----:|
> | Keyword | `nil` |

> #### Comparison characteristics
> Attempting to compare  [`Nil`](#21-nil) values using operators other than `==`, `!=`, `<==>` and `<!=>` with any other
> value is defined as a runtime error.
>
> A [`Nil`](#21-nil) value will never be equal to any other value that itself is not a [`Nil`](#21-nil).

> #### Truth characteristics
> A [`Nil`](#21-nil) value will always evaluate to [`false`](#24-boolean) when performing logical operations.

> #### Conversions
> **Boolean coalescence**  
> It is possible to coalesce a [`Nil`](#21-nil) value to a [`Boolean`](#24-boolean) value by stacking the logical 
> negation operator, as pictured by the following:
>
>```
> var a = !!nil;        // `a' is now [False <Boolean>]
> var type = typeof(a); // `type' is now ["Boolean" <String>]
>```
>
> **String conversion**  
> Any [`Nil`](#21-nil) value is subject to [`String`](#23-string) type conversion. All string conversion considerations
> have been described in section [4.1.5.1](04_expressions.md#to-string-operator).
>
> **Conversion to data types other than specified**  
> Attempting to convert a [`Nil`](#21-nil) value to an incompatible data type is defined as a runtime error.

### 2.2. `Number`

> #### Synopsis
> The [`Number`](#22-number) data type represents an IEEE-754-compatible floating-point number The actual precision may
> vary between language implementations but it must not be lower than 64 bits. It is the only numeric type available in
> EVIL.

> #### Language tokens
> |        Type        |        Value         |         Examples         |
> |:------------------:|:--------------------:|:------------------------:|
> | Regular expression | `([0-9]+)?\.?[0-9]+` |  `0.123` `.245` `2137`   |
> | Regular expression |   `0x[A-Fa-f0-9]+`   | `0xABCD` `0x2137` `0x1F` |
> |      Keyword       |        `NaN`         |           N/A            |
> |      Keyword       |     `Infinity`       |           N/A            |

> #### Comparison characteristics
> [`Number`](#22-number) values are subject to a range of comparison operations. A user can utilize one of the following
> operators to compare two [`Number`](#22-number) values: `>`, `>=`, `<` `<=`, `==`, `!=`, `<==>` or `<!=>`.
>
> Attempting to compare a [`Number`](#22-number) value with another value that is not a [`Number`](#22-number) using
> `>`, `>=`, `<` or `<=` is defined as a runtime error.

> #### Truth characteristics
> A [`Number`](#22-number) value will evaluate to [`true`](#24-boolean) when performing logical operations if and only
> if it is not equal to zero.

> #### Conversions
> **Boolean coalescence**  
> It is possible to coalesce a [`Number`](#22-number) value to a [`Boolean`](#24-boolean) value by stacking the logical
> negation operator, as pictured by the following:
> 
> ```
> var a = !!0;          // `a' is now [False <Boolean>]
> var type = typeof(a); // `type' is now ["Boolean" <String>]
> ```
>
> **String conversion**  
> Any [`Number`](#22-number) value is subject to [`String`](#23-string) type conversion. All string conversion 
> considerations have been described in section [4.1.5.1](04_expressions.md#to-string-operator).

> #### Important notes
> Being an IEEE-754-compliant data type, [`Number`](#22-number) is vulnerable to floating-point rounding errors.