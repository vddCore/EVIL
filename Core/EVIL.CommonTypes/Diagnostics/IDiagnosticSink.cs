namespace EVIL.CommonTypes.Diagnostics;

using System.Collections.Generic;

public interface IDiagnosticSink
{
    void Report(Diagnostic diagnostic);
    bool HasErrors { get; }
    int ErrorCount { get; }
    int WarningCount { get; }
    IReadOnlyList<Diagnostic> Diagnostics { get; }
}
