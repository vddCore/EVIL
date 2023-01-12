using System;
using System.IO;
using System.Linq;
using System.Text;
using EVIL.Grammar.Parsing;
using EVIL.Interpreter.Abstraction;
using EVIL.Interpreter.Execution;
using EVIL.Lexical;

namespace EVIL.Interpreter.Runtime.Library
{
    public class CoreLibrary
    {
        [ClrFunction("import")]
        public static DynValue Import(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1)
                .ExpectTypeAtIndex(0, DynValueType.String);

            var filePath = args[0].String;
            string lookupDirectory;

            if (interpreter.MainFilePath != null)
            {
                lookupDirectory = Path.GetDirectoryName(interpreter.MainFilePath);
            }
            else
            {
                lookupDirectory = System.Environment.CurrentDirectory;
            }

            var fullFilePath = Path.Combine(lookupDirectory!, filePath);
            if (!File.Exists(fullFilePath))
            {
                throw new ClrFunctionException($"File '{filePath}' does not exist.");
            }

            var isolatedEnv = new Environment();
            var isolatedInterpreter = new Execution.Interpreter(isolatedEnv);
            var sourceCode = File.ReadAllText(fullFilePath);

            isolatedEnv.LoadCoreRuntime();
            try
            {
                isolatedInterpreter.Execute(sourceCode);
            }
            catch (ExitStatementException)
            {
            }
            catch (RuntimeException re)
            {
                var sb = new StringBuilder();
                sb.Append($"Runtime error");

                if (re.Line != null)
                {
                    sb.Append($" on line {re.Line}");
                }

                sb.AppendLine($": {re.Message}");
                sb.AppendLine();

                if (re.EvilStackTrace == null || re.EvilStackTrace.Count == 0)
                {
                    sb.Append("No stack trace available.");
                }
                else
                {
                    foreach (var frame in re.EvilStackTrace)
                    {
                        sb.AppendLine(
                            $"at {frame.FunctionName}({string.Join(',', frame.ParameterNames)})\n" +
                            $"   invoked on line {(frame.InvokedAtLine > 0 ? frame.InvokedAtLine.ToString() : "<unknown or entry>")}\n" +
                            $"   defined on line {(frame.DefinedAtLine > 0 ? frame.DefinedAtLine.ToString() : "<unknown>")}"
                        );
                    }
                }

                throw new ClrFunctionException(sb.ToString());
            }
            catch (ParserException pe)
            {
                throw new ClrFunctionException($"Parser error on line {pe.ScannerState?.Line}: {pe.Message}");
            }
            catch (ScannerException se)
            {
                throw new ClrFunctionException($"Lexer error on line {se.Line}: {se.Message}");
            }

            isolatedInterpreter.Environment = interpreter.Environment;
            isolatedInterpreter.Execute(File.ReadAllText(fullFilePath));

            return DynValue.Zero;
        }

        [ClrFunction("type")]
        public static DynValue Type(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(1);
            return new(args[0].Type.ToString().ToLower());
        }

        [ClrFunction("strace")]
        public static DynValue Strace(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectNone();

            var trace = interpreter.Environment.StackTrace();

            if (!trace.Any())
                return DynValue.Zero;
            
            var tbl = new Table();
            
            for (var i = 0; i < trace.Count; i++)
            {
                var frame = new Table();
                var parameters = new Table();
                
                for (var j = 0; j < trace[i].ParameterNames.Count; j++)
                {
                    parameters[j] = new DynValue(trace[i].ParameterNames[j]);
                }
                
                frame["fn_name"] = new DynValue(trace[i].FunctionName);
                frame["def_at"] = new DynValue(trace[i].DefinedAtLine);
                frame["inv_at"] = new DynValue(trace[i].InvokedAtLine);
                frame["params"] = new DynValue(parameters);

                tbl[i] = new DynValue(frame);
            }

            return new(tbl);
        }

        [ClrFunction("strace_s")]
        public static DynValue StraceString(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectNone();
            
            var sb = new StringBuilder();

            foreach (var frame in interpreter.Environment.StackTrace())
            {
                sb.AppendLine(
                    $"at {frame.FunctionName}({string.Join(',', frame.ParameterNames)})\n" +
                    $"   invoked on line {(frame.InvokedAtLine > 0 ? frame.InvokedAtLine.ToString() : "<unknown or entry>")}\n" +
                    $"   defined on line {(frame.DefinedAtLine > 0 ? frame.DefinedAtLine.ToString() : "<unknown>")}"
                );
            }

            return new DynValue(sb.ToString());
        }

        [ClrFunction("isdef")]
        public static DynValue IsDefined(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectAtLeast(1)
                .ExpectAtMost(2)
                .ExpectTypeAtIndex(0, DynValueType.String);

            var name = args[0].String;

            var scope = "any";

            if (args.Count > 1)
            {
                if (args[1].Type == DynValueType.String)
                {
                    scope = args[1].String;
                }
            }

            return scope switch
            {
                "local" => new(interpreter.Environment.LocalScope.HasMember(name)),
                "global" => new(interpreter.Environment.GlobalScope.HasMember(name)),
                "any" => new(interpreter.Environment.LocalScope.FindInScopeChain(name) != null),
                _ => throw new ClrFunctionException("Unsupported scope type.")
            };
        }
    }
}