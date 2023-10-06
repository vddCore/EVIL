using EVIL.Grammar.AST.Base;
using EVIL.Grammar.AST.Expressions;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private Expression ConditionalExpression()
        {
            var node = LogicalOrExpression();

            if (CurrentToken.Type == TokenType.QuestionMark)
            {
                var (line, col) = Match(Token.QuestionMark);
                var trueExpression = AssignmentExpression();
                Match(Token.Colon);
                var falseExpression = ConditionalExpression();

                return new ConditionalExpression(node, trueExpression, falseExpression)
                    { Line = line, Column = col };
            }

            return node;
        }
    }
}