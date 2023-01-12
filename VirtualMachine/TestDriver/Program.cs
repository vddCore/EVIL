using System;
using System.IO;
using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.Grammar;
using EVIL.Grammar.Parsing;
using EVIL.Lexical;
using EVIL.Intermediate.Analysis;
using EVIL.Intermediate.CodeGeneration;
using EVIL.Intermediate.Storage;
using EVIL.RT;

namespace EVIL.VirtualMachine.TestDriver
{
    public static class Program
    {
        private static Disassembler _disassembler = new(new DisassemblerOptions
        {
            EmitExternHints = true,
            EmitParamTable = true,
            EmitFunctionHints = true,
            EmitLocalHints = true,
            EmitLocalTable = true,
            EmitExternTable = true,
            EmitFunctionParameters = true,
            EmitFunctionNames = true
        });
        
        private static Table _globalTable = new();

        private static Executable BuildExecutable(string filePath)
        {
            var lexer = new Lexer();
            var parser = new Parser(lexer, true);
            var compiler = new Compiler();
            
            EVIL.Grammar.AST.Nodes.Program program;
            try
            {
                var source = File.ReadAllText(filePath);
                lexer.LoadSource(source);
                program = parser.Parse();
            }
            catch (LexerException le)
            {
                Console.WriteLine($"Parsing error at ({le.Line}:{le.Column}): {le.Message}");
                return null;
            }
            catch (ParserException pe)
            {
                Console.WriteLine($"Parsing error at ({pe.Line}:{pe.Column}): {pe.Message}");
                return null;
            }
            
            Executable executable;
            try
            {
                executable = compiler.Compile(program);
            }
            catch (CompilerException e)
            {
                Console.WriteLine($"Compilation error at ({e.Line}:{e.Column}): {e.Message}");
                return null;
            }

            return executable;
        }

        public static void Main(string[] args)
        {
            var rt = new EvilRuntime(_globalTable);
            rt.LoadCoreRuntime();
            
            var evm = new EVM(_globalTable);
            var exe = BuildExecutable("/codespace/code/evil/testing.vil");

            EvxLinker.Link(exe, "a.evx");
            exe = EvxLoader.Load("a.evx");

            if (exe != null)
            {
                DisassembleExecutable(exe);
                Console.WriteLine("-[progRUN]------------");
                try
                {
                    evm.Load(exe);
                    evm.Run();
                }
                catch (VirtualMachineException e)
                {
                    Console.Write(e.Message);

                    if (e.InnerException != null)
                    {
                        Console.Write(": ");
                        Console.Write(e.InnerException.Message);
                    }
                    
                    Console.WriteLine();
                    Console.WriteLine(e.ExecutionContext.DumpCallStack());
                }
            }
        }

        private static void DisassembleExecutable(Executable executable)
        {
            Console.WriteLine("-[disASM]------------");
            Console.Write(_disassembler.Disassemble(executable));
        }
    }
}