using EVIL.AST.Base;
using EVIL.AST.Enums;

namespace EVIL.AST.Nodes
{
    public class UndefNode : AstNode
    {
        public string Name { get; set; }
        public UndefineType Type { get; set; }
    }
}
