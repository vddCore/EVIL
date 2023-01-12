namespace EVIL.Grammar.AST.Nodes
{
    public class WhileLoopNode : AstNode
    {
        public AstNode Expression { get; }
        public BlockStatementNode Statements { get; }

        public WhileLoopNode(AstNode expression, BlockStatementNode statements)
        {
            Expression = expression;
            Statements = statements;

            Reparent(Expression, Statements);
        }
    }
}
