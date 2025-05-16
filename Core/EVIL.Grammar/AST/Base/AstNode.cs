namespace EVIL.Grammar.AST.Base;

using System.Collections.Generic;
using System.Linq;

public abstract class AstNode
{
    private readonly Dictionary<string, object> _tags = [];
    
    public int Line { get; set; }
    public int Column { get; set; }
        
    public AstNode? Parent { get; set; }

    public bool IsConstant => this is ConstantExpression;

    public T SetTag<T>(string key, T value) where T : notnull
        => (T)(_tags[key] = value);

    public T GetTag<T>(string key)
        => (T)_tags[key];
    
    public void DeleteTag(string key)
        => _tags.Remove(key);

    public bool HasTag(string tag) 
        => _tags.ContainsKey(tag);
    
    internal T CopyMetadata<T>(AstNode from) where T : AstNode
    {
        Line = from.Line;
        Column = from.Column;
        Parent = from.Parent;

        return (T)this;
    }
        
    protected void Reparent(params AstNode?[] nodes)
    {
        foreach (var node in nodes)
        {
            if (node != null)
            {
                node!.Parent = this;
            }
        }
    }

    protected void Reparent(IEnumerable<AstNode?> nodes)
        => Reparent(nodes.ToArray());
}