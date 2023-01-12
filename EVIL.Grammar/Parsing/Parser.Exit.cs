using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
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
