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
        
        private AstNode PostModificationExpression()
        {
            var node = PostfixExpression();
            var token = Scanner.State.CurrentToken;

            while (_postModificationOperators.Contains(token.Type))
            {
                if (token.Type == TokenType.Increment)
                {
                    DisallowPrevious(TokenType.Decrement, TokenType.Increment);

                    var line = Match(TokenType.Increment);
                    node = new IncrementationNode(node, false) {Line = line};
                }
                else if (token.Type == TokenType.Decrement)
                {
                    DisallowPrevious(TokenType.Decrement, TokenType.Increment);

                    var line = Match(TokenType.Decrement);
                    node = new DecrementationNode(node, false) {Line = line};
                }

                token = Scanner.State.CurrentToken;
            }

            return node;
        }
    }
}