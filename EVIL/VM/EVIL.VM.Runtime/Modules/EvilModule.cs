namespace EVIL.VM.Runtime.Modules;

using System.Collections.Generic;
using System.Linq;
using EVIL.Core.Common.TypeSystem;
using EVIL.Core.Diagnostics;
using EVIL.VM.ExecutionEngine.Collections;
using EVIL.VM.ExecutionEngine.Concurrency;
using EVIL.VM.ExecutionEngine.Diagnostics;
using EVIL.VM.ExecutionEngine.TypeSystem;
using EVIL.VM.Runtime.Extensions;
using EVIL.VM.TranslationEngine;
using EVIL.VM.TranslationEngine.Diagnostics;
using static EVIL.Core.Common.TypeSystem.DynamicValueType;

public sealed class EvilModule : RuntimeModule
{
    public override string FullyQualifiedName => "evil";

    private static readonly Table _severities = new Table
    {
        { "VERBOSE", (int)CompilerMessageSeverity.Verbose },
        { "WARNING", (int)CompilerMessageSeverity.Warning },
        { "FATAL", (int)CompilerMessageSeverity.Fatal },
        { "INTERNAL_FAILURE", (int)CompilerMessageSeverity.InternalFailure },
    }.Freeze();

    private static readonly Table _diagnosticSeverities = new Table
    {
        { "HIDDEN", (int)DiagnosticSeverity.Hidden },
        { "INFO", (int)DiagnosticSeverity.Info },
        { "WARNING", (int)DiagnosticSeverity.Warning },
        { "ERROR", (int)DiagnosticSeverity.Error },
        { "FATAL", (int)DiagnosticSeverity.Fatal },
        { "INTERNAL_ERROR", (int)DiagnosticSeverity.InternalError },
    }.Freeze();

    [RuntimeModuleGetter("diagnostic.severity")]
    [EvilDocProperty(
        EvilDocPropertyMode.Get,
        "Retrieves a table mapping diagnostic severity names to numeric representations."
    )]
    private static DynamicValue DiagnosticSeverityValues(DynamicValue _)
        => _diagnosticSeverities;

    [RuntimeModuleGetter("compiler.severity")]
    [EvilDocProperty(
        EvilDocPropertyMode.Get,
        "Retrieves a table mapping error severity names to numeric representation fo compiler error severity.  \n" +
        "```\n" +
        "{\n" +
        "  VERBOSE: 0,\n" +
        "  WARNING: 1,\n" +
        "  FATAL: 2,\n" +
        "  INTERNAL_FAILURE: 3\n" +
        "}\n" +
        "```\n"
    )]
    private static DynamicValue CompilerErrorSeverity(DynamicValue _)
        => _severities;

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
    private static DynamicValue Compile(Fiber fiber, params DynamicValue[] args)
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
                { "chunk", rootChunk },
                { "diagnostics", DiagnosticsToDynamicValue(compiler.Log.Diagnostics) },
                { "log", compiler.Log.ToDynamicValue() }
            };
        }
        catch (CompilerException e)
        {
            return new Table
            {
                { "success", false },
                { "message", e.Message },
                { "diagnostics", DiagnosticsToDynamicValue(e.Log.Diagnostics) },
                { "log", e.Log.ToDynamicValue() }
            };
        }
    }

    private static Array DiagnosticsToDynamicValue(IEnumerable<Diagnostic> diagnostics)
    {
        var list = diagnostics.ToList();
        var array = new Array(list.Count);

        for (var i = 0; i < list.Count; i++)
        {
            var d = list[i];
            var table = new Table
            {
                { "id", d.Id },
                { "title", d.Descriptor.Title },
                { "severity", (int)d.Severity },
                { "severity_name", d.Severity.ToString() },
                { "category", d.Descriptor.Category.ToString() },
                { "message", d.Message },
                { "file_name", d.Location.FilePath ?? DynamicValue.Nil },
                { "line", d.Location.Line },
                { "column", d.Location.Column },
                { "end_line", d.Location.EndLine },
                { "end_column", d.Location.EndColumn },
                { "offset", d.Location.Offset },
                { "length", d.Location.Length }
            };

            if (d.RelatedLocations is { Count: > 0 })
            {
                var relArray = new Array(d.RelatedLocations.Count);
                for (var j = 0; j < d.RelatedLocations.Count; j++)
                {
                    var rel = d.RelatedLocations[j];
                    relArray[j] = new Table
                    {
                        { "file_name", rel.Location.FilePath ?? DynamicValue.Nil },
                        { "line", rel.Location.Line },
                        { "column", rel.Location.Column },
                        { "message", rel.Message }
                    };
                }
                table["related_locations"] = relArray;
            }
            else
            {
                table["related_locations"] = new Array(0);
            }

            if (d.SuggestedFixes is { Count: > 0 })
            {
                var fixArray = new Array(d.SuggestedFixes.Count);
                for (var j = 0; j < d.SuggestedFixes.Count; j++)
                {
                    var fix = d.SuggestedFixes[j];
                    fixArray[j] = new Table
                    {
                        { "title", fix.Title },
                        { "replacement", fix.ReplacementText }
                    };
                }
                table["code_fixes"] = fixArray;
            }
            else
            {
                table["code_fixes"] = new Array(0);
            }

            array[i] = table;
        }

        return array;
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

            if (chunk.ParameterInitializers.TryGetValue(i, out var initializer))
            {
                param["default_value"] = initializer;
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