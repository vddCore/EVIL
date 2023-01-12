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
            var right = Assignment();
            return new AssignmentNode(left, right) {Line = line};
        }
    }
}