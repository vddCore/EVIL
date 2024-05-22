using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EVIL.Ceres.ExecutionEngine;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.Ceres.Runtime;
using EVIL.Ceres.TranslationEngine;
using EVIL.CommonTypes.TypeSystem;

namespace EVIL.Ceres.LanguageTests
{
    public class TestRunner
    {
        private Dictionary<string, IEnumerable<Chunk>> _includeCache = new();

        private Stopwatch Stopwatch { get; } = new();
        private string[] TestDirectories { get; }
        private CeresVM VM { get; }
        private TextWriter TextOut { get; }
        private EvilRuntime Runtime { get; }

        private Dictionary<string, List<(Chunk TestChunk, string FailureReason)>> FailureLog { get; } = new();

        private TestRunnerOptions Options { get; }

        public TestRunner(CeresVM vm, TestRunnerOptions options)
        {
            VM = vm;
            Options = options;
            
            TestDirectories = Options.TestDirectories.Select(
                x => Path.GetFullPath(x)
            ).ToArray();
            
            TextOut = Options.TestOutput ?? Console.Out;
            
            Runtime = new EvilRuntime(vm);
        }

        public async Task<int> RunTests()
        {
            var result = 0;
            
            if ((result = Compile(out var testSets)) > 0)
                return result;

            foreach (var testSet in testSets)
            {
                if ((result = await Execute(testSet)) > 0)
                    return result;
            }

            return result;
        }

        private int Compile(out List<TestSet> testSets)
        {
            var compiler = new Compiler();
            compiler.RegisterAttributeProcessor("approximate", AttributeProcessors.ApproximateAttribute);
            compiler.RegisterAttributeProcessor("disasm", AttributeProcessors.DisasmAttribute);

            testSets = new List<TestSet>();
            
            foreach (var testDirectory in TestDirectories)
            {
                TextOut.WriteLine($"<[ TEST SET '{testDirectory}' ]>");
                var testSet = new TestSet(testDirectory);
                
                var paths = Directory
                    .GetFiles(testDirectory, "*.vil")
                    .OrderBy(x => x)
                    .ToList();

                foreach (var path in paths)
                {
                    var source = File.ReadAllText(path);
                    try
                    {
                        TextOut.Write($"  Compiling test '{path}'...");
                        var rootChunk = compiler.Compile(source, Path.GetFullPath(path));
                        testSet.AddTestRootChunk(path, rootChunk);
                        TextOut.WriteLine(" [ PASS ]");

                        if (compiler.Log.HasAnyMessages)
                        {
                            TextOut.Write(compiler.Log.ToString((s) => $"  | {s}"));
                            compiler.Log.Clear();
                        }
                    }
                    catch (CompilerException)
                    {
                        TextOut.WriteLine(" [ FAIL ]");
                        TextOut.WriteLine(compiler.Log.ToString((s) => $"  | {s}"));

                        if (Options.FailOnCompilerErrors)
                        {
                            TextOut.WriteLine("Test run aborted early due to a compilation error.");
                            return 1;
                        }

                        compiler.Log.Clear();
                    }
                }

                testSets.Add(testSet);
            }
            
            
            TextOut.WriteLine();
            return 0;
        }

        private async Task<int> Execute(TestSet testSet)
        {
            var failuresOccurred = false;
            
            foreach (var testRoot in testSet.TestRootChunks)
            {
                var passed = 0;
                var failed = 0;
                var ignored = 0;

                var path = testRoot.Key;
                TextOut.WriteLine($"--- [TEST '{path}'] ---");

                VM.Global.Clear();
                VM.Global.Set("__native_func", new((_, args) => { return args[3]; }));

                VM.Global.Set("__native_object", DynamicValue.FromObject(new object()));
                VM.Global.Set("__tricky", new TrickyTable());
                
                VM.Global.Set("__throw_test", new((fiber, args) =>
                {
                    return fiber.ThrowFromNative(args[0]);
                }));

                Runtime.RegisterBuiltInModules();
                Runtime.RegisterBuiltInFunctions();
                Runtime.RegisterModule<AssertModule>(out _);

                VM.MainFiber.SetCrashHandler((fiber, exception) =>
                {
                    TextOut.WriteLine($"Test crashed in root chunk: {exception.Message}");
                    TextOut.WriteLine(fiber.StackTrace(false));
                    TextOut.WriteLine(exception);
                    TextOut.WriteLine();
                });
                
                var testRootChunk = testRoot.Value;
                VM.MainFiber.Schedule(testRootChunk);
                VM.MainFiber.BlockUntilFinished();
                
                var testChunks = new List<Chunk>();
                foreach (var (name, _) in testRootChunk.NamedSubChunkLookup)
                {
                    var v = VM.Global[name];
                    if (v.Type == DynamicValueType.Chunk)
                    {
                        if (v.Chunk!.HasAttribute("test"))
                        {
                            testChunks.Add(v.Chunk);
                        }
                    }
                }

                if (testChunks.Count == 0)
                {
                    TextOut.WriteLine($"Test file '{path}' has no tests. Ignoring...");
                    continue;
                }

                if (VM.MainFiber.State == FiberState.Crashed)
                {
                    VM.MainFiber.Reset();
                    continue;
                }
                
                for (var i = 0; i < testChunks.Count; i++)
                {
                    var whenToDisassemble = "failure";

                    var chunk = testChunks[i];
                    if (chunk.TryGetAttribute("disasm", out var attr))
                    {
                        whenToDisassemble = attr.Values[0].String;
                    }

                    TextOut.Write($"[{i + 1}/{testChunks.Count}] ");

                    var result = await RunTestChunk(chunk, path);

                    if (whenToDisassemble != "never"
                        && (whenToDisassemble == "always"
                            || (whenToDisassemble == "failure" && result == false)
                        ))
                    {
                        Disassembler.Disassemble(chunk, TextOut);
                        TextOut.WriteLine();
                    }

                    if (result == false)
                    {
                        failed++;
                    }
                    else if (result == true)
                    {
                        passed++;
                    }
                    else
                    {
                        ignored++;
                    }
                }

                var verb = (ignored > 1 || ignored == 0)
                    ? "were"
                    : "was";

                failuresOccurred |= failed > 0;  
                    
                TextOut.WriteLine($"{passed} tests passed, {failed} failed, {ignored} {verb} ignored.");
                using (var process = Process.GetCurrentProcess())
                {
                    TextOut.WriteLine($"Total process memory so far: {process.WorkingSet64} bytes.");
                }
                TextOut.WriteLine();

                if (failuresOccurred && Options.FailOnTestErrors)
                {
                    break;
                }
            }

            ReportAnyTestFailures();

            if (failuresOccurred)
            {
                if (Options.FailOnTestErrors)
                {
                    TextOut.WriteLine("Test run aborted early due to one or more test failures.");
                }
                
                return 2;
            }

            return 0;
        }

        private async Task<bool?> RunTestChunk(Chunk chunk, string path)
        {
            var (ignore, why) = CheckIgnoreStatus(chunk);

            if (ignore)
            {
                TextOut.Write($"[IGNORED] '{chunk.Name}'");

                if (why != null)
                {
                    TextOut.Write($": {why}");
                }

                TextOut.WriteLine();
                return null;
            }

            var test = new Test(VM, chunk);

            Stopwatch.Reset();
            Stopwatch.Start();
            {
                await test.Run();
            }
            Stopwatch.Stop();

            var stamp = $"{Stopwatch.Elapsed.TotalMicroseconds}μs";

            if (test.Successful)
            {
                if (test.CallsAnyAsserts)
                {
                    TextOut.WriteLine($"[PASSED] '{chunk.Name}' [{stamp}]");
                }
                else
                {
                    var msg = $"No assertions were made. [{stamp}]";
                    
                    AddTestFailure(path, chunk, msg);
                    TextOut.WriteLine($"[INCONCLUSIVE] '{chunk.Name}': {msg}");
                }
            }
            else
            {
                var msg = new StringBuilder();
                msg.AppendLine($"{test.ErrorMessage} [{stamp}]");
                foreach (var line in test.StackTrace)
                {
                    msg.AppendLine($"      {line}");
                }

                AddTestFailure(path, chunk, msg.ToString());
                TextOut.WriteLine($"[FAILED] '{chunk.Name}': {msg}");
            }

            return test.Successful;
        }

        private (bool Ignore, string? Reason) CheckIgnoreStatus(Chunk chunk)
        {
            if (!chunk.TryGetAttribute("ignore", out var ignoreAttr))
            {
                return (false, null);
            }

            string? reason = null;
            if (ignoreAttr.Values.Count > 0)
            {
                reason = ignoreAttr.Values[0].ConvertToString().String;
            }

            return (true, reason);
        }

        private void AddTestFailure(string path, Chunk testChunk, string failureReason)
        {
            if (!FailureLog.TryGetValue(path, out var list))
            {
                list = new List<(Chunk TestChunk, string FailureReason)>();
                FailureLog.Add(path, list);
            }

            list.Add((testChunk, failureReason));
        }

        private void ReportAnyTestFailures()
        {
            if (FailureLog.Any())
            {
                TextOut.WriteLine("There were one or more of test failures/inconclusive results:");
            }

            foreach (var kvp in FailureLog)
            {
                TextOut.WriteLine($"{kvp.Key}: ");
                foreach (var result in kvp.Value)
                {
                    TextOut.WriteLine($"{result.TestChunk.Name} (def. at line {result.TestChunk.DebugDatabase.DefinedOnLine}): {result.FailureReason}");
                }
            }
        }
    }
}