# EVIL Debugger Guide
EVIL provides a simple debugging system designed for interactive terminal workflows, IDEs & editors (via the Debug Adapter Protocol), and embedding inside .NET host applications.

## ToC
- [1. Overview](#1-overview)
- [2. Quickstart: CLI Debugger (`evdb`)](#2-quickstart-cli-debugger-evdb)
- [3. CLI Command Reference](#3-cli-command-reference)
- [4. Interactive Debugging Walkthrough](#4-interactive-debugging-walkthrough)
- [5. Visual Studio Code & IDE Integration (DAP)](#5-visual-studio-code--ide-integration-dap)
- [6. .NET Embedding API](#6-programmatic-net-embedding-api)
- [7. Advanced Features](#7-advanced-features)
  - [Conditional & Hit-Count Breakpoints](#conditional--hit-count-breakpoints)
  - [Multi-Fiber (Coroutine) Debugging](#multi-fiber-coroutine-debugging)
  - [Runtime Variable Mutation](#runtime-variable-mutation)
  - [Bytecode & Instruction Stepping](#bytecode--instruction-stepping)

## 1. Overview

The EVIL debugging architecture consists of three integrated layers:

```
┌────────────────────────────────────────────────────────┐
│                      IDE / Client                      │
│   VS Code / Cursor (DAP)   │   CLI Shell (evdb)        │
└──────────────┬─────────────┴─────────────┬─────────────┘
               │ (JSON-RPC)                │
┌──────────────▼─────────────┐             │
│    DAP Server (evil --dap) │             │
└──────────────┬─────────────┘             │
               │                           │
┌──────────────▼───────────────────────────▼─────────────┐
│          Debugger Engine (EVIL.Debugger)               │
│    BreakpointManager  │  SteppingEngine  │  Evaluator  │
└──────────────────────────┬─────────────────────────────┘
                           │
┌──────────────────────────▼─────────────────────────────┐
│                 Ceres Virtual Machine                  │
│       Fibers  │  CallStack  │  ExecutionUnit Trap      │
└────────────────────────────────────────────────────────┘
```
Supports uninstrumented path execution, interactive CLI debugging, DAP and fiber switching.

## 2. Quickstart: CLI Debugger (`evdb`)
To start debugging any EVIL script, pass the `--debug` flag to the CLI (assumes EVIL in ${PATH}):
```bash
evil --debug path/to/script.vil
```

The debugger pauses execution on entry at the first statement of the script, displays a source context snippet, and gives you an interactive `(evdb)` prompt:

```text
[evdb] Stopped (Pause) at game.vil:1 in function '!_root_chunk' (IP: 0x0000)
=>    1: val Entity = {
      2:   new: self::fn(name, health = 100) -> {
      3:     name: name,
      4:     hp: health,
(evdb) 
```

---

## 3. CLI Command Reference
The `evdb` shell provides GDB/LLDB-style commands:

### Execution Control
| Command | Aliases | Description |
| :--- | :--- | :--- |
| `continue` | `c`, `cont` | Resume program execution until the next breakpoint, pause (or to function end if none). |
| `step` | `s` | **Step Into**: Step to the next statement, descending into function calls. |
| `next` | `n` | **Step Over**: Step to the next statement in the current frame, executing called functions without pausing. |
| `finish` | `fin` | **Step Out**: Continue execution until the current function returns to its caller. |
| `stepi` | `si` | **Instruction Step**: Execute exactly one virtual machine opcode instruction. |
| `quit` | `q`, `exit` | Abort execution and terminate the debugger process. |

### Breakpoints
| Command | Syntax | Description |
| :--- | :--- | :--- |
| `break` | `b <line>` | Set a breakpoint at `<line>` in the current source file. |
| | `b <file>:<line>` | Set a breakpoint at `<line>` in `<file>`. |
| | `b *<address>` | Set an address breakpoint at bytecode offset (e.g. `b *0x0010` or `b *16`). |
| | `b <target> <expr>` | Set a conditional breakpoint (e.g. `b 12 amount > 50` or `b *0x0010 x == 2`). |
| `delete` | `d <id>` | Delete breakpoint by numeric ID. |
| `breakpoints` | `bps`, `info break` | Display a table of all active breakpoints, locations, hit counts, and conditions. |

### Inspection & Evaluation
| Command | Aliases | Description |
| :--- | :--- | :--- |
| `print <expr>` | `p <expr>` | Evaluate any EVIL expression inside the active stack frame's scope and print the result. |
| `set <var> = <expr>` | - | Mutate a local variable, argument, or global in the current frame. |
| `set disasm-on-step <on\|off>` | - | Toggle automatic disassembly snippet display on every step (`s`, `n`, `fin`). |
| `locals` | `info locals` | Display all local variables defined in the selected stack frame. |
| `args` | `info args` | Display all arguments (and `self`) in the selected stack frame. |
| `backtrace` | `bt`, `where` | Display the call stack backtrace with frame indices and IP offsets. |
| `frame <idx>` | `f <idx>` | Select a stack frame by index (e.g. `f 1` to inspect the caller frame). |
| `list [line]` | `l` | Display source code surrounding the current execution pointer or a given line. |
| `disasm [flags] [target]` | `dis` | Disassemble bytecode. Defaults to an 11-instruction window around active IP with live value annotations. Supports relative range slicing (`r <from>:<to>`, e.g. `r 0:10`, `r -10:10`), flag bundling (`-am`, `-abm`), targets (`*<addr>`, `<line>`, `<func>`), and interleaved source. Type `help dis` for full usage. |
| `fibers` | `info fibers` | List all running fibers, their states, and call stack depths. |
| `fiber <id>` | - | Switch active debugging fiber by ID. |
| `help [command]` | `h`, `?` | Display general help or detailed help for a specific command (e.g. `help dis`, `help break`). |

---

## 4. Debugging Walkthrough
Consider the following script `combat.vil`:

```evil
val Entity = {
  new: self::fn(name, hp = 100) -> {
    name: name,
    hp: hp,
    take_damage: self::fn(amount) {
      self.hp -= amount;
      io.println("${self.name} took $amount damage. Remaining: ${self.hp}");
    }
  }
};

val player = Entity::new("Hero", 100);
player::take_damage(15);
player::take_damage(25);
```

### Step 1: Launch Debugger
```bash
evil --debug combat.vil
```

Output:
```text
[evdb] Stopped (Pause) at combat.vil:1 in function '!_root_chunk' (IP: 0x0000)
=>    1: val Entity = {
      2:   new: self::fn(name, hp = 100) -> {
      3:     name: name,
(evdb) 
```

### Step 2: Set Breakpoints & Continue
Set a breakpoint inside `take_damage` on line 7:
```text
(evdb) b 7
[evdb] Breakpoint #1 set at combat.vil:7
(evdb) c
```

Output:
```text
[evdb] Breakpoint #1 bound at combat.vil:7 (IP: 0x0000)

[evdb] Stopped (Breakpoint) at combat.vil:7 in function 'take_damage' (IP: 0x0000)
      5:     name: name,
      6:     hp: hp,
=>    7:     take_damage: self::fn(amount) {
      8:       self.hp -= amount;
      9:       io.println("${self.name} took $amount damage. Remaining: ${self.hp}");
     10:     }
(evdb) 
```

### Step 3: Inspect Variables & Evaluate Expressions
Inspect incoming arguments:
```text
(evdb) args
Arguments:
  self             = <Table: 3 entries>
  amount           = <Number> 15
```

Inspect expressions:
```text
(evdb) p self.name
self.name = <String> "Hero"

(evdb) p self.hp - amount
self.hp - amount = <Number> 85
```

### Step 4: Step Over Execution
Step to the subtraction statement:
```text
(evdb) n
[evdb] Stopped (Step) at combat.vil:8 in function 'take_damage' (IP: 0x0003)
      7:     take_damage: self::fn(amount) {
=>    8:       self.hp -= amount;
      9:       io.println("${self.name} took $amount damage. Remaining: ${self.hp}");
     10:     }
(evdb) n
[evdb] Stopped (Step) at combat.vil:9 in function 'take_damage' (IP: 0x000F)
      8:       self.hp -= amount;
=>    9:       io.println("${self.name} took $amount damage. Remaining: ${self.hp}");
     10:     }
(evdb) p self.hp
self.hp = <Number> 85
```

### Step 5: Mutate Variable at Runtime
Change `self.hp` on the fly:
```text
(evdb) set self.hp = 999
[evdb] Set self.hp = <Number> 999

(evdb) c
Hero took 15 damage. Remaining: 999
```

The program resumed with the mutated state!

## 5. Visual Studio Code & IDE Integration
The `evil` CLI includes a built-in Debug Adapter Protocol server that communicates over `stdio`. Any editor supporting DAP (VS Code, Cursor, Rider, Neovim, Emacs) can debug EVIL scripts natively.

### VS Code Configuration (`launch.json`)
Add the following configuration to `.vscode/launch.json`:

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "EVIL: Debug Script",
      "type": "evil",
      "request": "launch",
      "program": "${file}",
      "stopOnEntry": true
    }
  ]
}
```

### Connecting VS Code to `evil --dap`
If configuring via custom adapter or extension:

```json
{
  "command": "/usr/bin/evil",
  "args": ["--dap"]
}
```

### Supported IDE Debugging Features
- Breakpoints: Click in the margin to set/remove line breakpoints.
- Conditional Breakpoints: Right-click margin $\rightarrow$ "Add Conditional Breakpoint..." $\rightarrow$ enter any EVIL expression (e.g. `amount > 50`).
- Hit Count Breakpoints: Break after $N$ executions or on modulo (e.g. `% 10`).
- Variables Pane:
  - Locals: Local variables declared via `val` or `rw val`.
  - Arguments: Function arguments and `self`.
  - Globals: Global namespace members and loaded runtime modules.
- Watch Window: Live evaluation of expressions while stepping.
- Hover Inspection: Hover over any identifier to see its current runtime value.
- Call Stack: Interactive stack frame navigation across functions and coroutines.

## 6. Programmatic .NET Embedding API
When embedding EVIL inside a .NET host application (such as a game engine, editor, whatever you decided to hook this unholy thing to), you can attach a `DebuggerSession` directly to the `CeresVM`:

```csharp
using System;
using EVIL.Ceres.ExecutionEngine;
using EVIL.Debugger;
using EVIL.Ceres.Runtime;
using EVIL.Ceres.TranslationEngine;

var vm = new CeresVM();
var runtime = new EvilRuntime(vm);
runtime.RegisterBuiltInModules();

/* create a new debugger session. */
var debugger = new DebuggerSession(vm);

/* set breakpoints at runtime, via code. */
debugger.AddBreakpoint("main.vil", 15, condition: "health <= 0");

/* get notified when execution stops. */
debugger.OnStopped += (fiber, frame, reason) =>
{
    Console.WriteLine($"Paused at {frame.Chunk.Name}:{frame.IP} due to {reason}");

    /* can easily inspect locals... */
    var locals = debugger.GetFrameLocals(frame);
    foreach (var (k, v) in locals)
    {
        Console.WriteLine($"  {k} = {v}");
    }

    /* can evaluate evil expressions in the corrent frame context... */
    var eval = debugger.EvaluateInFrame(frame, "player.hp + 10");
    Console.WriteLine($"  Evaluated: {eval}");

    debugger.Continue();
};

var compiler = new Compiler();
var chunk = compiler.Compile(scriptSource, "main.vil");

vm.Run();
vm.MainFiber.Schedule(chunk);
await vm.MainFiber.BlockUntilFinishedAsync();
vm.Stop();
```

## 7. Advanced Features
### Conditional & Hit-Count Breakpoints
In `evdb`:
```text
# Break only when player.hp is less than 20
(evdb) b 14 player.hp < 20

# Break on the 10th time line 25 is hit
(evdb) b 25 == 10

# Break every 5th iteration
(evdb) b 30 % 5
```

### Multi-Fiber (Coroutine) Debugging
When a script yields or spawns coroutines via `yield<func>()`:
```text
(evdb) fibers
Fibers:
=> #10842013 MainFiber    State: Paused  (frames: 2)
   #21948172 Fiber #1     State: Running (frames: 1)

# Switch inspection to Fiber #1:
(evdb) fiber 21948172
Selected active Fiber #21948172.

(evdb) bt
#0 worker at combat.vil:22 (IP: 0x0004)
```

The debugger automatically freezes non-active fibers while you step through the target fiber.

### Runtime Variable Mutation
To fix a bug or change game state mid-run:
```text
(evdb) set player.hp = 100
[evdb] Set player.hp = <Number> 100

(evdb) set multiplier = 3.5
[evdb] Set multiplier = <Number> 3.5
```

### Bytecode & Instruction Stepping
Inspect virtual machine opcodes live with windowed disassembly, active instruction cursor `=>`, and runtime variable annotations:
```text
(evdb) dis
; Disassembly of take_damage (12 instructions, 21 bytes)
   0x0000: GETARG_0             ; arg[0] (self) = <Table> { hp: 100, name: "Hero" }
   0x0001: LDSTR 0x0000         ; "hp"
   0x0002: INDEX
=> 0x0003: GETARG_1             ; arg[1] (amount) = 15
   0x0004: SUB
   0x0005: SETLOCAL_0           ; local[0] (remaining)

(evdb) si
=> 0x0004: SUB
[evdb] Stopped (Step) at combat.vil:10 (IP: 0x0004)

(evdb) b *0x0004
[evdb] Breakpoint #3 set at IP 0x0004 in function 'take_damage'
```
Each `si` executes exactly one VM opcode and prints the active instruction disassembly. Use `set disasm-on-step on` to display the disassembly snippet on every step.

#### Relative Range Windowing (`r <from>:<to>`)
Specify custom instruction windows relative to the current instruction pointer (`frame.IP`):
```text
(evdb) dis r 0:10        # Current instruction and 10 instructions after (11 total)
(evdb) dis r -10:10      # 10 instructions before, current instruction, and 10 instructions after (21 total)
(evdb) dis r -5:0        # 5 instructions before up to the current instruction
(evdb) dis range 5:15    # Instructions from +5 to +15 offsets ahead
```

#### Mixing Flags and Flag Bundling
Combine rendering modes with single flags or bundled short flags:
```text
(evdb) dis -a -m         # Full chunk with interleaved source lines
(evdb) dis -am           # Bundled: all instructions + mixed source
(evdb) dis -abm          # Bundled: all instructions + raw hex bytes + mixed source
(evdb) dis -bm r 0:10    # Relative window with raw hex bytes and mixed source
```

#### Detailed Subcommand Help
Get comprehensive documentation, usage examples, and syntax options for any subcommand:
```text
(evdb) help dis          # or: dis help, dis -h, dis --help
(evdb) help break
(evdb) help print
(evdb) help set
(evdb) help backtrace
```
