using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        public ValStatement ReadWriteValStatement()
        {
            Match(Token.Rw);
            return ValStatement(true);
        }    
    }
}