namespace EVIL.CommonTypes.Diagnostics;

public static partial class CompilerDiagnostics
{
    public static readonly DiagnosticDescriptor RedundantSelfParameter = new(
        "EV8001", "Redundant Self Parameter",
        "An explicit 'self' parameter is not required for this function.",
        DiagnosticCategory.Inspection, DiagnosticSeverity.Warning
    );

    public static readonly DiagnosticDescriptor NoDefaultByArm = new(
        "EV8002", "No Default By Arm",
        "By-expression has no default arm and will always return 'nil' if input matching fails. " +
        "Consider adding a default arm using 'else' keyword.",
        DiagnosticCategory.Inspection, DiagnosticSeverity.Warning
    );

    public static readonly DiagnosticDescriptor UnusedVariable = new(
        "EV8003", "Unused Variable",
        "The local variable '{0}' is declared but its value is never used.",
        DiagnosticCategory.Inspection, DiagnosticSeverity.Warning
    );

    public static readonly DiagnosticDescriptor UnreachableCode = new(
        "EV8004", "Unreachable Code",
        "Unreachable code detected following '{0}' statement.",
        DiagnosticCategory.Inspection, DiagnosticSeverity.Warning
    );

    public static readonly DiagnosticDescriptor CatchlessTry = new(
        "EV8005", "Catchless Try Statement",
        "Try statement has no catch handler and will silently discard errors.",
        DiagnosticCategory.Inspection, DiagnosticSeverity.Warning
    );

    public static readonly DiagnosticDescriptor TableDuplicateKey = new(
        "EV8006", "Duplicate Table Key",
        "Duplicate table key '{0}' detected. Later definition overwrites earlier value.",
        DiagnosticCategory.Inspection, DiagnosticSeverity.Warning
    );
}