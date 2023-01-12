using System.Linq;
using EVIL.AST.Base;
using EVIL.AST.Enums;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        private static readonly TokenType[] _comparisonOperators = new[]
        {
            TokenType.CompareLessThan,
            TokenType.CompareGreaterThan,
            TokenType.CompareLessOrEqualTo,
            TokenType.CompareGreaterOrEqualTo,
            TokenType.CompareEqual,
            TokenType.CompareNotEqual
        };
        
        private AstNode Comparison()
        {
            var node = Expression();
            var token = Scanner.State.CurrentToken;

            while (_comparisonOperators.Contains(token.Type))
            {
                token = Scanner.State.CurrentToken;

                if (token.Type == TokenType.CompareLessThan)
                {
                    var line = Match(TokenType.CompareLessThan);
                    node = new BinaryOperationNode(node, BinaryOperationType.Less, Expression()) {Line = line};
                }
                else if (token.Type == TokenType.CompareGreaterThan)
                {
                    var line = Match(TokenType.CompareGreaterThan);
                    node = new BinaryOperationNode(node, BinaryOperationType.Greater, Expression()) {Line = line};
                }
                else if (token.Type == TokenType.CompareLessOrEqualTo)
                {
                    var line = Match(TokenType.CompareLessOrEqualTo);
                    node = new BinaryOperationNode(node, BinaryOperationType.LessOrEqual, Expression()) {Line = line};
                }
                else if (token.Type == TokenType.CompareGreaterOrEqualTo)
                {
                    var line = Match(TokenType.CompareGreaterOrEqualTo);
                    node = new BinaryOperationNode(node, BinaryOperationType.GreaterOrEqual, Expression())
                        {Line = line};
                }
                else if (token.Type == TokenType.CompareEqual)
                {
                    var line = Match(TokenType.CompareEqual);
                    node = new BinaryOperationNode(node, BinaryOperationType.Equal, Expression()) {Line = line};
                }
                else if (token.Type == TokenType.CompareNotEqual)
                {
                    var line = Match(TokenType.CompareNotEqual);
                    node = new BinaryOperationNode(node, BinaryOperationType.NotEqual, Expression()) {Line = line};
                }
            }

            return node;
        }
    }
}