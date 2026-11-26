namespace EVIL.Core.Analysis.Analyzers;

using System.Collections.Generic;
using EVIL.Core.Diagnostics;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Grammar.AST.Miscellaneous;
using EVIL.Core.Grammar.AST.Statements;

public sealed partial class UnusedVariableAnalyzer
{
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
            if (fnTargetedStatement.PrimaryTarget is IdentifierNode id)
            {
                MarkVariableReferenced(id.Name);
            }
            else
            {
                Visit(fnTargetedStatement.PrimaryTarget);
            }

            _scopes.Push(new Scope());

            if (fnTargetedStatement.ParameterList != null)
            {
                Visit(fnTargetedStatement.ParameterList);
            }

            Visit(fnTargetedStatement.InnerStatement);

            if (fnTargetedStatement.AttributeList != null)
            {
                Visit(fnTargetedStatement.AttributeList);
            }

            CloseScope();
        }

        protected override void Visit(FnIndexedStatement fnIndexedStatement)
        {
            Visit(fnIndexedStatement.Indexer);

            _scopes.Push(new Scope());

            if (fnIndexedStatement.ParameterList != null)
            {
                Visit(fnIndexedStatement.ParameterList);
            }

            Visit(fnIndexedStatement.InnerStatement);

            if (fnIndexedStatement.AttributeList != null)
            {
                Visit(fnIndexedStatement.AttributeList);
            }

            CloseScope();
        }

        protected override void Visit(FnExpression fnExpression)
        {
            _scopes.Push(new Scope());
            base.Visit(fnExpression);
            CloseScope();
        }

        protected override void Visit(SelfFnExpression selfFnExpression)
        {
            _scopes.Push(new Scope());
            base.Visit(selfFnExpression);
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
            MarkVariableReferenced(symbolReferenceExpression.Identifier);
            base.Visit(symbolReferenceExpression);
        }

        private void MarkVariableReferenced(string name)
        {
            foreach (var scope in _scopes)
            {
                for (var i = scope.Variables.Count - 1; i >= 0; i--)
                {
                    if (scope.Variables[i].Name == name)
                    {
                        scope.Variables[i].IsReferenced = true;
                        return;
                    }
                }
            }
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
    }
}