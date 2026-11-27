namespace EVIL.VM.ExecutionEngine.Marshaling;

using System;

public sealed class MarshalingException : InvalidOperationException
{
    public MarshalingDirection Direction { get; }
    public string SourceType { get; }
    public Type? TargetType { get; }

    internal MarshalingException(
        MarshalingDirection direction,
        string sourceType,
        Type? targetType,
        string message,
        Exception? innerException = null)
        : base(FormatMessage(direction, sourceType, targetType, message), innerException)
    {
        Direction = direction;
        SourceType = sourceType;
        TargetType = targetType;
    }

    private static string FormatMessage(
        MarshalingDirection direction,
        string sourceType,
        Type? targetType,
        string message)
    {
        return direction switch
        {
            MarshalingDirection.ClrToEvil =>
                $"Failed to marshal CLR type '{sourceType}' to an EVIL value: {message}",
            
            MarshalingDirection.EvilToClr =>
                $"Failed to marshal EVIL type '{sourceType}' to CLR type '{targetType?.FullName ?? targetType?.Name}': {message}",
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }
}