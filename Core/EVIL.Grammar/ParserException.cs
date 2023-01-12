using System;
using EVIL.Lexical;

namespace EVIL.Grammar
{
    public class ParserException : Exception
    {
        public LexerState LexerState { get; }

        public ParserException(string message) : base(message) { }

        public ParserException(string message, LexerState lexerState) : base(message)
            => LexerState = lexerState;
    }
}
