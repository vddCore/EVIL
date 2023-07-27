using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Miscellaneous
{
    public class IdentifierNode : AstNode
    {
        public string Name { get; }

        public IdentifierNode(string name)
        {
            Name = name;
        }
    }
}