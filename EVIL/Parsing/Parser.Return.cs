using EVIL.AST.Base;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        private AstNode Return()
        {
            var line = Match(TokenType.Ret);
            return new ReturnNode(Assignment()) { Line = line };
        }
    }
}
