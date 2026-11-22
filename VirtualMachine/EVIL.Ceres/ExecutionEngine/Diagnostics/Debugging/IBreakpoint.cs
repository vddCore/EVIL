namespace EVIL.Ceres.ExecutionEngine.Diagnostics.Debugging;

public interface IBreakpoint
{
    int Id { get; }
    string FilePath { get; }
    int Line { get; }
    int Column { get; }
    bool Enabled { get; set; }
    bool IsVerified { get; }
    string? Condition { get; }
    string? HitCondition { get; }
    int HitCount { get; }
    int BoundIP { get; }
    Chunk? BoundChunk { get; }
}
