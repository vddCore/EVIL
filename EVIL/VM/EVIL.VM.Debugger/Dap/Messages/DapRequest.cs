namespace EVIL.VM.Debugger.Dap.Messages;

using System.Text.Json.Serialization;

public class DapRequest : DapProtocolMessage
{
    [JsonPropertyName("command")]
    public string Command { get; set; } = string.Empty;

    [JsonPropertyName("arguments")]
    public System.Text.Json.JsonElement? Arguments { get; set; }
}