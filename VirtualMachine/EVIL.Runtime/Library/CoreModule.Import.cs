using System;
using System.IO;
using System.Text;
using EVIL.ExecutionEngine;
using EVIL.ExecutionEngine.Abstraction;
using EVIL.ExecutionEngine.Diagnostics;
using EVIL.ExecutionEngine.Interop;
using EVIL.Grammar;
using EVIL.Grammar.Parsing;
using EVIL.Intermediate.Analysis;
using EVIL.Intermediate.CodeGeneration;
using EVIL.Intermediate.Storage;
using EVIL.Lexical;

namespace EVIL.Runtime.Library
{
    public partial class CoreModule
    {
        [ClrFunction("import")]
        public static DynamicValue Import(ExecutionContext ctx, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectStringAtIndex(0);

            var vm = ctx.VirtualMachine;
            var g = vm.GlobalTable;
            
            var scriptHomeVal = g.Get(EvilEnvironmentVariable.ScriptHomeDirectory.Name);
            var libName = args[0].String;

            var patternVariable = EVM.DefaultImportPattern;
            var patternVal = g.Get(EvilEnvironmentVariable.LibraryLookupPaths.Name);

            if (patternVal.Type == DynamicValueType.String)
            {
                patternVariable = patternVal.String;
            }
            
            var patterns = patternVariable.Split(";");
            var log = new StringBuilder();

            var libPath = FindLibrary(
                libName,
                patterns,
                string.Empty,
                log
            );

            if (libPath == null && scriptHomeVal.Type == DynamicValueType.String)
            {
                libPath = FindLibrary(
                    libName,
                    patterns,
                    scriptHomeVal.String,
                    log
                );
            }

            if (libPath == null)
            {
                throw new EvilRuntimeException(
                    $"Could not find '{libName}' in any of the known lookup paths:\n{log}"
                );
            }

            try
            {
                return new DynamicValue(
                    EvxLoader.Load(libPath)
                             .Export()
                );
            }
            catch
            {
                // must be a text file otherwise.
            }

            try
            {
                var lexer = new Lexer();
                var parser = new Parser(lexer);
                var compiler = new Compiler();

                lexer.LoadSource(File.ReadAllText(libPath));
                var program = parser.Parse(false);
                var exe = compiler.Compile(program);

#if DEBUG
                Console.WriteLine(
                    new Disassembler(new() { EmitLineNumbers = false })
                    .Disassemble(exe)
                );
#endif
                
                return new DynamicValue(exe.Export());
            }
            catch (LexerException le)
            {
                throw new EvilRuntimeException($"'{libPath}': {le.Message} (" +
                                               $"line {le.Line}, column {le.Column})");
            }
            catch (ParserException pe)
            {
                throw new EvilRuntimeException($"'{libPath}': {pe.Message} (" +
                                               $"line {pe.Line}, column {pe.Column})");
            }
            catch (CompilerException ce)
            {
                throw new EvilRuntimeException($"'{libPath}': {ce.Message} (" +
                                               $"line {ce.Line}, column {ce.Column})");
            }
        }

        private static string FindLibrary(string libName, string[] patterns, string prefix = "", StringBuilder log = null)
        {
            foreach (var pat in patterns)
            {
                var libAbsolutePath = Path.Combine(
                    prefix,
                    pat.Replace("?", libName)
                );

                if (File.Exists(libAbsolutePath))
                {
                    return libAbsolutePath;
                }

                log?.AppendLine($"  no file '{libAbsolutePath}'");
            }

            return null;
        }
    }
}