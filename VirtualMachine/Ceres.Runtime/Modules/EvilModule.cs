using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;
using Ceres.TranslationEngine;
using Ceres.TranslationEngine.Diagnostics;

namespace Ceres.Runtime.Modules
{
    public sealed class EvilModule : RuntimeModule
    {
        public override string FullyQualifiedName => "evil";

        [RuntimeModuleFunction("compile", ReturnType = DynamicValue.DynamicValueType.Table)]
        private static DynamicValue Compile(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAtLeast(1)
                .ExpectAtMost(2)
                .ExpectStringAt(0, out var source)
                .OptionalStringAt(1, "<dynamic_source>", out var fileName);

            try
            {
                var compiler = new Compiler();
                var script = compiler.Compile(source, fileName);

                return new Table
                {
                    { "success", true },
                    { "script", MarshalScript(script) }
                };
            }
            catch (CompilerException e)
            {
                return new Table
                {
                    { "success", false },
                    { "message", e.Message },
                    { "log", MarshalCompilerLog(e.Log) }
                };
            }
        }

        private static Table MarshalScript(Script script)
        {
            var chunks = new Table();

            for (var i = 0; i < script.Chunks.Count; i++)
            {
                var chunk = script.Chunks[i];
                chunks[chunk.Name!] = chunk;
            }
            
            return new Table
            {
                { "main_chunk", script.MainChunkID },
                { "chunks", chunks }
            };
        }

        private static Table MarshalCompilerLog(CompilerLog log)
        {
            var ret = new Table();

            for (var i = 0; i < log.Messages.Count; i++)
            {
                var msg = log.Messages[i];

                ret[i] = new Table
                {
                    { "severity", (int)msg.Severity },
                    { "message", msg.Message },
                    { "file_name", msg.FileName ?? DynamicValue.Nil },
                    { "message_code", msg.MessageCode },
                    { "line", msg.Line },
                    { "column", msg.Column }
                };
            }

            return ret;
        }
    }
}