namespace EVIL.Grammar.AST
{
    public sealed class ParameterNode : AstNode
    {
        public string Name { get; }
        public ConstantExpression? Initializer { get; }

        public ParameterNode(string name, ConstantExpression? initializer)
        {
            Name = name;
            Initializer = initializer;

            if (Initializer != null)
                Reparent(Initializer);
        }
    }
}