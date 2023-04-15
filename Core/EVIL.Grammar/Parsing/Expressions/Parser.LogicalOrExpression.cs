using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private Expression LogicalOrExpression()
        {
            var node = LogicalAndExpression();
            var token = CurrentToken;

            while (token.Type == TokenType.LogicalOr)
            {
                var (line, col) = Match(Token.LogicalOr);
                
                node = new BinaryExpression(node, LogicalAndExpression(), BinaryOperationType.LogicalOr)
                    { Line = line, Column = col };

                token = CurrentToken;
            }

            return node;
        }
    }
}