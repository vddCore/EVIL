namespace EVIL.Intermediate
{
    public class DisassemblerOptions
    {
        public bool EmitLocalTable { get; set; } = true;
        public bool EmitExternTable { get; set; } = true;
        public bool EmitFunctionParameters { get; set; } = true;
        
        public bool EmitLocalHints { get; set; } = true;
        public bool EmitGlobalHints { get; set; } = true;
        public bool EmitFunctionHints { get; set; } = true;
        public bool EmitExternHints { get; set; } = true;
        public bool EmitFunctionNames { get; set; } = true;
        public bool EmitParamTable { get; set; } = true;
        public bool EmitParameterHints { get; set; } = true;
    }
}