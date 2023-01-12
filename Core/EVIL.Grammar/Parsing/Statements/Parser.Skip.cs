﻿using EVIL.Grammar.AST;
using EVIL.Grammar.AST.Nodes;
using EVIL.Lexical;

namespace EVIL.Grammar.Parsing
{
    public partial class Parser
    {
        private SkipStatement Skip()
        {
            if (_loopDescent == 0)
            {
                throw new ParserException("Unexpected 'skip' outside of a loop.", Lexer.State);
            }
            
            var line = Match(Token.Skip);
            return new SkipStatement { Line = line };
        }
    }
}