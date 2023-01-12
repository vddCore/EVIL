using System.Linq;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private static readonly TokenType[] _comparisonOperators = new[]
        {
            TokenType.LessThan,
            TokenType.GreaterThan,
            TokenType.LessThanOrEqual,
            TokenType.GreaterThanOrEqual,
        };

        private AstNode RelationalExpression()
        {
            var node = ShiftExpression();
            var token = Scanner.State.CurrentToken;

            while (_comparisonOperators.Contains(token.Type))
            {
                if (token.Type == TokenType.LessThan)
                {
                    var line = Match(TokenType.LessThan);
                    node = new BinaryOperationNode(node, ShiftExpression(), BinaryOperationType.Less) {Line = line};
                }
                else if (token.Type == TokenType.GreaterThan)
                {
                    var line = Match(TokenType.GreaterThan);
                    node = new BinaryOperationNode(node, ShiftExpression(), BinaryOperationType.Greater) {Line = line};
                }
                else if (token.Type == TokenType.LessThanOrEqual)
                {
                    var line = Match(TokenType.LessThanOrEqual);
                    node = new BinaryOperationNode(node, ShiftExpression(), BinaryOperationType.LessOrEqual)
                        {Line = line};
                }
                else if (token.Type == TokenType.GreaterThanOrEqual)
                {
                    var line = Match(TokenType.GreaterThanOrEqual);
                    node = new BinaryOperationNode(node, ShiftExpression(), BinaryOperationType.GreaterOrEqual)
                        {Line = line};
                }
                
                token = Scanner.State.CurrentToken;
            }

            return node;
        }
    }
}