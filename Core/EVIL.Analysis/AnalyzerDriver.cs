namespace EVIL.Analysis;

using System.Collections.Generic;
using EVIL.Diagnostics;
using EVIL.Analysis.Analyzers;
using EVIL.Grammar.AST.Miscellaneous;

public sealed class AnalyzerDriver
{
    private readonly List<IDiagnosticAnalyzer> _analyzers = [];

    public IReadOnlyList<IDiagnosticAnalyzer> Analyzers => _analyzers;

    public AnalyzerDriver()
    {
    }

    public AnalyzerDriver(IEnumerable<IDiagnosticAnalyzer> analyzers)
    {
        _analyzers.AddRange(analyzers);
    }

    public AnalyzerDriver AddAnalyzer(IDiagnosticAnalyzer analyzer)
    {
        _analyzers.Add(analyzer);
        return this;
    }

    public AnalyzerDriver AddAnalyzers(IEnumerable<IDiagnosticAnalyzer> analyzers)
    {
        _analyzers.AddRange(analyzers);
        return this;
    }

    public static AnalyzerDriver CreateDefault()
    {
        var driver = new AnalyzerDriver();
        driver.AddAnalyzer(new MissingByElseArmAnalyzer());
        driver.AddAnalyzer(new RedundantSelfParameterAnalyzer());
        driver.AddAnalyzer(new UnusedVariableAnalyzer());
        driver.AddAnalyzer(new UnreachableCodeAnalyzer());
        driver.AddAnalyzer(new CatchlessTryAnalyzer());
        driver.AddAnalyzer(new TableDuplicateKeyAnalyzer());
        return driver;
    }

    public void Analyze(ProgramNode program, string filePath, IDiagnosticSink sink)
    {
        if (_analyzers.Count == 0) return;

        var context = new AnalysisContext();
        foreach (var analyzer in _analyzers)
        {
            analyzer.Initialize(context);
        }

        var programContext = new ProgramAnalysisContext(program, filePath, sink);
        foreach (var action in context.ProgramActions)
        {
            action(programContext);
        }

        if (context.SyntaxNodeActions.Count > 0)
        {
            var walker = new AstWalker();
            walker.NodeVisiting = node =>
            {
                var nodeType = node.GetType();
                foreach (var (registeredType, actionList) in context.SyntaxNodeActions)
                {
                    if (registeredType.IsAssignableFrom(nodeType))
                    {
                        var syntaxCtx = new SyntaxNodeAnalysisContext(node, filePath, sink);
                        foreach (var action in actionList)
                        {
                            action(syntaxCtx);
                        }
                    }
                }
            };

            walker.Walk(program);
        }
    }
}
