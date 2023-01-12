using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private WhileStatement WhileLoop()
        {
            var line = Match(Token.While);
            Match(Token.LParenthesis);
            var expression = AssignmentExpression();
            Match(Token.RParenthesis);

            var statement = LoopDescent(() => Statement());
            
            return new WhileStatement(expression, statement) { Line = line };
        }
    }
}
