using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode Indexing(AstNode indexable)
        {
            int line;
            AstNode indexer;
            
            if (CurrentToken.Type == TokenType.Dot)
            {
                line = Match(Token.Dot);
                indexer = new StringNode(CurrentToken.Value);
                Match(Token.Identifier);
            }
            else // must be bracket then
            {
                line = Match(Token.LBracket);
                indexer = AssignmentExpression();
                Match(Token.RBracket);
            }

            return new IndexingNode(indexable, indexer, CurrentToken.Type == TokenType.Assign) {Line = line};
        }
    }
}