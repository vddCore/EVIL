using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private IndexerExpression IndexerExpression(Expression indexable)
        {
            int line, col;
            Expression indexer;

            if (CurrentToken.Type == TokenType.Dot)
            {
                (line, col) = Match(Token.Dot);

                var identifier = Identifier();
                indexer = new StringConstant(identifier.Name)
                {
                    Line = identifier.Line,
                    Column = identifier.Column
                };
            }
            else // must be bracket then
            {
                (line, col) = Match(Token.LBracket);

                indexer = AssignmentExpression();

                if (indexer is NilConstant)
                {
                    throw new ParserException(
                        "'nil' is not a valid indexer expression.", 
                        (indexer.Line, indexer.Column)
                    );
                }
                
                Match(Token.RBracket);
            }

            return new IndexerExpression(indexable, indexer, CurrentToken.Type == TokenType.Assign)
                { Line = line, Column = col };
        }
    }
}