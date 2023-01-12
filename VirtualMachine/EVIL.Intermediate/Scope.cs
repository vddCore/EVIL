using EVIL.Intermediate.Collections;

namespace EVIL.Intermediate
{
    public class Scope
    {
        public Chunk Chunk { get; }
        public Scope Parent { get; }
        
        public TwoWayDictionary<string, SymbolInfo> Symbols { get; } = new();

        public Scope(Chunk chunk)
        {
            Chunk = chunk;
        }

        public Scope(Chunk chunk, Scope parent)
            : this(chunk)
        {
            Parent = parent;
        }

        public SymbolInfo Find(string name)
        {
            var scope = this;

            while (scope != null)
            {
                if (scope.Symbols.TryGetByKey(name, out var sym))
                    return sym;

                scope = scope.Parent;
            }

            return SymbolInfo.Undefined;
        }
        
        public SymbolInfo DefineLocal(string name)
        {
            if (IsDefined(name))
                throw new DuplicateSymbolException($"Symbol '{name}' was already defined in the current scope.", name);

            var sym = new SymbolInfo(Chunk.LocalCount++, SymbolInfo.SymbolType.Local);
            Symbols.Add(name, sym);

            return sym;
        }

        public SymbolInfo DefineParameter(string name)
        {
            if (IsDefined(name))
                throw new DuplicateSymbolException($"Symbol '{name}' was already defined in the current scope.", name);

            var sym = new SymbolInfo(Chunk.ParameterCount++, SymbolInfo.SymbolType.Parameter);
            Symbols.Add(name, sym);

            return sym;
        }

        public void UndefineLocal(string name)
        {
            if (!IsDefined(name))
                throw new DuplicateSymbolException($"Symbol '{name}' was never defined in the current scope.", name);

            Symbols.RemoveByKey(name);
        }
        
        public bool IsDefined(string name)
        {
            return Symbols.TryGetByKey(name, out _);
        }
    }
}