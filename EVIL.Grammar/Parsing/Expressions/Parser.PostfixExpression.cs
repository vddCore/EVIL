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
            TokenType.Increment,
            TokenType.Decrement,
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
                else if (token.Type == TokenType.Increment)
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