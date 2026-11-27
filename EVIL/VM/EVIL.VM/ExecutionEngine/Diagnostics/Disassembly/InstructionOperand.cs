namespace EVIL.VM.ExecutionEngine.Diagnostics.Disassembly;

public readonly record struct InstructionOperand(
    OperandKind Kind,
    object RawValue,
    string FormattedText,
    string? ResolvedSymbol = null)
{
    public override string ToString() 
        => FormattedText;
}
