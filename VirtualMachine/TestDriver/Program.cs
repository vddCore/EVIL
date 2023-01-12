using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            EmitFunctionNames = true,
            EmitLineNumbers = false
        });
        
        private static Table _globalTable = new();

        private static Executable BuildExecutable(string filePath)
        {
            var lexer = new Lexer();
            var parser = new Parser(lexer);
            var compiler = new Compiler();
            
            EVIL.Grammar.AST.Nodes.Program program;
            try
            {
                var source = File.ReadAllText(filePath);
                lexer.LoadSource(source);
                program = parser.Parse(true);
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
            var exe = BuildExecutable("./Tests/table.vil");

            if (exe != null)
            {
                EvxWriter.Write(exe, "a.evx");
                exe = EvxLoader.Load("a.evx");
                
                DisassembleExecutable(exe);
                Console.WriteLine("-[progRUN]------------");
                try
                {
                    var main = exe.FindExposedChunk("main");

                    if (main == null)
                    {
                        Console.WriteLine("main() not found.");
                        return;
                    }

                    foreach (var chunk in exe.Chunks.Where(x => x.IsPublic))
                    {
                        _globalTable.Set(chunk.Name, chunk);
                    }
                    
                    evm.RunChunk(main);
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