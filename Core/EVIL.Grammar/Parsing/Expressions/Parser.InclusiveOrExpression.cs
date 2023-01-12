using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private Expression InclusiveOrExpression()
        {
            var node = ExclusiveOrExpression();
            var token = CurrentToken;

            while (token.Type == TokenType.BitwiseOr)
            {
                var (line, col) = Match(Token.BitwiseOr);

                node = new BinaryExpression(node, ExclusiveOrExpression(), BinaryOperationType.BitwiseOr)
                    { Line = line, Column = col };

                token = CurrentToken;
            }

            return node;
        }
    }
}