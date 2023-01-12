using System;
using System.IO;
using System.Linq;
using EVIL.CVIL.Utilities;
using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.Grammar;
using EVIL.Grammar.Parsing;
using EVIL.Intermediate.CodeGeneration;
using EVIL.Intermediate.Storage;
using EVIL.Lexical;
using EVIL.RT;
using Mono.Options;

namespace EVIL.EVIL
{
    internal static class Program
    {
        private static Table _global;
        private static EvilRuntime _rt;
        private static EVM _evm;

        private static string _filePath;
        private static bool _loadBinary;

        private static OptionSet _options = new()
        {
            { "b|binary", "Execute a compiled binary instead.", _ => _loadBinary = true },
            { "h|help", "Show this message.", _ => Workflow.ExitWithHelp(_options) }
        };
        
        internal static void Main(string[] args)
        {
            var extra = _options.Parse(args);

            if (extra.Count <= 0)
            {
                Workflow.ExitWithMessage("too few arguments.", -1);
            }

            _filePath = extra[0];
            
            if (!File.Exists(_filePath))
            {
                Workflow.ExitWithMessage($"`{_filePath}' does not exist.", -2);
            }

            Executable exe;
            if (_loadBinary)
            {
                exe = LoadBinaryFile(_filePath);
            }
            else
            {
                exe = CompileExecutable(_filePath);
            }

            if (exe == null)
            {
                // Technically it shouldn't ever reach this branch,
                // but just in case...
                return;
            }
            
            try
            {
                // Make sure we've got a root chunk present.
                _ = exe.RootChunk;
                
                _global = new();
                _rt = new(_global);
                SetUpEnvironment();
                
                _evm = new EVM(_global);
                _evm.RunExecutable(
                    exe,
                    extra.Skip(1)
                         .Select(x => new DynamicValue(x))
                         .ToArray()
                );
            }
            catch (VirtualMachineException vme)
            {
                Workflow.ExitWithMessage(
                    $"`{_filePath}': {vme.Message}\n" +
                    $"{_evm.DumpAllExecutionContexts()}",
                    -3
                );
            }
        }

        private static Executable LoadBinaryFile(string filePath)
        {
            try
            {
                return EvxLoader.Load(filePath);
            }
            catch (Exception e)
            {
                Workflow.ExitWithMessage($"`{filePath}': {e.Message}\nexecutable might be corrupted.", -4);
            }

            return null;
        }

        private static Executable CompileExecutable(string filePath)
        {
            var lexer = new Lexer();
            var parser = new Parser(lexer);
            var compiler = new Compiler();

            try
            {
                lexer.LoadSource(File.ReadAllText(filePath));
                var program = parser.Parse(true);
                return compiler.Compile(program);
            }
            catch (LexerException le)
            {
                Workflow.ExitWithMessage($"`{filePath}' ({le.Line}:{le.Column}): {le.Message}", -5);
            }
            catch (ParserException pe)
            {
                Workflow.ExitWithMessage($"`{filePath}' ({pe.Line}:{pe.Column}): {pe.Message}", -6);
            }
            catch (CompilerException ce)
            {
                Workflow.ExitWithMessage($"`{filePath}' ({ce.Line}:{ce.Column}): {ce.Message}", -7);
            }

            return null;
        }

        private static void SetUpEnvironment()
        {
            _evm.SetEnvironmentVariable(
                EvilEnvironmentVariable.ScriptHomeDirectory, 
                Path.GetDirectoryName(_filePath)
            );
        }
    }
}