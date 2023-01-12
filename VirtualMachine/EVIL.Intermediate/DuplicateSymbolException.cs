using System;

namespace EVIL.Intermediate
{
    public class DuplicateSymbolException : Exception
    {
        public string SymbolName { get; }

        public DuplicateSymbolException(string message, string symbolName)
            : base(message)
        {
            SymbolName = symbolName;
        }
    }
}