using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
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
