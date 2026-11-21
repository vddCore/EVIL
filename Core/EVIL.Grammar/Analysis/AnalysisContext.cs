namespace EVIL.Grammar.Analysis;

using System;
using System.Collections.Generic;
using EVIL.Grammar.AST.Base;

public sealed class AnalysisContext
{
    private readonly Dictionary<Type, List<Action<SyntaxNodeAnalysisContext>>> _syntaxNodeActions = new();
    private readonly List<Action<ProgramAnalysisContext>> _programActions = [];
    
    public IReadOnlyDictionary<Type, List<Action<SyntaxNodeAnalysisContext>>> SyntaxNodeActions => _syntaxNodeActions;
    public IReadOnlyList<Action<ProgramAnalysisContext>> ProgramActions => _programActions;

    public void RegisterSyntaxNodeAction<TNode>(Action<SyntaxNodeAnalysisContext<TNode>> action) where TNode : AstNode
    {
        var nodeType = typeof(TNode);
        if (!_syntaxNodeActions.TryGetValue(nodeType, out var actions))
        {
            actions = [];
            _syntaxNodeActions[nodeType] = actions;
        }

        actions.Add(ctx => action(
            new SyntaxNodeAnalysisContext<TNode>(
                (TNode)ctx.Node,
                ctx.FilePath,
                ctx.DiagnosticSink
            )
        ));
    }

    public void RegisterProgramAction(Action<ProgramAnalysisContext> action)
    {
        _programActions.Add(action);
    }
}
