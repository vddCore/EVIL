namespace EVIL.Grammar.Analysis.Analyzers;

using System.Collections.Generic;
using EVIL.CommonTypes.Diagnostics;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Grammar.AST.Statements;

public sealed class UnusedVariableAnalyzer : IDiagnosticAnalyzer
{
    public IReadOnlyCollection<DiagnosticDescriptor> SupportedDiagnostics { get; } = [CompilerDiagnostics.UnusedVariable];

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

    private sealed class ScopeVariableWalker : AstWalker
    {
        private readonly string _filePath;
        private readonly IDiagnosticSink _sink;
        private readonly Stack<Scope> _scopes = new();

        public ScopeVariableWalker(string filePath, IDiagnosticSink sink)
        {
            _filePath = filePath;
            _sink = sink;
        }

        private Scope CurrentScope 
            => _scopes.Peek();

        public override void Visit(ProgramNode programNode)
        {
            _scopes.Push(new Scope());
            base.Visit(programNode);
            CloseScope();
        }

        protected override void Visit(BlockStatement blockStatement)
        {
            _scopes.Push(new Scope());
            base.Visit(blockStatement);
            CloseScope();
        }

        protected override void Visit(FnStatement fnStatement)
        {
            _scopes.Push(new Scope());
            base.Visit(fnStatement);
            CloseScope();
        }

        protected override void Visit(FnTargetedStatement fnTargetedStatement)
        {
            _scopes.Push(new Scope());
            base.Visit(fnTargetedStatement);
            CloseScope();
        }

        protected override void Visit(FnIndexedStatement fnIndexedStatement)
        {
            _scopes.Push(new Scope());
            base.Visit(fnIndexedStatement);
            CloseScope();
        }

        protected override void Visit(FnExpression fnExpression)
        {
            _scopes.Push(new Scope());
            base.Visit(fnExpression);
            CloseScope();
        }

        protected override void Visit(ValStatement valStatement)
        {
            foreach (var kvp in valStatement.Definitions)
            {
                var name = kvp.Key.Name;
                var location = new SourceLocation(_filePath, kvp.Key.Line, kvp.Key.Column);
                CurrentScope.Variables.Add(new VariableInfo(name, location));
            }

            base.Visit(valStatement);
        }

        protected override void Visit(SymbolReferenceExpression symbolReferenceExpression)
        {
            var name = symbolReferenceExpression.Identifier;
            foreach (var scope in _scopes)
            {
                for (var i = scope.Variables.Count - 1; i >= 0; i--)
                {
                    if (scope.Variables[i].Name == name)
                    {
                        scope.Variables[i].IsReferenced = true;
                        base.Visit(symbolReferenceExpression);
                        return;
                    }
                }
            }

            base.Visit(symbolReferenceExpression);
        }

        private void CloseScope()
        {
            var scope = _scopes.Pop();
            
            foreach (var variable in scope.Variables)
            {
                if (!variable.IsReferenced && !variable.Name.StartsWith('_'))
                {
                    _sink.Report(
                        CompilerDiagnostics.UnusedVariable.Create(
                            variable.Location,
                            variable.Name
                        )
                    );
                }
            }
        }

        private sealed class Scope
        {
            public List<VariableInfo> Variables { get; } = [];
        }

        private sealed class VariableInfo
        {
            public string Name { get; }
            public SourceLocation Location { get; }
            public bool IsReferenced { get; set; }

            public VariableInfo(string name, SourceLocation location)
            {
                Name = name;
                Location = location;
            }
        }
    }
}
