using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ceres.ExecutionEngine;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime;
using Ceres.TranslationEngine;
using EVIL.Grammar;
using EVIL.Lexical;
using Mono.Options;

using Array = System.Array;
using EvilArray = Ceres.ExecutionEngine.Collections.Array;

namespace EVIL.evil
{
    public partial class EvmFrontEnd
    {
        private static CeresVM _vm = new();
        private static Compiler _compiler = new();
        private static IncludeHandler _includeHandler = new(_compiler);
        private static EvilRuntime _runtime = new(_vm);

        private static OptionSet _options = new()
        {
            { "h|help", "display this message and quit.", (h) => _displayHelpAndQuit = h != null },
            { "v|version", "display compiler and VM version information.", (v) => _displayVersionAndQuit = v != null },
            { "I|include-dir=", "add include directory to the list of search paths.", (I) => _includeHandler.AddIncludeSearchPath(I) },
        };

        private static bool _displayHelpAndQuit;
        private static bool _displayVersionAndQuit;

        public async Task Run(string[] args)
        {
            var extra = InitializeOptions(args);

            if (_displayHelpAndQuit)
            {
                Terminate(writeHelp: true);
            }

            if (_displayVersionAndQuit)
            {
                Terminate(BuildVersionBanner());
            }

            if (!extra.Any())
            {
                Terminate(
                    "Fatal: no input file.",
                    exitCode: ExitCode.NoInputFiles
                );
            }

            if (extra.Count > 1)
            {
                Terminate(
                    "Fatal: too many input files.",
                    exitCode: ExitCode.TooManyInputFiles
                );
            }

            var scriptPath = extra.First();
            if (!File.Exists(scriptPath))
            {
                Terminate(
                    "Fatal: input file does not exist.",
                    exitCode: ExitCode.InputFileDoesNotExist
                );
            }
            
            var scriptArgs = extra
                .Skip(1)
                .Select(x => new DynamicValue(x))
                .ToArray();
            
            var source = File.ReadAllText(scriptPath);

            Script script = null!;
            try
            {
                script = _compiler.Compile(source, scriptPath);
            }
            catch (LexerException le)
            {
                Terminate(
                    $"Syntax error in {scriptPath} ({le.Line}:{le.Column}): {le.Message}",
                    exitCode: ExitCode.LexerError
                );
            }
            catch (ParserException pe)
            {
                Terminate(
                    $"Syntax error in {scriptPath} ({pe.Line}:{pe.Column}): {pe.Message}",
                    exitCode: ExitCode.ParserError
                );
            }
            catch (CompilerException ce)
            {
                var msg = $"Compilation error in {scriptPath}:\n" +
                          $"  {ce.Log.Messages.Last()}";

                if (ce.InnerException != null)
                {
                    msg += "\n\nIn addition, another error occurred:\n";
                    msg += $"  {ce.InnerException.Message}";
                }
                
                Terminate(msg, exitCode: ExitCode.CompilerError);
            }

            if (!script.TryFindChunkByName("main", out var mainChunk))
            {
                Terminate(
                    "Fatal: No entry point found. The script being run must define a function named 'main'.",
                    exitCode: ExitCode.MissingEntryPoint
                );
            }

            if (_compiler.Log.HasAnyMessages)
            {
                Console.WriteLine(_compiler.Log.ToString());
            }

            _runtime.RegisterBuiltInModules();

            var initChunks = new List<Chunk>();
            foreach (var chunk in script.Chunks)
            {
                if (IsValidInitChunk(chunk))
                {
                    initChunks.Add(chunk);
                }

                if (SkipChunkRegistration(chunk))
                {
                    continue;
                }
                
                _vm.Global[chunk.Name] = chunk;
            }

            SetStandardGlobals(scriptPath);
            
            _vm.Scheduler.SetDefaultCrashHandler(CrashHandler);

            foreach (var initChunk in initChunks)
            {
                _vm.MainFiber.Schedule(initChunk, false);
            }
            
            _vm.MainFiber.Schedule(mainChunk, false, scriptArgs);
            _vm.MainFiber.Resume();
            _vm.Start();
            
            await _vm.MainFiber.BlockUntilFinishedAsync();
        }

        private List<string> InitializeOptions(string[] args)
        {
            List<string> fileNames = new();

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
                Terminate(
                    $"Fatal: {oe.Message}\nrun with `--help' to see usage instructions",
                    exitCode: ExitCode.GenericError
                );
            }

            return fileNames;
        }

        private bool IsValidInitChunk(Chunk chunk)
            => chunk.HasAttribute(AttributeNames.VmInit);

        private bool SkipChunkRegistration(Chunk chunk)
            => chunk.HasAttribute(AttributeNames.NoReg);

        private void SetStandardGlobals(string scriptPath)
        {
            SetIncludePathsGlobal();
            
            _vm.Global["__SCRIPT_HOME"] = Path.GetDirectoryName(scriptPath) ?? string.Empty;
            _vm.Global["__SCRIPT_FILE"] = Path.GetFileName(scriptPath);
            _vm.Global["__GLOBAL"] = _vm.Global;
        }
        
        private void SetIncludePathsGlobal()
        {
            var includeArray = new EvilArray(_includeHandler.IncludeSearchPaths.Count);
            for (var i = 0; i < includeArray.Length; i++)
            {
                includeArray[i] = _includeHandler.IncludeSearchPaths[i];
            }
            _vm.Global["__INCLUDE_PATHS"] = includeArray;
        }

        private void CrashHandler(Fiber fiber, Exception exception)
        {
            var callStack = fiber.CallStack;
            var top = callStack.Peek().As<ScriptStackFrame>();
            var fiberArray = _vm.Scheduler.Fibers.ToArray();
            var fiberIndex = Array.IndexOf(fiberArray, fiber);

            var sb = new StringBuilder();
            sb.AppendLine($"Runtime error in fiber {fiberIndex}, function {top.Chunk.Name} (def. in {top.Chunk.DebugDatabase.DefinedInFile}:{top.Chunk.DebugDatabase.DefinedOnLine}): {exception.Message}");
            sb.AppendLine();
            sb.AppendLine("Stack trace:");
            sb.Append(fiber.StackTrace(false));
            
            Terminate(sb.ToString(), exitCode: ExitCode.RuntimeError);
        }
    }
}