namespace EVIL.Grammar.AST.Nodes
{
    public class VariableNode : AstNode
    {
        public string Identifier { get; }
        public bool IsBeingDefined { get; }

        public VariableNode(string identifier, bool isBeingDefined = false)
        {
            Identifier = identifier;
            IsBeingDefined = isBeingDefined;
        }
    }
}