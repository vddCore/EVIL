using System;
using Ceres.TranslationEngine.Scoping;

namespace Ceres.TranslationEngine
{
    public class DuplicateSymbolException : Exception
    {
        internal Symbol ExistingSymbol { get; }

        public string SymbolName => ExistingSymbol.Name;
        public int Line => ExistingSymbol.DefinedOnLine;
        public int Column => ExistingSymbol.DefinedOnColumn;

        internal DuplicateSymbolException(Symbol existingSymbol, string message) 
            : base(message)
        {
            ExistingSymbol = existingSymbol;
        }
    }
}