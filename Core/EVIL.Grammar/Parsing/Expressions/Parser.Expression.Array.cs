namespace EVIL.Grammar.Parsing;

using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

public partial class Parser
{
    private ArrayExpression ArrayExpression()
    {
        int line, col;

        Expression? sizeExpression = null;
        var expressions = new List<Expression>();
        
        if (CurrentToken.Type == TokenType.Array)
        {
            (line, col) = Match(Token.Array);

            if (CurrentToken == Token.LParenthesis)
            {
                Match(Token.LParenthesis);

                if (CurrentToken != Token.RParenthesis)
                {
                    sizeExpression = AssignmentExpression();
                }

                Match(Token.RParenthesis);
            }

            if (CurrentToken == Token.LBrace)
            {
                Match(Token.LBrace);
                while (CurrentToken != Token.RBrace)
                {
                    expressions.Add(AssignmentExpression());

                    if (CurrentToken == Token.RBrace)
                    {
                        break;
                    }

                    Match(
                        Token.Comma,
                        "Expected '$expected' or '}', got '$actual'."
                    );
                }

                Match(Token.RBrace);
            }
            else
            {
                if (sizeExpression == null)
                {
                    throw new ParserException(
                        "Inferred array size requires an array initializer.",
                        (CurrentState.Line, CurrentState.Column)
                    );
                }
            }
        }
        else if (CurrentToken.Type == TokenType.LBracket)
        {
            (line, col) = Match(Token.LBracket);
            while (CurrentToken != Token.RBracket)
            {
                expressions.Add(AssignmentExpression());

                if (CurrentToken == Token.RBracket)
                {
                    break;
                }

                Match(
                    Token.Comma,
                    "Expected '$expected' or ']', got '$actual'."
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

        return new ArrayExpression(sizeExpression, expressions)
            { Line = line, Column = col };
    }
}