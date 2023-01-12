using System.Collections.Generic;
using EVIL.AST.Base;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        private static readonly List<TokenType> _actionOperators = new()
        {
            TokenType.Assign,
            TokenType.LParenthesis,
            TokenType.LBracket,
            TokenType.Increment,
            TokenType.Decrement
        };
        
        private AstNode Operator()
        {
            var node = LogicalExpression();
            var token = Scanner.State.CurrentToken;

            while (_actionOperators.Contains(token.Type))
            {
                token = Scanner.State.CurrentToken;
                
                if (token.Type == TokenType.Assign)
                    node = Assignment(node);
                else if (token.Type == TokenType.LParenthesis)
                    node = FunctionCall(node);
                else if (token.Type == TokenType.LBracket)
                    node = Indexing(node);
                else if (token.Type == TokenType.Increment)
                    node = PostIncrementation(node);
                else if (token.Type == TokenType.Decrement)
                    node = PostDecrementation(node);
            }

            return node;
        }
    }
}