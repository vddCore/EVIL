namespace EVIL.VM.Debugger.Cli;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using EVIL.VM.ExecutionEngine;
using EVIL.VM.ExecutionEngine.Concurrency;
using EVIL.VM.ExecutionEngine.Diagnostics;
using EVIL.VM.ExecutionEngine.Diagnostics.Debugging;
using EVIL.VM.ExecutionEngine.Diagnostics.Disassembly;
using EVIL.VM.ExecutionEngine.Diagnostics.Disassembly.Rendering;

public sealed class EvdbSession
{
    private readonly VirtualMachine _vm;
    private readonly DebuggerSession _debugger;
    private readonly string _initialScriptPath;
    private readonly Dictionary<string, string[]> _sourceFiles = new(StringComparer.OrdinalIgnoreCase);

    internal ScriptStackFrame? ActiveFrame { get; set; }

    private int _selectedFrameIndex;
    private bool _quitRequested;
    private StepMode _lastStepMode = StepMode.None;
    private bool _disasmOnStep;

    public EvdbSession(VirtualMachine vm, DebuggerSession debugger, string scriptPath)
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
        ActiveFrame = frame;
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
        Console.Write(
            $" in function '{(string.IsNullOrEmpty(frame.Chunk.Name) 
                ? "<anonymous>" 
                : frame.Chunk.Name)}'"
        );
        Console.ResetColor();
        Console.WriteLine($" (IP: 0x{frame.IP:X4})");

        if (_lastStepMode == StepMode.StepInstruction || _disasmOnStep)
        {
            PrintDisassemblySnippet(frame, (int)frame.IP, radius: 2);
        }
        else
        {
            PrintSourceContext(file, line);
        }

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
                    HandleHelp(args);
                    break;

                case "c":
                case "cont":
                case "continue":
                    _lastStepMode = StepMode.None;
                    _debugger.Continue();
                    return;

                case "s":
                case "step":
                    _lastStepMode = StepMode.StepInto;
                    _debugger.StepInto();
                    return;

                case "n":
                case "next":
                    _lastStepMode = StepMode.StepOver;
                    _debugger.StepOver();
                    return;

                case "fin":
                case "finish":
                    _lastStepMode = StepMode.StepOut;
                    _debugger.StepOut();
                    return;

                case "si":
                case "stepi":
                    _lastStepMode = StepMode.StepInstruction;
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
                case "loc":
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
                    HandleDisassemble(args);
                    break;

                case "i":
                case "info":
                    HandleInfo(args);
                    break;

                case "q":
                case "quit":
                case "exit":
                {
                    _quitRequested = true;
                    _debugger.Continue();
                    _vm.Stop();
                    Environment.Exit(0);
                    return;
                }

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
            if (ActiveFrame != null)
            {
                var curLine = ActiveFrame.Chunk.DebugDatabase.GetLineForIP((int)ActiveFrame.IP);
                var curFile = ActiveFrame.Chunk.DebugDatabase.DefinedInFile;
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

        if (target.StartsWith('*'))
        {
            var addrStr = target.TrimStart('*').Trim();
            int ip;
            if (addrStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(addrStr[2..], NumberStyles.HexNumber, null, out ip))
                {
                    Console.WriteLine($"Invalid hex address '{addrStr}'.");
                    return;
                }
            }
            else if (!int.TryParse(addrStr, out ip))
            {
                Console.WriteLine($"Invalid address '{addrStr}'.");
                return;
            }

            var frame = GetSelectedFrame();
            if (frame == null)
            {
                Console.WriteLine("No active stack frame to resolve address breakpoint.");
                return;
            }

            var addrBpId = _debugger.AddAddressBreakpoint(frame.Chunk, ip, condition);
            Console.WriteLine(
                $"[evdb] Breakpoint #{addrBpId} set at IP 0x{ip:X4} in function " +
                $"'{(string.IsNullOrEmpty(frame.Chunk.Name) ? "<anonymous>" : frame.Chunk.Name)}'" +
                $"{(condition != null ? $" if ({condition})" : "")}");
            return;
        }

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
            file = ActiveFrame?.Chunk.DebugDatabase.DefinedInFile ?? _initialScriptPath;
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
            Console.WriteLine(
                _debugger.RemoveBreakpoint(id)
                    ? $"[evdb] Breakpoint #{id} deleted."
                    : $"[evdb] Breakpoint #{id} not found."
            );
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
        if (args.StartsWith("disasm-on-step", StringComparison.OrdinalIgnoreCase))
        {
            var valPart = args["disasm-on-step".Length..]
                .Trim()
                .TrimStart('=')
                .Trim()
                .ToLowerInvariant();
            
            _disasmOnStep = valPart is "on" or "true" or "1" or "yes";
            
            Console.WriteLine($"[evdb] disasm-on-step = {(_disasmOnStep ? "on" : "off")}");
            return;
        }

        var parts = args.Split('=', 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            Console.WriteLine("Usage: set <variable> = <expression> OR set disasm-on-step on|off");
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
                Console.WriteLine(
                    $"{marker} #{i}  {ssf.Chunk.Name ?? "<anonymous>"} at {file}:{line} " +
                    $"(IP: 0x{ssf.PreviousOpCodeIP:X4})"
                );
            }
            else if (callStack[i] is NativeStackFrame nsf)
            {
                Console.WriteLine(
                    $"{marker} #{i}  clr!{nsf.NativeFunction.Method.DeclaringType?.Name}" +
                    $"::{nsf.NativeFunction.Method.Name}"
                );
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
        if (frame == null)
        {
            return;
        }

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
        if (frame == null)
        {
            return;
        }

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
            case "loc":
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

    internal void HandleDisassemble(string args, TextWriter? output = null)
    {
        output ??= Console.Out;
        var frame = GetSelectedFrame();
        if (frame == null)
        {
            output.WriteLine("No active stack frame.");
            return;
        }

        var tokens = args.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length > 0 
            && (tokens[0] is "help" or "-h" or "--help" 
                || tokens.Any(t => t is "help" or "--help")))
        {
            PrintSubcommandHelp("dis", output);
            return;
        }

        var showRawBytes = false;
        var showMixed = true;
        var includeSubChunks = false;
        var showAll = false;
        int count = 11;
        string? targetToken = null;
        (int from, int to)? relativeRange = null;

        for (var i = 0; i < tokens.Length; i++)
        {
            var token = tokens[i];
            var lower = token.ToLowerInvariant();

            if (lower is "r" or "range")
            {
                if (i + 1 < tokens.Length)
                {
                    i++;
                    var rangeArg = tokens[i];
                    if (TryParseRelativeRange(rangeArg, out var parsedRange))
                    {
                        relativeRange = parsedRange;
                    }
                    else
                    {
                        output.WriteLine($"Invalid relative range '{rangeArg}'. Expected format: r <before>:<after> (e.g. r 0:10, r -10:10)");
                        return;
                    }
                }
                else
                {
                    output.WriteLine("Usage: dis r <before>:<after> (e.g. dis r 0:10, dis r -10:10)");
                    return;
                }
            }
            else if (lower.StartsWith("r:") || lower.StartsWith("range:"))
            {
                var colonIdx = token.IndexOf(':');
                var rangeArg = token[(colonIdx + 1)..];
                if (TryParseRelativeRange(rangeArg, out var parsedRange))
                {
                    relativeRange = parsedRange;
                }
                else
                {
                    output.WriteLine($"Invalid relative range '{rangeArg}'. Expected format: r:<before>:<after> (e.g. r:0:10, r:-10:10)");
                    return;
                }
            }
            else switch (lower)
            {
                case "--bytes":
                    showRawBytes = true;
                    break;
                case "--mixed":
                    showMixed = true;
                    break;
                case "--no-mixed" or "--no-source":
                    showMixed = false;
                    break;
                case "--subchunks":
                    includeSubChunks = true;
                    break;
                case "--all" or "all":
                    showAll = true;
                    break;
                default:
                {
                    if (token.StartsWith('-') 
                        && !token.StartsWith("--") 
                        && token.Length > 1 
                        && char.IsLetter(token[1]))
                    {
                        for (var c = 1; c < token.Length; c++)
                        {
                            switch (char.ToLowerInvariant(token[c]))
                            {
                                case 'a': showAll = true; break;
                                case 'b': showRawBytes = true; break;
                                case 'm': showMixed = true; break;
                                case 's': includeSubChunks = true; break;
                                default:
                                    output.WriteLine($"Unknown flag '-{token[c]}'. Type 'help dis' for usage.");
                                    return;
                            }
                        }
                    }
                    else
                    {
                        if (int.TryParse(token, out var parsedCount)
                            && !token.StartsWith('*')
                            && !token.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        {
                            count = parsedCount;
                        }
                        else
                        {
                            targetToken = token;
                        }
                    }

                    break;
                }
            }
        }

        var targetChunk = frame.Chunk;
        int? startAddress = null;
        int? targetLine = null;

        if (!string.IsNullOrEmpty(targetToken))
        {
            if (targetToken.StartsWith('*'))
            {
                var addrStr = targetToken.TrimStart('*').Trim();
                if (addrStr.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(addrStr[2..], NumberStyles.HexNumber, null, out var hexIp))
                    {
                        startAddress = hexIp;
                    }
                }
                else if (int.TryParse(addrStr, out var decIp))
                {
                    startAddress = decIp;
                }
            }
            else if (targetToken.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(targetToken[2..], NumberStyles.HexNumber, null, out var hexIp))
                {
                    startAddress = hexIp;
                }
            }
            else
            {
                var root = (_vm.MainFiber.CallStack.Count > 0 
                    ? (_vm.MainFiber.CallStack[0] as ScriptStackFrame)?.Chunk 
                    : null
                ) ?? frame.Chunk;
                
                var found = FindSubChunk(frame.Chunk, targetToken) ?? FindSubChunk(root, targetToken);
                if (found != null)
                {
                    targetChunk = found;
                }
                else if (int.TryParse(targetToken, out var num))
                {
                    if (num >= 0 && num < frame.Chunk.SubChunkCount)
                    {
                        targetChunk = frame.Chunk.SubChunks[num];
                    }
                    else if (frame.Chunk.HasDebugInfo 
                             && frame.Chunk.DebugDatabase.GetFirstIPForLine(num) >= 0)
                    {
                        targetLine = num;
                    }
                }
            }
        }

        var disasm = targetChunk.Disassemble();

        var options = new DisassemblyFormatOptions
        {
            ShowAddresses = true,
            AddressFormat = AddressFormat.HexPrefixed,
            ShowRawBytes = showRawBytes,
            ShowSourceInterleaving = showMixed,
            ShowSymbols = true,
            ShowRuntimeValues = true,
            ShowBranchLabels = true,
            ShowBranchOffsets = true,
            ShowChunkHeader = false,
            IncludeSubChunks = includeSubChunks,
            HighlightIP = (targetChunk == frame.Chunk) 
                ? (int)frame.IP 
                : null,
            Breakpoints = _debugger.GetBreakpoints(),
            FrameContext = (targetChunk == frame.Chunk) 
                ? frame 
                : null,
            SourceProvider = GetSourceLines,
            ColorMode = ColorMode.Auto
        };

        output.WriteLine(
            $"Disassembly of " +
            $"{targetChunk.Name}{(targetChunk is { HasDebugInfo: true, DebugDatabase.DefinedOnLine: > 0 } 
                ? $" (def. on line {targetChunk.DebugDatabase.DefinedOnLine})" 
                : "")
            }:"
        );

        if (targetLine.HasValue)
        {
            var lineInsts = disasm.GetForLine(targetLine.Value);
            if (lineInsts.Count == 0)
            {
                output.WriteLine($"No instructions found for line {targetLine.Value}.");
                return;
            }

            DisassemblyFormatter.FormatInstructions(
                lineInsts,
                disasm,
                output,
                options
            );
        }
        else if (relativeRange.HasValue)
        {
            if (disasm.Instructions.Count == 0)
            {
                output.WriteLine("No instructions in chunk.");
                return;
            }

            var centerIp = (targetChunk == frame.Chunk) 
                ? (int)frame.IP 
                : 0;
            
            var idx = disasm.IndexOf(centerIp);
            
            if (idx < 0)
            {
                for (var k = disasm.Instructions.Count - 1; k >= 0; k--)
                {
                    if (disasm.Instructions[k].Offset <= centerIp)
                    {
                        idx = k;
                        break;
                    }
                }
                
                if (idx < 0)
                {
                    idx = 0;
                }
            }

            var startIdx = idx + relativeRange.Value.from;
            var endIdx = idx + relativeRange.Value.to;

            if (startIdx > endIdx)
            {
                (startIdx, endIdx) = (endIdx, startIdx);
            }

            startIdx = Math.Max(0, startIdx);
            endIdx = Math.Min(disasm.Instructions.Count - 1, endIdx);

            if (startIdx >= disasm.Instructions.Count || endIdx < 0 || startIdx > endIdx)
            {
                output.WriteLine("Specified relative range is outside of chunk instruction bounds.");
                return;
            }

            var slice = new List<Instruction>(endIdx - startIdx + 1);
            for (var k = startIdx; k <= endIdx; k++)
            {
                slice.Add(disasm.Instructions[k]);
            }

            DisassemblyFormatter.FormatInstructions(
                slice,
                disasm,
                output,
                options
            );
        }
        else if (startAddress.HasValue)
        {
            DisassemblyFormatter.FormatRange(
                disasm,
                output,
                startAddress.Value,
                count,
                options
            );
        }
        else if (showAll)
        {
            DisassemblyFormatter.FormatInstructions(
                disasm.Instructions,
                disasm,
                output,
                options
            );

            if (includeSubChunks && targetChunk.SubChunkCount > 0)
            {
                for (var s = 0; s < targetChunk.SubChunkCount; s++)
                {
                    var sub = targetChunk.SubChunks[s];
                    output.WriteLine();
                    output.WriteLine($"Subchunk '{sub.Name}':");
                    DisassemblyFormatter.FormatInstructions(
                        sub.Disassemble().Instructions,
                        sub.Disassemble(),
                        output,
                        options
                    );
                }
            }
        }
        else
        {
            var centerIp = (targetChunk == frame.Chunk) 
                ? (int)frame.IP 
                : 0;
            
            var radius = Math.Max(1, count / 2);
            
            DisassemblyFormatter.FormatWindow(
                disasm,
                output,
                centerIp,
                options, 
                radiusBefore: radius,
                radiusAfter: radius
            );
        }
    }

    private void PrintDisassemblySnippet(ScriptStackFrame frame, int ip, int radius = 2)
    {
        var disasm = frame.Chunk.Disassemble();
        var options = new DisassemblyFormatOptions
        {
            ShowAddresses = true,
            AddressFormat = AddressFormat.HexPrefixed,
            ShowRawBytes = false,
            ShowSourceInterleaving = true,
            ShowSymbols = true,
            ShowRuntimeValues = true,
            ShowBranchLabels = true,
            ShowBranchOffsets = true,
            ShowChunkHeader = false,
            HighlightIP = ip,
            Breakpoints = _debugger.GetBreakpoints(),
            FrameContext = frame,
            SourceProvider = GetSourceLines,
            ColorMode = ColorMode.Auto
        };

        DisassemblyFormatter.FormatWindow(
            disasm,
            Console.Out,
            ip,
            options,
            radiusBefore: radius,
            radiusAfter: radius
        );
    }

    private static Chunk? FindSubChunk(Chunk chunk, string name)
    {
        if (string.Equals(chunk.Name, name, StringComparison.OrdinalIgnoreCase))
        {
            return chunk;
        }

        for (var i = 0; i < chunk.SubChunkCount; i++)
        {
            var sub = chunk.SubChunks[i];
            if (string.Equals(sub.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return sub;
            }
            
            var nested = FindSubChunk(sub, name);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    private ScriptStackFrame? GetSelectedFrame()
    {
        var fiber = _debugger.ActiveFiber ?? _vm.MainFiber;
        if (_selectedFrameIndex < fiber.CallStack.Count)
        {
            return fiber.CallStack[_selectedFrameIndex] as ScriptStackFrame 
                ?? ActiveFrame;
        }
        
        return ActiveFrame;
    }

    private string[]? GetSourceLines(string filePath)
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
        }

        return lines;
    }

    private void PrintSourceContext(string filePath, int centerLine, int radius = 4)
    {
        var lines = GetSourceLines(filePath);
        if (lines == null || lines.Length == 0)
        {
            Console.WriteLine($"[Source file '{filePath}' not found on disk]");
            return;
        }

        var start = Math.Max(1, centerLine - radius);
        var end = Math.Min(lines.Length, centerLine + radius);
        var bps = _debugger.GetBreakpoints();

        for (var i = start; i <= end; i++)
        {
            var isCurrent = (i == centerLine);
            var hasBp = bps.Any(
                b => b.Line == i 
                  && Path.GetFileName(b.FilePath)
                         .Equals(Path.GetFileName(filePath), StringComparison.OrdinalIgnoreCase)
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

            Console.ForegroundColor = isCurrent 
                ? ConsoleColor.Yellow 
                : ConsoleColor.DarkGray;
            
            Console.Write($"{i,4}: ");

            Console.ForegroundColor = isCurrent 
                ? ConsoleColor.White 
                : ConsoleColor.Gray;
            
            Console.WriteLine(lines[i - 1]);
            Console.ResetColor();
        }
    }

    private static bool TryParseRelativeRange(string rangeStr, out (int from, int to) range)
    {
        range = default((int, int));
        
        var sep = rangeStr.Contains("..") ? ".." : ":";
        if (!rangeStr.Contains(sep))
        {
            return false;
        }

        var parts = rangeStr.Split(sep, 2, StringSplitOptions.TrimEntries);
        if (!int.TryParse(parts[0], out var from) || !int.TryParse(parts[1], out var to))
        {
            return false;
        }

        range = (from, to);
        return true;
    }

    internal static void HandleHelp(string args, TextWriter? output = null)
    {
        if (string.IsNullOrWhiteSpace(args))
        {
            PrintGeneralHelp(output);
            return;
        }

        PrintSubcommandHelp(args.Trim(), output);
    }

    internal static void PrintGeneralHelp(TextWriter? output = null)
    {
        var outWriter = output ?? Console.Out;
        outWriter.WriteLine(StringResources.GeneralHelp);
    }

    internal static void PrintSubcommandHelp(string subcmd, TextWriter? output = null)
    {
        var outWriter = output ?? Console.Out;
        switch (subcmd.ToLowerInvariant())
        {
            case "dis":
            case "disasm":
            case "disassemble":
                outWriter.WriteLine(StringResources.DisassembleSubCommandHelp);
                break;

            case "b":
            case "break":
            case "breakpoint":
                outWriter.WriteLine(StringResources.BreakpointSubCommandHelp);
                break;

            case "d":
            case "delete":
                outWriter.WriteLine(StringResources.DeleteSubCommandHelp);
                break;

            case "breakpoints":
            case "bps":
                outWriter.WriteLine(StringResources.BreakpointsSubCommandHelp);
                break;

            case "c":
            case "cont":
            case "continue":
                outWriter.WriteLine(StringResources.ContinueSubCommandHelp);
                break;

            case "s":
            case "step":
                outWriter.WriteLine(StringResources.StepSubCommandHelp);
                break;

            case "n":
            case "next":
                outWriter.WriteLine(StringResources.NextSubCommandHelp);
                break;

            case "fin":
            case "finish":
                outWriter.WriteLine(StringResources.FinishSubCommandHelp);
                break;

            case "si":
            case "stepi":
                outWriter.WriteLine(StringResources.StepiSubCommandHelp);
                break;

            case "p":
            case "print":
                outWriter.WriteLine(StringResources.PrintSubCommandHelp);
                break;

            case "set":
                outWriter.WriteLine(StringResources.SetSubCommandHelp);
                break;

            case "bt":
            case "backtrace":
            case "where":
                outWriter.WriteLine(StringResources.BacktraceSubCommandHelp);
                break;

            case "f":
            case "frame":
                outWriter.WriteLine(StringResources.FrameSubCommandHelp);
                break;

            case "loc":
            case "locals":
                outWriter.WriteLine(StringResources.LocalsSubCommandHelp);
                break;

            case "args":
                outWriter.WriteLine(StringResources.ArgsSubCommandHelp);
                break;

            case "fibers":
                outWriter.WriteLine(StringResources.FibersSubCommandHelp);
                break;

            case "fiber":
                outWriter.WriteLine(StringResources.FiberSubCommandHelp);
                break;

            case "info":
                outWriter.WriteLine(StringResources.InfoSubCommandHelp);
                break;

            case "l":
            case "list":
                outWriter.WriteLine(StringResources.ListSubCommandHelp);
                break;

            case "q":
            case "quit":
            case "exit":
                outWriter.WriteLine(StringResources.QuitSubCommandHelp);
                break;

            case "h":
            case "help":
            case "?":
                PrintGeneralHelp(outWriter);
                break;

            default:
                outWriter.WriteLine($"Unknown command '{subcmd}'. Type 'help' for available commands.");
                break;
        }
    }
}
