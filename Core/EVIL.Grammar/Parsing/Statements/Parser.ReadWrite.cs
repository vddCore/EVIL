using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        public VarStatement ReadWriteVariableDefinition()
        {
            Match(Token.Rw);
            return VariableDefinition(true);
        }    
    }
}