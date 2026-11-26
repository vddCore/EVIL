namespace EVIL.VirtualMachine.ExecutionEngine.Diagnostics.Disassembly.Rendering;

internal sealed class AnsiPalette
{
    public string Reset { get; }
    public string Bold { get; }
    public string Dim { get; }
    public string IpMarker { get; }
    public string BpMarker { get; }
    public string Address { get; }
    public string Bytes { get; }
    public string Label { get; }
    public string OpCode { get; }
    public string BranchOpCode { get; }
    public string CallOpCode { get; }
    public string Immediate { get; }
    public string Symbol { get; }
    public string RuntimeValue { get; }
    public string Comment { get; }

    public AnsiPalette(
        string reset, string bold, string dim, string ipMarker, string bpMarker,
        string address, string bytes, string label, string opCode, string branchOpCode,
        string callOpCode, string immediate, string symbol, string runtimeValue, string comment)
    {
        Reset = reset;
        Bold = bold;
        Dim = dim;
        IpMarker = ipMarker;
        BpMarker = bpMarker;
        Address = address;
        Bytes = bytes;
        Label = label;
        OpCode = opCode;
        BranchOpCode = branchOpCode;
        CallOpCode = callOpCode;
        Immediate = immediate;
        Symbol = symbol;
        RuntimeValue = runtimeValue;
        Comment = comment;
    }

    public static readonly AnsiPalette Colored = new(
        reset: "\u001b[0m",
        bold: "\u001b[1m",
        dim: "\u001b[90m",
        ipMarker: "\u001b[1;32m",
        bpMarker: "\u001b[1;31m",
        address: "\u001b[36m",
        bytes: "\u001b[90m",
        label: "\u001b[1;33m",
        opCode: "\u001b[1m",
        branchOpCode: "\u001b[1;35m",
        callOpCode: "\u001b[1;34m",
        immediate: "\u001b[33m",
        symbol: "\u001b[32m",
        runtimeValue: "\u001b[1;36m",
        comment: "\u001b[90m"
    );

    public static readonly AnsiPalette Monochrome = new(
        reset: "", bold: "", dim: "", ipMarker: "", bpMarker: "",
        address: "", bytes: "", label: "", opCode: "", branchOpCode: "",
        callOpCode: "", immediate: "", symbol: "", runtimeValue: "", comment: ""
    );
}