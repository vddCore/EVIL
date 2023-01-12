﻿using System.IO;
using System.Linq;
using System.Text;
using EVIL.Grammar;
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

            var isolatedInterpreter = new Execution.Interpreter(interpreter.Environment);
            var sourceCode = File.ReadAllText(fullFilePath);

            try
            {
                isolatedInterpreter.Execute(sourceCode, true);
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
                            $"at {frame.FunctionName}({string.Join(',', frame.Parameters)})\n" +
                            $"   invoked on line {(frame.InvokedAtLine > 0 ? frame.InvokedAtLine.ToString() : "<unknown or entry>")}\n" +
                            $"   defined on line {(frame.DefinedAtLine > 0 ? frame.DefinedAtLine.ToString() : "<unknown>")}"
                        );
                    }
                }

                throw new ClrFunctionException(sb.ToString());
            }
            catch (ParserException pe)
            {
                throw new ClrFunctionException($"Parser error at ({pe.Line}, {pe.Column}): {pe.Message}");
            }
            catch (LexerException le)
            {
                throw new ClrFunctionException($"Lexer error at ({le.Line}, {le.Column}) {le.Message}");
            }

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

            var trace = interpreter.StackTrace();

            if (!trace.Any())
                return DynValue.Zero;

            var tbl = new Table();

            for (var i = 0; i < trace.Count; i++)
            {
                var frame = new Table();
                var parameters = new Table();

                for (var j = 0; j < trace[i].Parameters.Count; j++)
                {
                    parameters[j] = new DynValue(trace[i].Parameters[j]);
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

            foreach (var frame in interpreter.StackTrace())
            {
                sb.AppendLine(
                    $"at {frame.FunctionName}({string.Join(',', frame.Parameters)})\n" +
                    $"   invoked on line {(frame.InvokedAtLine > 0 ? frame.InvokedAtLine.ToString() : "<unknown or entry>")}\n" +
                    $"   defined on line {(frame.DefinedAtLine > 0 ? frame.DefinedAtLine.ToString() : "<unknown>")}"
                );
            }

            return new DynValue(sb.ToString());
        }

        [ClrFunction("setglobal")]
        public static DynValue SetGlobal(Execution.Interpreter interpreter, FunctionArguments args)
        {
            args.ExpectExactly(2)
                .ExpectTypeAtIndex(0, DynValueType.String);

            interpreter.Environment.GlobalScope.Members[args[0].String] = args[1];
            return DynValue.Zero;
        }
    }
}