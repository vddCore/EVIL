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
> You, my dear reader, have already seen how to define a function in EVIL, assuming you've read one section above, but 
> for the sake of completeness, I have decided to provide a few examples here as well.

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