namespace EVIL.FE;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EVIL.Core.Common.TypeSystem;
using EVIL.Core.Diagnostics.Formatters;
using EVIL.Core.Grammar;
using EVIL.FE.Debugger;
using EVIL.FE.REPL;
using EVIL.FE.Utilities;
using EVIL.VM.Debugger;
using EVIL.VM.Debugger.Dap;
using EVIL.VM.ExecutionEngine;
using EVIL.VM.ExecutionEngine.Concurrency;
using EVIL.VM.ExecutionEngine.Diagnostics;
using EVIL.VM.ExecutionEngine.Diagnostics.Disassembly;
using EVIL.VM.ExecutionEngine.Diagnostics.Disassembly.Rendering;
using EVIL.VM.ExecutionEngine.TypeSystem;
using EVIL.VM.Runtime;
using EVIL.VM.TranslationEngine;
using Mono.Options;
using EvilArray = EVIL.VM.ExecutionEngine.Collections.Array;

public partial class EvmFrontEnd
{
    private static readonly VirtualMachine _vm = new(CrashHandler);
    private static readonly EvilRuntime _runtime = new(_vm);
    private static readonly RuntimeModuleLoader _runtimeModuleLoader = new(_runtime);

    private static readonly CompilerOptions _compilerOptions = new();
    private static Compiler? _compiler;
    
    private static bool _displayHelpAndQuit;
    private static bool _displayVersionAndQuit;
    private static bool _generateModuleDocsAndQuit;
    private static bool _disassembleCompiledScript;
    private static string? _disassembleTarget;
    private static bool _disasmRawBytes;
    private static bool _disasmNoSubchunks;
    private static bool _disasmNoSource;
    private static bool _optimizeCode;
    private static bool _compileOnly;
    private static bool _replMode;
    private static bool _debugMode;
    private static bool _dapMode;
    private static string _outputFileName = "a.evx";
    private static string _diagnosticsFormat = "standard";
    private static bool _warnAsError;
    private static int _maxErrors = 64;
    private static readonly List<string> _noWarnCodes = [];
    private static readonly List<string> _warnAsErrorCodes = [];
    private static readonly ManualResetEventSlim _crashHandled = new(false);

    private static readonly OptionSet _options = new()
    {
        { "h|help", "display this message and quit.", (h) => _displayHelpAndQuit = h != null },
        { "v|version", "display compiler and VM version information.", (v) => _displayVersionAndQuit = v != null },
        { "g|gen-docs", "generate documentation for all detected native modules.", (g) => _generateModuleDocsAndQuit = g != null },
        { "d|disasm:", "disassemble compiled script bytecode (optionally specify function name, e.g. -d=my_func).", (d) =>
        {
            _disassembleCompiledScript = true;
            _disassembleTarget = string.IsNullOrWhiteSpace(d) ? null : d.Trim();
        }},
        { "raw-bytes", "include raw opcode hex bytes in disassembly.", (b) => _disasmRawBytes = b != null },
        { "no-subchunks", "do not recurse into nested subchunks during disassembly.", (s) => _disasmNoSubchunks = s != null },
        { "no-source", "disable source code interleaving in disassembly.", (s) => _disasmNoSource = s != null },
        { "o|optimize", "optimize generated code.", (o) => _optimizeCode = o != null },
        { "repl", "start an interactive EVIL REPL.", (r) => _replMode = r != null },
        { "debug", "start script in interactive CLI debugger (evdb).", (b) => _debugMode = b != null },
        { "dap", "start Debug Adapter Protocol (DAP) server over stdio.", (p) => _dapMode = p != null },
        { "f|diagnostics-format=", "diagnostics formatting style: standard, rich, or json (default: standard).", f => _diagnosticsFormat = f },
        { "w|warn-as-error:", "treat warnings as errors (optionally comma-separated error codes, e.g. EV8003).", w =>
        {
            if (string.IsNullOrWhiteSpace(w))
            {
                _warnAsError = true;
            }
            else
            {
                foreach (var code in w.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (!string.IsNullOrWhiteSpace(code))
                    {
                        _warnAsErrorCodes.Add(code);
                    }
                }
            }
        }},
        { "nowarn=", "suppress specific warning codes (comma-separated, e.g. EV8003,EV8002).", n =>
        {
            if (!string.IsNullOrWhiteSpace(n))
            {
                foreach (var code in n.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    if (!string.IsNullOrWhiteSpace(code))
                    {
                        _noWarnCodes.Add(code);
                    }
                }
            }
        }},
        { "max-errors=", "maximum number of errors before stopping compilation (default: 20).", (int m) => _maxErrors = m },
        { "c|compile-only=", "don't run, compile only requires a file name", (o) =>
        {
            _compileOnly = !string.IsNullOrEmpty(o);
            _outputFileName = o;
        }}
    };
        
    public async Task Run(string[] args)
    {
        _replMode = false;
        var extra = InitializeOptions(args);

        if (_displayHelpAndQuit)
        {
            Diagnostics.Terminate(null, () =>
            {
                _options.WriteOptionDescriptions(
                    Console.Out
                );
            });
        }

        if (_displayVersionAndQuit)
        {
            Diagnostics.Terminate(
                VersionTools.BuildVersionBanner()
            );
        }

        if (_generateModuleDocsAndQuit)
        {
            GenerateModuleDocs();
            Diagnostics.Terminate();
        }

        if (_optimizeCode)
        {
            _compilerOptions.OptimizeCodeGeneration = true;
        }

        if (_dapMode)
        {
            var server = new DapServer(Console.OpenStandardInput(), Console.OpenStandardOutput());
            await server.RunAsync();
            return;
        }

        if (_replMode || extra.Count == 0)
        {
            var replSession = new ReplSession(
                Console.In,
                Console.Out,
                ConfigureCompilerOptions()
            );

            Console.Out.WriteLine(
                VersionTools.BuildVersionBanner()
            );
            
            await replSession.RunAsync();
            return;
        }

        switch (extra.Count)
        {
            case 0:
            {
                Diagnostics.Terminate(
                    "Fatal: no input file.",
                    exitCode: ExitCode.NoInputFiles
                );
                break;
            }

            case > 1:
            {
                Diagnostics.Terminate(
                    "Fatal: too many input files.",
                    exitCode: ExitCode.TooManyInputFiles
                );
                break;
            }
        }

        var scriptPath = extra.First();
        if (!File.Exists(scriptPath))
        {
            Diagnostics.Terminate(
                "Fatal: input file does not exist.",
                exitCode: ExitCode.InputFileDoesNotExist
            );
        }
            
        var scriptArgs = extra
            .Skip(1)
            .Select(x => new DynamicValue(x))
            .ToArray();
            
        var source = await File.ReadAllTextAsync(scriptPath);

        _compilerOptions.MaxErrors = _maxErrors;
        _compilerOptions.WarningsAsErrors = _warnAsError;
        
        foreach (var code in _warnAsErrorCodes)
        {
            _compilerOptions.WarningsAsErrorsCodes.Add(code);
        }
        
        foreach (var code in _noWarnCodes)
        {
            _compilerOptions.SuppressedCodes.Add(code);
        }

        _compiler = new Compiler(_compilerOptions);
        Chunk rootChunk = null!;
        try
        {
            rootChunk = _compiler.Compile(source, scriptPath);
        }
        catch (CompilerException ce)
        {
            var formatter = DiagnosticFormatter.GetByName(_diagnosticsFormat);
            if (formatter is RichDiagnosticFormatter rich)
            {
                rich.SourceProvider = path => path == scriptPath ? source : (File.Exists(path) ? File.ReadAllText(path) : null);
            }

            var diagnostics = ce.Log.Diagnostics;
            var msg = diagnostics.Count > 0
                ? formatter.FormatAll(diagnostics)
                : $"Compilation error in {scriptPath}:\n  {ce.Message}";

            if (ce.InnerException is not null 
                and not ParserException 
                and not DuplicateSymbolException)
            {
                msg += $"\n\n {ce.InnerException.Message}";
            }
                
            Diagnostics.Terminate(msg, exitCode: ExitCode.CompilerError);
        }

        if (_disassembleCompiledScript)
        {
            var options = new DisassemblyFormatOptions
            {
                ShowAddresses = true,
                AddressFormat = AddressFormat.HexPrefixed,
                ShowRawBytes = _disasmRawBytes,
                ShowSourceInterleaving = !_disasmNoSource,
                ShowSymbols = true,
                ShowBranchLabels = true,
                ShowBranchOffsets = true,
                ShowChunkHeader = true,
                IncludeSubChunks = !_disasmNoSubchunks,
                SourceProvider = path =>
                {
                    if (string.IsNullOrEmpty(path) || path == scriptPath)
                    {
                        return File.Exists(scriptPath) ? File.ReadAllLines(scriptPath) : source.Split('\n');
                    }
                    return File.Exists(path) ? File.ReadAllLines(path) : null;
                },
                ColorMode = ColorMode.Auto
            };

            if (!string.IsNullOrEmpty(_disassembleTarget))
            {
                var targetChunk = FindChunk(rootChunk, _disassembleTarget);
                if (targetChunk == null)
                {
                    Diagnostics.Terminate($"Fatal: function or subchunk '{_disassembleTarget}' not found in '{scriptPath}'.", exitCode: ExitCode.GenericError);
                    return;
                }

                Console.WriteLine($"Disassembly of '{targetChunk.Name}' in '{scriptPath}'\n-------------------");
                var targetDisasm = targetChunk.Disassemble();
                DisassemblyFormatter.FormatChunk(targetDisasm, Console.Out, options);
            }
            else
            {
                Console.WriteLine($"Disassembly of '{scriptPath}'\n-------------------");
                var disasm = rootChunk.Disassemble();
                DisassemblyFormatter.FormatChunk(disasm, Console.Out, options);
            }
        }

        if (_compiler.Log.HasAnyMessages)
        {
            var formatter = DiagnosticFormatter.GetByName(_diagnosticsFormat);
            if (formatter is RichDiagnosticFormatter rich)
            {
                rich.SourceProvider = path => path == scriptPath ? source : (File.Exists(path) ? File.ReadAllText(path) : null);
            }

            var diagnostics = _compiler.Log.Diagnostics;
            if (diagnostics.Count > 0)
            {
                Console.WriteLine(formatter.FormatAll(diagnostics));
            }
        }

        if (_compileOnly)
        {
            try
            {
                await using var fs = new FileStream(
                    Path.Combine(
                        Environment.CurrentDirectory,
                        _outputFileName
                    ),
                    FileMode.Create
                );

                rootChunk.Serialize(fs);
            }
            catch (Exception e)
            {
                Diagnostics.Terminate($"Fatal: unable to write executable to disk - {e.Message}");
            }

            return;
        }

        RegisterAllModules();
        SetStandardGlobals(scriptPath);

        try
        {
            _runtime.RegisterBuiltInFunctions();
        }
        catch (EvilRuntimeException e)
        {
            var msg = $"Internal error: {e.Message}";
            if (e.InnerException is CompilerException ce)
            {
                msg += "See below for details:\n\n";
                msg += $"{ce.Message}\n{ce.Log}";
            }

            Diagnostics.Terminate(msg);
        }

        if (_debugMode)
        {
            var debugger = new DebuggerSession(_vm);
            _ = new EvdbSession(_vm, debugger, scriptPath);
            debugger.Pause();

            _vm.MainFiber.Schedule(rootChunk, scriptArgs);
            _vm.Run();
            await _vm.MainFiber.BlockUntilFinishedAsync();
            _vm.Stop();
            return;
        }

        _vm.MainFiber.Schedule(rootChunk, scriptArgs);
        _vm.Run();
        await _vm.MainFiber.BlockUntilFinishedAsync();

        if (_vm.MainFiber.State == FiberState.Crashed)
        {
            _crashHandled.Wait(TimeSpan.FromSeconds(5));
            _vm.Stop();
            Environment.Exit((int)ExitCode.RuntimeError);
        }
        
        _vm.Stop();
    }

    private static List<string> InitializeOptions(string[] args)
    {
        List<string> fileNames = [];

        try
        {
            var extra = _options.Parse(args);

            if (extra != null)
            {
                fileNames.AddRange(extra);
            }
        }
        catch (OptionException oe)
        {
            Diagnostics.Terminate(
                $"Fatal: {oe.Message}\nrun with `--help' to see usage instructions",
                exitCode: ExitCode.GenericError
            );
        }

        return fileNames;
    }

    private static CompilerOptions ConfigureCompilerOptions()
    {
        _compilerOptions.MaxErrors = _maxErrors;
        _compilerOptions.WarningsAsErrors = _warnAsError;

        foreach (var code in _warnAsErrorCodes)
        {
            _compilerOptions.WarningsAsErrorsCodes.Add(code);
        }

        foreach (var code in _noWarnCodes)
        {
            _compilerOptions.SuppressedCodes.Add(code);
        }

        return _compilerOptions;
    }

    private void GenerateModuleDocs()
    {
        var modules = RegisterAllModules();
        var docsPath = Path.Combine(
            AppContext.BaseDirectory,
            "docs"
        );
            
        Directory.CreateDirectory(docsPath);
            
        foreach (var module in modules)
        {
            var filePath = Path.Combine(docsPath, $"evrt_{module.FullyQualifiedName}.doc.md");
            Console.WriteLine($"Generating '{filePath}'...");
                
            using (var sw = new StreamWriter(filePath))
            {
                sw.WriteLine(module.Describe());
            }
        }
    }

    private static List<RuntimeModule> RegisterAllModules()
    {
        var ret = new List<RuntimeModule>();
            
        ret.AddRange(_runtime.RegisterBuiltInModules());

        var moduleStorePath = Path.Combine(AppContext.BaseDirectory, "modules");
        if (Directory.Exists(moduleStorePath))
        {
            try
            {
                ret.AddRange(_runtimeModuleLoader.RegisterUserRuntimeModules(moduleStorePath));
            }
            catch (RuntimeModuleLoadException rmle)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Failed to load user runtime module '{rmle.FilePath}'.");
                sb.AppendLine(rmle.ToString());
                    
                Diagnostics.Terminate(sb.ToString(), exitCode: ExitCode.ModuleLoaderFailed);
            }
        }

        return ret;
    }

    private static void SetStandardGlobals(string scriptPath)
    {
        _vm.Global["_G"] = _vm.Global;
            
        _vm.Global["__SCRIPT_HOME"] = (
            Path.GetDirectoryName(scriptPath) ?? string.Empty
        ).Replace('\\', '/');
            
        _vm.Global["__SCRIPT_FILE"] = Path.GetFileName(scriptPath);

        var importConfigPath = Path.Combine(
            AppContext.BaseDirectory,
            "config", 
            "default.imports"
        );

        var evilHomeDir = AppContext.BaseDirectory
            .Replace('\\', '/')
            .TrimEnd('/');

        var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            .Replace('\\', '/')
            .TrimEnd('/');
            
        if (File.Exists(importConfigPath))
        {
            var lines = File.ReadAllLines(importConfigPath);
            var array = new EvilArray(lines.Length);

            for (var i = 0; i < lines.Length; i++)
            {
                array[i] = lines[i]
                    .Replace("$EVILHOME", evilHomeDir)
                    .Replace("$APPDATA", appDataDir);
            }

            _vm.Global["__IMPORT_PATHS"] = array;
        }
    }
        
    private static void CrashHandler(Fiber fiber, Exception exception)
    {
        try
        {
            var fiberArray = _vm.Scheduler.Fibers.ToArray();
            var fiberIndex = Array.IndexOf(fiberArray, fiber);

            StackFrame[] callStack;

            if (exception is UserUnhandledExceptionException uuee)
            {
                callStack = uuee.EvilStackTrace;
            }
            else
            {
                callStack = fiber.CallStack.ToArray();
            }

            TerminateWithError(fiberIndex, callStack, exception);
        }
        finally
        {
            _crashHandled.Set();
        }
    }

    private static void TerminateWithError(int fiberIndex, StackFrame[] callStack, Exception exception)
    {
        var sb = new StringBuilder();

        if (callStack.Length == 0)
        {
            sb.AppendLine($"Runtime error in fiber {fiberIndex}: {exception.Message}");
            
            Diagnostics.Terminate(
                sb.ToString(),
                exitCode: ExitCode.RuntimeError
            );
            
            return;
        }

        ScriptStackFrame? scriptTop = null;
        for (var i = 0; i < callStack.Length; i++)
        {
            if (callStack[i] is ScriptStackFrame ssf)
            {
                scriptTop = ssf;
                break;
            }
        }

        if (scriptTop != null)
        {
            var dd = scriptTop.Chunk.DebugDatabase;
            sb.Append($"Runtime error in fiber {fiberIndex}, function {scriptTop.Chunk.Name}");
                
            if (!string.IsNullOrEmpty(scriptTop.Chunk.DebugDatabase.DefinedInFile))
            {
                sb.Append($" (def. in {scriptTop.Chunk.DebugDatabase.DefinedInFile}");
                    
                if (scriptTop.Chunk.DebugDatabase.DefinedOnLine > 0)
                {
                    sb.Append($", on line {scriptTop.Chunk.DebugDatabase.DefinedOnLine}");
                }

                sb.Append(')');
            }
            sb.AppendLine(": ");
                
            sb.Append($"{dd.DefinedInFile}:{dd.GetLineForIP((int)scriptTop.PreviousOpCodeIP)}: ");
            sb.Append(exception.Message);
        }
        else
        {
            sb.AppendLine($"Runtime error in fiber {fiberIndex}: {exception.Message}");
        }

        if (exception is UserUnhandledExceptionException uuee)
        {
            if (uuee.EvilExceptionObject.Type == DynamicValueType.Error)
            {
                var msg = uuee.EvilExceptionObject.Error!["msg"];
                if (msg.Type == DynamicValueType.String)
                {
                    sb.Append($" ({msg.String})");
                }
            }
            else if (uuee.EvilExceptionObject.Type != DynamicValueType.Nil)
            {
                sb.Append($" ({uuee.EvilExceptionObject.ConvertToString().String!})");
            }
        }
            
        sb.AppendLine();
        sb.AppendLine("Stack trace:");
        sb.Append(Fiber.StackTrace(callStack));
            
        Diagnostics.Terminate(
            sb.ToString(),
            exitCode: ExitCode.RuntimeError
        );
    }

    private static Chunk? FindChunk(Chunk chunk, string name)
    {
        if (string.Equals(chunk.Name, name, StringComparison.OrdinalIgnoreCase))
        {
            return chunk;
        }

        for (var i = 0; i < chunk.SubChunkCount; i++)
        {
            var sub = chunk.SubChunks[i];
            var found = FindChunk(sub, name);
            
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}