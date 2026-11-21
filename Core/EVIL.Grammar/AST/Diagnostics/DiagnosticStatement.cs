namespace EVIL.Grammar.AST.Diagnostics;

using EVIL.Grammar.AST.Base;

public sealed class DiagnosticStatement : Statement
{
    public string? ErrorMessage { get; }

    public DiagnosticStatement(string? errorMessage = null)
    {
        ErrorMessage = errorMessage;
    }
}
