using System;
using System.IO;
using System.Linq;
using Environment = EVIL.Interpreter.Environment;

namespace EVIL.REPL
{
    internal class Program
    {
        private static Environment _environment;
        private static Interpreter.Execution.Interpreter _interpreter;
        private static bool _stayInteractive;

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
                if (args.Contains("--help"))
                {
                    Console.WriteLine("Ghetto EVIL interpreter with interactive mode.");
                    Console.WriteLine("Usage: EVIL.REPL [file] [options]");
                    Console.WriteLine("  --help: this message");
                    Console.WriteLine("  -i | --interactive: stay in interactive mode after executing a script.");

                    return;
                }

                if (args.Contains("-i"))
                {
                    _stayInteractive = true;
                }

                if (!File.Exists(args[0]))
                {
                    Console.WriteLine($"File '{args[0]}' does not exist.");
                    return;
                }

                using (var sr = new StreamReader(args[0]))
                {
                    _interpreter.Execute(sr.ReadToEnd());
                }

                if (_stayInteractive)
                    InteractiveMode();
            }
        }

        private static void InteractiveMode()
        {
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