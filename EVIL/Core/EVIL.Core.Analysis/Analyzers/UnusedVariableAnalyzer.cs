namespace EVIL.Core.Analysis.Analyzers;

using System.Collections.Generic;
using EVIL.Core.Diagnostics;

public sealed partial class UnusedVariableAnalyzer : IDiagnosticAnalyzer
{
    public IReadOnlyCollection<DiagnosticDescriptor> SupportedDiagnostics { get; } = [
        CompilerDiagnostics.UnusedVariable
    ];

    public void Initialize(AnalysisContext context)
    {
        context.RegisterProgramAction(AnalyzeProgram);
    }

    private static void AnalyzeProgram(ProgramAnalysisContext context)
    {
        var walker = new ScopeVariableWalker(
            context.FilePath,
            context.DiagnosticSink
        );
        
        walker.Walk(context.Program);
    }
}
