namespace EVIL.Core.Grammar;

using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Constants;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Lexical;

public partial class Parser
{
    private IndexerExpression IndexerExpression(Expression indexable, bool createTableIfNonExistent)
    {
        int line, col;
        
        Expression? keyExpression = null;
        Expression? startExpression = null;
        Expression? endExpression = null;
        
        var isRange = false;
        var isFromEndIndex = false;
        var startIsFromEnd = false;
        var endIsFromEnd = false;
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
            keyExpression = new StringConstant(identifier.Name, false)
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

            if (CurrentToken.Type == TokenType.Range)
            {
                Match(Token.Range);
                
                isRange = true;

                if (CurrentToken.Type != TokenType.RBracket)
                {
                    (endExpression, endIsFromEnd) = ParseIndexerBound();
                }
            }
            else
            {
                var (firstExpression, firstIsFromEnd) = ParseIndexerBound();

                if (CurrentToken.Type == TokenType.Range)
                {
                    Match(Token.Range);
                    
                    isRange = true;
                    startExpression = firstExpression;
                    startIsFromEnd = firstIsFromEnd;

                    if (CurrentToken.Type != TokenType.RBracket)
                    {
                        (endExpression, endIsFromEnd) = ParseIndexerBound();
                    }
                }
                else if (firstIsFromEnd)
                {
                    startExpression = firstExpression;
                    isFromEndIndex = true;
                }
                else
                {
                    keyExpression = firstExpression;
                }
            }

            if (createTableIfNonExistent && (isRange || isFromEndIndex))
            {
                throw new ParserException(
                    "Range and from-end indexers cannot define indexed functions.",
                    (line, col)
                );
            }

            Match(Token.RBracket);

            if ((isRange || isFromEndIndex)
                && (_assignmentOperators.Contains(CurrentToken.Type) 
                    || CurrentToken.Type is TokenType.Increment or TokenType.Decrement))
            {
                throw new ParserException(
                    "Range and from-end indexers are not assignable.",
                    (CurrentToken.Line, CurrentToken.Column)
                );
            }
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
                keyExpression,
                startExpression,
                endExpression,
                isRange,
                isFromEndIndex,
                startIsFromEnd,
                endIsFromEnd,
                isConditional,
                CurrentToken.Type == TokenType.Assign,
                createTableIfNonExistent)
            { Line = line, Column = col };
    }

    private (Expression Expression, bool IsFromEnd) ParseIndexerBound()
    {
        var isFromEnd = CurrentToken.Type == TokenType.Length;
        if (isFromEnd)
        {
            Match(Token.Length);
        }

        var expression = AssignmentExpression();
        if (expression is NilConstant)
        {
            throw new ParserException(
                "'nil' is not a valid indexer expression.",
                (expression.Line, expression.Column)
            );
        }

        return (expression, isFromEnd);
    }
}