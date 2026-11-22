namespace EVIL.Debugger.Cli;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EVIL.Ceres.ExecutionEngine;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.Diagnostics.Debugging;
using EVIL.Ceres.ExecutionEngine.Diagnostics.Disassembly;

public sealed class EvdbSession
{
    private readonly CeresVM _vm;
    private readonly DebuggerSession _debugger;
    private readonly string _initialScriptPath;
    private readonly Dictionary<string, string[]> _sourceFiles = new(StringComparer.OrdinalIgnoreCase);

    private ScriptStackFrame? _activeFrame;
    private int _selectedFrameIndex;
    private bool _quitRequested;

    public EvdbSession(CeresVM vm, DebuggerSession debugger, string scriptPath)
    {
        _vm = vm;
        _debugger = debugger;
        _initialScriptPath = scriptPath;

        _debugger.OnStopped += HandleStopped;
        _debugger.OnBreakpointBound += bp =>
        {
            Console.WriteLine($"[evdb] Breakpoint #{bp.Id} bound at {bp.FilePath}:{bp.Line} (IP: 0x{bp.BoundIP:X4})");
        };
    }

    private void HandleStopped(Fiber fiber, ScriptStackFrame frame, StopReason reason)
    {
        _activeFrame = frame;
        _selectedFrameIndex = 0;

        var line = frame.Chunk.DebugDatabase.GetLineForIP((int)frame.IP);
        var file = frame.Chunk.DebugDatabase.DefinedInFile;
        if (string.IsNullOrEmpty(file))
        {
            file = _initialScriptPath;
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"[evdb] Stopped ({reason}) at ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"{Path.GetFileName(file)}:{line}");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($" in function '{(string.IsNullOrEmpty(frame.Chunk.Name) ? "<anonymous>" : frame.Chunk.Name)}'");
        Console.ResetColor();
        Console.WriteLine($" (IP: 0x{frame.IP:X4})");

        PrintSourceContext(file, line);
        EnterPromptLoop();
    }

    private void EnterPromptLoop()
    {
        while (_debugger.IsPaused && !_quitRequested)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("(evdb) ");
            Console.ResetColor();

            var line = Console.ReadLine();
            if (line == null)
            {
                break;
            }

            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                continue;
            }

            var parts = trimmed.Split(' ', 2, StringSplitOptions.TrimEntries);
            var cmd = parts[0].ToLowerInvariant();
            var args = parts.Length > 1 ? parts[1] : string.Empty;

            switch (cmd)
            {
                case "h":
                case "help":
                case "?":
                    PrintHelp();
                    break;

                case "c":
                case "cont":
                case "continue":
                    _debugger.Continue();
                    return;

                case "s":
                case "step":
                    _debugger.StepInto();
                    return;

                case "n":
                case "next":
                    _debugger.StepOver();
                    return;

                case "fin":
                case "finish":
                    _debugger.StepOut();
                    return;

                case "si":
                case "stepi":
                    _debugger.StepInstruction();
                    return;

                case "b":
                case "break":
                    HandleAddBreakpoint(args);
                    break;

                case "d":
                case "delete":
                    HandleDeleteBreakpoint(args);
                    break;

                case "breakpoints":
                case "bps":
                    ListBreakpoints();
                    break;

                case "p":
                case "print":
                    HandleEvaluate(args);
                    break;

                case "set":
                    HandleSet(args);
                    break;

                case "bt":
                case "backtrace":
                case "where":
                    PrintBacktrace();
                    break;

                case "f":
                case "frame":
                    HandleSelectFrame(args);
                    break;

                case "l":
                case "list":
                    HandleList(args);
                    break;

                case "locals":
                    PrintLocals();
                    break;

                case "args":
                    PrintArguments();
                    break;

                case "fibers":
                    PrintFibers();
                    break;

                case "fiber":
                    HandleSelectFiber(args);
                    break;

                case "dis":
                case "disasm":
                    DisassembleCurrentChunk();
                    break;

                case "info":
                    HandleInfo(args);
                    break;

                case "q":
                case "quit":
                case "exit":
                    _quitRequested = true;
                    _debugger.Continue();
                    _vm.Stop();
                    Environment.Exit(0);
                    return;

                default:
                    Console.WriteLine($"Unknown command '{cmd}'. Type 'help' for available commands.");
                    break;
            }
        }
    }

    private void HandleAddBreakpoint(string args)
    {
        if (string.IsNullOrWhiteSpace(args))
        {
            if (_activeFrame != null)
            {
                var curLine = _activeFrame.Chunk.DebugDatabase.GetLineForIP((int)_activeFrame.IP);
                var curFile = _activeFrame.Chunk.DebugDatabase.DefinedInFile;
                if (curLine > 0)
                {
                    var bp = _debugger.AddBreakpoint(curFile, curLine);
                    Console.WriteLine($"[evdb] Breakpoint #{bp} set at {Path.GetFileName(curFile)}:{curLine}");
                    return;
                }
            }
            Console.WriteLine("Usage: break <line> OR break <file>:<line> [condition]");
            return;
        }

        var tokens = args.Split(' ', 2, StringSplitOptions.TrimEntries);
        var target = tokens[0];
        var condition = tokens.Length > 1 ? tokens[1] : null;

        string file;
        int line;

        if (target.Contains(':'))
        {
            var split = target.Split(':', 2);
            file = split[0];
            if (!int.TryParse(split[1], out line))
            {
                Console.WriteLine($"Invalid line number '{split[1]}'.");
                return;
            }
        }
        else
        {
            file = _activeFrame?.Chunk.DebugDatabase.DefinedInFile ?? _initialScriptPath;
            if (!int.TryParse(target, out line))
            {
                Console.WriteLine($"Invalid line number '{target}'.");
                return;
            }
        }

        var id = _debugger.AddBreakpoint(file, line, 0, condition);
        Console.WriteLine($"[evdb] Breakpoint #{id} set at {Path.GetFileName(file)}:{line}{(condition != null ? $" if ({condition})" : "")}");
    }

    private void HandleDeleteBreakpoint(string args)
    {
        if (int.TryParse(args.Trim(), out var id))
        {
            if (_debugger.RemoveBreakpoint(id))
            {
                Console.WriteLine($"[evdb] Breakpoint #{id} deleted.");
            }
            else
            {
                Console.WriteLine($"[evdb] Breakpoint #{id} not found.");
            }
        }
        else
        {
            Console.WriteLine("Usage: delete <breakpoint_id>");
        }
    }

    private void ListBreakpoints()
    {
        var bps = _debugger.GetBreakpoints();
        if (bps.Count == 0)
        {
            Console.WriteLine("No breakpoints currently set.");
            return;
        }

        Console.WriteLine("Num  File:Line             Status    Hits  Condition");
        Console.WriteLine("---  --------------------  --------  ----  ---------");
        foreach (var bp in bps)
        {
            var loc = $"{Path.GetFileName(bp.FilePath)}:{bp.Line}";
            var status = bp.IsVerified 
                ? "verified" 
                : "pending";
            
            Console.WriteLine($"{bp.Id,-3}  {loc,-20}  {status,-8}  {bp.HitCount,-4}  {bp.Condition ?? ""}");
        }
    }

    private void HandleEvaluate(string expr)
    {
        if (string.IsNullOrWhiteSpace(expr))
        {
            Console.WriteLine("Usage: print <expression>");
            return;
        }

        var frame = GetSelectedFrame();
        if (frame == null)
        {
            Console.WriteLine("No active stack frame.");
            return;
        }

        var result = _debugger.EvaluateInFrame(frame, expr);
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"{expr} = ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(result.ToString());
        Console.ResetColor();
    }

    private void HandleSet(string args)
    {
        var parts = args.Split('=', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            Console.WriteLine("Usage: set <variable> = <expression>");
            return;
        }

        var frame = GetSelectedFrame();
        if (frame == null)
        {
            Console.WriteLine("No active stack frame.");
            return;
        }

        var varName = parts[0];
        var exprVal = _debugger.EvaluateInFrame(frame, parts[1]);
        _debugger.SetFrameLocal(frame, varName, exprVal);
        Console.WriteLine($"[evdb] Set {varName} = {exprVal}");
    }

    private void PrintBacktrace()
    {
        var fiber = _debugger.ActiveFiber ?? _vm.MainFiber;
        var callStack = fiber.CallStack.ToArray();

        Console.WriteLine($"Backtrace for Fiber #{fiber.GetHashCode()}:");
        for (var i = 0; i < callStack.Length; i++)
        {
            var marker = i == _selectedFrameIndex ? "=>" : "  ";
            if (callStack[i] is ScriptStackFrame ssf)
            {
                var line = ssf.Chunk.DebugDatabase.GetLineForIP((int)ssf.PreviousOpCodeIP);
                var file = Path.GetFileName(ssf.Chunk.DebugDatabase.DefinedInFile);
                Console.WriteLine($"{marker} #{i}  {ssf.Chunk.Name ?? "<anonymous>"} at {file}:{line} (IP: 0x{ssf.PreviousOpCodeIP:X4})");
            }
            else if (callStack[i] is NativeStackFrame nsf)
            {
                Console.WriteLine($"{marker} #{i}  clr!{nsf.NativeFunction.Method.DeclaringType?.Name}::{nsf.NativeFunction.Method.Name}");
            }
        }
    }

    private void HandleSelectFrame(string args)
    {
        if (int.TryParse(args.Trim(), out var idx))
        {
            var fiber = _debugger.ActiveFiber ?? _vm.MainFiber;
            if (idx >= 0 && idx < fiber.CallStack.Count)
            {
                _selectedFrameIndex = idx;
                var frame = GetSelectedFrame();
                if (frame != null)
                {
                    var line = frame.Chunk.DebugDatabase.GetLineForIP((int)frame.PreviousOpCodeIP);
                    var file = frame.Chunk.DebugDatabase.DefinedInFile;
                    Console.WriteLine($"Selected frame #{idx}: {frame.Chunk.Name} at {Path.GetFileName(file)}:{line}");
                    PrintSourceContext(file, line);
                }
            }
            else
            {
                Console.WriteLine($"Frame index #{idx} out of range (0..{fiber.CallStack.Count - 1}).");
            }
        }
        else
        {
            Console.WriteLine($"Current frame: #{_selectedFrameIndex}");
        }
    }

    private void PrintLocals()
    {
        var frame = GetSelectedFrame();
        if (frame == null) return;

        var locals = _debugger.GetFrameLocals(frame);
        if (locals.Count == 0)
        {
            Console.WriteLine("No locals in active frame.");
            return;
        }

        Console.WriteLine("Locals:");
        foreach (var (k, v) in locals)
        {
            Console.WriteLine($"  {k,-16} = {v}");
        }
    }

    private void PrintArguments()
    {
        var frame = GetSelectedFrame();
        if (frame == null) return;

        var args = _debugger.GetFrameArguments(frame);
        if (args.Count == 0)
        {
            Console.WriteLine("No arguments in active frame.");
            return;
        }

        Console.WriteLine("Arguments:");
        foreach (var (k, v) in args)
        {
            Console.WriteLine($"  {k,-16} = {v}");
        }
    }

    private void PrintFibers()
    {
        var fibers = _debugger.GetFibers();
        Console.WriteLine("Fibers:");
        for (var i = 0; i < fibers.Count; i++)
        {
            var marker = fibers[i] == _debugger.ActiveFiber ? "=>" : "  ";
            var name = fibers[i] == _vm.MainFiber ? "MainFiber" : $"Fiber #{i}";
            
            Console.WriteLine(
                $"{marker} #{fibers[i].GetHashCode()} {name,-12} " +
                $"State: {fibers[i].State} (frames: {fibers[i].CallStack.Count})"
            );
        }
    }

    private void HandleSelectFiber(string args)
    {
        if (int.TryParse(args.Trim(), out var id))
        {
            var fibers = _debugger.GetFibers();
            var target = fibers.FirstOrDefault(f => f.GetHashCode() == id);
            if (target != null)
            {
                _debugger.ActiveFiber = target;
                _selectedFrameIndex = 0;
                Console.WriteLine($"Selected active Fiber #{id}.");
            }
            else
            {
                Console.WriteLine($"Fiber #{id} not found.");
            }
        }
        else
        {
            Console.WriteLine("Usage: fiber <fiber_id>");
        }
    }

    private void HandleInfo(string args)
    {
        var sub = args.Trim().ToLowerInvariant();
        switch (sub)
        {
            case "locals":
            case "l":
                PrintLocals();
                break;
            
            case "args":
            case "a":
                PrintArguments();
                break;
            
            case "break":
            case "breakpoints":
            case "b":
                ListBreakpoints();
                break;
            
            case "fibers":
                PrintFibers();
                break;
            
            default:
                Console.WriteLine("Available info topics: locals, args, breakpoints, fibers");
                break;
        }
    }

    private void HandleList(string args)
    {
        var frame = GetSelectedFrame();
        if (frame == null)
        {
            return;
        }

        var curLine = frame.Chunk.DebugDatabase.GetLineForIP((int)frame.IP);
        var curFile = frame.Chunk.DebugDatabase.DefinedInFile;

        if (int.TryParse(args.Trim(), out var customLine))
        {
            curLine = customLine;
        }

        PrintSourceContext(curFile, curLine, radius: 7);
    }

    private void DisassembleCurrentChunk()
    {
        var frame = GetSelectedFrame();
        if (frame == null)
        {
            return;
        }

        Console.WriteLine($"Disassembly of {frame.Chunk.Name}:");
        Disassembler.Disassemble(frame.Chunk, Console.Out);
    }

    private ScriptStackFrame? GetSelectedFrame()
    {
        var fiber = _debugger.ActiveFiber ?? _vm.MainFiber;
        if (_selectedFrameIndex < fiber.CallStack.Count)
        {
            return fiber.CallStack[_selectedFrameIndex] as ScriptStackFrame ?? _activeFrame;
        }
        
        return _activeFrame;
    }

    private void PrintSourceContext(string filePath, int centerLine, int radius = 4)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            filePath = _initialScriptPath;
        }

        if (!_sourceFiles.TryGetValue(filePath, out var lines))
        {
            if (File.Exists(filePath))
            {
                lines = File.ReadAllLines(filePath);
                _sourceFiles[filePath] = lines;
            }
            else
            {
                Console.WriteLine($"[Source file '{filePath}' not found on disk]");
                return;
            }
        }

        var start = Math.Max(1, centerLine - radius);
        var end = Math.Min(lines.Length, centerLine + radius);
        var bps = _debugger.GetBreakpoints();

        for (var i = start; i <= end; i++)
        {
            var isCurrent = (i == centerLine);
            var hasBp = bps.Any(
                b => b.Line == i && Path.GetFileName(b.FilePath).Equals(Path.GetFileName(filePath),
                StringComparison.OrdinalIgnoreCase)
            );

            if (isCurrent)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("=> ");
            }
            else if (hasBp)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(" * ");
            }
            else
            {
                Console.Write("   ");
            }

            Console.ForegroundColor = isCurrent ? ConsoleColor.Yellow : ConsoleColor.DarkGray;
            Console.Write($"{i,4}: ");

            Console.ForegroundColor = isCurrent ? ConsoleColor.White : ConsoleColor.Gray;
            Console.WriteLine(lines[i - 1]);
            Console.ResetColor();
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine(@"
EVIL Command-Line Debugger (evdb)
---------------------------------
Execution:
  c, continue         Resume program execution until next breakpoint or pause.
  s, step             Step into the next statement or function call.
  n, next             Step over the next statement.
  fin, finish         Step out of current function to caller.
  si, stepi           Execute exactly one VM opcode instruction.
  q, quit             Abort execution and exit debugger.

Breakpoints:
  b, break <line>             Set a breakpoint on current file at <line>.
  b, break <file>:<line>      Set a breakpoint on <file> at <line>.
  b <line> <condition>        Set a conditional breakpoint (e.g. b 12 x > 5).
  d, delete <id>              Delete breakpoint by ID.
  breakpoints, bps            List all active breakpoints.

Inspection:
  p, print <expr>     Evaluate and print an expression in the active stack frame.
  set <var> = <expr>  Mutate a local variable in the active stack frame.
  bt, backtrace       Display call stack frames.
  f, frame <index>    Select active stack frame for inspection.
  locals              Display local variables in the active stack frame.
  args                Display arguments in the active stack frame.
  fibers              Display all running coroutines/fibers.
  fiber <id>          Select active fiber.
  l, list [line]      Display source code context.
  dis, disasm         Disassemble bytecode of the current chunk.
  h, help             Display this help message.
");
    }
}
