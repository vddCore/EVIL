using System.Collections.Generic;
using System.Globalization;

namespace EVIL.Grammar.AST.Nodes
{
    public class IndexingNode : AstNode
    {
        public AstNode Indexable { get; }
        public AstNode KeyExpression { get; }

        public bool WillBeAssigned { get; }

        public IndexingNode(AstNode indexable, AstNode keyExpression, bool willBeAssigned)
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
            
            while (current is IndexingNode indexable)
            {
                stack.Push(indexable.GetKeyStringRepresentation());
                current = indexable.Indexable;
            }

            if (current is VariableNode vn)
            {
                stack.Push(vn.Identifier);
            }

            return string.Join('.', stack);
        }

        public string GetKeyStringRepresentation()
        {
            return KeyExpression switch
            {
                NumberNode num => num.Value.ToString(CultureInfo.InvariantCulture),
                StringNode str => str.Value,
                VariableNode vn => vn.Identifier,
                _ => "???"
            };
        }
    }
}

