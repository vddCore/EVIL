namespace EVIL.Ceres.ExecutionEngine.Diagnostics.Disassembly;

using System;
using System.Collections.Generic;
using System.Linq;
using EVIL.Ceres.ExecutionEngine.Diagnostics.Debugging;

public sealed class Instruction
{
    public required int Offset { get; init; }
    public required int Length { get; init; }
    public required OpCode OpCode { get; init; }
    public required string Mnemonic { get; init; }
    public required IReadOnlyList<InstructionOperand> Operands { get; init; }
    public required ReadOnlyMemory<byte> RawBytes { get; init; }

    public string? Label { get; internal set; }
    public int? TargetOffset { get; init; }
    public string? SymbolicAnnotation { get; internal set; }
    public StatementDebugRecord? Statement { get; init; }
    public int Line { get; init; } = -1;

    public bool IsJumpTarget => !string.IsNullOrEmpty(Label);

    public bool IsBranch => OpCode is OpCode.JUMP
                                   or OpCode.FJMP
                                   or OpCode.TJMP
                                   or OpCode.NJMP
                                   or OpCode.VJMP;

    public bool IsReturn => OpCode is OpCode.RET or OpCode.YRET;
    public bool IsCall => OpCode is OpCode.INVOKE or OpCode.TAILINVOKE;
    public bool IsThrow => OpCode is OpCode.THROW;
    public bool IsYield => OpCode is OpCode.YIELD;

    public override string ToString()
    {
        var operandsStr = Operands.Count > 0
            ? " " + string.Join(" ", Operands.Select(o => o.FormattedText))
            : string.Empty;

        var annotStr = !string.IsNullOrEmpty(SymbolicAnnotation)
            ? $" ({SymbolicAnnotation})"
            : string.Empty;

        return $"{Offset:X8}: {Mnemonic}{operandsStr}{annotStr}";
    }
}
