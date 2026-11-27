namespace EVIL.VM.TranslationEngine.Repl;

using System.Collections.Generic;
using EVIL.VM.TranslationEngine.Scoping;

public sealed class ReplCompilerSession
{
    private Dictionary<string, Symbol> _symbols = new();

    public Compiler Owner { get; }

    internal ReplCompilerSession(Compiler owner)
        => Owner = owner;

    internal Dictionary<string, Symbol> CreateSnapshot()
        => new(_symbols);

    internal void Commit(Dictionary<string, Symbol> symbols)
        => _symbols = symbols;
}