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
                var line = Match(Token.NameOf);
                return new NameOfExpression(UnaryExpression()) {Line = line};
            }
            else if (token.Type == TokenType.Length)
            {
                var line = Match(Token.Length);
                return new UnaryExpression(UnaryExpression(), UnaryOperationType.Length) { Line = line };
            }

            return UnaryExpression();
        }
    }
}