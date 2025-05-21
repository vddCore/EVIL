namespace EVIL.Grammar.Parsing;

using System.Linq;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

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
        var node = PatternExpression();
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