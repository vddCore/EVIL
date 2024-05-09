using System.Collections.Generic;
using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Expressions
{
    public class ArrayExpression : Expression
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
}