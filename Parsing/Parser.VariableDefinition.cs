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

            var variable = Variable(null, true);
            var assignment = Assignment(variable);

            return new VariableDefinitionNode(variable, assignment) { Line = line };
        }
    }
}