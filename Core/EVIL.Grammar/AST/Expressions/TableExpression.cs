using System.Collections.Generic;
using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Expressions
{
    public sealed class TableExpression : Expression
    {
        public List<Expression> Initializers { get; }
        public bool Keyed { get; }

        public TableExpression(List<Expression> initializers, bool keyed)
        {
            Initializers = initializers;
            Keyed = keyed;

            Reparent(Initializers);
        }
    }
}