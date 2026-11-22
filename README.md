# EVIL
### Ensign Virtual Interpreted Language
I just wanted Lua for .NET with nice syntax, man...

Don't get me wrong, I love Lua, but having it interact with the .NET world is pain. At least it was when I started this project lol. These days it's just my comfort project to work on when I feel like shit.

So... I stole ideas from modern languages and packed it into whatever the fuck this is.

## What is this?
EVIL is an embeddable scripting language that comes with a custom virtual machine. Keeps the ideas I really enjoyed in Lua: metamethods, coroutines, sandbox, etc. but with a syntax that's somewhat fun to write (it is for me, fuck off).

Performance-wise, it's not trying to beat a JIT. It's meant for game logic, configs, whatever. You can try using it for general-purpose scripting but I wouldn't really recommend that.

## Why would I care? Anyone can make a programming language these days.
**Best selling feature I guess - the sandbox**   
Runs inside an isolated virtual machine, so scripts only have access to the modules and host functions you explicitly register. Unless I fucked up and you somehow exit the sandbox (report please uwu?).

**Syntax modern languages made you lazy with**  
▐ String interpolation: `"Status: ${status}, HP: ${hp}"` and `"$var"` instead of string formatting routines.  
▐ Safe navigation: `a?.b?.c` and `arr?[idx]`.  
▐ Null coalescing: `cfg.port ?? 8080`, plus `?? throw "Missing config"`.  
▐ Arrow functions: `fn(x) -> x * 2` alongside standard block bodies.  
▐ Read-only by default: `val x = 10;` is immutable. If you want mutation, you write `rw val x = 10;`.   
▐ **Pattern matching (`by`)** It's a `switch` but with 2 letters instead of 6. Supporting both shallow matching (`->`) and deep matching (`=>`):
```evil
  val role = by user.id {
    0 -> "Root",
    1 -> "Admin",
    else: "Nobody"
  };
```  
▐ **Table expansion (`with`)**  
  Non-destructive extension without writing manual cloning loops (thanks, C#):
```evil
  val base = { x: 10, y: 20 };
  val extended = base with { z: 30, y: 25 };
```  
▐ **Cooperative concurrency (Fibers)**  
Built-in coroutines via `yield<target>(args)`. If you've ever scripted dialogue trees you know how painful callbacks are.

▐ **Flexible diagnostics system**   
The compiler outputs source spans with error codes and sensible messages. Can also build and register your own analyzers.

## Okay sure, whatever. Examples?
```evil
// Tables, methods, and self-binding
val Entity = {
  new: self::fn(name, health = 100) -> {
    name: name,
    hp: health,
    bag: [ "Herb", "Dagger" ],

    take_damage: self::fn(amount) {
      self.hp -= amount;
      io.println("${self.name} took $amount damage. Remaining HP: ${self.hp}");
    }
  }
};

val player = Entity::new("Player", 100);
val item = player.bag?[0] ?? "Nothing";
io.println("Looted: $item");

val assessment = by player.hp {
  0 -> "Dead. Yay(?)",
  100 -> "Healthy.",
  else: "Injured."
};
io.println("State: $assessment");

val buffed_player = player with { shield: 20 };
buffed_player::take_damage(15);
```

## Trying it out
Requires .NET installed (duh).

```bash
git clone https://github.com/vddCore/EVIL.git
cd EVIL

dotnet run --project FrontEnd/EVIL.evil -- VirtualMachine/Tests/EVIL.Ceres.LanguageTests/tests/001_truth.vil
```

The CLI tool comes with a few useful inspection utilities:
```bash
# Disassemble compiled bytecode to inspect VM opcodes.
dotnet run --project FrontEnd/EVIL.evil -- -d script.vil

# Compile directly to a binary bytecode chunk.
# Usually .evx but who am I to enforce how you use this.
dotnet run --project FrontEnd/EVIL.evil -- -c bundle.evx script.vil

# Switch between various compiler log formatters.
dotnet run --project FrontEnd/EVIL.evil -- -f rich script.vil
```

## Embedding in .NET
I'm just as lazy as you are, so I made that simple:

```csharp
using System;
using EVIL.Ceres.ExecutionEngine;
using EVIL.Ceres.Runtime;
using EVIL.Ceres.TranslationEngine;

var vm = new CeresVM();
var runtime = new EvilRuntime(vm);

runtime.RegisterBuiltInModules();

var compiler = new Compiler();
var chunk = compiler.Compile(@"
    fn greet(target) -> ""Hello, "" + target + ""!"";
    io.println(greet(""world""));
");

vm.Run();
vm.MainFiber.Schedule(chunk);
await vm.MainFiber.BlockUntilFinishedAsync();
vm.Stop();
```
To expose your own host capabilities, create a class inheriting from `RuntimeModule` and register it, or attach dynamic callbacks to `vm.Global`.

## Extras
▐ **EBNF Grammar**  
The complete formal syntax specification is available in [Docs/Syntax/evil.ebnf](Docs/Syntax/evil.ebnf).  

▐ **Basic editor tooling**  
Syntax files for VS Code, Notepad++, and KatePart can be found in `Shared/highlighters/`.  

▐ **Module Documentation**  
 Run the CLI with `-g` to automatically export markdown documentation for all detected runtime modules.

## Project map
```
Core/
 |- EVIL.Lexical/: Lexical contains... The lexer...
 |- EVIL.Grammar/: Parser, AST.
 |- EVIL.Analysis/: Diagnostic analyzers and style validation.
 \- EVIL.Diagnostics/: Error reporting and formatting.
VirtualMachine/
 |- EVIL.Ceres: Execution & translation engines, VM.
 \- EVIL.Ceres.Runtime: Standard library modules and C# interop helpers.
                         Feel free to use as a reference for your own modules.
FrontEnd/
 \- EVIL.evil/: Standalone CLI compiler and script runner.

Docs/
 \- Syntax/: LALR(1)/GLR rules, grammar validator.

Shared/
 \- Highlighters, artwork... Anything that isn't code goes here.
```

## Have you used an eyy-ayy to build this?
I can't believe we're living in times I have to write this. **Fuck no**. Why would I take fun out of my comfort project.

Shame where shame's due though, I used an LLM to generate the Flex/Bison rules from the source code. Because that's anything BUT fun.

## License
EVIL is licensed under LGPLv3. See [LICENSE.md](LICENSE.md) for details.
