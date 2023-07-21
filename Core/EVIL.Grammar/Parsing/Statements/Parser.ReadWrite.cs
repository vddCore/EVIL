using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        public VariableDefinition ReadWriteVariableDefinition()
        {
            Match(Token.Rw);
            return VariableDefinition(true);
        }    
    }
}