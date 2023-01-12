using EVIL.AST.Base;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        public AstNode Indexing(string identifier)
        {
            var variableNode = Variable(identifier);
            var line = Match(TokenType.LBracket);
            var keyExpression = LogicalExpression();
            Match(TokenType.RBracket);

            return new IndexingNode(variableNode, keyExpression) { Line = line };
        }
    }
}
