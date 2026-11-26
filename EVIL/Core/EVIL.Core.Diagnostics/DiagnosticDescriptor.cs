namespace EVIL.Core.Diagnostics;

using System.Collections.Generic;

public sealed record DiagnosticDescriptor(
    string Id,
    string Title,
    string MessageFormat,
    DiagnosticCategory Category,
    DiagnosticSeverity DefaultSeverity,
    string? HelpLinkUri = null,
    string? Description = null,
    params string[] CustomTags
)
{
    public Diagnostic Create(SourceLocation location, params object[] messageArgs)
    {
        var formattedMessage = messageArgs is { Length: > 0 }
            ? string.Format(MessageFormat, messageArgs)
            : MessageFormat;

        return new Diagnostic(
            this,
            location,
            DefaultSeverity,
            formattedMessage,
            messageArgs
        );
    }

    public Diagnostic Create(
        SourceLocation location,
        IReadOnlyList<RelatedLocation>? relatedLocations,
        params object[] messageArgs)
    {
        var formattedMessage = messageArgs is { Length: > 0 }
            ? string.Format(MessageFormat, messageArgs)
            : MessageFormat;

        return new Diagnostic(
            this, 
            location, 
            DefaultSeverity, 
            formattedMessage,
            messageArgs,
            relatedLocations
        );
    }

    public Diagnostic CreateWithSeverity(
        DiagnosticSeverity severity,
        SourceLocation location,
        params object[] messageArgs)
    {
        var formattedMessage = messageArgs is { Length: > 0 }
            ? string.Format(MessageFormat, messageArgs)
            : MessageFormat;

        return new Diagnostic(
            this,
            location,
            severity,
            formattedMessage,
            messageArgs
        );
    }
}
