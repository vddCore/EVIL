using System.Linq;
using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
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
            TokenType.In
        };

        private Expression RelationalExpression()
        {
            var node = ShiftExpression();
            var token = CurrentToken;

            while (_comparisonOperators.Contains(token.Type))
            {
                if (token.Type == TokenType.LessThan)
                {
                    var (line, col) = Match(Token.LessThan);

                    node = new BinaryExpression(node, ShiftExpression(), BinaryOperationType.Less)
                        { Line = line, Column = col };
                }
                else if (token.Type == TokenType.GreaterThan)
                {
                    var (line, col) = Match(Token.GreaterThan);

                    node = new BinaryExpression(node, ShiftExpression(), BinaryOperationType.Greater)
                        { Line = line, Column = col };
                }
                else if (token.Type == TokenType.LessThanOrEqual)
                {
                    var (line, col) = Match(Token.LessThanOrEqual);

                    node = new BinaryExpression(node, ShiftExpression(), BinaryOperationType.LessOrEqual)
                        { Line = line, Column = col };
                }
                else if (token.Type == TokenType.GreaterThanOrEqual)
                {
                    var (line, col) = Match(Token.GreaterThanOrEqual);

                    node = new BinaryExpression(node, ShiftExpression(), BinaryOperationType.GreaterOrEqual)
                        { Line = line, Column = col };
                }
                else if (token.Type == TokenType.In)
                {
                    var (line, col) = Match(Token.In);

                    node = new BinaryExpression(node, ShiftExpression(), BinaryOperationType.ExistsIn)
                        { Line = line, Column = col };
                }

                token = CurrentToken;
            }

            return node;
        }
    }
}