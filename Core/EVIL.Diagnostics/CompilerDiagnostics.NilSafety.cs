namespace EVIL.Diagnostics;

public static partial class CompilerDiagnostics
{
    public static readonly DiagnosticDescriptor DangerousUnsafeDerefAfterElvis = new(
        "EV8007", "Dangerous Unsafe Dereference After Safe Navigation",
        "Unconditional member access '{0}' follows a conditional navigation and will throw at runtime if the target is nil.",
        DiagnosticCategory.Inspection, DiagnosticSeverity.Warning
    );

    public static readonly DiagnosticDescriptor RedundantSafeNavigation = new(
        "EV8008", "Redundant Safe Navigation",
        "Safe navigation operator '?.' is redundant because the target expression '{0}' is guaranteed non-nil.",
        DiagnosticCategory.Inspection, DiagnosticSeverity.Warning
    );

    public static readonly DiagnosticDescriptor InvalidConditionalAssignment = new(
        "EV8009", "Invalid Conditional Assignment Target",
        "Conditional assignment target '{0}' is invalid: target cannot contain unconditional dereferences after safe navigation or target an immutable literal.",
        DiagnosticCategory.Inspection, DiagnosticSeverity.Error
    );

    public static readonly DiagnosticDescriptor RedundantNilCoalesce = new(
        "EV8010", "Redundant Nil Coalescing",
        "Nil-coalescing operator '??' is redundant because the left operand '{0}' is guaranteed non-nil.",
        DiagnosticCategory.Inspection, DiagnosticSeverity.Warning
    );

    public static readonly DiagnosticDescriptor UncheckedNilInvocation = new(
        "EV8011", "Unchecked Nil Invocation",
        "Invocation of '{0}' may fail at runtime because the callee is conditionally dereferenced and may be nil.",
        DiagnosticCategory.Inspection, DiagnosticSeverity.Warning
    );

    public static readonly DiagnosticDescriptor NilComparisonTypeMismatch = new(
        "EV8012", "Nil Comparison Type Mismatch",
        "Relational comparison operator '{0}' cannot be used with 'nil'. Only equality operators ('==' and '!=') are supported for nil comparisons.",
        DiagnosticCategory.Inspection, DiagnosticSeverity.Warning
    );

    public static readonly DiagnosticDescriptor NilBranchExhaustiveness = new(
        "EV8013", "Nil Branch Exhaustiveness",
        "By-expression on conditional expression '{0}' lacks a 'nil' arm or an 'else' default arm.",
        DiagnosticCategory.Inspection, DiagnosticSeverity.Warning
    );

    public static readonly DiagnosticDescriptor UncheckedGlobalDereference = new(
        "EV8014", "Unchecked Ambient Global Dereference",
        "Dereference of ambient global variable '{0}' without nil-check or safe navigation. Ambient globals may evaluate to nil at runtime and crash.",
        DiagnosticCategory.Inspection, DiagnosticSeverity.Warning
    );
}
