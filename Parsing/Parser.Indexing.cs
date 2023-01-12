using EVIL.AST.Base;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        public AstNode Indexing(AstNode indexable)
        {
            var line = Match(TokenType.LBracket);
            var keyExpression = LogicalExpression();
            Match(TokenType.RBracket);

            return new IndexingNode(indexable, keyExpression) { Line = line };
        }
    }
}
