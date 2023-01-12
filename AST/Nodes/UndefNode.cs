using EVIL.AST.Base;
using EVIL.AST.Enums;

namespace EVIL.AST.Nodes
{
    public class UndefNode : AstNode
    {
        public string Identifier { get; }

        public UndefNode(string identifier)
        {
            Identifier = identifier;
        }
    }
}
