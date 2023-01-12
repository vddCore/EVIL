using EVIL.AST.Base;
using EVIL.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Parsing
{
    public partial class Parser
    {
        private AstNode VariableDefinition()
        {
            var line = Match(TokenType.Var);

            var identifier = Scanner.State.CurrentToken.Value.ToString();
            Match(TokenType.Identifier);

            var assignment = Assignment(identifier);

            return new VariableDefinitionNode(identifier, assignment) { Line = line };
        }
    }
}