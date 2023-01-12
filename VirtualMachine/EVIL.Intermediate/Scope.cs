using EVIL.Intermediate.Collections;

namespace EVIL.Intermediate
{
    public class Scope
    {
        private static int GlobalCount { get; set; }
        private static TwoWayDictionary<string, SymbolInfo> Global { get; } = new();
        
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
            SymbolInfo sym;
            var scope = this;

            while (scope != null)
            {
                if (scope.Symbols.TryGetByKey(name, out sym))
                    return sym;

                scope = scope.Parent;
            }

            if (Global.TryGetByKey(name, out sym))
                return sym;
            
            return SymbolInfo.Undefined;
        }
        
        public SymbolInfo DefineLocal(string name)
        {
            if (IsLocalDefined(name))
                throw new DuplicateSymbolException($"Symbol '{name}' was already defined in the current scope.", name);

            var sym = new SymbolInfo(Chunk.LocalCount++, SymbolInfo.SymbolType.Local);
            Symbols.Add(name, sym);

            return sym;
        }

        public SymbolInfo DefineParameter(string name)
        {
            if (IsLocalDefined(name))
                throw new DuplicateSymbolException($"Local '{name}' was already defined in the current scope.", name);

            var sym = new SymbolInfo(Chunk.ParameterCount++, SymbolInfo.SymbolType.Parameter);
            Symbols.Add(name, sym);

            return sym;
        }
        
        public static SymbolInfo DefineGlobal(string name)
        {
            if (IsGlobalDefined(name))
                throw new DuplicateSymbolException($"Global '{name}' was already defined.", name);

            var sym = new SymbolInfo(GlobalCount++, SymbolInfo.SymbolType.Global);
            Global.Add(name, sym);

            return sym;
        }

        public void UndefineLocal(string name)
        {
            if (!IsLocalDefined(name))
                throw new DuplicateSymbolException($"Local '{name}' was never defined in the current scope.", name);

            Symbols.RemoveByKey(name);
        }

        public static bool IsGlobalDefined(string name)
        {
            return Global.TryGetByKey(name, out _);
        }
        
        public bool IsLocalDefined(string name)
        {
            return Symbols.TryGetByKey(name, out _);
        }

        public void Clear()
        {
            Symbols.Clear();
        }

        public static void ClearGlobals()
        {
            Global.Clear();
        }
    }
}