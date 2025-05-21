namespace EVIL.Grammar.Parsing;

using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

public partial class Parser
{
    private ValStatement ReadWriteValStatement()
    {
        Match(Token.Rw);
        return ValStatement(true);
    }    
}