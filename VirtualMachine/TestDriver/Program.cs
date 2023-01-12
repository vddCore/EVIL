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
        private static Table _globalTable = new();

        public static void Main(string[] args)
        {
            var lexer = new Lexer();
            var parser = new Parser(lexer, true);
            var compiler = new Compiler();
            var disasm = new Disassembler(new DisassemblerOptions
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
            
            EVIL.Grammar.AST.Nodes.Program program;
            try
            {
                var source = File.ReadAllText("./test_asgn2.vil");
                lexer.LoadSource(source);
                program = parser.Parse();
            }
            catch (LexerException le)
            {
                Console.WriteLine($"Parsing error at ({le.Line}:{le.Column}): {le.Message}");
                return;
            }
            catch (ParserException pe)
            {
                Console.WriteLine($"Parsing error at ({pe.Line}:{pe.Column}): {pe.Message}");
                return;
            }
            
            Executable executable;
            try
            {
                executable = compiler.Compile(program);
            }
            catch (CompilerException e)
            {
                Console.WriteLine($"Compilation error at ({e.Line}:{e.Column}): {e.Message}");
                return;
            }
            Console.WriteLine("-[disASM]-------------");
            Console.Write(disasm.Disassemble(executable));
            
            EvxLinker.Link(executable, "a.evx");
            var exe2 = EvxLoader.Load("a.evx");
            
            Console.WriteLine("-[progRUN]------------");
            var rt = new EvilRuntime(_globalTable);
            rt.LoadCoreRuntime();
            
            var evm = new EVM(exe2, _globalTable);
            var callback = evm.FindExposedChunk("callback_a");
            evm.InvokeCallback(callback, new DynamicValue(1234));
        }
    }
}