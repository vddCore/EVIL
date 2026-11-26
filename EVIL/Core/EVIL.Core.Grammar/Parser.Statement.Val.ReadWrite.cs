namespace EVIL.Core.Grammar;

using EVIL.Core.Grammar.AST.Statements;
using EVIL.Core.Lexical;

public partial class Parser
{
    private ValStatement ReadWriteValStatement()
    {
        Match(Token.Rw);
        return ValStatement(true);
    }    
}