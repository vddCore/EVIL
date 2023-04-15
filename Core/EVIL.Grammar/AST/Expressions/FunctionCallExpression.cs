using System.Collections.Generic;

namespace EVIL.Grammar.AST.Expressions
{
    public sealed class FunctionCallExpression : Expression
    {
        public Expression Callee { get; }
        public List<Expression> Arguments { get; }

        public FunctionCallExpression(Expression callee, List<Expression> arguments)
        {
            Callee = callee;
            Arguments = arguments;

            Reparent(Callee);

            for (var i = 0; i < Arguments.Count; i++)
                Reparent(Arguments[i]);
        }
    }
}