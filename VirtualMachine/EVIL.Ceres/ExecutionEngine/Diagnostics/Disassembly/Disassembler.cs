namespace EVIL.Ceres.ExecutionEngine.Diagnostics.Disassembly;

using System.IO;
using EVIL.Ceres.ExecutionEngine.Diagnostics.Disassembly.Rendering;

public static class Disassembler
{
    public static void Disassemble(
        Chunk chunk,
        TextWriter output,
        DisassemblyOptions? options = null,
        int indentLevel = 0)
    {
        options ??= new DisassemblyOptions();
        var disasm = InstructionDecoder.Decode(chunk);

        var formatOptions = new DisassemblyFormatOptions
        {
            ShowAddresses = true,
            AddressFormat = AddressFormat.Hex8,
            ShowRawBytes = false,
            ShowLineNumbers = options.WriteLineNumbersWhenAvailable,
            ShowSourceInterleaving = false,
            ShowSymbols = options.WriteLocalNames 
                          || options.WriteParameterNames 
                          || options.WriteClosureInfo,
            ShowRuntimeValues = false,
            ShowBranchLabels = false,
            ShowBranchOffsets = false,
            ShowChunkHeader = true,
            IncludeSubChunks = true,
            IndentLevel = indentLevel,
            ColorMode = ColorMode.Never
        };

        DisassemblyFormatter.FormatChunk(disasm, output, formatOptions);
    }
}
