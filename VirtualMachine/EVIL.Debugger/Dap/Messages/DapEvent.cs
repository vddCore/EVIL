namespace EVIL.Debugger.Dap.Messages;

using System.Text.Json.Serialization;

public class DapEvent : DapProtocolMessage
{
    [JsonPropertyName("event")]
    public string Event { get; set; }

    [JsonPropertyName("body")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Body { get; set; }

    public DapEvent(string eventName, object? body = null)
    {
        Type = "event";
        
        Event = eventName;
        Body = body;
    }
}