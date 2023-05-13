# EVIL Programming Language & Syntax Reference

## 3. Statements

> **Note**  
> *Function* and *Chunk* refer to the same operational concept in EVIL, with the important distinction being that a 
> *Function* is an EVIL syntax construct that is processed by the compiler, while *Chunk* is a bytecode equivalent of a
> *Function*.

### 3.1 Top-level statements
Top-level statements are statements that can only be used at the 0th level of nesting in a [Script](01_script_overview.md).
They are not allowed to have a parent node that isn't the top-level Program AST node, but they can have one or more 
child nodes.

#### 3.1.1. Attribute statement
> **Synopsis**  
> An attribute statement is a compile-time statement that allows the user to provide additional metadata about a
> [Chunk](05_chunks.md) that can then be retrieved at runtime via reflection mechanisms, if the runtime libraries 
> are configured to allow such an operation.
> 
> A Chunk can have any number of attributes and their names do not need to be unique.
> 
> Attribute names are completely up to the user, but are limited by the valid identifier token criteria. An attribute
> can contain any number of constant values and declare any number of attribute properties.
> 
> Attribute value list is optional, but if present, it must precede attribute property list. Likewise, an attribute
> property list is optional, but if present, it must appear after the aforementioned value list.
> 
> Given this is all compile-time metadata, only constant expressions are allowed as values.

> **Example: Declaring function attributes in EVIL**  
> To declare an attribute, a user can utilize one of the following syntax constructs:
> ```
> // attr_example.vil
> 
> // Single attribute with a value list.
> #[attribute1("asdf", 123.45, 2222)]
> fn my_func() { }
> 
> // Single attribute without a value list.
> #[attribute1]
> fn my_func2() { }
> 
> // Single attribute with a value list and a property list
> #[attribute1("asdf", 123, my_property = "my_value")]
> fn my_func3() { }
> 
> // A chunk can have any number of attributes...
> #[attribute1("asdf")]
> #[attribute1(333)]
> #[attribute2(false)]
> fn my_func4() { }
> 
> // ...and multiple attributes can be a part of a single declaration.
> #[attr1("asdf"); attr1(333); attr2(false)]
> #[attr4(nil, this = "is valid too")]
> fn my_func5() { }
> ```

> **Example: Enumerating EVIL function attributes in .NET**
> ```csharp
> using System;
> using EVIL.Grammar.Parsing;
> using Ceres.TranslationEngine;
> 
> static void Main() 
> {
>     var parser = new Parser();
>     var compiler = new Compiler();
> 
>     // Don't forget to handle exceptions here.
>     var program = parser.Parse(File.ReadAllText("attr_example.vil"));
>     var script = compiler.Compile(program);
> 
>     foreach (var chunk in script.Chunks)
>     {
>         Console.WriteLine($"Chunk '{chunk.Name}' {")
>         foreach (var attr in chunk.Attributes)
>         {
>             Console.WriteLine($"  Attribute '{attr.Name}' {");
>             Console.WriteLine($"    Values:");
>             for (var i = 0; i < attrs.Values.Length; i++)
>             {
>                 Console.WriteLine($"      [{i}] = {attrs.Values[i]}");
>             }
> 
>             Console.WriteLine();
> 
>             Console.WriteLine($"    Properties:");
>             foreach (kvp in attrs.Properties)
>             {
>                 Console.WriteLine($"      [{kvp.Key}] = {kvp.Value}");
>             }
>             Console.WriteLine($"  }");
>         }
>         Console.WriteLine("}");
>     }
> }
> ```

#### 3.1.2 Function definition statement
##### 3.1.2.1 `fn` Keyword
> **Synopsis**  
> The `fn` keyword allows the user to define a [Chunk](05_chunks.md) belonging to a specific 
> [Script](01_script_overview.md). 
> 
> A valid Script function consists of a name, followed by a parameter list (can be empty), followed by either a 
> [Block statement](03_statements.md#321-block-statement) or an expression body.
> 
> The Ceres VM translation engine does not enforce unique function names, and so there may be multiple Chunks sharing 
> the same name. The ambiguity resolution behavior is left for the end-user to decide. The three suggested approaches 
> are:
> 1. Throwing an exception if duplicate names are found in a Script after successful compilation.
> 2. Discarding all but the first function sharing one name.
> 3. Instead of setting the function as a global, create a [Table](02_data_types.md#25-table), then set functions at the
>    respective numerical indices in the exact order the functions were defined in a script, finally setting said Table
>    as a global with the name all found functions share.
> 

> **Example: Defining functions in EVIL**
> ```
> // An empty, parameterless function.
> fn func_1() {}
> 
> // An empty, parameterless function with a dead (useless) ret.
> fn func_2() { ret; }
> 
> // An empty function taking 2 parameters and doing absolutely nothing with them.
> fn func_4(a, b) {}
> 
> // A function taking 2 parameters, adding them together and returning the result of the operation.
> fn func_5(a, b) { ret a + b; }
> 
> // As above, but shorter - an expression body.
> fn func_6(a, b) -> a + b;
> ```

##### 3.1.2.2 Function parameter initializers
> **Synopsis**  
> Function parameters can have default value initializers, however, the default values must appear after all the
> uninitialized parameters. If a parameter is uninitialized, and the amount of arguments doesn't match the parameter
> count for a given Chunk, it's assumed to have a value of [`Nil`](02_data_types.md#21-nil). 
> 
> In addition, all parameter initializers are limited to using constant expressions only, therefore,
> only [`Nil`](02_data_types.md#21-nil), [`Number`](02_data_types.md#22-number), [`String`](02_data_types.md#23-string),
> or [`Boolean`](02_data_types.md#24-boolean) data types are allowed as initializer value types.

> **Example: Defining functions with parameter initializers in EVIL**
> ```
> // If func_1 is invoked with just one parameter, e.g. func_1(2), then `b' will
> // be equal to 21.37 because of its default value. Therefore, the result of the
> // the operation will be 23.37.
> //
> fn func_1(a, b = 21.37) -> a + b;
>
> // Illegal - initializers must appear after all uninitialized parameters.
> fn func_2(a = 21.37, b) -> a + b;
> ```

### 3.2 Parented statements
Parented statements are - as the name suggests - statements that cannot exist in a top-level context.

#### 3.2.1 Block statement
> **Synopsis**  
> A block statement serves two purposes:
> 1. Organizes a group of statements into an easily readable piece of information.
> 2. Enclose a new lexical scope to put local variables into.

> **Example: Demonstration of block statements and scoping in EVIL**
> ```
> fn func()
> // already a block statement here
> { 
>   // block statements can be empty
>   // useless, but there you go
>   {}
> 
>   // `a' is not a local yet, so it refers to a global variable
>   a = 20; 
>   {
>       // func local 0 is created, value is 21.37
>       var a = 21.37;
> 
>       // func local 1 is created, value is 21.37
>       var b = a;
> 
>       // block statements can be nested indefinitely
>       {
>           // func local 2 is created, value is 11.11;
>           var c = 11.11;
>       }
> 
>       // `c' went out of scope, so using it here will get a global `c'
>       //
>       // because global `c' was never set, it'll result in a Nil value
>       // when fetched, so `var d' creates func local 3 and it's initialized
>       // to global `c' - a Nil value in this scope.
>       var d = c;
>   }
>   
>   // func local 4 is created, value is copied from global `a' 
>   /  because local `a' went out of scope, so `b' is 20 now
>   var b = a; 
>   {
>       // func local 5 is created, value is 21.88
>       var a = 21.88
> 
>       // `b' was declared and defined in outer scope,
>       // so it's still an outer local
>       b = a;
>   }
>   //
>   // because of the above, `b' (local 4) is 21.88 now.
> }
> ```