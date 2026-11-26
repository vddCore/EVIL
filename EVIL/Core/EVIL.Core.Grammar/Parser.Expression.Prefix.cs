namespace EVIL.Core.Grammar;

using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Lexical;

public partial class Parser
{
    private Expression PrefixExpression()
    {
        var token = CurrentToken;

        switch (token.Type)
        {
            case TokenType.Increment:
            {
                var (line, col) = Match(Token.Increment);
                var target = UnaryExpression();
                
                if (target is IndexerExpression { IsRange: true }
                           or IndexerExpression { IsFromEndIndex: true })
                {
                    throw new ParserException(
                        "Range and from-end indexers are not assignable.",
                        (line, col)
                    );
                }

                return new IncrementationExpression(target, true)
                    { Line = line, Column = col };
            }
            
            case TokenType.Decrement:
            {
                var (line, col) = Match(Token.Decrement);
                var target = UnaryExpression();
                
                if (target is IndexerExpression { IsRange: true }
                           or IndexerExpression { IsFromEndIndex: true })
                {
                    throw new ParserException(
                        "Range and from-end indexers are not assignable.",
                        (line, col)
                    );
                }

                return new DecrementationExpression(target, true)
                    { Line = line, Column = col };
            }

            default:
            {
                return UnaryExpression();
            }
        }
    }
}