namespace EVIL.Ceres.ExecutionEngine.Diagnostics.Debugging;

public sealed record StatementDebugRecord(
    int IP,
    int Line,
    int Column,
    int EndLine,
    int EndColumn,
    bool IsStepBoundary = true
);
