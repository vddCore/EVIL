namespace EVIL.Grammar.AST.Nodes
{
    public class VariableDefinition : Statement
    {
        public string Identifier { get; }
        public Expression Initializer { get; }

        public VariableDefinition(string identifier, Expression initializer)
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