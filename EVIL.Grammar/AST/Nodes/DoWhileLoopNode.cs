namespace EVIL.Grammar.AST.Nodes
{
    public class DoWhileLoopNode : AstNode
    {
        public AstNode ConditionExpression { get; }
        public BlockStatementNode Statements { get; }

        public DoWhileLoopNode(AstNode conditionExpression, BlockStatementNode statements)
        {
            ConditionExpression = conditionExpression;
            Statements = statements;
        }
    }
}