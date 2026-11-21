namespace EVIL.CommonTypes.Diagnostics;

public sealed record RelatedLocation(
    SourceLocation Location, 
    string Message
);
