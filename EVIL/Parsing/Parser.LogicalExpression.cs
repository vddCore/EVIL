using System.Linq;
using EVIL.AST.Base;
using EVIL.AST.Enums;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        private static readonly TokenType[] _logicalExpressionOperators = new[]
        {
            TokenType.And,
            TokenType.Or
        };

        private AstNode LogicalExpression()
        {
            var node = Comparison();
            var token = Scanner.State.CurrentToken;

            while (_logicalExpressionOperators.Contains(token.Type))
            {
                token = Scanner.State.CurrentToken;

                if (token.Type == TokenType.Or)
                {
                    var line = Match(TokenType.Or);
                    node = new BinaryOperationNode(node, BinaryOperationType.Or, Comparison()) { Line = line };
                }
                else if (token.Type == TokenType.And)
                {
                    var line = Match(TokenType.And);
                    node = new BinaryOperationNode(node, BinaryOperationType.And, Comparison()) { Line = line };
                }
            }
            
            return node;
        }
    }
}