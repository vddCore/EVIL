using EVIL.AST.Base;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        public AstNode PostDecrementation(AstNode left)
        {
            var line = Match(TokenType.Decrement);
            return new PostDecrementationNode(left) { Line = line };
        }
    }
}
