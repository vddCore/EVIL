namespace EVIL.Grammar.Parsing;

using System.Collections.Generic;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Miscellaneous;
using EVIL.Lexical;

public partial class Parser
{
    private ArgumentList ArgumentList()
    {
        var arguments = new List<Expression>();
        var isVariadic = false;

        var (line, col) = Match(Token.LParenthesis);
        while (CurrentToken.Type != TokenType.RParenthesis)
        {
            if (CurrentToken.Type == TokenType.EOF)
            {
                throw new ParserException(
                    $"Unexpected EOF in argument list.",
                    (line, col)
                );
            }

            if (CurrentToken.Type == TokenType.Multiply)
            {
                var (xline, xcol) = Match(Token.Multiply);
                    
                if (CurrentToken.Type != TokenType.RParenthesis)
                {
                    throw new ParserException(
                        "Variadic parameter specifier must reside at the end of the parameter list.",
                        (xline, xcol)
                    );
                }

                isVariadic = true;
                    
                arguments.Add(
                    new ExtraArgumentsExpression(true) 
                        { Line = xline, Column = xcol }
                );
            }
            else
            {
                arguments.Add(AssignmentExpression());
            }

            if (CurrentToken.Type == TokenType.RParenthesis)
                break;

            Match(Token.Comma, "Expected $expected or ')', got $actual");
        }
        Match(Token.RParenthesis);

        return new ArgumentList(arguments, isVariadic)
            { Line = line, Column = col };
    }
}