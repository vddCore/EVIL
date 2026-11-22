namespace EVIL.Debugger.Dap;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EVIL.Ceres.ExecutionEngine;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.Diagnostics.Debugging;
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
                    supportsSetVariable = true
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

                            var line = ssf.Chunk.DebugDatabase.GetLineForIP((int)ssf.PreviousOpCodeIP);
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
                                column = 1
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
                _session?.StepOver();
                SendSuccessResponse(req);
                break;
            }

            case "stepIn":
            {
                _session?.StepInto();
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
}
