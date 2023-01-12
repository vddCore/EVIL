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

            AstNode left = Variable(identifier);;
            if (Scanner.State.CurrentToken.Type == TokenType.LBracket)
            {
                left = Indexing(left);
            }

            var line = Match(TokenType.Assign);
            var right = LogicalExpression();

            return new AssignmentNode(left, right, isLocal) {Line = line};
        }
    }
}