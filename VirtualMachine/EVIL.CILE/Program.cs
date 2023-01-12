using System;
using System.IO;
using System.Linq;
using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.Grammar;
using EVIL.Grammar.Parsing;
using EVIL.Intermediate.Analysis;
using EVIL.Intermediate.CodeGeneration;
using EVIL.Intermediate.Storage;
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
        
        private static bool _disassembleCompiledProgram;
        private static bool _compileOnly;

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

            var absolutePath = Path.GetFullPath(args[0]);
            var text = File.ReadAllText(args[0]);
            
            var programArguments = args.Skip(1)
                .Where(x => !x.StartsWith("-"))
                .Select(x => new DynamicValue(x))
                .ToArray();

            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == "-d")
                {
                    _disassembleCompiledProgram = true;
                }
                
                if (args[i] == "-c")
                {
                    _compileOnly = true;
                }
            }

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
            
            if (_disassembleCompiledProgram)
            {
                var disasm = new Disassembler();
                Console.WriteLine(disasm.Disassemble(exe));
            }

            if (_compileOnly)
            {
                var outDir = Path.GetDirectoryName(absolutePath);
                var outFileName = Path.GetFileNameWithoutExtension(absolutePath);
                EvxLinker.Link(exe, Path.Combine(outDir, outFileName + ".evx"));
                return;
            }
            
            var rt = new EvilRuntime(_globals);
            rt.LoadCoreRuntime();
            
            try
            {
                _evm.ImportLookupPaths.Add(Path.GetDirectoryName(absolutePath));
                _evm.Load(exe);
            }
            catch (Exception e)
            {
                TerminateWithError($"Failed to load the produced executable: {e.Message}");
            }

            try
            {
                _evm.RunChunk("main", null, programArguments);
            }
            catch (VirtualMachineException e)
            {
                TerminateWithDump(e);
            }
        }

        private static void TerminateWithDump(VirtualMachineException vme)
        {
            Console.Write(vme.Message);

            if (vme.InnerException != null)
            {
                Console.Write(": ");
                Console.WriteLine(vme.InnerException.Message);
            }
            else
            {
                Console.WriteLine();
            }
            
            Console.WriteLine(vme.ExecutionContext.DumpCallStack());
            
        }

        private static void TerminateWithError(string err, int code = -1)
        {
            Console.WriteLine(err);
            Environment.Exit(code);
        }
    }
}