using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
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
                var line = Match(Token.LogicalOr);
                node = new BinaryExpression(node, LogicalAndExpression(), BinaryOperationType.LogicalOr) {Line = line};
                
                token = CurrentToken;
            }

            return node;
        }
    }
}