# EVIL Programming Language & Syntax Reference

## 3. Statements

### 3.1 Top-level statements

#### 3.1.1. Attribute statement
> **Synopsis**  
> An attribute statement is a compile-time statement that allows the user to provide additional metadata about a chunk 
> that can then be retrieved at runtime via reflection mechanisms, if the runtime libraries are is configured to allow 
> such an operation.
> 
> A function can have any number of attributes and their names do not need to be unique.
> 
> Attribute names are completely up to the user, but are limited by the valid identifier token criteria. An attribute
> can contain any number of constant values and declare any number of attribute properties.
> 
> Attribute value list is optional, but if present, it must precede attribute property list. Likewise, an attribute
> property list is optional, but if present, it must appear after the aforementioned value list.
> 
> Given the fact this is all compile-time metadata, only constant expressions are allowed as values.

> **Usage inside EVIL**  
> To declare an attribute you can use one of the following syntax constructs:
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
> // A function can have any number of attributes...
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

> **Usage inside .NET**
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