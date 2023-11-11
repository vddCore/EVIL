using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Miscellaneous
{
    public sealed class ParameterNode : AstNode
    {
        public IdentifierNode Identifier { get; }
        public bool ReadWrite { get; }
        public bool NilAccepting { get; }
        
        public ConstantExpression? Initializer { get; }

        public ParameterNode(
            IdentifierNode identifier,
            bool readWrite,
            bool nilAccepting,
            ConstantExpression? initializer)
        {
            Identifier = identifier;
            ReadWrite = readWrite;
            NilAccepting = nilAccepting;
            Initializer = initializer;

            Reparent(Identifier);
            
            if (Initializer != null)
                Reparent(Initializer);
        }
    }
}