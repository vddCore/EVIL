using EVIL.AST.Base;

namespace EVIL.AST.Nodes
{
    public class TableIndexingNode : AstNode
    {
        public VariableNode Variable { get; }
        public AstNode KeyExpression { get; }

        public TableIndexingNode(VariableNode variable, AstNode keyExpression)
        {
            Variable = variable;
            KeyExpression = keyExpression;
        }
    }
}
