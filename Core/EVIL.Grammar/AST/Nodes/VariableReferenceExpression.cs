namespace EVIL.Grammar.AST.Nodes
{
    public class VariableReferenceExpression : Expression
    {
        public string Identifier { get; }

        public VariableReferenceExpression(string identifier)
        {
            Identifier = identifier;
        }
    }
}