namespace EVIL.Core.Diagnostics;

public static partial class CompilerDiagnostics
{
    public static readonly DiagnosticDescriptor KeyedExpansionTableExpected = new(
        "EV3001", "Keyed Expansion Table Expected", "The target table expression of a with-expression must be keyed.",
        DiagnosticCategory.Type, DiagnosticSeverity.Error
    );
}