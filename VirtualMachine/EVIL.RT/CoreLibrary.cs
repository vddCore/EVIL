using System.IO;
using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.ExecutionEngine.Diagnostics;
using EVIL.ExecutionEngine.Interop;
using EVIL.Grammar;
using EVIL.Grammar.Parsing;
using EVIL.Intermediate.CodeGeneration;
using EVIL.Lexical;
using EVIL.RT;

namespace EVIL.Interpreter.Runtime.Library
{
    public class CoreLibrary
    {
        [ClrFunction("import")]
        public static DynamicValue Import(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectStringAtIndex(0);

            var relativeFilePath = args[0].String;
            string targetPath = null;
            
            foreach (var path in ctx.VirtualMachine.ImportLookupPaths)
            {
                var tempPath = Path.Combine(path, relativeFilePath);

                if (File.Exists(tempPath))
                {
                    targetPath = tempPath;
                    break;
                }
            }

            if (targetPath == null)
            {
                throw new EvilRuntimeException(
                    $"Could not find '{relativeFilePath}' in any of the known lookup paths."
                );
            }

            try
            {
                var lexer = new Lexer();
                var parser = new Parser(lexer, false);
                var compiler = new Compiler();

                lexer.LoadSource(File.ReadAllText(targetPath));
                var program = parser.Parse();
                var exe = compiler.Compile(program);
                
                var tempVm = new EVM(new Table());
                var rt = new EvilRuntime(tempVm.GlobalTable);
                rt.LoadCoreRuntime();
                
                tempVm.ImportLookupPaths.Add(Path.GetDirectoryName(targetPath));
                tempVm.Load(exe);

                var chunk = tempVm.FindExposedChunk("export");

                if (chunk == null)
                {
                    throw new EvilRuntimeException(
                        $"Error during import: '{targetPath}' has no function named 'export'."
                    );
                }

                tempVm.InvokeCallback(chunk, null);
                tempVm.Start();
                
                var retVal = new DynamicValue(tempVm.MainExecutionContext.EvaluationStackTop, false);

                tempVm.Stop();
                tempVm.MainExecutionContext.Reset();
                tempVm.GlobalTable.Entries.Clear();

                return retVal;
            }
            catch (LexerException le)
            {
                throw new EvilRuntimeException($"Error while importing '{relativeFilePath}': {le.Message} (" +
                                               $"line {le.Line}, column {le.Column})");
            }
            catch (ParserException pe)
            {
                throw new EvilRuntimeException($"Error while importing '{relativeFilePath}': {pe.Message} (" +
                                               $"line {pe.Line}, column {pe.Column})");
            }
            catch (CompilerException ce)
            {
                throw new EvilRuntimeException($"Error while importing '{relativeFilePath}': {ce.Message} (" +
                                               $"line {ce.Line}, column {ce.Column})");
            }
        }
        
        [ClrFunction("type")]
        public static DynamicValue Type(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1);
            return new(args[0].Type.ToString().ToLower());
        }

        [ClrFunction("strace_s")]
        public static DynamicValue StraceString(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectNone();
            return new DynamicValue(ctx.DumpCallStack());
        }

        [ClrFunction("setglobal")]
        public static DynamicValue SetGlobal(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(2)
                .ExpectTypeAtIndex(0, DynamicValueType.String);

            ctx.VirtualMachine.GlobalTable.Set(args[0], args[1]);
            return args[1];
        }

        [ClrFunction("uint8")]
        public static DynamicValue Uint8Conv(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new((byte)args[0].Number);
        }
        
        [ClrFunction("sint8")]
        public static DynamicValue Sint8Conv(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new((sbyte)args[0].Number);
        }
        
        [ClrFunction("uint16")]
        public static DynamicValue Uint16Conv(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new((ushort)args[0].Number);
        }
        
        [ClrFunction("sint16")]
        public static DynamicValue Sint16Conv(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new((short)args[0].Number);
        }
        
        [ClrFunction("uint32")]
        public static DynamicValue Uint32Conv(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new((uint)args[0].Number);
        }
        
        [ClrFunction("sint32")]
        public static DynamicValue Sint32Conv(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new((int)args[0].Number);
        }
        
        [ClrFunction("uint64")]
        public static DynamicValue Uint64Conv(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new((ulong)args[0].Number);
        }
        
        [ClrFunction("sint64")]
        public static DynamicValue Sint64Conv(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynamicValueType.Number);

            return new((long)args[0].Number);
        }
    }
}