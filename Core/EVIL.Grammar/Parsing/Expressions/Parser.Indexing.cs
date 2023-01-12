using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private IndexerExpression Indexing(Expression indexable)
        {
            int line, col;
            Expression indexer;

            if (CurrentToken.Type == TokenType.Dot)
            {
                (line, col) = Match(Token.Dot);

                indexer = new StringConstant(CurrentToken.Value);
                Match(Token.Identifier);
            }
            else // must be bracket then
            {
                (line, col) = Match(Token.LBracket);

                indexer = AssignmentExpression();
                Match(Token.RBracket);
            }

            return new IndexerExpression(indexable, indexer, CurrentToken.Type == TokenType.Assign)
                { Line = line, Column = col };
        }
    }
}