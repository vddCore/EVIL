namespace EVIL.VM.ExecutionEngine.Diagnostics;

public sealed record BlockProtectionInfo
{
    public int BodyStartAddress { get; internal set; }
    public int BodyEndAddress { get; internal set; }
    public int HandlerAddress { get; internal set; }
}