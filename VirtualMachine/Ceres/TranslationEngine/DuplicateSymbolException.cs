using System;

namespace Ceres.TranslationEngine
{
    public class DuplicateSymbolException : Exception
    {
        public string SymbolName { get; }

        public DuplicateSymbolException(string symbolName, string message) 
            : base(message)
        {
            SymbolName = symbolName;
        }
    }
}