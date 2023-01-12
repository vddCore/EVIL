using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        public AstNode Break()
        {
            var line = Match(TokenType.Break);
            return new BreakNode { Line = line };
        }
    }
}
