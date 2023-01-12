using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private VariableDefinition VariableDefinition()
        {
            var line = Match(Token.Var);

            var identifier = CurrentToken.Value;
            Match(Token.Identifier);

            Expression initializer = null;
            
            if (CurrentToken.Type == TokenType.Assign)
            {
                Match(Token.Assign);
                initializer = AssignmentExpression();
            }

            return new VariableDefinition(identifier, initializer) { Line = line };
        }
    }
}