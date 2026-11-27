namespace EVIL.VM.Debugger.Dap.Messages;

using System.Text.Json.Serialization;

public class DapProtocolMessage
{
    [JsonPropertyName("seq")]
    public int Seq { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}