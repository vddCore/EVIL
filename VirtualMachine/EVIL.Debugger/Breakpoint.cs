namespace EVIL.Debugger;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.Diagnostics.Debugging;

public sealed class Breakpoint : IBreakpoint
{
    public int Id { get; init; }
    public string FilePath { get; init; } = string.Empty;
    public int Line { get; init; }
    public int Column { get; init; }
    public string? Condition { get; init; }
    public string? HitCondition { get; init; }
    public int HitCount { get; internal set; }
    public bool Enabled { get; set; } = true;

    public bool IsVerified { get; internal set; }
    public int BoundIP { get; internal set; } = -1;
    public Chunk? BoundChunk { get; internal set; }

    public bool MatchesHitCondition()
    {
        if (string.IsNullOrWhiteSpace(HitCondition))
            return true;

        var trimmed = HitCondition.Trim();
        if (trimmed.StartsWith('%'))
        {
            if (int.TryParse(trimmed[1..].Trim(), out var modulo) && modulo > 0)
                return HitCount % modulo == 0;
        }
        else if (trimmed.StartsWith(">="))
        {
            if (int.TryParse(trimmed[2..].Trim(), out var target))
                return HitCount >= target;
        }
        else if (trimmed.StartsWith('>'))
        {
            if (int.TryParse(trimmed[1..].Trim(), out var target))
                return HitCount > target;
        }
        else if (trimmed.StartsWith("=="))
        {
            if (int.TryParse(trimmed[2..].Trim(), out var target))
                return HitCount == target;
        }
        else if (int.TryParse(trimmed, out var target))
        {
            return HitCount >= target;
        }

        return true;
    }

    public override string ToString()
        => $"Breakpoint #{Id} at {FilePath}:{Line}{(Column > 0 ? $":{Column}" : "")} " +
           $"({(IsVerified ? $"verified @ IP: 0x{BoundIP:X4}" : "pending")})";
}
