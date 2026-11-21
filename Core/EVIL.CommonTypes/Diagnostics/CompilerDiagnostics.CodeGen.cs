namespace EVIL.CommonTypes.Diagnostics;

public static partial class CompilerDiagnostics
{
    public static readonly DiagnosticDescriptor IllegalAssignmentTarget = new(
        "EV4001", "Illegal Assignment Target", "Illegal assignment target.",
        DiagnosticCategory.CodeGen, DiagnosticSeverity.Error
    );

    public static readonly DiagnosticDescriptor IllegalIncrementationTarget = new(
        "EV4002", "Illegal Incrementation Target", "Illegal incrementation target.",
        DiagnosticCategory.CodeGen, DiagnosticSeverity.Error
    );

    public static readonly DiagnosticDescriptor IllegalDecrementationTarget = new(
        "EV4003", "Illegal Decrementation Target", "Illegal decrementation target.",
        DiagnosticCategory.CodeGen, DiagnosticSeverity.Error
    );

    public static readonly DiagnosticDescriptor SelfUsedInSelfUnawareFunction = new(
        "EV4004", "Self in Self-Unaware Function", "Attempt to use 'self' in a self-unaware function.",
        DiagnosticCategory.CodeGen, DiagnosticSeverity.Error
    );

    public static readonly DiagnosticDescriptor TooManyInitializersForConstSizeArray = new(
        "EV4005", "Too Many Initializers", "Attempt to initialize a constant size array with too many entries.",
        DiagnosticCategory.CodeGen, DiagnosticSeverity.Error
    );

    public static readonly DiagnosticDescriptor FnStatementRedefinedExistingChunk = new(
        "EV4006", "Function Redefined",
        "Named function '{0}' defined on line {1} is re-defining a previously defined function of the same name in {2} on line {3}.",
        DiagnosticCategory.CodeGen, DiagnosticSeverity.Warning
    );

    public static readonly DiagnosticDescriptor FnTargetedStatementRedefinedExistingChunk = new(
        "EV4007", "Targeted Function Redefined",
        "Targeted function '{0}' defined on line {1} has re-defined a previously defined function of the same name in {2} on line {3}.",
        DiagnosticCategory.CodeGen, DiagnosticSeverity.Warning
    );

    public static readonly DiagnosticDescriptor FnIndexedStatementRedefinedExistingChunk = new(
        "EV4008", "Indexed Function Redefined",
        "Indexed function '{0}' defined on line {1} has re-defined a previously defined function of the same name in {2} on line {3}.",
        DiagnosticCategory.CodeGen, DiagnosticSeverity.Warning
    );

    public static readonly DiagnosticDescriptor MissingErrorInformation = new(
        "EV4009", "Missing Error Information", "An error expression must have at least an implicit message constant.",
        DiagnosticCategory.CodeGen, DiagnosticSeverity.Error
    );

    public static readonly DiagnosticDescriptor InternalCompilerError = new(
        "EV4999", "Internal Compiler Error", "Internal compiler error: {0}",
        DiagnosticCategory.CodeGen, DiagnosticSeverity.InternalError
    );
}