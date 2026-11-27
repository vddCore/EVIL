namespace EVIL.VM.TranslationEngine.Scoping;

using System.Collections.Generic;
using EVIL.VM.ExecutionEngine.Diagnostics;

internal class Scope
{
    private readonly IReadOnlyDictionary<string, Symbol>? _replSymbols;
    
    public Chunk Chunk { get; }
    public Scope? Parent { get; }

    public Dictionary<string, Symbol> Symbols { get; } = new();
    public Dictionary<string, Symbol> Closures { get; } = new();

    private Scope(Chunk functionName, IReadOnlyDictionary<string, Symbol>? replSymbols = null)
    {
        Chunk = functionName;
        _replSymbols = replSymbols;
    }

    private Scope(Chunk functionName, Scope parent)
        : this(functionName, parent._replSymbols)
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

        if (sym is { IsClosure: false })
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

        if (sym is { IsClosure: false })
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

        if (_replSymbols?.TryGetValue(name, out var replSymbol) == true)
        {
            return (false, replSymbol);
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

    public static Scope CreateRoot(
        Chunk chunk,
        IReadOnlyDictionary<string, Symbol>? replSymbols = null)
        => new(chunk, replSymbols);
}