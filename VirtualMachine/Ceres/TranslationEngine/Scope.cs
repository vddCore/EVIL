using System.Collections.Generic;

namespace Ceres.TranslationEngine
{
    public class Scope
    {
        public Dictionary<string, Symbol> Symbols { get; } = new();

        public Scope? Parent { get; }

        private Scope()
        {
        }

        private Scope(Scope parent)
        {
            Parent = parent;
        }

        public Symbol DefineLocal(string name, int id, bool writeable)
        {
            var sym = Find(name);
            
            if (sym != null)
            {
                throw new DuplicateSymbolException(
                    name,
                    $"Symbol '{name}' was already defined in the current scope."
                );
            }

            var symbol = new Symbol(name, id, Symbol.SymbolType.Local, writeable);
            Symbols.Add(name, symbol);

            return symbol;
        }

        public Symbol DefineParameter(string name, int id, bool writeable)
        {
            var sym = Find(name);
            
            if (sym != null)
            {
                throw new DuplicateSymbolException(
                    name,
                    $"Symbol '{name}' was already defined in the current scope."
                );
            }

            var symbol = new Symbol(name, id, Symbol.SymbolType.Parameter, writeable);
            Symbols.Add(name, symbol);

            return symbol;
        }

        public Symbol? Find(string name)
        {
            var currentScope = this;

            while (currentScope != null)
            {
                if (currentScope.Symbols.TryGetValue(name, out var sym))
                    return sym;

                currentScope = currentScope.Parent;
            }

            return null;
        }

        public Scope Descend()
            => new(this);

        public static Scope CreateRoot()
            => new();
    }
}