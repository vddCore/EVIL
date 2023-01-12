using EVIL.AST.Base;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        private AstNode Assignment(string identifier = null, bool isLocal = false)
        {
            if (isLocal)
                Match(TokenType.LocalVar);

            var variable = Variable(identifier);

            var line = Match(TokenType.Assign);
            var right = LogicalExpression();

            return new AssignmentNode(variable, right, isLocal) { Line = line };
        }
    }
}
