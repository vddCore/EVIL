namespace EVIL.FE.REPL.SyntaxHighlighting;

public sealed record TokenStyle(string Name)
{
    public static TokenStyle None { get; } = new(nameof(None));
    public static TokenStyle Keyword { get; } = new(nameof(Keyword));
    public static TokenStyle Type { get; } = new(nameof(Type));
    public static TokenStyle Literal { get; } = new(nameof(Literal));
    public static TokenStyle Number { get; } = new(nameof(Number));
    public static TokenStyle String { get; } = new(nameof(String));
    public static TokenStyle Operator { get; } = new(nameof(Operator));
    public static TokenStyle Comment { get; } = new(nameof(Comment));
}