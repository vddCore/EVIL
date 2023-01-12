namespace EVIL.Grammar.AST.Nodes
{
    public class VariableReference : Expression
    {
        public string Identifier { get; }

        public VariableReference(string identifier)
        {
            Identifier = identifier;
        }
    }
}