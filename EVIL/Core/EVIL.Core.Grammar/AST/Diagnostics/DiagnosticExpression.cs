namespace EVIL.Core.Grammar.AST.Diagnostics;

using EVIL.Core.Grammar.AST.Base;

public sealed class DiagnosticExpression : Expression
{
    public string? ErrorMessage { get; }

    public DiagnosticExpression(string? errorMessage = null)
    {
        ErrorMessage = errorMessage;
    }
}
