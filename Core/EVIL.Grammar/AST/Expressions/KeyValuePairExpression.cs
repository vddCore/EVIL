﻿namespace EVIL.Grammar.AST.Expressions;

using EVIL.Grammar.AST.Base;

public sealed class KeyValuePairExpression : Expression
{
    public Expression KeyNode { get; }
    public Expression ValueNode { get; }

    public KeyValuePairExpression(Expression keyNode, Expression valueNode)
    {
        KeyNode = keyNode;
        ValueNode = valueNode;

        Reparent(KeyNode, ValueNode);
    }
}