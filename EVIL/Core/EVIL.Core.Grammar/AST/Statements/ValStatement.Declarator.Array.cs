namespace EVIL.Core.Grammar.AST.Statements;

using System.Collections.Generic;
using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Miscellaneous;

public sealed class ArrayPatternValDeclarator : ValDeclarator
{
    public IReadOnlyList<PatternBindingNode> Bindings { get; }

    public ArrayPatternValDeclarator(
        IReadOnlyList<PatternBindingNode> bindings,
        Expression initializer
    ) : base(initializer)
    {
        Bindings = [..bindings];
        
        foreach (var binding in Bindings)
        {
            Reparent(binding);
        }
    }
}