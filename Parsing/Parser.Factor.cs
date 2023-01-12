using System.Collections.Generic;
using EVIL.AST.Base;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        private List<TokenType> _suffixOperators = new()
        {
            TokenType.Increment,
            TokenType.Decrement,
            TokenType.LParenthesis,
            TokenType.LBracket
        };

        private AstNode Factor()
        {
            var node = Terminal();
            var token = Scanner.State.CurrentToken;

            while (_suffixOperators.Contains(token.Type))
            {
                token = Scanner.State.CurrentToken;

                if (token.Type == TokenType.LParenthesis)
                {
                    node = FunctionCall(node);
                }
                else if (token.Type == TokenType.LBracket)
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
            }

            return node;
        }
    }
}