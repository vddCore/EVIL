namespace EVIL.Core.Grammar.AST.Miscellaneous;

using EVIL.Core.Grammar.AST.Base;

public sealed class TablePatternField : AstNode
{
    public PatternBindingNode Binding { get; }
    public string Key { get; }

    public TablePatternField(PatternBindingNode binding, string key)
    {
        Binding = binding;
        Key = key;

        Reparent(Binding);
    }
}