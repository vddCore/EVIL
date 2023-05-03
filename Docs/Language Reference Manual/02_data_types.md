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

> #### Type conversions
> **Boolean coalescence**  
> It is possible to coalesce a [`Nil`](#21-nil) value to a [`Boolean`](#24-boolean) value by stacking the logical
> negation operator:
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
> |        Type        |        Value         |         Example          |
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

> #### Type conversions
> **Boolean coalescence**  
> It is possible to coalesce a [`Number`](#22-number) value to a [`Boolean`](#24-boolean) value by stacking the logical
> negation operator:
>
> ```
> var a = !!0;          // `a' is now [False <Boolean>]
> var type = typeof(a); // `type' is now ["Boolean" <String>]
> ```
>
> **String conversion**  
> Any [`Number`](#22-number) value is subject to [`String`](#23-string) type conversion. All string conversion
> considerations have been described in section [4.1.5.1](04_expressions.md#4151-to-string-operator).

> #### Important notes
> Being an IEEE-754-compliant data type, [`Number`](#22-number) is vulnerable to floating-point rounding errors.

### 2.3. `String`

> #### Synopsis
> The [`String`](#23-string) data type represents a sequence of UTF-8 characters.

> #### Token structure
> A valid [`String`](#23-string) token:  
> - Must be enclosed within double quotes: `"string"`.
> - Must either contain a sequence of UTF-8 characters, or be empty.
> - May contain one or more of the following escape sequences:  
>  * `\a`: ASCII value `0x07`. Known as "bell" or "alert". 
>  * `\b`: ASCII value `0x08`. Known as "backspace".
>  * `\f`: ASCII value `0x0C`. Known as "form feed" or "New page".
>  * `\n`: ASCII value `0x0A`. Known as "line feed" or "New line".
>  * `\r`: ASCII value `0x0D`. Known as "carriage return".
>  * `\t`: ASCII value `0x09`. Known as "horizontal tab".
>  * `\v`: ASCII value `0x0B`. Known as "vertical tab".
>  * `\"`: Quotation mark for use within strings.
>  * `\\`: Backslash for use within strings.
>  * `\uHHHH`: UTF-8 character in hexadecimal notation. Always 4 digits long.
>  * `\xHHHH`: Alternate escape sequence for the above. It can be difficult to get rid of habits, after all.
>
> Below are a few examples of valid EVIL [`String`](#23-string) tokens:
>  - `"the quick brown fox jumps over the lazy dog"`
>  - `"zażółć gęślą jaźń"`
>  - `"this is a unicode character: \u1234"`
>  - `"this is also a unicode character: \xFF01"`
>  - `"Printing\a this would\a startle a lonely\a programmer at night.\a"`
>  - `"This would span\nover multiple lines\nin most terminal emulator software."`

> #### Comparison characteristics
> Attempting to compare [`String`](#23-string) values using operators other than `==`, `!=`, `<==>` and `<!=>` with 
> any other value type - including [`String`](#23-string) values themselves - is defined as a runtime error. If you need
> to compare the lengths of two strings, see section [4.2.6.1](04_expressions.md#4261-length-of-operator).

> #### Truth characteristics
> A [`String`](#23-string) value will always evaluate to [`true`](#24-boolean) regardless of the value it may currently 
> be holding.

> #### Type conversions
> **Boolean coalescence**  
> It is possible to coalesce a [`String`](#23-string) value to a [`Boolean`](#24-boolean) value by stacking the logical
> negation operator:
>
> ```
> var a = !!"a string"; // `a' is now [True <Boolean>]
> var type = typeof(a); // `type' is now ["Boolean" <String>]
> ```
>
> **String conversion**  
> Usage of `@` operator on any [`String`](#23-string) value is defined as a no-op: a copy of the same value is returned.
> For more details on the `@` operator, see section [4.2.5.1](04_expressions.md#4251-to-string-operator).
> 
> **Number conversion**  
> A [`String`](#23-string) value may be converted to a [`Number`](#22-number) value using the `$` operator. If the 
> string being converted meets conversion criteria and the conversion succeeds, a [`Number`](#22-number) value matching 
> the one from the [`String`](#23-string) provided is returned. If for any reason the conversion fails, a runtime error 
> will occur. For more details on the `$` operator, see section [4.2.5.2](04_expressions.md#4252-to-number-operator).