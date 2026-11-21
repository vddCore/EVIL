namespace EVIL.CommonTypes.Diagnostics;

public sealed record CodeFix(
    string Title, 
    string ReplacementText,
    SourceLocation TargetSpan
);
