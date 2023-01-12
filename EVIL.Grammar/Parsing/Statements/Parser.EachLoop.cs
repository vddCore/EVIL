using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode EachLoop()
        {
            var line = Match(TokenType.Each);

            Match(TokenType.LParenthesis);
            var keyVar = VariableDefinition();
            Match(TokenType.Comma);
            var valueVar = VariableDefinition();

            Match(TokenType.Colon);

            var tableNode = AssignmentExpression();
            
            Match(TokenType.RParenthesis);

            return new EachLoopNode(keyVar, valueVar, tableNode, BlockStatement()) { Line = line };
        }
    }
}
