namespace EVIL.Ceres.ExecutionEngine.Diagnostics
{
    public class BlockProtectionInfo
    {
        public int EnterLabelId { get; internal set; }
        public int StartAddress { get; internal set; }
        public int Length { get; internal set; }
        public int HandlerAddress { get; internal set; }
    }
}