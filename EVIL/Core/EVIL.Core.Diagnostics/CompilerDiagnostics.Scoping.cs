namespace EVIL.Core.Diagnostics;

public static partial class CompilerDiagnostics
{
    public static readonly DiagnosticDescriptor DuplicateSymbolInScope = new(
        "EV2001", "Duplicate Symbol in Scope",
        "The symbol '{0}' already exists in this scope and is a {1} (previously defined on line {2}, column {3}).",
        DiagnosticCategory.Semantic, DiagnosticSeverity.Error
    );

    public static readonly DiagnosticDescriptor AttemptToWriteReadOnly = new(
        "EV2002", "Write to Read-Only Symbol",
        "Attempt to set a value of a read-only {0} '{1}' (defined on line {2}).",
        DiagnosticCategory.Semantic, DiagnosticSeverity.Error
    );
}