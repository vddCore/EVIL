using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private Expression PrefixExpression()
        {
            var token = CurrentToken;

            if (token.Type == TokenType.NameOf)
            {
                var (line, col) = Match(Token.NameOf);
                
                return new NameOfExpression(UnaryExpression())
                    { Line = line, Column = col };
            }
            else if (token.Type == TokenType.Length)
            {
                var (line, col) = Match(Token.Length);
                
                return new UnaryExpression(UnaryExpression(), UnaryOperationType.Length) 
                    { Line = line, Column = col };
            }
            else if (token.Type == TokenType.Increment)
            {
                var (line, col) = Match(Token.Increment);
                
                return new IncrementationExpression(UnaryExpression(), true) 
                    { Line = line, Column = col };
            }
            else if (token.Type == TokenType.Decrement)
            {
                var (line, col) = Match(Token.Decrement);
                
                return new DecrementationExpression(UnaryExpression(), true) 
                    { Line = line, Column = col };
            }

            return UnaryExpression();
        }
    }
}