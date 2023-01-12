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

            AstNode initializer = null;
            
            if (CurrentToken.Type == TokenType.Assign)
            {
                Match(TokenType.Assign);
                initializer = AssignmentExpression();
            }

            return new VariableDefinitionNode(identifier, initializer) { Line = line };
        }
    }
}