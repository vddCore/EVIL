namespace EVIL.Grammar;

using EVIL.Grammar.AST.Constants;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

public partial class Parser
{
    private ErrorExpression ErrorExpression()
    {
        var (line, col) = Match(Token.Error);

        StringConstant? implicitMessage = null;

        if (CurrentToken.Type == TokenType.LParenthesis)
        {
            Match(Token.LParenthesis);
            var constant = ConstantExpression();

            if (constant is not StringConstant stringConstant)
            {
                throw new ParserException(
                    "Expected a string for the implicit error message.",
                    (constant.Line, constant.Column)
                );
            }

            implicitMessage = stringConstant;
            Match(Token.RParenthesis);
        }

        TableExpression? userData = null;

        if (implicitMessage != null)
        {
            if (CurrentToken.Type == TokenType.LBrace)
            {
                userData = TableExpression();
            }
        }
        else
        {
            userData = TableExpression();
        }
            
        return new ErrorExpression(implicitMessage, userData)
            { Line = line, Column = col };
    }
}