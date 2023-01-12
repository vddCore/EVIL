using EVIL.AST.Base;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        private AstNode WhileLoop()
        {
            var line = Match(TokenType.While);
            Match(TokenType.LParenthesis);
            var expression = LogicalExpression();
            Match(TokenType.RParenthesis);
            
            Match(TokenType.LBrace);
            var statementList = LoopStatementList();
            Match(TokenType.RBrace);

            return new WhileLoopNode(expression, statementList) { Line = line };
        }
    }
}
