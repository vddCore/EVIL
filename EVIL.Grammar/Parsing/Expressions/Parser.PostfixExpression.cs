using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private List<TokenType> _postfixOperators = new()
        {
            TokenType.LParenthesis,
            TokenType.LBracket,
            TokenType.Dot
        };

        private AstNode PostfixExpression()
        {
            var node = PrimaryExpression();
            var token = Scanner.State.CurrentToken;

            while (_postfixOperators.Contains(token.Type))
            {
                if (token.Type == TokenType.LParenthesis)
                {
                    node = FunctionCall(node);
                }
                else if (token.Type == TokenType.LBracket || token.Type == TokenType.Dot)
                {
                    node = Indexing(node);
                }

                token = Scanner.State.CurrentToken;
            }

            return node;
        }
    }
}