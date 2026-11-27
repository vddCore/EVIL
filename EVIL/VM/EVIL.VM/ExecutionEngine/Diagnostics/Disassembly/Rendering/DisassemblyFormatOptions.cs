namespace EVIL.VM.ExecutionEngine.Diagnostics.Disassembly.Rendering;

using System;
using System.Collections.Generic;
using EVIL.VM.ExecutionEngine.Diagnostics.Debugging;

public sealed class DisassemblyFormatOptions
{
    public bool ShowAddresses { get; set; } = true;
    public AddressFormat AddressFormat { get; set; } = AddressFormat.HexPrefixed;
    public bool ShowRawBytes { get; set; } = false;
    public int MaxRawBytes { get; set; } = 5;

    public bool ShowLineNumbers { get; set; } = true;
    public bool ShowSourceInterleaving { get; set; } = true;
    public bool ShowSymbols { get; set; } = true;
    public bool ShowRuntimeValues { get; set; } = true;
    public bool ShowBranchLabels { get; set; } = true;
    public bool ShowBranchOffsets { get; set; } = true;

    public bool ShowChunkHeader { get; set; } = false;
    public bool IncludeSubChunks { get; set; } = false;
    public int IndentLevel { get; set; } = 0;

    public int? HighlightIP { get; set; }
    public IReadOnlyList<IBreakpoint>? Breakpoints { get; set; }
    public ScriptStackFrame? FrameContext { get; set; }
    public Func<string, string[]?>? SourceProvider { get; set; }

    public ColorMode ColorMode { get; set; } = ColorMode.Never;

    public static DisassemblyFormatOptions Default => new();

    public static DisassemblyFormatOptions ForDebugger(
        int? highlightIp = null,
        IReadOnlyList<IBreakpoint>? breakpoints = null,
        ScriptStackFrame? frameContext = null,
        Func<string, string[]?>? sourceProvider = null,
        bool showRawBytes = false)
    {
        return new DisassemblyFormatOptions
        {
            ShowAddresses = true,
            AddressFormat = AddressFormat.HexPrefixed,
            ShowRawBytes = showRawBytes,
            ShowSourceInterleaving = true,
            ShowSymbols = true,
            ShowRuntimeValues = true,
            ShowBranchLabels = true,
            ShowBranchOffsets = true,
            ShowChunkHeader = false,
            IncludeSubChunks = false,
            HighlightIP = highlightIp,
            Breakpoints = breakpoints,
            FrameContext = frameContext,
            SourceProvider = sourceProvider,
            ColorMode = ColorMode.Never
        };
    }
}
