using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private List<TokenType> _postfixOperators = new()
        {
            TokenType.LParenthesis,
            TokenType.LBracket,
            TokenType.Dot,
            TokenType.Increment,
            TokenType.Decrement
        };

        private Expression PostfixExpression()
        {
            var node = PrimaryExpression();
            var token = CurrentToken;

            __incdec:
            if (token.Type == TokenType.Increment)
            {
                var line = Match(Token.Increment);
                return new IncrementationExpression(node, false) { Line = line };
            }
            else if (token.Type == TokenType.Decrement)
            {
                var line = Match(Token.Decrement);
                return new DecrementationExpression(node, false) { Line = line };
            }

            while (_postfixOperators.Contains(token.Type))
            {
                if (token.Type == TokenType.Increment || token.Type == TokenType.Decrement)
                    goto __incdec;
                
                if (token.Type == TokenType.LParenthesis)
                {
                    node = FunctionCall(node);
                }
                else if (token.Type == TokenType.LBracket || token.Type == TokenType.Dot)
                {
                    node = Indexing(node);
                }

                token = CurrentToken;
            }

            return node;
        }
    }
}