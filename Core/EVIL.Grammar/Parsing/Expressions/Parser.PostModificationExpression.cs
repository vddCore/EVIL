using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private List<TokenType> _postModificationOperators = new()
        {
            TokenType.Increment,
            TokenType.Decrement
        };
        
        private Expression PostModificationExpression()
        {
            var node = PostfixExpression();
            var token = CurrentToken;

            while (_postModificationOperators.Contains(token.Type))
            {
                if (token.Type == TokenType.Increment)
                {
                    DisallowPrevious(TokenType.Decrement, TokenType.Increment);

                    var line = Match(Token.Increment);
                    node = new IncrementationExpression(node, false) {Line = line};
                }
                else if (token.Type == TokenType.Decrement)
                {
                    DisallowPrevious(TokenType.Decrement, TokenType.Increment);

                    var line = Match(Token.Decrement);
                    node = new DecrementationExpression(node, false) {Line = line};
                }

                token = CurrentToken;
            }

            return node;
        }
    }
}