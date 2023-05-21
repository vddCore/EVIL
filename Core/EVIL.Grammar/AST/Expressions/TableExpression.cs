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

            for (var i = 0; i < Initializers.Count; i++)
                Reparent(Initializers[i]);
        }
    }
}