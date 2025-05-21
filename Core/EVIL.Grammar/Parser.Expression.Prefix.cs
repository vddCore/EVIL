namespace EVIL.Grammar;

using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

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
                
                return new IncrementationExpression(UnaryExpression(), true) 
                    { Line = line, Column = col };
            }
            
            case TokenType.Decrement:
            {
                var (line, col) = Match(Token.Decrement);
                
                return new DecrementationExpression(UnaryExpression(), true) 
                    { Line = line, Column = col };
            }

            default:
            {
                return UnaryExpression();
            }
        }
    }
}