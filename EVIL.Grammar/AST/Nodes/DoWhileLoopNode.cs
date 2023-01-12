namespace EVIL.Grammar.AST.Nodes
{
    public class DoWhileLoopNode : AstNode
    {
        public AstNode ConditionExpression { get; }
        public AstNode Statement { get; }

        public DoWhileLoopNode(AstNode conditionExpression, AstNode statement)
        {
            ConditionExpression = conditionExpression;
            Statement = statement;

            Reparent(ConditionExpression, Statement);
        }
    }
}