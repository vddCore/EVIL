using System.Collections.Generic;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private List<TokenType> _shiftOperators = new()
        {
            TokenType.ShiftLeft,
            TokenType.ShiftRight
        };

        private AstNode ShiftExpression()
        {
            var node = AdditiveExpression();
            var token = Scanner.State.CurrentToken;

            while (_shiftOperators.Contains(token.Type))
            {
                if (token.Type == TokenType.ShiftLeft)
                {
                    var line = Match(TokenType.ShiftLeft);
                    node = new BinaryOperationNode(node, AdditiveExpression(), BinaryOperationType.ShiftLeft)
                        {Line = line};
                }
                else if (token.Type == TokenType.ShiftRight)
                {
                    var line = Match(TokenType.ShiftRight);
                    node = new BinaryOperationNode(node, AdditiveExpression(), BinaryOperationType.ShiftRight)
                        {Line = line};
                }
                
                token = Scanner.State.CurrentToken;
            }

            return node;
        }
    }
}