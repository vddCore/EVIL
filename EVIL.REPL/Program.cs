using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using EVIL.Grammar.Parsing;
using EVIL.Interpreter.Execution;
using EVIL.Lexical;
using Environment = EVIL.Interpreter.Environment;

namespace EVIL.REPL
{
    internal class Program
    {
        private static Environment _environment;
        private static Interpreter.Execution.Interpreter _interpreter;
        private static bool _stayInteractive;
        
        private static string _entryPointFunctionName;
        private static bool _refuseToRunTopLevelCode;
        
        private static List<string> _entryPointArgs = new();

        internal static void Main(string[] args)
        {
            _environment = new Environment();
            _environment.LoadCoreRuntime();

            _interpreter = new Interpreter.Execution.Interpreter(_environment);

            if (args.Length == 0)
            {
                InteractiveMode();
            }
            else
            {
                for (var i = 1; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "-h":
                            Help();
                            return;
                        
                        case "-i":
                            _stayInteractive = true;
                            continue;
                        
                        case "-e":
                            if (++i >= args.Length)
                            {
                                Console.WriteLine("'-e' requires a string.");
                                return;
                            }

                            _entryPointFunctionName = args[i];
                            continue;
                        
                        case "-c":
                            
                       
                        case "-r":
                            _refuseToRunTopLevelCode = true;
                            continue;
                        
                        default:
                            _entryPointArgs.Add(args[i]);
                            break;
                    }
                }

                if (File.Exists(args[0]))
                {                    
                    using (var sr = new StreamReader(args[0]))
                    {
                        try
                        {
                            if (!string.IsNullOrEmpty(_entryPointFunctionName))
                            {
                                _interpreter.Execute(
                                    sr.ReadToEnd(),
                                    _entryPointFunctionName,
                                    _entryPointArgs.ToArray(),
                                    _refuseToRunTopLevelCode
                                );
                            }
                            else
                            {
                                _interpreter.Execute(sr.ReadToEnd());
                            }
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

                            Console.WriteLine(sb.ToString());
                        }
                        catch (ParserException pe)
                        {
                            Console.WriteLine($"Parser error on line {pe.ScannerState?.Line}: {pe.Message}");
                        }
                        catch (ScannerException se)
                        {
                            Console.WriteLine($"Lexer error on line {se.Line}: {se.Message}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"File '{args[0]}' does not exist.");
                }

                if (_stayInteractive)
                    InteractiveMode();
            }
        }

        private static void Help()
        {
            Console.WriteLine("Ghetto EVIL interpreter with interactive mode.");
            Console.WriteLine($"Usage: {Path.GetFileName(Assembly.GetExecutingAssembly().Location)} [file] [options] [arguments_to_script]");
            Console.WriteLine("  -h: this message");
            Console.WriteLine("  -i: stay in interactive mode after executing a script.");
            Console.WriteLine("  -e <name>: specify entry-point function name.");
            Console.WriteLine("  -r: Refuse to run top-level code that is not a function definition.");
            Console.WriteLine("  -c: Compile a .NET module instead of running anything.");
        }

        private static void InteractiveMode()
        {
            Console.WriteLine("Entering interactive mode.");
            
            while (true)
            {
                Console.Write("EVIL> ");
                var input = Console.ReadLine();

                try
                {
                    var ret = _interpreter.Execute(input);
                    Console.WriteLine(ret.AsString().String);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}