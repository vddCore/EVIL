using EVIL.AST.Base;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        public AstNode Skip()
        {
            var line = Match(TokenType.Skip);
            return new SkipNode() { Line = line };
        }
    }
}
