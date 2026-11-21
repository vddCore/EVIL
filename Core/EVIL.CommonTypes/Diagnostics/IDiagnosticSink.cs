namespace EVIL.CommonTypes.Diagnostics;

using System.Collections.Generic;

public interface IDiagnosticSink
{
    bool HasErrors { get; }
    int ErrorCount { get; }
    int WarningCount { get; }
    IReadOnlyList<Diagnostic> Diagnostics { get; }
    
    void Report(Diagnostic diagnostic);
}
