using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode WhileLoop()
        {
            var line = Match(TokenType.While);
            Match(TokenType.LParenthesis);
            var expression = AssignmentExpression();
            Match(TokenType.RParenthesis);

            var statements = LoopDescent(BlockStatement);
            
            return new WhileLoopNode(expression, statements) { Line = line };
        }
    }
}
