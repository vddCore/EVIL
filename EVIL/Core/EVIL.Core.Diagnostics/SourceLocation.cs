namespace EVIL.Core.Diagnostics;

public readonly record struct SourceLocation(
    string? FilePath,
    int Line,
    int Column,
    int EndLine = 0,
    int EndColumn = 0,
    int Offset = 0,
    int Length = 0
)
{
    public static readonly SourceLocation None = new(null, 0, 0);

    public bool HasLocation => Line > 0 || Column > 0 || !string.IsNullOrEmpty(FilePath);
    public bool IsSpan => EndLine > 0 && EndColumn > 0;

    public override string ToString()
    {
        if (string.IsNullOrEmpty(FilePath))
        {
            return Line > 0 
                ? (Column > 0 ? $"({Line},{Column})" : $"({Line})")
                : string.Empty;
        }

        return Line > 0
            ? (Column > 0 
                ? $"{FilePath}({Line},{Column})" 
                : $"{FilePath}({Line})"
            ) : FilePath;
    }
}
