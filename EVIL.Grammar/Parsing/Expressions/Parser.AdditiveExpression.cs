using System.Linq;
using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private static readonly TokenType[] _additiveOperators = new[]
        {
            TokenType.Plus,
            TokenType.Minus,
        };

        private Expression AdditiveExpression()
        {
            var node = MultiplicativeExpression();
            var token = CurrentToken;

            while (_additiveOperators.Contains(token.Type))
            {
                if (token.Type == TokenType.Plus)
                {
                    var line = Match(Token.Plus);
                    node = new BinaryExpression(node, MultiplicativeExpression(), BinaryOperationType.Plus)
                        {Line = line};
                }
                else if (token.Type == TokenType.Minus)
                {
                    var line = Match(Token.Minus);
                    node = new BinaryExpression(node, MultiplicativeExpression(), BinaryOperationType.Minus)
                        {Line = line};
                }

                token = CurrentToken;
            }

            return node;
        }
    }
}