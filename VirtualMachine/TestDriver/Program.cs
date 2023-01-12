﻿using System.Runtime.CompilerServices;
using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.Grammar;
using EVIL.Grammar.Parsing;
using EVIL.Lexical;
using EVIL.Intermediate;
using EVIL.Intermediate.Analysis;
using EVIL.Intermediate.CodeGeneration;
using EVIL.Intermediate.Storage;

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

        private static DynamicValue StrChrFunction(EVM evm, params DynamicValue[] args)
        {
            return new DynamicValue(((char)args[0].Number).ToString());
        }

        public static void Main(string[] args)
        {
            // var lexer = new Lexer();
            // var parser = new Parser(lexer, true);
            // var compiler = new Compiler();
            // var disasm = new Disassembler(new DisassemblerOptions
            // {
            //     EmitExternHints = true,
            //     EmitParamTable = true,
            //     EmitFunctionHints = true,
            //     EmitLocalHints = true,
            //     EmitLocalTable = true,
            //     EmitExternTable = true,
            //     EmitFunctionParameters = true,
            //     EmitFunctionNames = true
            // });
            //
            // EVIL.Grammar.AST.Nodes.Program program;
            // try
            // {
            //     var source = File.ReadAllText("./test_asgn2.vil");
            //     lexer.LoadSource(source);
            //     program = parser.Parse();
            // }
            // catch (LexerException le)
            // {
            //     Console.WriteLine($"Parsing error at ({le.Line}, {le.Column}): {le.Message}");
            //     return;
            // }
            // catch (ParserException pe)
            // {
            //     Console.WriteLine($"Parsing error at {pe.Line}, {pe.Column}): {pe.Message}");
            //     return;
            // }
            //
            // Executable executable;
            // try
            // {
            //     executable = compiler.Compile(program);
            // }
            // catch (CompilerException e)
            // {
            //     Console.WriteLine($"Compilation error at ({e.Line}, {e.Column}): {e.Message}");
            //     return;
            // }
            // Console.WriteLine("-[disASM]-------------");
            // Console.Write(disasm.Disassemble(executable));
            //
            // using var fs = new FileStream("a.evx", FileMode.Create);
            // using var l = new Linker(executable);
            // l.Link(fs);
            //
            using var fs2 = new FileStream("a.evx", FileMode.Open);
            using var ldr = new Loader(fs2);
            
            var exe2 = ldr.Load();
            
            // Console.WriteLine("-[progRUN]------------");
            var evm = new EVM(exe2);
            
            var ioTable = new Table();
            ioTable.Set(new("print"), new DynamicValue(PrintClrFunction));
            ioTable.Set(new("println"), new DynamicValue(PrintLnClrFunction));
            evm.SetGlobal("io", new(ioTable));
            
            var strtable = new Table();
            strtable.Set(new("chr"), new DynamicValue(StrChrFunction));
            evm.SetGlobal("str", new(strtable));
            
            evm.Run();
            // Console.WriteLine("-[evSTACK]------------");
            // Console.WriteLine(evm.DumpEvaluationStack());
        }
    }
}