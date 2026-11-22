namespace EVIL.Ceres.ExecutionEngine.Diagnostics.Disassembly;

public enum OperandKind
{
    None,
    ImmediateByte,
    ImmediateInt32,
    ImmediateInt64,
    ImmediateDouble,
    StringPoolIndex,
    SubChunkIndex,
    LocalSlot,
    ArgumentSlot,
    ClosureSlot,
    JumpTargetAddress,
    SwitchTableRef,
    ProtectedBlockId
}
