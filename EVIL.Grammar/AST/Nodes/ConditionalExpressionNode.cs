namespace EVIL.Grammar.AST.Nodes
{
    public class ConditionalExpressionNode : AstNode
    {
        public AstNode Condition { get; }
        
        public AstNode TrueExpression { get; }
        public AstNode FalseExpression { get; }

        public ConditionalExpressionNode(AstNode condition, AstNode trueExpression, AstNode falseExpression)
        {
            Condition = condition;
            
            TrueExpression = trueExpression;
            FalseExpression = falseExpression;

            Reparent(Condition, TrueExpression, FalseExpression);
        }
    }
}