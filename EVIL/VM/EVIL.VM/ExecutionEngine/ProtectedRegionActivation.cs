namespace EVIL.VM.ExecutionEngine;

using EVIL.VM.ExecutionEngine.Diagnostics;

public struct ProtectedRegionActivation(
    int regionId,
    BlockProtectionInfo region,
    int evaluationStackDepth)
{
    public int RegionId { get; } = regionId;
    public BlockProtectionInfo Region { get; } = region;
    public int EvaluationStackDepth { get; } = evaluationStackDepth;
    public ProtectedRegionPhase Phase { get; private set; } = ProtectedRegionPhase.Body;

    public void BeginHandlerTransfer()
        => Phase = ProtectedRegionPhase.Handler;
}