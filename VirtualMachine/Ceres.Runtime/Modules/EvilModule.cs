using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.Runtime.Extensions;
using Ceres.TranslationEngine;
using EVIL.CommonTypes.TypeSystem;
using static EVIL.CommonTypes.TypeSystem.DynamicValueType;

namespace Ceres.Runtime.Modules
{
    public sealed class EvilModule : RuntimeModule
    {
        public override string FullyQualifiedName => "evil";

        [RuntimeModuleFunction("compile")]
        [EvilDocFunction(
            "Attempts to compile the provided EVIL source.",
            Returns = "A Table containing either the script data (`.success == true`), " +
                      "or the compiler error message and compilation log (`.success == false`).",
            ReturnType = DynamicValueType.Table
        )]
        [EvilDocArgument("source", "Source code that is to be compiled.", String)]
        [EvilDocArgument(
            "file_name", 
             "File name to be embedded into compiled script's metadata.",
            String,
            DefaultValue = "<dynamic_source>"
        )]
        private static DynamicValue Compile(Fiber _, params DynamicValue[] args)
        {
            args.ExpectAtLeast(1)
                .ExpectAtMost(2)
                .ExpectStringAt(0, out var source)
                .OptionalStringAt(1, "<dynamic_source>", out var fileName);

            try
            {
                var compiler = new Compiler();
                var rootChunk = compiler.Compile(source, fileName);

                return new Table
                {
                    { "success", true },
                    { "chunk", rootChunk }
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

        [RuntimeModuleFunction("reflect")]
        [EvilDocFunction(
            "Retrieves metadata information of the given Function.",
            Returns = "A Table containing the reflected Function's metadata.",
            ReturnType = DynamicValueType.Table
        )]
        [EvilDocArgument(
            "function",
            "The Function whose information is to be retrieved.",
            DynamicValueType.Chunk
        )]
        private static DynamicValue Reflect(Fiber _, params DynamicValue[] args)
        {
            args.ExpectExactly(1)
                .ExpectChunkAt(0, out var function);

            var attrs = new Array(function.Attributes.Count);
            for (var i = 0; i < function.Attributes.Count; i++)
            {
                attrs[i] = function.Attributes[i].ToDynamicValue();
            }

            return new Table
            {
                { "name", function.Name },
                { "attributes", attrs },
                { "local_info", BuildLocalInfo(function) },
                { "param_info", BuildParameterInfo(function) }
            };
        }
        
        private static Array BuildLocalInfo(Chunk chunk)
        {
            var array = new Array(chunk.LocalCount);

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

                array[i] = local;
            }

            return array;
        }

        private static Array BuildParameterInfo(Chunk chunk)
        {
            var array = new Array(chunk.ParameterCount);

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

                array[i] = param;
            }

            return array;
        }
    }
}