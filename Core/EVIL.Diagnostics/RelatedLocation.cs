namespace EVIL.Diagnostics;

public sealed record RelatedLocation(
    SourceLocation Location, 
    string Message
);
