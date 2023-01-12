using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.Grammar.Parsing;
using EVIL.Lexical;
using EVIL.Intermediate;

namespace EVIL.VirtualMachine.TestDriver
{
    public static class Program
    {
        private static DynamicValue TestClrFunction(EVM evm, params DynamicValue[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                Console.Write(args[i].AsString());
            }
            
            return DynamicValue.Zero;
        }
        
        public static void Main(string[] args)
        {
            var lexer = new Lexer();
            var parser = new Parser(lexer, true);
            var compiler = new Compiler();
            var disasm = new Disassembler();

            var source = File.ReadAllText("./test_asgn2.vil");
            lexer.LoadSource(source);
            var programTreeRoot = parser.Parse();
            
            Executable executable;
            try
            {
                executable = compiler.Compile(programTreeRoot);
            }
            catch (CompilerException e)
            {
                Console.WriteLine($"Compilation error on line {e.Line}: {e.Message}");
                return;
            }
            
            Console.WriteLine(disasm.Disassemble(executable));
            Console.WriteLine("---------------------");
            
            var evm = new EVM(executable);
            evm.SetGlobal("print", new DynamicValue(TestClrFunction));
            evm.Run();
            
            Console.WriteLine(evm.DumpEvaluationStack());
        }
    }
}