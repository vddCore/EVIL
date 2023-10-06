using System.Collections.Generic;
using Ceres.ExecutionEngine.Diagnostics;

namespace Ceres.TranslationEngine.Scoping
{
    internal class Scope
    {
        public Scope? Parent { get; }

        public Dictionary<string, Symbol> Symbols { get; } = new();
        public Dictionary<string, Symbol> Closures { get; } = new();

        private Scope()
        {
        }

        private Scope(Scope parent)
        {
            Parent = parent;
        }

        public Symbol DefineLocal(
            string name,
            int id,
            bool writeable,
            int definedOnLine,
            int definedOnColumn)
        {
            var sym = Find(name);

            if (sym != null && !sym.Value.IsClosure)
            {
                throw new DuplicateSymbolException(
                    name,
                    $"Symbol '{name}' was already defined in the current scope."
                );
            }

            var symbol = new Symbol(
                name,
                id,
                Symbol.SymbolType.Local,
                writeable,
                definedOnLine,
                definedOnColumn,
                null
            );
            
            Symbols.Add(name, symbol);
            return symbol;
        }

        public Symbol DefineParameter(
            string name,
            int id,
            bool writeable,
            int definedOnLine,
            int definedOnColumn)
        {
            var sym = Find(name);

            if (sym != null && !sym.Value.IsClosure)
            {
                throw new DuplicateSymbolException(
                    name,
                    $"Symbol '{name}' was already defined in the current scope."
                );
            }

            var symbol = new Symbol(
                name,
                id,
                Symbol.SymbolType.Parameter,
                writeable,
                definedOnLine,
                definedOnColumn,
                null
            );
            
            Symbols.Add(name, symbol);
            return symbol;
        }

        public Symbol DefineClosure(
            string name,
            int id,
            bool writeable,
            int definedOnLine,
            int definedOnColumn,
            ClosureInfo closureInfo
        )
        {
            var sym = Find(name);

            if (sym != null)
            {
                throw new DuplicateSymbolException(
                    name,
                    $"Closure '{name}' was already defined in the current scope."
                );
            }

            var symbol = new Symbol(
                name,
                id,
                Symbol.SymbolType.Closure,
                writeable,
                definedOnLine,
                definedOnColumn,
                closureInfo
            );

            Closures.Add(name, symbol);
            return symbol;
        }

        public (bool IsClosure, Symbol Symbol)? Find(string name)
        {
            var currentScope = this;

            while (currentScope != null)
            {
                if (currentScope.Symbols.TryGetValue(name, out var symbol))
                    return (false, symbol);

                if (currentScope.Closures.TryGetValue(name, out var closure))
                    return (true, closure);
                
                currentScope = currentScope.Parent;
            }

            return null;
        }

        public bool IsSymbolWriteable(string identifier, out (bool IsClosure, Symbol Symbol)? sym)
        {
            sym = Find(identifier);

            if (sym == null)
            {
                // Globals are always writeable.
                return true;
            }

            return sym.Value.Symbol.ReadWrite;
        }

        public void Clear()
            => Symbols.Clear();

        public Scope Descend()
            => new(this);

        public static Scope CreateRoot()
            => new();
    }
}