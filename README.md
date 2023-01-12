# EVIL
Ensign Virtual Interpreted Language - first interpreter and a programming language I implemented. Ever.

# Why
Fuck if I know. Research? The entire thing is probably useless for advanced stuff.

# How do I write in this?
Good question! I have no idea either. Probably using a text editor of sorts...

# How do I use this?
Import the project and...
```CSharp
using EVIL;
using Environment = EVIL.Environment;

class Program
{
  private static Interpreter _interpreter;
  private static Environment _environment;
  
  private static readonly string _source = "fn main() { print("hello, world!") }"
  
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
No. But here's the gist:

#### Data types
Number (double-precision), string, table, function.


```
// All names functions must be top-level.
//
fn main(args) {
  // lua-like count operator is present...
  print("there are " + @#args)
  
  // locals are valid only in the block defining it
  local frac = 0.1234
  local int = 100
  
  // globals are valid throughout the script
  some_gvar = 666
  some_other_gvar = some_global_var << 2 // bitwise operators are present as in any C-like
  
  some_gvar += 100
  print_global() // won't work if executing top-down without entry-point, forward declarations not supported
 
  for (local i = 0; i < 100; i++) {
    if (i % 0 == 0 && i != 0) {
      skip // same as continue in normal languages
    }
    
    if (i == 101) {
      exit // terminates script execution immediately
    }
    
    if (i == 89) {
      break // breaks out of the loop
    }
    
    print(@i)
    
    local j = math.floor(i / 2)
    while (j) { // anything non-zero is treated as truth, 'true' and 'false' are interpreter aliases for 1 and 0
      // ? is 'nameof' operator, returns name of a variable in current scope...
      print(?j + @j)
      j--
    }
  }
 
  // functions are 1st-class citizens
  local f = fn(a, b) { ret a ^ b }
  print(@f(10, 30))
  
  // table initialization is supported...
  local t = { 1, 2, "dog", f, f(13, 15) }
  
  // you can iterate over tables...
  each (k, v : t) {
    sys.print(@k + ": " + @v)
  }
  
  local dog = t[2] // tables are 0-indexed...
  local o = dog[1] // you can also index strings
  
  // ret anywhere is optional and defaults to 0 if not present.
  ret 0
}

fn add(a, b) { ret a + b }

fn print_global() {
  // @ is unary operator meaning "whatever it is, i want it as a string"
  print(@some_global_var)
}

// there's no namespace separation, dots are allowed in identifier names
fn util.thing() {
  ret 69*0x420 // hex numbers are supported
}
```
