namespace EVIL.Grammar.AST.Nodes
{
    public class FunctionCallNode : AstNode
    {
        public AstNode Left { get; }
        public ArgumentListNode ArgumentList { get; }

        public FunctionCallNode(AstNode left, ArgumentListNode argumentList)
        {
            Left = left;
            ArgumentList = argumentList;

            Reparent(left);
            Reparent(ArgumentList);
        }
    }
}