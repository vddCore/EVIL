using System;
using System.IO;
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

        private static bool _loadBinary;

        private static OptionSet _options = new()
        {
            { "b|binary", "Execute a compiled binary instead.", _ => _loadBinary = true },
            { "h|help", "Show this message.", _ => Workflow.ExitWithHelp(_options) }
        };
        
        internal static void Main(string[] args)
        {
            var extra = _options.Parse(args);

            if (extra.Count > 1)
            {
                Workflow.ExitWithMessage("too many arguments.", -1);
            }

            var filePath = extra[0];
            
            if (!File.Exists(filePath))
            {
                Workflow.ExitWithMessage($"`{filePath}' does not exist.", -2);
            }

            Executable exe;
            if (_loadBinary)
            {
                exe = LoadBinaryFile(filePath);
            }
            else
            {
                exe = CompileExecutable(filePath);
            }

            if (exe == null)
            {
                return;
            }
            
            try
            {
                // Make sure we've got a root chunk present.
                _ = exe.RootChunk;
                
                _global = new();
                _rt = new(_global);
                _evm = new EVM(_global);

                _evm.RunExecutable(exe);
            }
            catch (VirtualMachineException vme)
            {
                Workflow.ExitWithMessage(
                    $"`{filePath}': {vme.Message}\n" +
                    $"{_evm.DumpAllExecutionContexts()}"
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
                Workflow.ExitWithMessage($"`{filePath}': {e.Message}\nexecutable might be corrupted.", -3);
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
                Workflow.ExitWithMessage($"`{filePath}' ({le.Line}:{le.Column}): {le.Message}");
            }
            catch (ParserException pe)
            {
                Workflow.ExitWithMessage($"`{filePath}' ({pe.Line}:{pe.Column}): {pe.Message}");
            }
            catch (CompilerException ce)
            {
                Workflow.ExitWithMessage($"`{filePath}' ({ce.Line}:{ce.Column}): {ce.Message}");
            }

            return null;
        }
    }
}