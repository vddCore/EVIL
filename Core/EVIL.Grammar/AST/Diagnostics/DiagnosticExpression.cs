namespace EVIL.Grammar.AST.Diagnostics;

using EVIL.Grammar.AST.Base;

public sealed class DiagnosticExpression : Expression
{
    public string? ErrorMessage { get; }

    public DiagnosticExpression(string? errorMessage = null)
    {
        ErrorMessage = errorMessage;
    }
}
