namespace EVIL.Grammar.AST.Nodes
{
    public class VariableDefinitionNode : AstNode
    {
        public string Identifier { get; }
        public AstNode Initializer { get; }

        public VariableDefinitionNode(string identifier, AstNode initializer)
        {
            Identifier = identifier;
            Initializer = initializer;

            if (Initializer != null)
            {
                Reparent(Initializer);
            }
        }
    }
}