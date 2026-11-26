namespace EVIL.Core.Grammar.AST.Expressions;

using System.Collections.Generic;
using EVIL.Core.Grammar.AST.Base;

public sealed class ArrayExpression : Expression
{
    public Expression? SizeExpression { get; }
    public List<Expression> Initializers { get; }

    public ArrayExpression(Expression? sizeExpression, List<Expression> initializers)
    {
        SizeExpression = sizeExpression;
        Initializers = initializers;
            
        Reparent(SizeExpression);
        Reparent(Initializers);
    }
}