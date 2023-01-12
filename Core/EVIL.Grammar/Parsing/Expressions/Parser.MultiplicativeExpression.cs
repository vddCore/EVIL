using System.Linq;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private static readonly TokenType[] _multiplicativeOperators = new[]
{
            TokenType.Multiply,
            TokenType.Divide,
            TokenType.Modulo
        };

        private Expression MultiplicativeExpression()
        {
            var node = UnaryExpression();
            var token = CurrentToken;

            while (_multiplicativeOperators.Contains(token.Type))
            {
                if (token.Type == TokenType.Multiply)
                {
                    var line = Match(Token.Multiply);
                    node = new BinaryExpression(node, UnaryExpression(), BinaryOperationType.Multiply) { Line = line };
                }
                else if (token.Type == TokenType.Divide)
                {
                    var line = Match(Token.Divide);
                    node = new BinaryExpression(node, UnaryExpression(), BinaryOperationType.Divide) { Line = line };
                }
                else if (token.Type == TokenType.Modulo)
                {
                    var line = Match(Token.Modulo);
                    node = new BinaryExpression(node, UnaryExpression(), BinaryOperationType.Modulo) { Line = line };
                }                
                
                token = CurrentToken;
            }
            
            return node;
        }
    }
}
