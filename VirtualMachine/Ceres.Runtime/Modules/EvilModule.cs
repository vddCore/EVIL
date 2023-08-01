using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;
using Ceres.TranslationEngine;

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
                    { "script", script.ToDynamicValue() }
                };
            }
            catch (CompilerException e)
            {
                return new Table
                {
                    { "success", false },
                    { "message", e.Message },
                    { "log", e.Log.ToDynamicValue() }
                };
            }
        }

        [RuntimeModuleFunction("reflect", ReturnType = DynamicValue.DynamicValueType.Table)]
        private static DynamicValue Reflect(Fiber fiber, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectChunkAt(0, out var chunk);

            var attrs = new Table();
            for (var i = 0; i < chunk.Attributes.Count; i++)
            {
                attrs[i] = chunk.Attributes[i].ToDynamicValue();
            }

            return new Table
            {
                { "name", chunk.Name ?? DynamicValue.Nil },
                { "attributes", attrs },
                { "local_info", BuildLocalInfo(chunk) },
                { "param_info", BuildParameterInfo(chunk) }
            };
        }
        
        private static Table BuildLocalInfo(Chunk chunk)
        {
            var ret = new Table();

            for (var i = 0; i < chunk.LocalCount; i++)
            {
                var local = new Table { { "id", i } };
                
                if (chunk.DebugDatabase.TryGetLocalName(i, out var localName))
                {
                    local["name"] = localName;
                }

                if (chunk.DebugDatabase.TryGetLocalRwState(i, out var rw))
                {
                    local["is_rw"] = rw;
                }

                ret[i] = local;
            }

            return ret;
        }

        private static Table BuildParameterInfo(Chunk chunk)
        {
            var ret = new Table();

            for (var i = 0; i < chunk.ParameterCount; i++)
            {
                var param = new Table { { "id", i } };

                if (chunk.DebugDatabase.TryGetParameterName(i, out var parameterName))
                {
                    param["name"] = parameterName;
                }

                if (chunk.ParameterInitializers.ContainsKey(i))
                {
                    param["default_value"] = chunk.ParameterInitializers[i];
                }

                if (chunk.DebugDatabase.TryGetParameterRwState(i, out var rw))
                {
                    param["is_rw"] = rw;
                }

                ret[i] = param;
            }

            return ret;
        }
    }
}