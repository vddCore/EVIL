namespace EVIL.Core.Grammar.AST.Diagnostics;

using EVIL.Core.Grammar.AST.Base;

public sealed class DiagnosticNode : Statement
{
    public string? ErrorMessage { get; }

    public DiagnosticNode(string? errorMessage = null)
    {
        ErrorMessage = errorMessage;
    }
}
