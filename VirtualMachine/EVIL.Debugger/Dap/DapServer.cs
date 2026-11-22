namespace EVIL.Debugger.Dap;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EVIL.Ceres.ExecutionEngine;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.Diagnostics.Debugging;
using EVIL.Ceres.ExecutionEngine.Diagnostics.Disassembly;
using EVIL.Ceres.Runtime;
using EVIL.Ceres.TranslationEngine;
using EVIL.Debugger.Dap.Messages;

public sealed class DapServer
{
    private readonly Stream _input;
    private readonly Stream _output;
    private readonly Lock _writeLock = new();
    private int _seq = 1;
    private bool _running = true;

    private CeresVM? _vm;
    private EvilRuntime? _runtime;
    private DebuggerSession? _session;
    private Chunk? _rootChunk;

    private readonly ConcurrentDictionary<int, ScriptStackFrame> _frameLookup = new();
    private readonly ConcurrentDictionary<int, Chunk> _chunkById = new();
    private readonly ConcurrentDictionary<Chunk, int> _idByChunk = new();
    private int _nextChunkId = 1;

    private readonly ConcurrentDictionary<
        int,
        IReadOnlyDictionary<string, Ceres.ExecutionEngine.TypeSystem.DynamicValue>
    > _scopeVariablesLookup = [];
    
    private int _nextFrameId = 1;
    private int _nextVarRef = 1000;

    public DapServer(Stream input, Stream output)
    {
        _input = input;
        _output = output;
    }

    public DapServer(Stream input, Stream output, CeresVM? vm, DebuggerSession? session, Chunk? rootChunk)
        : this(input, output)
    {
        _vm = vm;
        _session = session;
        _rootChunk = rootChunk;
        if (rootChunk != null)
        {
            RegisterChunkTree(rootChunk);
        }
    }

    public async Task RunAsync()
    {
        var reader = new StreamReader(_input, Encoding.UTF8);

        while (_running)
        {
            var header = await ReadHeaderAsync(reader);
            if (header == null)
            {
                break;
            }

            if (!header.TryGetValue("Content-Length", out var lengthStr) || !int.TryParse(lengthStr, out var contentLength))
            {
                continue;
            }

            var buffer = new char[contentLength];
            var read = 0;
            while (read < contentLength)
            {
                var n = await reader.ReadAsync(buffer, read, contentLength - read);
                if (n == 0) break;
                read += n;
            }

            var json = new string(buffer, 0, read);
            if (string.IsNullOrWhiteSpace(json)) continue;

            try
            {
                var request = JsonSerializer.Deserialize<DapRequest>(json);
                if (request != null)
                {
                    await HandleRequestAsync(request);
                }
            }
            catch (Exception ex)
            {
                SendErrorResponse(0, "error", ex.Message);
            }
        }
    }

    private static async Task<Dictionary<string, string>?> ReadHeaderAsync(StreamReader reader)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        while (true)
        {
            var line = await reader.ReadLineAsync();
            if (line == null)
            {
                return null;
            }
            
            if (string.IsNullOrEmpty(line))
            {
                break;
            }

            var parts = line.Split(':', 2);
            if (parts.Length == 2)
            {
                headers[parts[0].Trim()] = parts[1].Trim();
            }
        }
        
        return headers;
    }

    private void SendMessage(DapProtocolMessage message)
    {
        message.Seq = _seq++;
        var json = JsonSerializer.Serialize(message, message.GetType());
        var bytes = Encoding.UTF8.GetBytes(json);
        var header = $"Content-Length: {bytes.Length}\r\n\r\n";
        var headerBytes = Encoding.UTF8.GetBytes(header);

        lock (_writeLock)
        {
            _output.Write(headerBytes, 0, headerBytes.Length);
            _output.Write(bytes, 0, bytes.Length);
            _output.Flush();
        }
    }

    private void SendSuccessResponse(DapRequest req, object? body = null)
    {
        SendMessage(new DapResponse
        {
            RequestSeq = req.Seq,
            Command = req.Command,
            Success = true,
            Body = body
        });
    }

    private void SendErrorResponse(int reqSeq, string command, string message)
    {
        SendMessage(new DapResponse
        {
            RequestSeq = reqSeq,
            Command = command,
            Success = false,
            Message = message
        });
    }

    private async Task HandleRequestAsync(DapRequest req)
    {
        switch (req.Command)
        {
            case "initialize":
            {
                SendSuccessResponse(req, new
                {
                    supportsConfigurationDoneRequest = true,
                    supportsConditionalBreakpoints = true,
                    supportsHitConditionalBreakpoints = true,
                    supportsEvaluateForHovers = true,
                    supportsSetVariable = true,
                    supportsDisassembleRequest = true,
                    supportsInstructionBreakpoints = true,
                    supportsSteppingGranularity = true
                });

                SendMessage(new DapEvent("initialized"));
                break;
            }

            case "launch":
            {
                var args = req.Arguments;
                var program = (args.HasValue && args.Value.TryGetProperty("program", out var progProp)) ? progProp.GetString() : null;
                var stopOnEntry = (args.HasValue && args.Value.TryGetProperty("stopOnEntry", out var soeProp)) && soeProp.GetBoolean();

                if (string.IsNullOrEmpty(program) || !File.Exists(program))
                {
                    SendErrorResponse(req.Seq, req.Command, $"Script file '{program}' does not exist.");
                    return;
                }

                _vm = new CeresVM();
                _runtime = new EvilRuntime(_vm);
                _runtime.RegisterBuiltInModules();
                _runtime.RegisterBuiltInFunctions();

                _session = new DebuggerSession(_vm);
                _session.OnStopped += (fiber, _, reason) =>
                {
                    SendMessage(new DapEvent("stopped", new
                    {
                        reason = reason switch
                        {
                            StopReason.Breakpoint => "breakpoint",
                            StopReason.Step => "step",
                            StopReason.Pause => "pause",
                            StopReason.Exception => "exception",
                            _ => "entry"
                        },
                        threadId = fiber.GetHashCode(),
                        allThreadsStopped = true
                    }));
                };

                _session.OnResumed += fiber =>
                {
                    SendMessage(new DapEvent("continued", new
                    {
                        threadId = fiber.GetHashCode(),
                        allThreadsContinued = true
                    }));
                };

                var source = await File.ReadAllTextAsync(program);
                var compiler = new Compiler();
                _rootChunk = compiler.Compile(source, program);
                RegisterChunkTree(_rootChunk);

                SendSuccessResponse(req);

                if (stopOnEntry)
                {
                    _session.Pause();
                }

                _ = Task.Run(async () =>
                {
                    _vm.MainFiber.Schedule(_rootChunk);
                    _vm.Run();
                    await _vm.MainFiber.BlockUntilFinishedAsync();
                    _vm.Stop();

                    SendMessage(new DapEvent("terminated"));
                    SendMessage(new DapEvent("exited", new { exitCode = 0 }));
                });

                break;
            }

            case "setBreakpoints":
            {
                var args = req.Arguments;
                var sourcePath = args?.GetProperty("source").GetProperty("path").GetString()
                              ?? string.Empty;
                var breakpoints = new List<object>();

                if (_session != null)
                {
                    _session.BreakpointManager.ClearBreakpoints(sourcePath);

                    if (args?.TryGetProperty("breakpoints", out var bpsProp) == true)
                    {
                        foreach (var bpElem in bpsProp.EnumerateArray())
                        {
                            var line = bpElem.GetProperty("line").GetInt32();
                            var cond = bpElem.TryGetProperty("condition", out var cProp) 
                                ? cProp.GetString() 
                                : null;
                            
                            var hit = bpElem.TryGetProperty("hitCondition", out var hProp) 
                                ? hProp.GetString() 
                                : null;

                            var bpId = _session.AddBreakpoint(sourcePath, line, 0, cond, hit);
                            var bpObj = _session.GetBreakpoints();
                            var match = bpObj.FirstOrDefault(b => b.Id == bpId);

                            breakpoints.Add(new
                            {
                                id = bpId,
                                verified = match?.IsVerified ?? false,
                                line = match?.Line ?? line
                            });
                        }
                    }
                }

                SendSuccessResponse(req, new { breakpoints });
                break;
            }

            case "configurationDone":
            {
                SendSuccessResponse(req);
                break;
            }

            case "threads":
            {
                var threads = new List<object>();
                if (_session != null)
                {
                    var fibers = _session.GetFibers();
                    for (var i = 0; i < fibers.Count; i++)
                    {
                        threads.Add(new
                        {
                            id = fibers[i].GetHashCode(),
                            name = fibers[i] == _vm?.MainFiber ? "MainFiber" : $"Fiber #{i}"
                        });
                    }
                }
                SendSuccessResponse(req, new { threads });
                break;
            }

            case "stackTrace":
            {
                var stackFrames = new List<object>();
                _frameLookup.Clear();

                if (_session?.ActiveFiber != null)
                {
                    var callStack = _session.ActiveFiber.CallStack.ToArray(skipNativeFrames: true);
                    for (var i = 0; i < callStack.Length; i++)
                    {
                        if (callStack[i] is ScriptStackFrame ssf)
                        {
                            var fId = _nextFrameId++;
                            _frameLookup[fId] = ssf;

                            var chunkId = GetOrRegisterChunk(ssf.Chunk);
                            var ip = (int)ssf.PreviousOpCodeIP;
                            var addr = chunkId == 0 
                                ? $"0x{ip:X4}" 
                                : $"0x{((ulong)(uint)chunkId << 32) | (uint)ip:X8}";

                            var line = ssf.Chunk.DebugDatabase.GetLineForIP(ip);
                            stackFrames.Add(new
                            {
                                id = fId,
                                name = ssf.Chunk.Name ?? "<anonymous>",
                                source = new
                                {
                                    name = Path.GetFileName(ssf.Chunk.DebugDatabase.DefinedInFile),
                                    path = ssf.Chunk.DebugDatabase.DefinedInFile
                                },
                                line = line > 0 ? line : 1,
                                column = 1,
                                instructionPointerReference = addr
                            });
                        }
                    }
                }

                SendSuccessResponse(req, new { stackFrames, totalFrames = stackFrames.Count });
                break;
            }

            case "scopes":
            {
                var frameId = req.Arguments?.GetProperty("frameId").GetInt32() ?? 0;
                var scopes = new List<object>();

                if (_frameLookup.TryGetValue(frameId, out var ssf) && _session != null)
                {
                    var locals = _session.GetFrameLocals(ssf);
                    var args = _session.GetFrameArguments(ssf);

                    var localsRef = _nextVarRef++;
                    var argsRef = _nextVarRef++;
                    var globalsRef = _nextVarRef++;

                    _scopeVariablesLookup[localsRef] = locals;
                    _scopeVariablesLookup[argsRef] = args;

                    scopes.Add(new
                    {
                        name = "Locals",
                        variablesReference = localsRef,
                        expensive = false
                    });

                    scopes.Add(new
                    {
                        name = "Arguments",
                        variablesReference = argsRef,
                        expensive = false
                    });
                }

                SendSuccessResponse(req, new { scopes });
                break;
            }

            case "variables":
            {
                var varRef = req.Arguments?.GetProperty("variablesReference").GetInt32() ?? 0;
                var variables = new List<object>();

                if (_scopeVariablesLookup.TryGetValue(varRef, out var vars))
                {
                    foreach (var (k, v) in vars)
                    {
                        variables.Add(new
                        {
                            name = k,
                            value = v.ToString(),
                            type = v.Type.ToString(),
                            variablesReference = 0
                        });
                    }
                }

                SendSuccessResponse(req, new { variables });
                break;
            }

            case "evaluate":
            {
                var expr = req.Arguments?.GetProperty("expression").GetString() ?? "";
                var frameId = req.Arguments?.TryGetProperty("frameId", out var fProp) == true ? fProp.GetInt32() : 0;

                if (_session != null && _frameLookup.TryGetValue(frameId, out var ssf))
                {
                    var res = _session.EvaluateInFrame(ssf, expr);
                    SendSuccessResponse(req, new
                    {
                        result = res.ToString(),
                        type = res.Type.ToString(),
                        variablesReference = 0
                    });
                }
                else
                {
                    SendSuccessResponse(req, new
                    {
                        result = "<no frame available>",
                        variablesReference = 0
                    });
                }
                break;
            }

            case "setVariable":
            {
                var name = req.Arguments?.GetProperty("name").GetString() ?? "";
                var valStr = req.Arguments?.GetProperty("value").GetString() ?? "";
                var varRef = req.Arguments?.GetProperty("variablesReference").GetInt32() ?? 0;

                if (_session?.ActiveFiber?.CallStack.Peek() is ScriptStackFrame ssf)
                {
                    _session.SetFrameLocal(ssf, name, new Ceres.ExecutionEngine.TypeSystem.DynamicValue(valStr));
                }

                SendSuccessResponse(req, new { value = valStr });
                break;
            }

            case "continue":
            {
                _session?.Continue();
                SendSuccessResponse(req, new { allThreadsContinued = true });
                break;
            }

            case "next":
            {
                var granularity = req.Arguments?.TryGetProperty(
                    "granularity",
                    out var gProp
                ) == true ? gProp.GetString() : null;
                
                if (string.Equals(granularity, "instruction", StringComparison.OrdinalIgnoreCase))
                {
                    _session?.StepInstruction();
                }
                else
                {
                    _session?.StepOver();
                }
                SendSuccessResponse(req);
                break;
            }

            case "stepIn":
            {
                var granularity = req.Arguments?.TryGetProperty(
                    "granularity",
                    out var gProp
                ) == true ? gProp.GetString() : null;
                
                if (string.Equals(granularity, "instruction", StringComparison.OrdinalIgnoreCase))
                {
                    _session?.StepInstruction();
                }
                else
                {
                    _session?.StepInto();
                }
                SendSuccessResponse(req);
                break;
            }

            case "stepOut":
            {
                _session?.StepOut();
                SendSuccessResponse(req);
                break;
            }

            case "pause":
            {
                _session?.Pause();
                SendSuccessResponse(req);
                break;
            }

            case "disassemble":
            {
                var args = req.Arguments;
                if (!args.HasValue)
                {
                    SendErrorResponse(req.Seq, req.Command, "Arguments are required for disassemble.");
                    break;
                }

                var memoryRef = args.Value.TryGetProperty(
                    "memoryReference",
                    out var memProp
                ) ? memProp.GetString() ?? "" : "";
                
                var byteOffset = args.Value.TryGetProperty(
                    "offset",
                    out var offProp
                ) ? offProp.GetInt32() : 0;
                
                var instructionOffset = args.Value.TryGetProperty(
                    "instructionOffset", 
                    out var instOffProp
                ) ? instOffProp.GetInt32() : 0;
                
                var instructionCount = args.Value.TryGetProperty(
                    "instructionCount",
                    out var countProp
                ) ? countProp.GetInt32() : 50;
                
                var resolveSymbols = !args.Value.TryGetProperty(
                    "resolveSymbols",
                    out var resProp
                ) || resProp.GetBoolean();

                if (!TryResolveMemoryLocation(memoryRef, out var targetChunk, out var baseOffset, out var chunkId) || targetChunk == null)
                {
                    SendErrorResponse(req.Seq, req.Command, $"Unable to resolve memory reference '{memoryRef}'.");
                    break;
                }

                var effectiveAddress = Math.Max(0, baseOffset + byteOffset);
                var disasm = targetChunk.Disassemble();
                var instructions = disasm.Instructions;

                var startIdx = disasm.IndexOf(effectiveAddress);
                if (startIdx < 0)
                {
                    for (var i = instructions.Count - 1; i >= 0; i--)
                    {
                        if (instructions[i].Offset <= effectiveAddress)
                        {
                            startIdx = i;
                            break;
                        }
                    }
                    
                    if (startIdx < 0)
                    {
                        startIdx = 0;
                    }
                }

                startIdx += instructionOffset;
                var clampedStart = Math.Max(0, startIdx);
                var takeCount = Math.Max(0, Math.Min(instructionCount, instructions.Count - clampedStart));

                var result = new List<DisassembledInstructionDto>(takeCount);
                for (var i = clampedStart; i < clampedStart + takeCount; i++)
                {
                    var inst = instructions[i];
                    var addr = chunkId == 0 
                        ? $"0x{inst.Offset:X4}" 
                        : $"0x{((ulong)(uint)chunkId << 32) | (uint)inst.Offset:X8}";

                    var hexBytes = BitConverter.ToString(inst.RawBytes.ToArray()).Replace("-", " ");

                    var operandsStr = inst.Operands.Count > 0
                        ? " " + string.Join(" ", inst.Operands.Select(o => o.FormattedText))
                        : string.Empty;

                    var annotStr = (resolveSymbols && !string.IsNullOrEmpty(inst.SymbolicAnnotation))
                        ? $"  ; {inst.SymbolicAnnotation}"
                        : string.Empty;

                    var instText = $"{inst.Mnemonic}{operandsStr}{annotStr}";

                    string? symbol = null;
                    if (resolveSymbols)
                    {
                        if (inst.Offset == 0 && !string.IsNullOrEmpty(targetChunk.Name))
                        {
                            symbol = targetChunk.Name;
                        }
                        else if (!string.IsNullOrEmpty(inst.Label))
                        {
                            symbol = inst.Label;
                        }
                    }

                    SourceDto? location = null;
                    int? line = null;
                    int? column = null;
                    int? endLine = null;
                    int? endColumn = null;

                    var sourceFile = targetChunk.DebugDatabase.DefinedInFile;
                    if (inst.Statement != null)
                    {
                        line = inst.Statement.Line;
                        column = inst.Statement.Column;
                        endLine = inst.Statement.EndLine;
                        endColumn = inst.Statement.EndColumn;
                    }
                    else if (inst.Line > 0)
                    {
                        line = inst.Line;
                        column = 1;
                        endLine = inst.Line;
                        endColumn = 1;
                    }

                    if (!string.IsNullOrEmpty(sourceFile) && line > 0)
                    {
                        location = new SourceDto
                        {
                            Name = Path.GetFileName(sourceFile),
                            Path = sourceFile
                        };
                    }

                    result.Add(new DisassembledInstructionDto
                    {
                        Address = addr,
                        InstructionBytes = hexBytes,
                        Instruction = instText,
                        Symbol = symbol,
                        Location = location,
                        Line = line,
                        Column = column,
                        EndLine = endLine,
                        EndColumn = endColumn
                    });
                }

                SendSuccessResponse(req, new { instructions = result });
                break;
            }

            case "setInstructionBreakpoints":
            {
                var args = req.Arguments;
                var breakpoints = new List<object>();

                if (_session != null && args.HasValue && args.Value.TryGetProperty("breakpoints", out var bpsProp))
                {
                    foreach (var bpElem in bpsProp.EnumerateArray())
                    {
                        var instRef = bpElem.GetProperty(
                            "instructionReference"
                        ).GetString() ?? "";
                        
                        var offset = bpElem.TryGetProperty(
                            "offset", 
                            out var oProp
                        ) ? oProp.GetInt32() : 0;
                        
                        var cond = bpElem.TryGetProperty(
                            "condition",
                            out var cProp
                        ) ? cProp.GetString() : null;
                        
                        var hit = bpElem.TryGetProperty(
                            "hitCondition",
                            out var hProp
                        ) ? hProp.GetString() : null;

                        if (TryResolveMemoryLocation(
                                instRef,
                                out var targetChunk,
                                out var baseOffset,
                                out var chunkId) 
                            && targetChunk != null)
                        {
                            var targetIP = Math.Max(0, baseOffset + offset);
                            var bpId = _session.AddAddressBreakpoint(targetChunk, targetIP, cond, hit);
                            var addr = chunkId == 0 
                                ? $"0x{targetIP:X4}" 
                                : $"0x{((ulong)(uint)chunkId << 32) | (uint)targetIP:X8}";

                            breakpoints.Add(new
                            {
                                id = bpId,
                                verified = true,
                                instructionReference = addr,
                                offset = 0
                            });
                        }
                        else
                        {
                            breakpoints.Add(new
                            {
                                id = 0,
                                verified = false,
                                instructionReference = instRef,
                                message = "Unable to resolve instruction reference address"
                            });
                        }
                    }
                }

                SendSuccessResponse(req, new { breakpoints });
                break;
            }

            case "disconnect":
            {
                _session?.Detach();
                _vm?.Stop();
                _running = false;
                
                SendSuccessResponse(req);
                break;
            }

            default:
            {
                SendErrorResponse(req.Seq, req.Command, $"Command '{req.Command}' is not supported.");
                break;
            }
        }
    }
    
    private void RegisterSubchunks(Chunk parent)
    {
        for (var i = 0; i < parent.SubChunkCount; i++)
        {
            var sub = parent.SubChunks[i];
            
            if (!_idByChunk.ContainsKey(sub))
            {
                var id = _nextChunkId++;
                _chunkById[id] = sub;
                _idByChunk[sub] = id;
                
                RegisterSubchunks(sub);
            }
        }
    }

    private void RegisterChunkTree(Chunk root)
    {
        _chunkById[0] = root;
        _idByChunk[root] = 0;

        RegisterSubchunks(root);
    }

    private int GetOrRegisterChunk(Chunk chunk)
    {
        if (_idByChunk.TryGetValue(chunk, out var id))
        {
            return id;
        }

        var newId = _chunkById.IsEmpty && chunk == _rootChunk 
            ? 0 
            : _nextChunkId++;
        
        _chunkById[newId] = chunk;
        _idByChunk[chunk] = newId;

        for (var i = 0; i < chunk.SubChunkCount; i++)
        {
            GetOrRegisterChunk(chunk.SubChunks[i]);
        }

        return newId;
    }

    private bool TryResolveMemoryLocation(string memoryRef, out Chunk? targetChunk, out int targetOffset, out int chunkId)
    {
        targetChunk = null;
        targetOffset = 0;
        chunkId = 0;

        if (string.IsNullOrWhiteSpace(memoryRef))
        {
            return false;
        }

        if (memoryRef.Contains(':'))
        {
            var parts = memoryRef.Split(':', 2);
            var chunkPart = parts[0].Trim();
            var offsetPart = parts[1].Trim();

            if (int.TryParse(chunkPart, out var parsedChunkId) 
                && _chunkById.TryGetValue(parsedChunkId, out var foundChunk))
            {
                targetChunk = foundChunk;
                chunkId = parsedChunkId;
            }
            else if (_rootChunk != null)
            {
                targetChunk = FindSubChunk(_rootChunk, chunkPart);
                if (targetChunk != null)
                {
                    chunkId = GetOrRegisterChunk(targetChunk);
                }
            }

            if (offsetPart.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                _ = int.TryParse(
                    offsetPart[2..],
                    NumberStyles.HexNumber,
                    null,
                    out targetOffset
                );
            }
            else
            {
                _ = int.TryParse(offsetPart, out targetOffset);
            }

            return targetChunk != null;
        }

        ulong rawValue;
        if (memoryRef.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            if (!ulong.TryParse(memoryRef[2..], NumberStyles.HexNumber, null, out rawValue))
            {
                return false;
            }
        }
        else if (!ulong.TryParse(memoryRef, out rawValue))
        {
            if (_rootChunk != null)
            {
                targetChunk = FindSubChunk(_rootChunk, memoryRef);
                if (targetChunk != null)
                {
                    chunkId = GetOrRegisterChunk(targetChunk);
                    targetOffset = 0;
                    return true;
                }
            }
            return false;
        }

        var parsedId = (int)(rawValue >> 32);
        var parsedOffset = (int)(rawValue & 0xFFFFFFFF);

        if (parsedId > 0 && _chunkById.TryGetValue(parsedId, out var c))
        {
            targetChunk = c;
            targetOffset = parsedOffset;
            chunkId = parsedId;
            return true;
        }

        if (_session?.ActiveFiber?.CallStack.Peek() is ScriptStackFrame activeSsf)
        {
            var activeChunk = activeSsf.Chunk;
            var activeId = GetOrRegisterChunk(activeChunk);
            if (activeId > 0 && memoryRef.Length <= 10)
            {
                targetChunk = activeChunk;
                targetOffset = parsedOffset;
                chunkId = activeId;
                return true;
            }
        }

        if (_rootChunk != null)
        {
            targetChunk = _rootChunk;
            targetOffset = parsedOffset;
            chunkId = 0;
            
            return true;
        }

        return false;
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
}
