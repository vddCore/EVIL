using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        public AstNode Indexing(AstNode indexable)
        {
            var line = Match(TokenType.LBracket);
            var expr = Assignment();
            Match(TokenType.RBracket);

            return new IndexingNode(indexable, expr, Scanner.State.CurrentToken.Type == TokenType.Assign) {Line = line};
        }
    }
}