namespace EVIL.Grammar.Analysis;

using System.Collections.Generic;
using EVIL.CommonTypes.Diagnostics;

public interface IDiagnosticAnalyzer
{
    IReadOnlyCollection<DiagnosticDescriptor> SupportedDiagnostics { get; }

    void Initialize(AnalysisContext context);
}
