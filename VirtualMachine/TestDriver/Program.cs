using EVIL.ExecutionEngine;
using EVIL.Grammar.Parsing;
using EVIL.Lexical;
using EVIL.Intermediate;

namespace EVIL.VirtualMachine.TestDriver
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var lexer = new Lexer();
            var parser = new Parser(lexer, true);
            var compiler = new Compiler();
            var disasm = new Disassembler();

            var source = File.ReadAllText("./test_asgn2.vil");
            lexer.LoadSource(source);
            var programTreeRoot = parser.Parse();
            var executable = compiler.Compile(programTreeRoot);
            Console.WriteLine(disasm.Disassemble(executable));
            Console.WriteLine("---------------------");

            var evm = new EVM(executable);
            evm.Run();
            
            Console.WriteLine(evm.DumpEvaluationStack());
        }
    }
}