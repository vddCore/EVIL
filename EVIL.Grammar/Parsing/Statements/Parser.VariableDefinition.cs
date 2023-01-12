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

            var identifier = CurrentToken.Value;
            Match(TokenType.Identifier);
            
            var assignment = AssignmentExpression(new VariableNode(identifier));

            return new VariableDefinitionNode(identifier, assignment) { Line = line };
        }
    }
}