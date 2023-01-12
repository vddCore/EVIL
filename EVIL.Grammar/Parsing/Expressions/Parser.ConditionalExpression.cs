using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode ConditionalExpression()
        {
            var node = LogicalOrExpression();

            if (CurrentToken.Type == TokenType.QuestionMark)
            {
                Match(TokenType.QuestionMark);
                var trueExpression = AssignmentExpression();
                Match(TokenType.Colon);
                var falseExpression = ConditionalExpression();

                return new ConditionalExpressionNode(node, trueExpression, falseExpression);
            }
            
            return node;
        }
    }
}