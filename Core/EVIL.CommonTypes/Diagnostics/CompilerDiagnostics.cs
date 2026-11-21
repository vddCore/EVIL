namespace EVIL.CommonTypes.Diagnostics;

public static class CompilerDiagnostics
{
    // Lexical (EV0xxx)
    public static readonly DiagnosticDescriptor UnterminatedString = new(
        "EV0100", "Unterminated String", "Unterminated string.",
        DiagnosticCategory.Lexical, DiagnosticSeverity.Error
    );

    public static readonly DiagnosticDescriptor MalformedNumericConstant = new(
        "EV0101", "Malformed Numeric Constant", "Malformed numeric constant '{0}'.",
        DiagnosticCategory.Lexical, DiagnosticSeverity.Error
    );

    public static readonly DiagnosticDescriptor UnexpectedCharacter = new(
        "EV0102", "Unexpected Character", "Character '{0}' was unexpected at this time.",
        DiagnosticCategory.Lexical, DiagnosticSeverity.Error
    );

    public static readonly DiagnosticDescriptor UnterminatedInterpolatedExpression = new(
        "EV0103", "Unterminated Interpolated Expression", "Unterminated expression in interpolated string.",
        DiagnosticCategory.Lexical, DiagnosticSeverity.Error
    );

    // Syntax (EV1xxx)
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

    // Declarations & Scoping (EV2xxx)
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

    // Expressions & Types (EV3xxx)
    public static readonly DiagnosticDescriptor KeyedExpansionTableExpected = new(
        "EV3001", "Keyed Expansion Table Expected", "The target table expression of a with-expression must be keyed.",
        DiagnosticCategory.Type, DiagnosticSeverity.Error
    );

    // Bytecode & Codegen (EV4xxx)
    public static readonly DiagnosticDescriptor IllegalAssignmentTarget = new(
        "EV4001", "Illegal Assignment Target", "Illegal assignment target.",
        DiagnosticCategory.Codegen, DiagnosticSeverity.Error
    );

    public static readonly DiagnosticDescriptor IllegalIncrementationTarget = new(
        "EV4002", "Illegal Incrementation Target", "Illegal incrementation target.",
        DiagnosticCategory.Codegen, DiagnosticSeverity.Error
    );

    public static readonly DiagnosticDescriptor IllegalDecrementationTarget = new(
        "EV4003", "Illegal Decrementation Target", "Illegal decrementation target.",
        DiagnosticCategory.Codegen, DiagnosticSeverity.Error
    );

    public static readonly DiagnosticDescriptor SelfUsedInSelfUnawareFunction = new(
        "EV4004", "Self in Self-Unaware Function", "Attempt to use 'self' in a self-unaware function.",
        DiagnosticCategory.Codegen, DiagnosticSeverity.Error
    );

    public static readonly DiagnosticDescriptor TooManyInitializersForConstSizeArray = new(
        "EV4005", "Too Many Initializers", "Attempt to initialize a constant size array with too many entries.",
        DiagnosticCategory.Codegen, DiagnosticSeverity.Error
    );

    public static readonly DiagnosticDescriptor FnStatementRedefinedExistingChunk = new(
        "EV4006", "Function Redefined",
        "Named function '{0}' defined on line {1} is re-defining a previously defined function of the same name in {2} on line {3}.",
        DiagnosticCategory.Codegen, DiagnosticSeverity.Warning
    );

    public static readonly DiagnosticDescriptor FnTargetedStatementRedefinedExistingChunk = new(
        "EV4007", "Targeted Function Redefined",
        "Targeted function '{0}' defined on line {1} has re-defined a previously defined function of the same name in {2} on line {3}.",
        DiagnosticCategory.Codegen, DiagnosticSeverity.Warning
    );

    public static readonly DiagnosticDescriptor FnIndexedStatementRedefinedExistingChunk = new(
        "EV4008", "Indexed Function Redefined",
        "Indexed function '{0}' defined on line {1} has re-defined a previously defined function of the same name in {2} on line {3}.",
        DiagnosticCategory.Codegen, DiagnosticSeverity.Warning
    );

    public static readonly DiagnosticDescriptor MissingErrorInformation = new(
        "EV4009", "Missing Error Information", "An error expression must have at least an implicit message constant.",
        DiagnosticCategory.Codegen, DiagnosticSeverity.Error
    );

    public static readonly DiagnosticDescriptor InternalCompilerError = new(
        "EV4999", "Internal Compiler Error", "Internal compiler error: {0}",
        DiagnosticCategory.Codegen, DiagnosticSeverity.InternalError
    );

    // Quality, Style & Inspections (EV8xxx)
    public static readonly DiagnosticDescriptor RedundantSelfParameter = new(
        "EV8001", "Redundant Self Parameter",
        "An explicit 'self' parameter is not required for this function.",
        DiagnosticCategory.Inspection, DiagnosticSeverity.Warning
    );

    public static readonly DiagnosticDescriptor NoDefaultByArm = new(
        "EV8002", "No Default By Arm",
        "By-expression has no default arm and will always return 'nil' if input matching fails. Consider adding a default arm using 'else' keyword.",
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
}
