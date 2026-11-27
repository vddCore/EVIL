namespace EVIL.FE.REPL;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using EVIL.Core.Diagnostics.Formatters;
using EVIL.Core.Grammar;
using EVIL.Core.Grammar.AST.Miscellaneous;
using EVIL.Core.Lexical;
using EVIL.FE.Utilities;
using EVIL.VM.ExecutionEngine;
using EVIL.VM.ExecutionEngine.Concurrency;
using EVIL.VM.ExecutionEngine.Diagnostics;
using EVIL.VM.ExecutionEngine.TypeSystem;
using EVIL.VM.Runtime;
using EVIL.VM.TranslationEngine;
using EVIL.VM.TranslationEngine.Repl;
using EvilArray = EVIL.VM.ExecutionEngine.Collections.Array;

public sealed class ReplSession
{
    private const string ReplFileName = "<repl>";
    private const string PrimaryPrompt = ">>> ";
    private const string ContinuationPrompt = "... ";

    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly LineEditor _lineEditor = new();
    private readonly Func<ConsoleKeyInfo>? _readKey;
    private readonly VirtualMachine _vm;
    private readonly EvilRuntime _runtime;
    private readonly RuntimeModuleLoader _runtimeModuleLoader;
    private readonly Parser _parser;
    private readonly Compiler _compiler;
    private readonly ReplCompilerSession _compilerSession;

    private Exception? _runtimeException;

    public ReplSession(
        TextReader input,
        TextWriter output,
        CompilerOptions? compilerOptions = null,
        Func<ConsoleKeyInfo>? readKey = null
    )
    {
        _input = input;
        _output = output;
        _readKey = readKey ?? (
            ReferenceEquals(input, Console.In)
            && ReferenceEquals(output, Console.Out)
            && !Console.IsInputRedirected
            && !Console.IsOutputRedirected
                ? () => Console.ReadKey(intercept: true)
                : null
        );
        _vm = new VirtualMachine(HandleRuntimeException);
        _runtime = new EvilRuntime(_vm);
        _runtimeModuleLoader = new RuntimeModuleLoader(_runtime);
        _parser = new Parser(filePath: ReplFileName);
        _compiler = new Compiler(compilerOptions ?? CompilerOptions.Default);
        _compilerSession = _compiler.CreateReplSession();
    }

    public async Task RunAsync()
    {
        try
        {
            InitializeRuntime();
            _vm.Run();
            await ReadSubmissionsAsync();
        }
        finally
        {
            _vm.Stop();
        }
    }

    private void InitializeRuntime()
    {
        _runtime.RegisterBuiltInModules();

        var moduleStorePath = Path.Combine(AppContext.BaseDirectory, "modules");
        if (Directory.Exists(moduleStorePath))
        {
            try
            {
                _runtimeModuleLoader.RegisterUserRuntimeModules(moduleStorePath);
            }
            catch (RuntimeModuleLoadException exception)
            {
                _output.WriteLine($"Failed to load user runtime module '{exception.FilePath}'.");
                _output.WriteLine(exception);
            }
        }

        SetStandardGlobals();
        _runtime.RegisterBuiltInFunctions();
    }

    private void SetStandardGlobals()
    {
        _vm.Global["_G"] = _vm.Global;
        _vm.Global["__SCRIPT_HOME"] = Environment.CurrentDirectory.Replace('\\', '/');
        _vm.Global["__SCRIPT_FILE"] = ReplFileName;

        var importConfigPath = Path.Combine(AppContext.BaseDirectory, "config", "default.imports");
        if (!File.Exists(importConfigPath))
        {
            return;
        }

        var evilHomeDir = AppContext.BaseDirectory
            .Replace('\\', '/')
            .TrimEnd('/');
        
        var appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
            .Replace('\\', '/')
            .TrimEnd('/');
        
        var lines = File.ReadAllLines(importConfigPath);
        var importPaths = new EvilArray(lines.Length);

        for (var i = 0; i < lines.Length; i++)
        {
            importPaths[i] = lines[i]
                .Replace("$EVILHOME", evilHomeDir)
                .Replace("$APPDATA", appDataDir);
        }

        _vm.Global["__IMPORT_PATHS"] = importPaths;
    }

    private async Task ReadSubmissionsAsync()
    {
        var source = new StringBuilder();
        var continuation = false;

        while (true)
        {
            _output.Write(
                continuation 
                    ? ContinuationPrompt 
                    : PrimaryPrompt
            );
            
            await _output.FlushAsync();

            var line = await _lineEditor.ReadLineAsync(_input, _output, _readKey);
            if (line == null)
            {
                return;
            }

            if (source.Length == 0 && string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            source.AppendLine(line);

            ProgramNode program;
            try
            {
                program = _parser.Parse(source.ToString());
            }
            catch (ParserException exception) when (exception.IsIncomplete)
            {
                continuation = true;
                continue;
            }
            catch (ParserException exception)
            {
                _output.WriteLine($"Syntax error: {exception.Message}");
                source.Clear();
                continuation = false;
                continue;
            }
            catch (LexerException exception)
            {
                _output.WriteLine($"Syntax error: {exception.Message}");
                source.Clear();
                continuation = false;
                continue;
            }

            try
            {
                var chunk = _compiler.CompileForRepl(program, _compilerSession, ReplFileName);
                await ExecuteSubmissionAsync(chunk);
            }
            catch (CompilerException exception)
            {
                ReportCompilationError(exception);
            }

            source.Clear();
            continuation = false;
        }
    }

    private async Task ExecuteSubmissionAsync(Chunk chunk)
    {
        _runtimeException = null;
        _vm.MainFiber.Schedule(chunk);
        await _vm.MainFiber.BlockUntilFinishedAsync();

        if (_vm.MainFiber.State == FiberState.Crashed)
        {
            _output.WriteLine($"Runtime error: {_runtimeException?.Message ?? "Execution failed."}");
        }
        else if (_compiler.HasReplResult)
        {
            var value = _vm.MainFiber.PopValue();
            _output.WriteLine(value.ConvertToString().String);
        }

        _vm.MainFiber.Reset();
    }

    private void ReportCompilationError(CompilerException exception)
    {
        var diagnostics = exception.Log.Diagnostics;
        if (diagnostics.Count == 0)
        {
            _output.WriteLine($"Compilation error: {exception.Message}");
            return;
        }

        var formatter = DiagnosticFormatter.GetByName("standard");
        _output.WriteLine(formatter.FormatAll(diagnostics));
    }

    private void HandleRuntimeException(Fiber _, Exception exception)
    {
        _runtimeException = exception;
    }
}