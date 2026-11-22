namespace EVIL.Analysis;

using System.Collections.Generic;
using EVIL.Diagnostics;

public interface IDiagnosticAnalyzer
{
    IReadOnlyCollection<DiagnosticDescriptor> SupportedDiagnostics { get; }

    void Initialize(AnalysisContext context);
}
