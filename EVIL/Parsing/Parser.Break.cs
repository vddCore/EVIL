using EVIL.AST.Base;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        public AstNode Break()
        {
            var line = Match(TokenType.Break);
            return new BreakNode() { Line = line };
        }
    }
}
