using EVIL.AST.Base;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        public AstNode PostIncrementation(string identifier)
        {
            var line = Match(TokenType.Increment);
            return new PostIncrementationNode(Variable(identifier)) { Line = line };
        }
    }
}
