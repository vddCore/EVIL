using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode EachLoop()
        {
            var line = Match(Token.Each);

            Match(Token.LParenthesis);
            var keyVar = VariableDefinition();
            Match(Token.Comma);
            var valueVar = VariableDefinition();

            Match(Token.Colon);

            var tableNode = AssignmentExpression();
            
            Match(Token.RParenthesis);

            var statements = LoopDescent(BlockStatement);
            
            return new EachLoopNode(keyVar, valueVar, tableNode, statements) { Line = line };
        }
    }
}
