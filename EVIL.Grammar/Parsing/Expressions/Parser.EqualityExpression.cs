using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private List<TokenType> _equalityOperators = new()
        {
            TokenType.Equal,
            TokenType.NotEqual
        };
        
        private AstNode EqualityExpression()
        {
            var node = RelationalExpression();
            var token = Scanner.State.CurrentToken;

            while (_equalityOperators.Contains(token.Type))
            {
                if (token.Type == TokenType.Equal)
                {
                    var line = Match(TokenType.Equal);
                    node = new BinaryOperationNode(node, RelationalExpression(), BinaryOperationType.Equal) {Line = line};
                }
                else if (token.Type == TokenType.NotEqual)
                {
                    var line = Match(TokenType.NotEqual);
                    node = new BinaryOperationNode(node, RelationalExpression(), BinaryOperationType.NotEqual) {Line = line};
                }
                
                token = Scanner.State.CurrentToken;
            }

            return node;
        }
    }
}