using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Miscellaneous
{
    public sealed class ParameterNode : AstNode
    {
        public string Name { get; }
        public bool ReadWrite { get; }
        
        public ConstantExpression? Initializer { get; }

        public ParameterNode(string name, bool readWrite, ConstantExpression? initializer)
        {
            Name = name;
            ReadWrite = readWrite;
            Initializer = initializer;

            if (Initializer != null)
                Reparent(Initializer);
        }
    }
}