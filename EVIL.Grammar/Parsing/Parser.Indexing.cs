using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        public AstNode Indexing(AstNode indexable)
        {
            int line;
            AstNode indexer;
            
            if (Scanner.State.CurrentToken.Type == TokenType.Dot)
            {
                line = Match(TokenType.Dot);
                indexer = new StringNode(Scanner.State.CurrentToken.Value.ToString());
                Match(TokenType.Identifier);
            }
            else // must be bracket then
            {
                line = Match(TokenType.LBracket);
                indexer = AssignmentExpression();
                Match(TokenType.RBracket);
            }

            return new IndexingNode(indexable, indexer, Scanner.State.CurrentToken.Type == TokenType.Assign) {Line = line};
        }
    }
}