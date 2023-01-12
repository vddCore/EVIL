using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
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
