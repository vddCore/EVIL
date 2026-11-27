namespace EVIL.VM.Debugger.Dap.Messages;

using System.Collections.Generic;
using System.Text.Json.Serialization;

public sealed class SourceDto
{
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; set; }

    [JsonPropertyName("path")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Path { get; set; }

    [JsonPropertyName("sourceReference")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? SourceReference { get; set; }
}

public sealed class DisassembleArguments
{
    [JsonPropertyName("memoryReference")]
    public string MemoryReference { get; set; } = string.Empty;

    [JsonPropertyName("offset")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Offset { get; set; }

    [JsonPropertyName("instructionOffset")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? InstructionOffset { get; set; }

    [JsonPropertyName("instructionCount")]
    public int InstructionCount { get; set; } = 50;

    [JsonPropertyName("resolveSymbols")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? ResolveSymbols { get; set; }
}

public sealed class DisassembledInstructionDto
{
    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("instructionBytes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? InstructionBytes { get; set; }

    [JsonPropertyName("instruction")]
    public string Instruction { get; set; } = string.Empty;

    [JsonPropertyName("symbol")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Symbol { get; set; }

    [JsonPropertyName("location")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public SourceDto? Location { get; set; }

    [JsonPropertyName("line")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Line { get; set; }

    [JsonPropertyName("column")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Column { get; set; }

    [JsonPropertyName("endLine")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? EndLine { get; set; }

    [JsonPropertyName("endColumn")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? EndColumn { get; set; }
}

public sealed class DisassembleResponseBody
{
    [JsonPropertyName("instructions")]
    public List<DisassembledInstructionDto> Instructions { get; set; } = [];
}
