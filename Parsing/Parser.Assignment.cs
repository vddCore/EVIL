using EVIL.AST.Base;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        private AssignmentNode Assignment(AstNode left)
        {           
            var line = Match(TokenType.Assign);
            var right = Operator();
            return new AssignmentNode(left, right) {Line = line};
        }
    }
}