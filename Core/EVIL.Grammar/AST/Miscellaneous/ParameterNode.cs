using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Miscellaneous
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