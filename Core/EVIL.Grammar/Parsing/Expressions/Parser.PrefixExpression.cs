using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private Expression PrefixExpression()
        {
            var token = CurrentToken;

            if (token.Type == TokenType.Increment)
            {
                var (line, col) = Match(Token.Increment);
                
                return new IncrementationExpression(RuntimeExpression(), true) 
                    { Line = line, Column = col };
            }
            else if (token.Type == TokenType.Decrement)
            {
                var (line, col) = Match(Token.Decrement);
                
                return new DecrementationExpression(RuntimeExpression(), true) 
                    { Line = line, Column = col };
            }

            return RuntimeExpression();
        }
    }
}