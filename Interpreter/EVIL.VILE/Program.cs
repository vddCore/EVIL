using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EVIL.Grammar;
using EVIL.Interpreter.Abstraction;
using EVIL.Interpreter.Diagnostics;
using EVIL.Interpreter.Execution;
using EVIL.Lexical;
using EVIL.VILE.Extensibility;

namespace EVIL.VILE
{
    internal static class Program
    {
        private static string _script;
        private static Task _loadTask;
        
        public static async Task Main(string[] args)
        {
            _loadTask = LoadScript(args);
            var execTask = ExecuteLoadedScript(args);

            try
            {
                await execTask;
            }
            catch (RuntimeException re)
            {
                var sb = new StringBuilder();
                
                if (re.InnerException is InvalidDynValueTypeException idv)
                {
                    sb.AppendLine($"Type error - attempted to treat a {idv.ActualType} as a {idv.RequestedType}");
                }
                else
                {
                    sb.Append($"Runtime error - ");
                    sb.AppendLine($"{re.Message}");
                }
                
                if (re.Line != null)
                {
                    sb.Append($" on line {re.Line}");
                }

                sb.AppendLine();
                
                if (re.EvilStackTrace == null || re.EvilStackTrace.Count == 0)
                {
                    sb.Append("No stack trace available.");
                }
                else
                {
                    sb.Append(FormatStackTrace(re.EvilStackTrace));
                }

                Console.WriteLine(sb.ToString());
            }
            catch (ParserException pe)
            {
                Console.WriteLine($"Syntax error on line {pe.LexerState.Line}: {pe.Message}");
            }
            catch (LexerException le)
            {
                Console.WriteLine($"Lexical error on line {le.Line}: {le.Message}");
            }
            catch (ExitStatementException)
            {
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected error: {e.Message}");
            }
        }

        private static async Task LoadScript(string[] args)
        {
            if (args.Length < 1)
                throw new Exception("Expected script file path, found nothing.");

            var filePath = args[0];

            if (!File.Exists(filePath))
                throw new Exception($"Script '{filePath}' does not exist.");

            using (var sr = new StreamReader(filePath))
                _script = await sr.ReadToEndAsync();
        }

        private static async Task ExecuteLoadedScript(string[] args)
        {
            var modLoader = new ModuleLoader(
                Path.Combine(AppContext.BaseDirectory, "modules")
            );

            var executionEngine = new Interpreter.Execution.Interpreter();
            executionEngine.Environment.LoadCoreRuntime();
            
            try
            {
                foreach (var type in modLoader.Libraries)
                    executionEngine.Environment.RegisterPackage(type);
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERR: {e.Message}");
            }

            await _loadTask;
            await executionEngine.ExecuteAsync(_script, "main", args.Skip(1).ToArray());
        }
        
        private static string FormatStackTrace(List<StackFrame> stackTrace)
        {
            var sb = new StringBuilder();

            foreach (var frame in stackTrace)
            {
                sb.AppendLine(
                    $"at {frame.FunctionName}({string.Join(',', frame.Parameters)})\n" +
                    $"   invoked on line {(frame.InvokedAtLine > 0 ? frame.InvokedAtLine.ToString() : "<unknown or entry>")}\n" +
                    $"   defined on line {(frame.DefinedAtLine > 0 ? frame.DefinedAtLine.ToString() : "<unknown>")}"
                );
            }

            return sb.ToString();
        }
    }
}