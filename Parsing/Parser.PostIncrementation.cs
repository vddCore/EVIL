using EVIL.AST.Base;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        public AstNode PostIncrementation(AstNode left)
        {
            var line = Match(TokenType.Increment);
            return new PostIncrementationNode(left) { Line = line };
        }
    }
}
