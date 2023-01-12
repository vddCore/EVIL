using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class IndexingNode : AstNode
    {
        public VariableNode Variable { get; }
        public AstNode KeyExpression { get; }

        public IndexingNode(VariableNode variable, AstNode keyExpression)
        {
            Variable = variable;
            KeyExpression = keyExpression;
        }
    }
}
