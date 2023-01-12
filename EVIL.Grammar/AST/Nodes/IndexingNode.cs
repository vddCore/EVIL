using System.Collections.Generic;

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
                DecimalNode num => num.Value.ToString(),
                StringNode str => str.Value,
                VariableNode vn => vn.Identifier,
                _ => "???"
            };
        }
    }
}

