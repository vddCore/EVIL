﻿using EVIL.Grammar.AST.Statements;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private ValStatement ReadWriteValStatement()
        {
            Match(Token.Rw);
            return ValStatement(true);
        }    
    }
}