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
            var expression = Assignment();
            Match(TokenType.RParenthesis);
            
            Match(TokenType.LBrace);
            var statementList = LoopStatementList();
            Match(TokenType.RBrace);

            return new WhileLoopNode(expression, statementList) { Line = line };
        }
    }
}
