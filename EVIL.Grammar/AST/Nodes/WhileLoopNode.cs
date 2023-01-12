namespace EVIL.Grammar.AST.Nodes
{
    public class WhileLoopNode : AstNode
    {
        public AstNode Expression { get; }
        public AstNode Statement { get; }

        public WhileLoopNode(AstNode expression, AstNode statement)
        {
            Expression = expression;
            Statement = statement;

            Reparent(Expression, Statement);
        }
    }
}
