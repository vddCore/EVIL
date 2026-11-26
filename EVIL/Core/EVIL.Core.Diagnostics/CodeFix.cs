namespace EVIL.Core.Diagnostics;

public sealed record CodeFix(
    string Title, 
    string ReplacementText,
    SourceLocation TargetSpan
);
