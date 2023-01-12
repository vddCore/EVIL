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

            lexer.LoadSource(
                @"
                var a = 120;

                fn add(a, b) {
                    ret a + b;
                }

                fn main(a, b, c) {
                    var x = 10;

                    a = add($@(-24 + 30 * 3 / 22), 2) - x; 
                    b = 1200; 
                    c = 24 + 450 / 23.1 - 6.9 + 21.37 * 4.20; 

                    ret a * b / c;
                }

                main(1, 2, 3, 4, 5);"
            );

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