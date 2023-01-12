using System.Linq;
using EVIL.AST.Base;
using EVIL.AST.Enums;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        private static readonly TokenType[] _expressionOperators = new[]
        {
            TokenType.Plus,
            TokenType.Minus,
            TokenType.ShiftLeft,
            TokenType.ShiftRight
        };

        private AstNode Expression()
        {
            var node = Term();
            var token = Scanner.State.CurrentToken;

            while (_expressionOperators.Contains(token.Type))
            {
                token = Scanner.State.CurrentToken;

                if (token.Type == TokenType.Plus)
                {
                    var line = Match(TokenType.Plus);
                    node = new BinaryOperationNode(node, BinaryOperationType.Plus, Term()) { Line = line };
                }
                else if (token.Type == TokenType.Minus)
                {
                    var line = Match(TokenType.Minus);
                    node = new BinaryOperationNode(node, BinaryOperationType.Minus, Term()) { Line = line };
                }
                else if (token.Type == TokenType.ShiftLeft)
                {
                    var line = Match(TokenType.ShiftLeft);
                    node = new BinaryOperationNode(node, BinaryOperationType.ShiftLeft, Term()) { Line = line };
                }
                else if (token.Type == TokenType.ShiftRight)
                {
                    var line = Match(TokenType.ShiftRight);
                    node = new BinaryOperationNode(node, BinaryOperationType.ShiftRight, Term()) { Line = line };
                }
            }
            
            return node;
        }

    }
}
