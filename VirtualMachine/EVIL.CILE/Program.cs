using System;
using System.IO;
using System.Linq;
using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.Grammar;
using EVIL.Grammar.Parsing;
using EVIL.Intermediate.Analysis;
using EVIL.Intermediate.CodeGeneration;
using EVIL.Lexical;
using EVIL.RT;

namespace EVIL.CILE
{
    internal static class Program
    {
        private static Lexer _lexer = new();
        private static Parser _parser = new(_lexer, true);
        private static Compiler _compiler = new();

        private static Table _globals = new();
        private static EVM _evm = new(_globals);

        internal static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                TerminateWithError("Executable file path missing.");
            }

            if (!File.Exists(args[0]))
            {
                TerminateWithError($"Path '{args[0]}' does not exist.");
            }

            var text = File.ReadAllText(args[0]);
            var programArguments = args.Skip(1)
                .Select(x => new DynamicValue(x))
                .ToArray();

            Executable exe = null;

            try
            {
                _lexer.LoadSource(text);
                var programTree = _parser.Parse();
                exe = _compiler.Compile(programTree);
            }
            catch (LexerException le)
            {
                TerminateWithError($"Syntax error at ({le.Line}:{le.Column}): {le.Message}");
            }
            catch (ParserException pe)
            {
                TerminateWithError($"Syntax error at ({pe.Line}:{pe.Column}): {pe.Message}");
            }
            catch (CompilerException ce)
            {
                TerminateWithError($"Compiler error at ({ce.Line}:{ce.Column}): {ce.Message}");
            }

            var rt = new EvilRuntime(_globals);
            rt.LoadCoreRuntime();
            
            try
            {
                var disasm = new Disassembler();
                Console.WriteLine(disasm.Disassemble(exe));

                _evm.Load(exe);
            }
            catch (Exception e)
            {
                TerminateWithError($"Failed to load the produced executable: {e.Message}");
            }

            try
            {
                _evm.Run(programArguments);
            }
            catch (Exception e)
            {
                TerminateWithDump(e);
            }
        }

        private static void TerminateWithDump(Exception e)
        {
            Console.WriteLine("--------[callSTK]");
            _evm.DumpCallStack();

            Console.WriteLine("--------[evalSTK]");
            _evm.DumpEvaluationStack();
            

            TerminateWithError($"{e}");
        }

        private static void TerminateWithError(string err, int code = -1)
        {
            Console.WriteLine(err);
            Environment.Exit(code);
        }
    }
}