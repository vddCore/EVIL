namespace EVIL.Grammar.AST.Diagnostics;

using EVIL.Grammar.AST.Base;

public sealed class DiagnosticNode : Statement
{
    public string? ErrorMessage { get; }

    public DiagnosticNode(string? errorMessage = null)
    {
        ErrorMessage = errorMessage;
    }
}
