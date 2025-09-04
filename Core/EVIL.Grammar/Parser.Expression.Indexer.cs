namespace EVIL.Grammar;

using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

public partial class Parser
{
    private IndexerExpression IndexerExpression(Expression indexable, bool createTableIfNonExistent)
    {
        int line, col;
        Expression indexer;
        var isConditional = false;

        if (CurrentToken.Type is TokenType.Dot or TokenType.Elvis)
        {
            switch (CurrentToken.Type)
            {
                case TokenType.Dot:
                    (line, col) = Match(Token.Dot);
                    break;
                case TokenType.Elvis:
                    (line, col) = Match(Token.Elvis);
                    isConditional = true;
                    break;
                default:
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
        else if (CurrentToken.Type is TokenType.LBracket or TokenType.ElvisArray)
        {
            switch (CurrentToken.Type)
            {
                case TokenType.LBracket:
                    (line, col) = Match(Token.LBracket);
                    break;
                case TokenType.ElvisArray:
                    (line, col) = Match(Token.ElvisArray);
                    isConditional = true;
                    break;
                default:
                    throw new ParserException(
                        $"Unexpected token '{CurrentToken}'.",
                        (CurrentToken.Line, CurrentToken.Column)
                    );
            }

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
        else
        {
            throw new ParserException(
                $"Unexpected token '{CurrentToken}'.",
                (CurrentToken.Line, CurrentToken.Column)
            );
        }

        return new IndexerExpression(
                indexable, 
                indexer, 
                isConditional,
                CurrentToken.Type == TokenType.Assign,
                createTableIfNonExistent)
            { Line = line, Column = col };
    }
}