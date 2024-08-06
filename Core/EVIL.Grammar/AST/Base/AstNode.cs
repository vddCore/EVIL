namespace EVIL.Grammar.AST.Base;

using System.Collections.Generic;
using System.Linq;

public abstract class AstNode
{
    public int Line { get; set; }
    public int Column { get; set; }
        
    public AstNode? Parent { get; set; }

    public bool IsConstant => this is ConstantExpression;
        
    internal T CopyMetadata<T>(AstNode from) where T : AstNode
    {
        Line = from.Line;
        Column = from.Column;
        Parent = from.Parent;
            
        return (this as T)!;
    }
        
    protected void Reparent(params AstNode?[] nodes)
    {
        for (var i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] != null)
            {
                nodes[i]!.Parent = this;
            }
        }
    }

    protected void Reparent(IEnumerable<AstNode?> nodes)
        => Reparent(nodes.ToArray());
}