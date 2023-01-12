using System;
using System.Collections.Generic;
using System.IO;
using Environment = EVIL.Interpreter.Environment;

namespace EVIL.REPL
{
    internal class Program
    {
        private static Environment _environment;
        private static Interpreter.Execution.Interpreter _interpreter;
        private static bool _stayInteractive;
        
        private static string _entryPointFunctionName;
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
                                    _entryPointArgs.ToArray()
                                );
                            }
                            else
                            {
                                _interpreter.Execute(sr.ReadToEnd());
                            }
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine(e.Message);
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
            Console.WriteLine("Usage: EVIL.REPL [file] [options] [arguments_to_script]");
            Console.WriteLine("  -h: this message");
            Console.WriteLine("  -i: stay in interactive mode after executing a script.");
            Console.WriteLine("  -e <name>: specify entry-point function name.");
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