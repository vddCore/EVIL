namespace EVIL.Diagnostics;

public static partial class CompilerDiagnostics
{
    public static readonly DiagnosticDescriptor SyntaxError = new(
        "EV1000", "Syntax Error", "{0}",
        DiagnosticCategory.Syntax, DiagnosticSeverity.Error
    );

    public static readonly DiagnosticDescriptor UnexpectedToken = new(
        "EV1001", "Unexpected Token", "Expected {0}, got {1}.",
        DiagnosticCategory.Syntax, DiagnosticSeverity.Error
    );

    public static readonly DiagnosticDescriptor MissingSemicolon = new(
        "EV1002", "Missing Semicolon", "Expected ';' after statement.",
        DiagnosticCategory.Syntax, DiagnosticSeverity.Error
    );

    public static readonly DiagnosticDescriptor IncompatibleTableKey = new(
        "EV1003", "Incompatible Table Key", "{0}",
        DiagnosticCategory.Syntax, DiagnosticSeverity.Error
    );

    public static readonly DiagnosticDescriptor UnexpectedSyntaxNode = new(
        "EV1004", "Unexpected Syntax Node", "'{0}' was not expected at this time.",
        DiagnosticCategory.Syntax, DiagnosticSeverity.Error
    );

    public static readonly DiagnosticDescriptor BreakOutsideLoop = new(
        "EV1005", "Break Outside Loop", "Unexpected 'break' outside of a loop.",
        DiagnosticCategory.Syntax, DiagnosticSeverity.Error
    );

    public static readonly DiagnosticDescriptor SkipOutsideLoop = new(
        "EV1006", "Skip Outside Loop", "Unexpected 'skip' outside of a loop.",
        DiagnosticCategory.Syntax, DiagnosticSeverity.Error
    );

}