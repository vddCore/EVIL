using EVIL.AST.Base;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        public AstNode TableIndexing(string identifier)
        {
            var variableNode = Variable(identifier);
            var line = Match(TokenType.LBracket);
            var keyExpression = Comparison();

            Match(TokenType.RBracket);

            return new TableIndexingNode(variableNode, keyExpression) { Line = line };
        }
    }
}
