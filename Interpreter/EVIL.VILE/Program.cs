using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EVIL.Grammar;
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
                Console.WriteLine($"Runtime exception: {re.Message}");

                foreach (var frame in re.EvilStackTrace)
                {
                    Console.WriteLine($"  {frame.FunctionName}:{re.Line} (inv. at {frame.InvokedAtLine})");
                }
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
    }
}