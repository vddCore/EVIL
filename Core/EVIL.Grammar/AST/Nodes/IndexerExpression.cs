using System.Collections.Generic;
using System.Globalization;

namespace EVIL.Grammar.AST.Nodes
{
    public class IndexerExpression : Expression
    {
        public Expression Indexable { get; }
        public Expression KeyExpression { get; }

        public bool WillBeAssigned { get; }

        public IndexerExpression(Expression indexable, Expression keyExpression, bool willBeAssigned)
        {
            Indexable = indexable;
            KeyExpression = keyExpression;
            
            WillBeAssigned = willBeAssigned;

            Reparent(Indexable, KeyExpression);
        }

        public string BuildChainStringRepresentation()
        {
            var stack = new Stack<string>();
            var current = Indexable;
            
            stack.Push(GetKeyStringRepresentation());
            
            while (current is IndexerExpression indexable)
            {
                stack.Push(indexable.GetKeyStringRepresentation());
                current = indexable.Indexable;
            }

            if (current is VariableReferenceExpression vn)
            {
                stack.Push(vn.Identifier);
            }

            return string.Join('.', stack);
        }

        public string GetKeyStringRepresentation()
        {
            return KeyExpression switch
            {
                NumberExpression num => num.Value.ToString(CultureInfo.InvariantCulture),
                StringConstant str => str.Value,
                VariableReferenceExpression vn => vn.Identifier,
                _ => "???"
            };
        }
    }
}

