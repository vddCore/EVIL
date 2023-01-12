using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private Expression NameOfExpression()
        {
            var token = CurrentToken;
            
            if (token.Type == TokenType.NameOf)
            {
                var line = Match(Token.NameOf);
                return new NameOfExpression(UnaryExpression()) {Line = line};
            }

            return UnaryExpression();
        }
    }
}