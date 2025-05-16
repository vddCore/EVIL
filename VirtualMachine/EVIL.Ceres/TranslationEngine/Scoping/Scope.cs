namespace EVIL.Ceres.TranslationEngine.Scoping;

using System.Collections.Generic;
using EVIL.Ceres.ExecutionEngine.Diagnostics;

internal class Scope
{
    public Chunk Chunk { get; }
    public Scope? Parent { get; }

    public Dictionary<string, Symbol> Symbols { get; } = new();
    public Dictionary<string, Symbol> Closures { get; } = new();

    private Scope(Chunk functionName)
    {
        Chunk = functionName;
    }

    private Scope(Chunk functionName, Scope parent)
        : this(functionName)
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
                sym.Value.Symbol,
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
                sym.Value.Symbol,
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
                sym.Value.Symbol,
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
        => new(Chunk, this);

    public static Scope CreateRoot(Chunk chunk)
        => new(chunk);
}