using System.Linq;
using EVIL.AST.Base;
using EVIL.AST.Enums;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        private static readonly TokenType[] _termOperators = new[]
{
            TokenType.Multiply,
            TokenType.Divide,
            TokenType.Nand,
            TokenType.Modulo,
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
                    node = new BinaryOperationNode(node, BinaryOperationType.Multiply, Factor()) { Line = line };
                }
                else if (token.Type == TokenType.Divide)
                {
                    var line = Match(TokenType.Divide);
                    node = new BinaryOperationNode(node, BinaryOperationType.Divide, Factor()) { Line = line };
                }
                else if (token.Type == TokenType.Nand)
                {
                    var line = Match(TokenType.Nand);
                    node = new BinaryOperationNode(node, BinaryOperationType.Nand, Factor()) { Line = line };
                }
                else if (token.Type == TokenType.Modulo)
                {
                    var line = Match(TokenType.Modulo);
                    node = new BinaryOperationNode(node, BinaryOperationType.Modulo, Factor()) { Line = line };
                }
            }
            return node;
        }
    }
}
