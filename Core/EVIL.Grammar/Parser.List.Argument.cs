namespace EVIL.Grammar;

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
            switch (CurrentToken.Type)
            {
                case TokenType.EOF:
                {
                    throw new ParserException(
                        "Unexpected EOF in argument list.",
                        (line, col)
                    );
                }
                
                case TokenType.Asterisk:
                {
                    var (xline, xcol) = Match(Token.Asterisk);
                    
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
                    break;
                }

                default:
                {
                    arguments.Add(AssignmentExpression());
                    break;
                }
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