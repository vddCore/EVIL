namespace EVIL.Core.Analysis;

using System.Collections.Generic;
using EVIL.Core.Diagnostics;

public interface IDiagnosticAnalyzer
{
    IReadOnlyCollection<DiagnosticDescriptor> SupportedDiagnostics { get; }

    void Initialize(AnalysisContext context);
}
