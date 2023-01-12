using EVIL.Intermediate.Collections;

namespace EVIL.Intermediate
{
    public class Scope
    {
        public Executable Executable { get; }
        public Chunk Chunk { get; }
        public Scope Parent { get; }
        
        public TwoWayDictionary<string, SymbolInfo> Symbols { get; } = new();

        public Scope(Executable executable, Chunk chunk)
        {
            Executable = executable;
            Chunk = chunk;
        }

        public Scope(Executable executable, Chunk chunk, Scope parent)
            : this(executable, chunk)
        {
            Parent = parent;
        }

        public SymbolInfo Find(string name)
        {
            SymbolInfo sym;
            var scope = this;

            while (scope != null)
            {
                if (scope.Symbols.TryGetByKey(name, out sym))
                    return sym;

                scope = scope.Parent;
            }

            if (Executable.IsGlobalDefined(name))
                return SymbolInfo.Global;
            
            return SymbolInfo.Undefined;
        }
        
        public SymbolInfo DefineLocal(string name)
        {
            if (IsLocalDefined(name))
                throw new DuplicateSymbolException($"Local symbol '{name}' was already defined in the current scope.", name);

            var sym = new SymbolInfo(Chunk.Locals.Count, SymbolInfo.SymbolType.Local);
            Chunk.Locals.Add(name);
            
            Symbols.Add(name, sym);

            return sym;
        }

        public SymbolInfo DefineParameter(string name)
        {
            if (IsLocalDefined(name))
                throw new DuplicateSymbolException($"Local symbol '{name}' was already defined in the current scope.", name);

            var sym = new SymbolInfo(Chunk.Parameters.Count, SymbolInfo.SymbolType.Parameter);
            Chunk.Parameters.Add(name);
            
            Symbols.Add(name, sym);

            return sym;
        }
        
        public void UndefineLocal(string name)
        {
            if (!IsLocalDefined(name))
                throw new DuplicateSymbolException($"Local symbol '{name}' was never defined in the current scope.", name);

            Symbols.RemoveByKey(name);
        }
        
        public bool IsLocalDefined(string name)
        {
            return Symbols.TryGetByKey(name, out _);
        }

        public void Clear()
        {
            Symbols.Clear();
        }
    }
}