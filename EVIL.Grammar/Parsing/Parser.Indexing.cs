using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private IndexerExpression Indexing(Expression indexable)
        {
            int line;
            Expression indexer;
            
            if (CurrentToken.Type == TokenType.Dot)
            {
                line = Match(Token.Dot);
                indexer = new StringConstant(CurrentToken.Value);
                Match(Token.Identifier);
            }
            else // must be bracket then
            {
                line = Match(Token.LBracket);
                indexer = AssignmentExpression();
                Match(Token.RBracket);
            }

            return new IndexerExpression(indexable, indexer, CurrentToken.Type == TokenType.Assign) {Line = line};
        }
    }
}