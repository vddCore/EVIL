using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode DoWhileLoop()
        {
            Match(Token.Do);

            var statements = LoopDescent(() => Statement());
            
            Match(Token.While);
            Match(Token.LParenthesis);
            var conditionExpression = AssignmentExpression();
            Match(Token.RParenthesis);

            return new DoWhileLoopNode(conditionExpression, statements);
        }
    }
}