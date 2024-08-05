using EVIL.Grammar.AST.Base;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private Expression PatternExpression()
        {
            if (CurrentToken.Type == TokenType.By)
            {
                return ByExpression();
            }
            else
            {
                var node = PrefixExpression();
                var token = CurrentToken;

                while (token.Type == TokenType.With)
                {
                    node = WithExpression(node);
                    token = CurrentToken;
                }

                return node;
            }
        }
    }
}