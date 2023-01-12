using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode DoWhileLoop()
        {
            Match(TokenType.Do);
            var statements = BlockStatement();

            Match(TokenType.While);
            Match(TokenType.LParenthesis);
            var conditionExpression = AssignmentExpression();
            Match(TokenType.RParenthesis);

            return new DoWhileLoopNode(conditionExpression, statements);
        }
    }
}