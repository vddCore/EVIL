using System;

namespace EVIL.Intermediate
{
    public class DuplicateSymbolException : Exception
    {
        public string SymbolName { get; }

        public DuplicateSymbolException(string symbolName)
            : base($"Symbol '{symbolName}' was already defined in the current scope.")
        {
            SymbolName = symbolName;
        }
    }
}