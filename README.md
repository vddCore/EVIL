# EVIL
Ensign Virtual Interpreted Language - first interpreter and a programming language I implemented. Ever. And likely the last.

# Why
Fuck if I know. Research? ~~The entire thing is probably useless for advanced stuff.~~ Might be useful for simple tasks right now.

# How do I write in this?
Good question! I have no idea either. Probably using a text editor of sorts...

# How do I use this?
Import the project and...
```CSharp
using System.Threading.Tasks;

using EVIL;
using EVIL.Abstraction;
using Environment = EVIL.Environment;

class Program
{
  private static Interpreter _interpreter;
  private static Environment _environment;
  
  private static readonly string _source = "fn main() { print(\"hello, world!\") }"
  
  static async Task Main(string[] args)
  {
    _interpreter = new Interpreter();
    
    _environment = new Environment(_interpreter);
    _environment.LoadCoreRuntime();
    
    _enviroment.RegisterBuiltIn("print", (interp, args) => {
      Console.WriteLine(string.Join(' ', args.Select(x => x.AsString().String));
      return DynValue.Zero;
    });
    
    await _interpreter.ExecuteAsync(_source, "main", new string[0]);
  }
}
```
# There any syntax reference?
Check the wiki. Can't be arsed? Below's the gist.

#### Data types
Number, string, table, function.

Strings are encoded using UTF-16 and support basic control codes and unicode character inserts.  
Numbers are 128-bit floating point numbers, hexadecimal format integers are supported but are internally converted to a C# decimal in the reference implementation - if deciding to reimplement the language, use at least a 64-bit floating point number.

#### Operators
All regular operators work as you would expect them to work in a C-like language, however there are some EVIL-specific operators - these are documented below. Sometimes the basic arithmetic operators have additional functionality for other data types. Consult the wiki for details.

##### Utility
`@` - converts whatever's after it to a string representation.  
`#` - returns a variable's length - valid for strings and tables.  
`?` - looks for a symbol in scope and returns its name as a string.  
`??` - checks for a key's existence in a table or a letter's existence in a string.  
`->` - accesses members of a table using identifier as a key.  
`<-` - initializes a table key using a value.  

##### Arithmetic
`+` - addition/concatenation  
`-` - subtraction  
`/` - division  
`*` - multiplication  
`%` - modulo  
`$` - floor operator  

All arithmetic operators support compound versions of themselves (e.g. `+=`)  

##### Bitwise
`<<` - shift left  
`>>` - shift right  
`^` - XOR  
`&` - AND  
`|` - OR  
`~` - NOT  

##### Logical
`||` - Alternative  
`&&` - Conjunction  

##### Comparison
`>` - greater than ('longer than' for strings)  
`<` - lesser than ('shorter than' for strings)  
`>=` - greater than or equal to (analogous for string)   
`<=` - lesser than or equal to (analogous for string)  
`==` - equal to  
`!=` - not equal to  

#### Keywords
`fn` - define a function  
`for` - c-style for-loop  
`each` - foreach-loop  
`undef` - undefine a previously defined symbol  
`while` - while-loop  
`var` - variable definition  
`if` - condition  
`elif` - else-if  
`else` - else  
`break` - break out of a loop  
`skip` - `continue` in normal languages  
`ret` - return a value and finish function execution  
`true` - interpreter alias for `1`  
`false` - interpreter alias for `0`  
`exit` - terminate script execution immediately  
