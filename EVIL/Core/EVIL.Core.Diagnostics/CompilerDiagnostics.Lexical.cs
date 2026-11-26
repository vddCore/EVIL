namespace EVIL.Core.Diagnostics;

public static partial class CompilerDiagnostics
{
    public static readonly DiagnosticDescriptor LexicalError = new(
        "EV0001", "Lexical Error", "{0}",
        DiagnosticCategory.Lexical, DiagnosticSeverity.Error
    );

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
}