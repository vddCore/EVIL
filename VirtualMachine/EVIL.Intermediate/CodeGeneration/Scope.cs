using EVIL.Intermediate.CodeGeneration;
using EVIL.Intermediate.CodeGeneration.Collections;

namespace EVIL.Intermediate
{
    public class Scope
    {
        private Compiler _compiler;
        
        public Executable Executable { get; }
        public Chunk Chunk { get; }
        public Scope Parent { get; }

        public TwoWayDictionary<string, SymbolInfo> Symbols { get; } = new();

        public Scope(Compiler compiler, Executable executable, Chunk chunk)
        {
            _compiler = compiler;
            
            Executable = executable;
            Chunk = chunk;
        }

        public Scope(Compiler compiler, Executable executable, Chunk chunk, Scope parent)
            : this(compiler, executable, chunk)
        {
            Parent = parent;
        }

        public (Scope, SymbolInfo) Find(string name)
        {
            SymbolInfo sym;
            var scope = this;

            while (scope != null)
            {
                if (scope.Symbols.TryGetByKey(name, out sym))
                    return (scope, sym);

                scope = scope.Parent;
            }

            if (_compiler.IsGlobalDefined(name))
                return (null, SymbolInfo.Global);
            
            return (null, SymbolInfo.Undefined);
        }

        public SymbolInfo DefineExtern(string name, string ownerChunkName, int ownerLocalId, bool isParam)
        {
            if (IsLocalDefined(name))
                throw new DuplicateSymbolException(name, _compiler.CurrentLine, _compiler.CurrentColumn);

            var sym = new SymbolInfo(Chunk.Externs.Count, SymbolInfo.SymbolType.Extern);
            Chunk.Externs.Add(new ExternInfo(name, ownerChunkName, ownerLocalId, isParam));

            Symbols.Add(name, sym);

            return sym;
        }
        
        public SymbolInfo DefineLocal(string name)
        {
            if (IsLocalDefined(name))
                throw new DuplicateSymbolException(name, _compiler.CurrentLine, _compiler.CurrentColumn);

            if (Symbols.Forward.ContainsKey(name))
            {
                return Symbols.Forward[name];
            }
            else
            {
                var sym = new SymbolInfo(Chunk.Locals.Count, SymbolInfo.SymbolType.Local);
                Chunk.Locals.Add(name);

                Symbols.Add(name, sym);
                return sym;
            }
        }

        public SymbolInfo DefineParameter(string name)
        {
            if (IsLocalDefined(name))
                throw new DuplicateSymbolException(name, _compiler.CurrentLine, _compiler.CurrentColumn);

            var sym = new SymbolInfo(Chunk.Parameters.Count, SymbolInfo.SymbolType.Parameter);
            Chunk.Parameters.Add(name);
            
            Symbols.Add(name, sym);

            return sym;
        }
        
        public void UndefineLocal(string name)
        {
            if (!IsLocalDefined(name))
                throw new MissingSymbolException(name, _compiler.CurrentLine, _compiler.CurrentColumn);
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