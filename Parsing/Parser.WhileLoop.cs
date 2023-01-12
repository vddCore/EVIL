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
            var expression = Comparison();
            Match(TokenType.Do);
            var statementList = LoopStatementList();
            Match(TokenType.End);

            return new WhileLoopNode(expression, statementList) { Line = line };
        }
    }
}
