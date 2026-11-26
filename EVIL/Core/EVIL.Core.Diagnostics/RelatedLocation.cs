namespace EVIL.Core.Diagnostics;

public sealed record RelatedLocation(
    SourceLocation Location, 
    string Message
);
