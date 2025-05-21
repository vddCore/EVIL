namespace EVIL.Grammar.Parsing;

using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Lexical;

public partial class Parser
{
    private ByExpression ByExpression()
    {
        var (line, col) = Match(Token.By);

        var qualifier = AssignmentExpression();
        Match(Token.LBrace);

        var arms = new List<ByArmNode>();
        AstNode? elseArm = null;

        while (true)
        {
            if (CurrentToken.Type == TokenType.EOF)
            {
                throw new ParserException(
                    "Unexpected EOF in a by-expression.",
                    (CurrentToken.Line, CurrentToken.Column)
                );
            }

            var (armLine, armCol) = (CurrentToken.Line, CurrentToken.Column);

            if (CurrentToken.Type == TokenType.RBrace)
            {
                if (arms.Count == 0)
                {
                    throw new ParserException(
                        "Empty by-expressions are not allowed.",
                        (line, col)
                    );
                }
            }

            if (CurrentToken.Type == TokenType.Else)
            {
                if (arms.Count == 0)
                {
                    throw new ParserException(
                        "'else' is illegal with no selector arms specified.",
                        (armLine, armCol)
                    );
                }

                Match(Token.Else);
                Match(Token.Colon);

                if (CurrentToken.Type == TokenType.Throw)
                {
                    elseArm = ThrowStatement();
                }
                else
                {
                    elseArm = AssignmentExpression();
                }


                if (CurrentToken.Type != TokenType.RBrace)
                {
                    throw new ParserException(
                        "'else' arm must be specified at the end of selector list.",
                        (armLine, armCol)
                    );
                }

                break;
            }
            else
            {
                var selector = AssignmentExpression();
                var deepEquality = false;

                switch (CurrentToken.Type)
                {
                    case TokenType.RightArrow:
                    {
                        Match(Token.RightArrow);
                        deepEquality = false;
                        break;
                    }

                    case TokenType.Associate:
                    {
                        Match(Token.Associate);
                        deepEquality = true;
                        break;
                    }

                    default:
                    {
                        throw new ParserException(
                            $"Expected '->' or '=>', found '{CurrentToken.Value}'.",
                            (CurrentToken.Line, CurrentToken.Column)
                        );
                    }
                }

                AstNode valueArm;
                if (CurrentToken.Type == TokenType.Throw)
                {
                    valueArm = ThrowStatement();
                }
                else
                {
                    valueArm = AssignmentExpression();
                }

                arms.Add(new ByArmNode(selector, valueArm, deepEquality) { Line = armLine, Column = armCol });
            }

            if (CurrentToken.Type == TokenType.RBrace)
            {
                break;
            }

            Match(Token.Comma);
        }

        Match(Token.RBrace);

        return new ByExpression(qualifier, arms, elseArm) { Line = line, Column = col };
    }
}