namespace Ceres.ExecutionEngine.Diagnostics
{
    public record BlockProtectionInfo(
        int StartAddress,
        int Length,
        int HandlerAddress
    );
}