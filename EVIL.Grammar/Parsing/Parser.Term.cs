using System.Linq;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private static readonly TokenType[] _termOperators = new[]
{
            TokenType.Multiply,
            TokenType.Divide,
            TokenType.BitwiseAnd,
            TokenType.BitwiseOr,
            TokenType.BitwiseXor,
            TokenType.Modulo,
            TokenType.ExistsIn
        };

        private AstNode Term()
        {
            var node = Factor();
            var token = Scanner.State.CurrentToken;

            while (_termOperators.Contains(token.Type))
            {
                token = Scanner.State.CurrentToken;

                if (token.Type == TokenType.Multiply)
                {
                    var line = Match(TokenType.Multiply);
                    node = new BinaryOperationNode(node, Factor(), BinaryOperationType.Multiply) { Line = line };
                }
                else if (token.Type == TokenType.Divide)
                {
                    var line = Match(TokenType.Divide);
                    node = new BinaryOperationNode(node, Factor(), BinaryOperationType.Divide) { Line = line };
                }
                else if (token.Type == TokenType.BitwiseAnd)
                {
                    var line = Match(TokenType.BitwiseAnd);
                    node = new BinaryOperationNode(node, Factor(), BinaryOperationType.BitwiseAnd) { Line = line };
                }
                else if (token.Type == TokenType.BitwiseOr)
                {
                    var line = Match(TokenType.BitwiseOr);
                    node = new BinaryOperationNode(node, Factor(), BinaryOperationType.BitwiseOr) { Line = line };
                }
                else if (token.Type == TokenType.BitwiseXor)
                {
                    var line = Match(TokenType.BitwiseXor);
                    node = new BinaryOperationNode(node, Factor(), BinaryOperationType.BitwiseXor) { Line = line };
                }
                else if (token.Type == TokenType.Modulo)
                {
                    var line = Match(TokenType.Modulo);
                    node = new BinaryOperationNode(node, Factor(), BinaryOperationType.Modulo) { Line = line };
                }
                else if (token.Type == TokenType.ExistsIn)
                {
                    var line = Match(TokenType.ExistsIn);
                    node = new BinaryOperationNode(node, Factor(), BinaryOperationType.ExistsIn) { Line = line };
                }
            }
            return node;
        }
    }
}
