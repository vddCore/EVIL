namespace EVIL.VM.ExecutionEngine.Diagnostics.Disassembly.Rendering;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using EVIL.Core.Common.TypeSystem;
using EVIL.VM.ExecutionEngine.Diagnostics.Debugging;
using EVIL.VM.ExecutionEngine.TypeSystem;

public static class DisassemblyFormatter
{
    public static void FormatChunk(
        ChunkDisassembly disasm,
        TextWriter output,
        DisassemblyFormatOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(disasm);
        ArgumentNullException.ThrowIfNull(output);

        options ??= DisassemblyFormatOptions.Default;
        var palette = ResolvePalette(options.ColorMode);
        var indent = new string(' ', options.IndentLevel);

        if (options.ShowChunkHeader)
        {
            FormatChunkHeader(disasm.Chunk, output, indent);
        }

        var prevLine = -1;
        for (var i = 0; i < disasm.Instructions.Count; i++)
        {
            var inst = disasm.Instructions[i];
            FormatInstructionWithContext(inst, disasm, output, options, palette, indent, ref prevLine);
        }

        if (options.ShowChunkHeader)
        {
            output.WriteLine($"{indent}  }}");

            if (options.IncludeSubChunks && disasm.Chunk.SubChunks.Count > 0)
            {
                output.WriteLine();
                output.WriteLine($"{indent}  .SUBCHUNKS {{");
                for (var i = 0; i < disasm.Chunk.SubChunkCount; i++)
                {
                    var subChunk = disasm.Chunk.SubChunks[i];
                    var subDisasm = subChunk.Disassemble();
                    var subOptions = new DisassemblyFormatOptions
                    {
                        ShowAddresses = options.ShowAddresses,
                        AddressFormat = options.AddressFormat,
                        ShowRawBytes = options.ShowRawBytes,
                        MaxRawBytes = options.MaxRawBytes,
                        ShowLineNumbers = options.ShowLineNumbers,
                        ShowSourceInterleaving = options.ShowSourceInterleaving,
                        ShowSymbols = options.ShowSymbols,
                        ShowRuntimeValues = options.ShowRuntimeValues,
                        ShowBranchLabels = options.ShowBranchLabels,
                        ShowBranchOffsets = options.ShowBranchOffsets,
                        ShowChunkHeader = true,
                        IncludeSubChunks = options.IncludeSubChunks,
                        IndentLevel = options.IndentLevel + 4,
                        HighlightIP = options.HighlightIP,
                        Breakpoints = options.Breakpoints,
                        FrameContext = options.FrameContext,
                        SourceProvider = options.SourceProvider,
                        ColorMode = options.ColorMode
                    };
                    FormatChunk(subDisasm, output, subOptions);
                }

                output.WriteLine($"{indent}  }}");
            }

            output.WriteLine($"{indent}}}");
        }
    }

    public static void FormatInstructions(
        IReadOnlyList<Instruction> instructions,
        ChunkDisassembly disasm,
        TextWriter output,
        DisassemblyFormatOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(instructions);
        ArgumentNullException.ThrowIfNull(disasm);
        ArgumentNullException.ThrowIfNull(output);

        options ??= DisassemblyFormatOptions.Default;
        var palette = ResolvePalette(options.ColorMode);
        var indent = new string(' ', options.IndentLevel);
        var prevLine = -1;

        for (var i = 0; i < instructions.Count; i++)
        {
            FormatInstructionWithContext(
                instructions[i],
                disasm,
                output,
                options,
                palette,
                indent,
                ref prevLine
            );
        }
    }

    public static void FormatWindow(
        ChunkDisassembly disasm,
        TextWriter output,
        int centerIp,
        DisassemblyFormatOptions? options = null,
        int radiusBefore = 5,
        int radiusAfter = 5)
    {
        ArgumentNullException.ThrowIfNull(disasm);
        ArgumentNullException.ThrowIfNull(output);

        var window = disasm.GetWindow(
            centerIp,
            radiusBefore,
            radiusAfter
        );

        FormatInstructions(window, disasm, output, options);
    }

    public static void FormatRange(
        ChunkDisassembly disasm,
        TextWriter output,
        int startIp,
        int count,
        DisassemblyFormatOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(disasm);
        ArgumentNullException.ThrowIfNull(output);

        var slice = disasm.GetRange(startIp, count);
        FormatInstructions(slice, disasm, output, options);
    }

    public static void FormatInstruction(
        Instruction inst,
        ChunkDisassembly disasm,
        TextWriter output,
        DisassemblyFormatOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(inst);
        ArgumentNullException.ThrowIfNull(disasm);
        ArgumentNullException.ThrowIfNull(output);

        options ??= DisassemblyFormatOptions.Default;
        var palette = ResolvePalette(options.ColorMode);
        var indent = new string(' ', options.IndentLevel);
        var prevLine = -1;

        FormatInstructionWithContext(
            inst,
            disasm,
            output,
            options,
            palette,
            indent,
            ref prevLine
        );
    }

    private static void FormatInstructionWithContext(
        Instruction inst,
        ChunkDisassembly disasm,
        TextWriter output,
        DisassemblyFormatOptions options,
        AnsiPalette palette,
        string indent,
        ref int prevLine)
    {
        if (inst.Line > 0 && inst.Line != prevLine)
        {
            prevLine = inst.Line;
            FormatSourceContext(inst.Line, disasm.Chunk, output, options, palette, indent);
        }

        if (inst.IsJumpTarget && options.ShowBranchLabels)
        {
            output.WriteLine($"{indent}{palette.Label}{inst.Label}:{palette.Reset}");
        }

        var isCurrentIp = options.HighlightIP.HasValue && options.HighlightIP.Value == inst.Offset;
        var bp = FindBreakpoint(
            inst.Offset,
            disasm.Chunk,
            options.Breakpoints
        );

        string gutter;
        if (isCurrentIp && bp != null)
        {
            gutter = $"{palette.IpMarker}=>{palette.Reset}{palette.BpMarker}* {palette.Reset}";
        }
        else if (isCurrentIp)
        {
            gutter = $"{palette.IpMarker}=>  {palette.Reset}";
        }
        else if (bp != null)
        {
            gutter = $" {palette.BpMarker}*  {palette.Reset}";
        }
        else
        {
            gutter = "    ";
        }

        output.Write(
            options.ShowChunkHeader && options.HighlightIP == null
                ? $"{indent}    "
                : $"{indent}{gutter}"
        );

        if (options.ShowAddresses)
        {
            string addrStr = options.AddressFormat switch
            {
                AddressFormat.Hex4 => $"{inst.Offset:X4}: ",
                AddressFormat.Hex8 => $"{inst.Offset:X8}: ",
                _ => $"0x{inst.Offset:X8}: "
            };
            
            output.Write($"{palette.Address}{addrStr}{palette.Reset}");
        }

        if (options.ShowRawBytes)
        {
            var maxBytes = Math.Max(1, options.MaxRawBytes);
            var span = inst.RawBytes.Span;
            var sb = new StringBuilder();

            var printCount = Math.Min(span.Length, maxBytes);
            for (var b = 0; b < printCount; b++)
            {
                sb.Append($"{span[b]:X2} ");
            }

            if (span.Length > maxBytes)
            {
                sb.Append("+ ");
            }

            var totalWidth = (maxBytes * 3) + 2;
            var bytesFormatted = sb.ToString().PadRight(totalWidth);
            
            output.Write($"{palette.Bytes}{bytesFormatted}{palette.Reset}");
        }

        var opColor = palette.OpCode;
        if (inst.IsBranch)
        {
            opColor = palette.BranchOpCode;
        }
        else if (inst.IsCall || inst.IsReturn)
        {
            opColor = palette.CallOpCode;
        }

        var mnemonicPadded = inst.Mnemonic;
        if (inst.Operands.Count > 0 || !string.IsNullOrEmpty(inst.SymbolicAnnotation))
        {
            mnemonicPadded = mnemonicPadded.PadRight(14);
        }

        output.Write($"{opColor}{mnemonicPadded}{palette.Reset}");

        if (inst.Operands.Count > 0)
        {
            output.Write(" ");
            for (var opIdx = 0; opIdx < inst.Operands.Count; opIdx++)
            {
                if (opIdx > 0)
                {
                    output.Write(" ");
                }

                var operand = inst.Operands[opIdx];
                output.Write($"{palette.Immediate}{operand.FormattedText}{palette.Reset}");
            }
        }

        var annot = BuildAnnotation(inst, options, palette);
        if (!string.IsNullOrEmpty(annot))
        {
            output.Write($" {annot}");
        }

        if (bp != null && !options.ShowChunkHeader)
        {
            output.Write($" {palette.BpMarker}[BP #{bp.Id}]{palette.Reset}");
        }

        output.WriteLine();
    }

    private static void FormatSourceContext(
        int line,
        Chunk chunk,
        TextWriter output,
        DisassemblyFormatOptions options,
        AnsiPalette palette,
        string indent)
    {
        if (options is { ShowSourceInterleaving: false, ShowLineNumbers: false } && !options.ShowChunkHeader)
        {
            return;
        }

        string? sourceLine = null;
        if (options is { ShowSourceInterleaving: true, SourceProvider: not null })
        {
            var file = chunk.DebugDatabase.DefinedInFile;
            var lines = options.SourceProvider(file);
            if (lines != null && line >= 1 && line <= lines.Length)
            {
                sourceLine = lines[line - 1].Trim();
            }
        }

        if (!string.IsNullOrEmpty(sourceLine))
        {
            output.WriteLine(
                $"{indent}   {palette.Comment}; line {line}: " +
                $"{sourceLine}{palette.Reset}"
            );
        }
        else if (options.ShowChunkHeader)
        {
            output.WriteLine($"{indent}   line {line}:");
        }
        else if (options.ShowLineNumbers)
        {
            output.WriteLine(
                $"{indent}   {palette.Comment};" +
                $" line {line}:{palette.Reset}"
            );
        }
    }

    private static string? BuildAnnotation(
        Instruction inst,
        DisassemblyFormatOptions options,
        AnsiPalette palette)
    {
        if (!options.ShowSymbols)
        {
            return null;
        }

        if (options is { ShowRuntimeValues: true, FrameContext: not null })
        {
            var liveVal = TryGetRuntimeValueForInstruction(inst, options.FrameContext);
            if (liveVal != null)
            {
                var symName = !string.IsNullOrEmpty(inst.SymbolicAnnotation) ? inst.SymbolicAnnotation : null;
                if (symName != null)
                {
                    return $"{palette.Symbol}({symName} = " +
                           $"{palette.RuntimeValue}{liveVal}{palette.Symbol}){palette.Reset}";
                }

                return $"{palette.Symbol}(= {palette.RuntimeValue}{liveVal}{palette.Symbol}){palette.Reset}";
            }
        }

        if (inst is { IsBranch: true, TargetOffset: not null }
            && options is { ShowBranchOffsets: true, ShowChunkHeader: false })
        {
            var delta = inst.TargetOffset.Value - inst.Offset;
            var sign = delta >= 0 ? "+" : "";
            var targetLabel = $"L_{inst.TargetOffset.Value:X4}";
            return $"{palette.Comment}; to {targetLabel} ({sign}{delta} bytes){palette.Reset}";
        }

        return !string.IsNullOrEmpty(inst.SymbolicAnnotation)
            ? $"{palette.Symbol}({inst.SymbolicAnnotation}){palette.Reset}"
            : null;
    }

    private static string? TryGetRuntimeValueForInstruction(Instruction inst, ScriptStackFrame frame)
    {
        try
        {
            switch (inst.OpCode)
            {
                case OpCode.GETLOCAL_0:
                case OpCode.SETLOCAL_0:
                    return FormatFrameLocal(frame, 0);
                
                case OpCode.GETLOCAL_1:
                case OpCode.SETLOCAL_1:
                    return FormatFrameLocal(frame, 1);
                
                case OpCode.GETLOCAL_2:
                case OpCode.SETLOCAL_2:
                    return FormatFrameLocal(frame, 2);
                
                case OpCode.GETLOCAL_3:
                case OpCode.SETLOCAL_3:
                    return FormatFrameLocal(frame, 3);

                case OpCode.GETLOCAL_B:
                case OpCode.SETLOCAL_B:
                case OpCode.GETLOCAL:
                case OpCode.SETLOCAL:
                {
                    if (inst.Operands.Count > 0 && inst.Operands[0].RawValue is int localSlot)
                    {
                        return FormatFrameLocal(frame, localSlot);
                    }

                    break;
                }

                case OpCode.GETARG_0:
                case OpCode.SETARG_0:
                    return FormatFrameArg(frame, 0);
                
                case OpCode.GETARG_1:
                case OpCode.SETARG_1:
                    return FormatFrameArg(frame, 1);
                
                case OpCode.GETARG_2:
                case OpCode.SETARG_2:
                    return FormatFrameArg(frame, 2);
                
                case OpCode.GETARG_3:
                case OpCode.SETARG_3:
                    return FormatFrameArg(frame, 3);

                case OpCode.GETARG_B:
                case OpCode.SETARG_B:
                case OpCode.GETARG:
                case OpCode.SETARG:
                {
                    if (inst.Operands.Count > 0 && inst.Operands[0].RawValue is int argSlot)
                    {
                        return FormatFrameArg(frame, argSlot);
                    }

                    break;
                }
                
                case OpCode.GETCLOSURE_0:
                case OpCode.SETCLOSURE_0:
                    return FormatFrameUpvalue(frame, 0);
                
                case OpCode.GETCLOSURE_1:
                case OpCode.SETCLOSURE_1:
                    return FormatFrameUpvalue(frame, 1);
                
                case OpCode.GETCLOSURE_2:
                case OpCode.SETCLOSURE_2:
                    return FormatFrameUpvalue(frame, 2);
                
                case OpCode.GETCLOSURE_3:
                case OpCode.SETCLOSURE_3:
                    return FormatFrameUpvalue(frame, 3);

                case OpCode.GETCLOSURE_B:
                case OpCode.SETCLOSURE_B:
                case OpCode.GETCLOSURE:
                case OpCode.SETCLOSURE:
                {
                    if (inst.Operands.Count > 0 && inst.Operands[0].RawValue is int uvSlot)
                    {
                        return FormatFrameUpvalue(frame, uvSlot);
                    }

                    break;
                }
            }
        }
        catch
        {
            /* ignore */
        }

        return null;
    }

    private static string? FormatFrameLocal(ScriptStackFrame frame, int slot)
    {
        if (frame.Locals != null && slot >= 0 && slot < frame.Locals.Length)
        {
            return FormatDynamicValueCompact(frame.Locals[slot]);
        }

        return null;
    }

    private static string? FormatFrameArg(ScriptStackFrame frame, int slot)
    {
        if (frame.Arguments != null && slot >= 0 && slot < frame.Arguments.Length)
        {
            return FormatDynamicValueCompact(
                frame.Arguments[slot]
            );
        }

        return null;
    }

    private static string? FormatFrameUpvalue(ScriptStackFrame frame, int slot)
    {
        if (frame.Upvalues != null && slot >= 0 && slot < frame.Upvalues.Length)
        {
            return FormatDynamicValueCompact(frame.Upvalues[slot].Get());
        }

        return null;
    }

    private static string FormatDynamicValueCompact(DynamicValue value)
    {
        return value.Type switch
        {
            DynamicValueType.Nil => "nil",
            DynamicValueType.Boolean => value.Boolean.ToString().ToLowerInvariant(),
            DynamicValueType.Number => value.Number.ToString(CultureInfo.InvariantCulture),
            DynamicValueType.String => value.String != null ? $"\"{value.String}\"" : "\"\"",
            DynamicValueType.Table => $"[Table: {value.Table?.Length ?? 0} items]",
            DynamicValueType.Array => $"[Array: {value.Array?.Length ?? 0} items]",
            DynamicValueType.Fiber => $"[Fiber: {value.Fiber?.State}]",
            DynamicValueType.Chunk => $"[fn {value.Chunk?.Name ?? "anon"}]",
            DynamicValueType.NativeFunction => "[native fn]",
            DynamicValueType.TypeCode => value.TypeCode.ToString(),
            _ => value.ToString() ?? "<value>"
        };
    }

    private static IBreakpoint? FindBreakpoint(int ip, Chunk chunk, IReadOnlyList<IBreakpoint>? breakpoints)
    {
        if (breakpoints == null || breakpoints.Count == 0)
        {
            return null;
        }

        for (var i = 0; i < breakpoints.Count; i++)
        {
            var bp = breakpoints[i];
            if (bp.Enabled && bp.BoundIP == ip && (bp.BoundChunk == null || bp.BoundChunk == chunk))
            {
                return bp;
            }
        }

        return null;
    }

    private static void FormatChunkHeader(Chunk chunk, TextWriter output, string indent)
    {
        output.Write($"{indent}.CHUNK ");
        output.Write(chunk.Name);
        output.Write(" ");

        if (chunk is { HasDebugInfo: true, DebugDatabase.DefinedOnLine: > 0 })
        {
            output.Write($"(def. on line {chunk.DebugDatabase.DefinedOnLine}) ");
        }

        output.WriteLine("{");

        if (chunk.MayThrow)
        {
            output.WriteLine($"{indent} .MAY_THROW");
            output.WriteLine();
        }

        output.WriteLine($"{indent}  .LOCALS {chunk.LocalCount}");
        output.WriteLine($"{indent}  .CLOSRS {chunk.ClosureCount}");
        output.WriteLine($"{indent}  .PARAMS {chunk.ParameterCount}");

        for (var i = 0; i < chunk.ParameterCount; i++)
        {
            if (chunk.IsSelfAware && i == 0)
            {
                output.WriteLine($"{indent}    .PARAM SELF({i})");
                continue;
            }

            output.WriteLine(chunk.ParameterInitializers.TryGetValue(i, out var value)
                ? $"{indent}    .PARAM DO_INIT({i}): {value.ConvertToString().String}"
                : $"{indent}    .PARAM NO_INIT({i})"
            );
        }

        if (chunk.Attributes.Any())
        {
            output.WriteLine();
            DumpAttributes(chunk, output);
        }

        output.WriteLine();
        output.WriteLine($"{indent}  .TEXT {{");
    }

    private static void DumpAttributes(Chunk chunk, TextWriter output)
    {
        foreach (var attribute in chunk.Attributes)
        {
            output.Write($"  .ATTR {attribute.Name}");
            if (attribute.Values.Count > 0)
            {
                output.Write("(");
                output.Write(string.Join(", ", attribute.Values.Select(StringifyDynamicValue)));
                output.Write(")");
            }

            output.WriteLine();
        }
    }

    private static string StringifyDynamicValue(DynamicValue value)
    {
        if (value.Type == DynamicValueType.String)
        {
            return $"\"{value.String}\"";
        }

        return value.ConvertToString().String!;
    }

    private static AnsiPalette ResolvePalette(ColorMode mode)
    {
        if (mode == ColorMode.Always)
        {
            return AnsiPalette.Colored;
        }
        
        if (mode == ColorMode.Auto && !Console.IsOutputRedirected)
        {
            return AnsiPalette.Colored;
        }
        
        return AnsiPalette.Monochrome;
    }
}