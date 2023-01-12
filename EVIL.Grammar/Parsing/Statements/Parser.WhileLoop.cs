using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode WhileLoop()
        {
            var line = Match(Token.While);
            Match(Token.LParenthesis);
            var expression = AssignmentExpression();
            Match(Token.RParenthesis);

            var statements = LoopDescent(BlockStatement);
            
            return new WhileLoopNode(expression, statements) { Line = line };
        }
    }
}
