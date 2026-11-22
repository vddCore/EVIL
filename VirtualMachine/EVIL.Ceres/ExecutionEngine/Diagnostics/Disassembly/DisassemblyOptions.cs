namespace EVIL.Ceres.ExecutionEngine.Diagnostics.Disassembly;

public class DisassemblyOptions
{
    public bool WriteLineNumbersWhenAvailable { get; set; } = true;
    public bool WriteLocalNames { get; set; } = true;
    public bool WriteParameterNames { get; set; } = true;
    public bool WriteClosureInfo { get; set; } = true;
    public bool WriteSubChunkNames { get; set; } = true;
    public bool WriteLabelAddresses { get; set; } = true;
    public bool WriteNumericConstants { get; set; } = true;
    public bool WriteStringConstants { get; set; } = true;
}