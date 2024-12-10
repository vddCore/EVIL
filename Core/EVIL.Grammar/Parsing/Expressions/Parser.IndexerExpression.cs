namespace EVIL.Grammar.Parsing;

using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

public partial class Parser
{
    private IndexerExpression IndexerExpression(Expression indexable)
    {
        int line, col;
        Expression indexer;
        var isConditional = false;

        if (CurrentToken.Type == TokenType.Dot || CurrentToken.Type == TokenType.Elvis)
        {
            if (CurrentToken.Type == TokenType.Dot)
            {
                (line, col) = Match(Token.Dot);
            }
            else if (CurrentToken.Type == TokenType.Elvis)
            {
                (line, col) = Match(Token.Elvis);
                isConditional = true;
            }
            else
            {
                throw new ParserException(
                    $"Unexpected token '{CurrentToken}'.",
                    (CurrentToken.Line, CurrentToken.Column)
                );
            }

            var identifier = Identifier();
            indexer = new StringConstant(identifier.Name, false)
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

        return new IndexerExpression(
                indexable, 
                indexer, 
                isConditional,
                CurrentToken.Type == TokenType.Assign)
            { Line = line, Column = col };
    }
}