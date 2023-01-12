using EVIL.AST.Base;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        public AstNode TableAssignment(string identifier)
        {
            var variable = Variable(identifier);
            var line = Match(TokenType.LBracket);
            var keyExpression = Comparison();
            Match(TokenType.RBracket);

            Match(TokenType.Assign);

            var valueExpression = Comparison();

            return new TableAssignmentNode(variable, keyExpression, valueExpression) { Line = line };
        }
    }
}
