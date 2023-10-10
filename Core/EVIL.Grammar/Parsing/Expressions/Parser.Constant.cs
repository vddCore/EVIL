using System.Globalization;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Constants;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private ConstantExpression Constant()
        {
            var token = CurrentToken;

            switch (token.Type)
            {
                case TokenType.Minus:
                case TokenType.Number:
                {
                    var (line, col) = (0, 0);
                    var sign = 1;
                    if (token.Type == TokenType.Minus)
                    {
                        sign = -1;
                        (line, col) = Match(Token.Minus);
                        token = CurrentToken;
                        Match(Token.Number);
                    }
                    else
                    {
                        (line, col) = Match(Token.Number);
                    }

                    return new NumberConstant(sign * double.Parse(token.Value!))
                        { Line = line, Column = col };
                }
                case TokenType.NaN:
                {
                    var (line, col) = Match(Token.NaN);
                    
                    return new NumberConstant(double.NaN)
                        { Line = line, Column = col };
                }
                case TokenType.Infinity:
                {
                    var (line, col) = Match(Token.Infinity);

                    return new NumberConstant(double.PositiveInfinity)
                        { Line = line, Column = col };
                }
                case TokenType.HexInteger:
                {
                    var (line, col) = Match(Token.HexInteger);

                    return new NumberConstant(long.Parse(token.Value!, NumberStyles.HexNumber))
                        { Line = line, Column = col };
                }
                case TokenType.True:
                {
                    var (line, col) = Match(Token.True);

                    return new BooleanConstant(true)
                        { Line = line, Column = col };
                }
                case TokenType.False:
                {
                    var (line, col) = Match(Token.False);

                    return new BooleanConstant(false)
                        { Line = line, Column = col };
                }
                case TokenType.String:
                {
                    var (line, col) = Match(Token.String);

                    return new StringConstant(token.Value!)
                        { Line = line, Column = col };
                }
                case TokenType.Nil:
                {
                    var (line, col) = Match(Token.Nil);

                    return new NilConstant
                        { Line = line, Column = col };
                }
                default:
                {
                    throw new ParserException(
                        $"Unexpected token {token}",
                        (_lexer.State.Line, _lexer.State.Column)
                    );
                }
            }
        }
    }
}