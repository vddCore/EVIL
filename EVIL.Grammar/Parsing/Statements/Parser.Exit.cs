using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode Exit()
        {
            var line = Match(Token.Exit);
            return new ExitNode { Line = line };
        }
    }
}
