namespace EVIL.Core.Grammar;

using System.Linq;
using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Lexical;

public partial class Parser
{
    private static readonly TokenType[] _multiplicativeOperators =
    [
        TokenType.Asterisk,
        TokenType.Slash,
        TokenType.Modulo
    ];

    private Expression MultiplicativeExpression()
    {
        return MultiplicativeExpression(PatternExpression());
    }

    private Expression MultiplicativeExpression(Expression node)
    {
        var token = CurrentToken;

        while (_multiplicativeOperators.Contains(token.Type))
        {
            switch (token.Type)
            {
                case TokenType.Asterisk:
                {
                    var (line, col) = Match(Token.Asterisk);

                    node = new BinaryExpression(node, PatternExpression(), BinaryOperationType.Multiply)
                        { Line = line, Column = col };
                    break;
                }
                
                case TokenType.Slash:
                {
                    var (line, col) = Match(Token.Slash);

                    node = new BinaryExpression(node, PatternExpression(), BinaryOperationType.Divide)
                        { Line = line, Column = col };
                    break;
                }
                
                case TokenType.Modulo:
                {
                    var (line, col) = Match(Token.Modulo);

                    node = new BinaryExpression(node, PatternExpression(), BinaryOperationType.Modulo)
                        { Line = line, Column = col };
                    break;
                }
            }

            token = CurrentToken;
        }

        return node;
    }
}