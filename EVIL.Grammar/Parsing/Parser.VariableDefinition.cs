using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode VariableDefinition()
        {
            var line = Match(TokenType.Var);

            var variable = Variable();
            var assignment = Assignment(variable);

            return new VariableDefinitionNode(variable, assignment) { Line = line };
        }
    }
}