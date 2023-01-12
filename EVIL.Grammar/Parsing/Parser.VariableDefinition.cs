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

            var identifier = Scanner.State.CurrentToken.Value.ToString();
            Match(TokenType.Identifier);
            
            var assignment = Assignment(new VariableNode(identifier));

            return new VarNode(identifier, assignment) { Line = line };
        }
    }
}