using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ceres.ExecutionEngine;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.TranslationEngine;
using EVIL.Grammar;
using EVIL.Grammar.Parsing;
using EVIL.Lexical;

namespace Insitor
{
    public class TestRunner
    {
        private string TestDirectory { get; }
        private CeresVM VM { get; }
        private TextWriter TextOut { get; }

        private Dictionary<string, Script> TestScripts { get; } = new();
        private Dictionary<string, List<(Chunk TestChunk, string FailureReason)>> FailureLog { get; } = new();

        public TestRunner(string testDirectory, CeresVM vm, TextWriter? textOut = null)
        {
            TestDirectory = testDirectory;
            VM = vm;
            TextOut = textOut ?? Console.Out;

            CompileTests();
        }

        private void CompileTests()
        {
            var paths = Directory
                .GetFiles(TestDirectory, "*.vil")
                .ToList();

            var parser = new Parser();
            var compiler = new Compiler();
            
            compiler.RegisterAttributeProcessor("approximate", AttributeProcessors.ApproximateAttribute);
            compiler.RegisterAttributeProcessor("disasm", AttributeProcessors.DisasmAttribute);

            foreach (var path in paths)
            {
                var source = File.ReadAllText(path);
                try
                {
                    var program = parser.Parse(source);
                    TestScripts.Add(path, compiler.Compile(program));
                }
                catch (LexerException le)
                {
                    throw new TestBuildPhaseException($"Failed to parse file '{path}'.", le);
                }
                catch (ParserException pe)
                {
                    throw new TestBuildPhaseException($"Failed to parse file '{path}'.", pe);
                }
                catch (CompilerException ce)
                {
                    throw new TestBuildPhaseException($"Failed to compile file '{path}'.", ce);
                }
            }
        }

        public async Task RunTests()
        {
            foreach (var testdesc in TestScripts)
            {
                var passed = 0;
                var failed = 0;
                var ignored = 0;

                var path = testdesc.Key;
                TextOut.WriteLine($"--- [TEST '{path}'] ---");

                var testScript = testdesc.Value;

                var testChunks = testScript.Chunks.Where(
                    x => x.Attributes.FirstOrDefault(
                        a => a.Name == "test"
                    ) != null
                ).ToList();

                if (testChunks.Count == 0)
                {
                    TextOut.WriteLine($"Test file '{testScript}' has no tests. Ignoring...");
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
                    if (VM.MainFiber.TryPeekValue(out _))
                    {
                        TextOut.WriteLine("[!!] Stack imbalance detected.");
                    }

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

                TextOut.WriteLine($"{passed} tests passed, {failed} failed, {ignored} {verb} ignored.");
                TextOut.WriteLine();
            }
            
            ReportAnyTestFailures();
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

            var testAttr = chunk.GetAttribute("test");
            if (testAttr.Values.Count > 0)
            {
                var expected = testAttr.Values[0];
                await VM.MainFiber.ScheduleAsync(chunk);
                if (!VM.MainFiber.TryPopValue(out var actual))
                {
                    var msg = $"expected '{expected}', but the test returned no value.";

                    AddTestFailure(path, chunk, msg);
                    TextOut.WriteLine($"[FAILED] '{chunk.Name}': {msg}");

                    return false;
                }
                else
                {
                    ApproximateIfSpecified(chunk, ref actual);

                    if (expected.IsEqualTo(actual).IsTruth)
                    {
                        TextOut.WriteLine($"[PASSED] '{chunk.Name}': test completed successfully.");
                        return true;
                    }
                    else
                    {
                        var msg = $"{actual} is not equal to expected value '{expected}'.";

                        AddTestFailure(path, chunk, msg);
                        TextOut.WriteLine($"[FAILED] '{chunk.Name}': {msg}");

                        return false;
                    }
                }
            }
            else
            {
                await VM.MainFiber.ScheduleAsync(chunk);

                if (!VM.MainFiber.TryPopValue(out var returnValue))
                {
                    var msg = "No value was returned.";

                    AddTestFailure(path, chunk, msg);
                    TextOut.WriteLine($"[FAILED] '{chunk.Name}': {msg}");

                    return false;
                }
                else
                {
                    if (returnValue.IsTruth)
                    {
                        TextOut.WriteLine($"[PASSED] '{chunk.Name}': test completed successfully.");
                        return true;
                    }
                    else
                    {
                        var msg = $"Test returned a failure state '{returnValue}'.";

                        AddTestFailure(path, chunk, msg);
                        TextOut.WriteLine($"[FAILED] '{chunk.Name}': {msg}");

                        return false;
                    }
                }
            }
        }

        private void ApproximateIfSpecified(Chunk chunk, ref DynamicValue value)
        {
            if (chunk.TryGetAttribute("approximate", out var approximateAttr))
            {
                var digits = 1;
                if (approximateAttr.Values.Any())
                {
                    digits = (int)approximateAttr.Values[0].Number;
                }

                value = new DynamicValue(Math.Round(value.Number, digits));
            }
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
                TextOut.WriteLine("There were one or more of test failures:");
            }

            foreach (var kvp in FailureLog)
            {
                TextOut.WriteLine($"  {kvp.Key}: ");
                foreach (var result in kvp.Value)
                {
                    TextOut.WriteLine($"    {result.TestChunk.Name}: {result.FailureReason}");
                }
                
                TextOut.WriteLine();
            }
        }
    }
}