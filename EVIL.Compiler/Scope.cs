using System.Collections.Generic;

namespace EVIL.Compiler
{
    public class Scope
    {
        private Dictionary<string, SymbolInfo> _symbols = new();

        public Scope Parent { get; }
        public IReadOnlyDictionary<string, SymbolInfo> Symbols => _symbols;

        public Scope()
        {
        }
        
        private Scope(Scope parent)
        {
            Parent = parent;
        }
        
        public Scope Descend()
        {
            return new(this);
        }

        public void Clear()
        {
            _symbols.Clear();
        }
        
        public SymbolInfo Define(string name, SymbolType type, object value = null)
        {
            if (_symbols.ContainsKey(name))
                throw new CompilerException($"Symbol '{name}' was already defined in this scope.");

            var symbol = new SymbolInfo
            {
                Type = type,
                Value = value
            };

            _symbols.Add(name, symbol);

            return symbol;
        }

        public void Undefine(string name)
        {
            if (!_symbols.ContainsKey(name))
                throw new CompilerException($"Symbol '{name}' was never defined in this scope.");

            _symbols.Remove(name);
        }

        public SymbolInfo FindInScope(string name, out Scope owner)
        {
            var current = this;

            while (current != null)
            {
                if (_symbols.ContainsKey(name))
                {
                    owner = current;
                    return _symbols[name];
                }

                current = current.Parent;
            }

            throw new CompilerException($"Symbol '{name}' was not defined in this scope.");
        }
    }
}