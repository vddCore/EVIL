using System.Runtime.CompilerServices;
using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.Grammar;
using EVIL.Grammar.Parsing;
using EVIL.Lexical;
using EVIL.Intermediate;

namespace EVIL.VirtualMachine.TestDriver
{
    public static class Program
    {
        private static DynamicValue PrintClrFunction(EVM evm, params DynamicValue[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                Console.Write(args[i].AsString());
            }
            
            return DynamicValue.Zero;
        }
        
        private static DynamicValue PrintLnClrFunction(EVM evm, params DynamicValue[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                Console.Write(args[i].AsString());
            }
            Console.WriteLine();
            
            return DynamicValue.Zero;
        }

        
        public static void Main(string[] args)
        {
            var lexer = new Lexer();
            var parser = new Parser(lexer, true);
            var compiler = new Compiler();
            var disasm = new Disassembler();

            EVIL.Grammar.AST.Nodes.Program program;
            try
            {
                var source = File.ReadAllText("./test_asgn2.vil");
                lexer.LoadSource(source);
                program = parser.Parse();
            }
            catch (LexerException le)
            {
                Console.WriteLine($"Parsing error at ({le.Line}, {le.Column}): {le.Message}");
                return;
            }
            catch (ParserException pe)
            {
                Console.WriteLine($"Parsing error at {pe.Line}, {pe.Column}): {pe.Message}");
                return;
            }
            
            Executable executable;
            try
            {
                executable = compiler.Compile(program);
            }
            catch (CompilerException e)
            {
                Console.WriteLine($"Compilation error on line ({e.Line}, {e.Column}): {e.Message}");
                return;
            }
            Console.WriteLine("-[disASM]-------------");
            Console.Write(disasm.Disassemble(executable));
            
            Console.WriteLine("-[progRUN]------------");
            var evm = new EVM(executable);
            
            var ioTable = new Table();
            ioTable.Set(new("print"), new DynamicValue(PrintClrFunction));
            ioTable.Set(new("println"), new DynamicValue(PrintLnClrFunction));
            evm.SetGlobal("io", new(ioTable));
            
            evm.Run();
            Console.WriteLine("-[evSTACK]------------");
            Console.WriteLine(evm.DumpEvaluationStack());
        }
    }
}