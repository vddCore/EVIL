using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private AstNode VariableDefinition()
        {
            var line = Match(Token.Var);

            var identifier = CurrentToken.Value;
            Match(Token.Identifier);

            AstNode initializer = null;
            
            if (CurrentToken.Type == TokenType.Assign)
            {
                Match(Token.Assign);
                initializer = AssignmentExpression();
            }

            return new VariableDefinitionNode(identifier, initializer) { Line = line };
        }
    }
}