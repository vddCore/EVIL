namespace EVIL.Grammar;

using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

public partial class Parser
{
    private Expression PrimaryExpression()
    {
        var token = CurrentToken;

        switch (token.Type)
        {
            case TokenType.LParenthesis:
            {
                var (line, col) = Match(Token.LParenthesis);

                var node = AssignmentExpression();
                node.Line = line;
                node.Column = col;

                Match(Token.RParenthesis);

                return node;
            }

            case TokenType.LBrace:
            {
                return TableExpression();
            }

            case TokenType.LBracket:
            case TokenType.Array:
            {
                return ArrayExpression();
            }

            case TokenType.Error:
            {
                return ErrorExpression();
            }

            case TokenType.Ellipsis:
            {
                var (line, col) = Match(Token.Ellipsis);

                return new ExtraArgumentsExpression(false)
                    { Line = line, Column = col };
            }

            case TokenType.Fn:
            {
                return FnExpression();
            }

            case TokenType.Identifier:
            {
                return SymbolReferenceExpression();
            }

            case TokenType.Self:
            {
                return SelfExpression();
            }

            case TokenType.TypeOf:
            {
                return TypeOfExpression();
            }

            case TokenType.Yield:
            {
                return YieldExpression();
            }

            default:
            {
                return ConstantExpression();
            }
        }
    }
}