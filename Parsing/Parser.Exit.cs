using EVIL.AST.Base;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        public AstNode Exit()
        {
            var line = Match(TokenType.Exit);
            return new ExitNode() { Line = line };
        }
    }
}
