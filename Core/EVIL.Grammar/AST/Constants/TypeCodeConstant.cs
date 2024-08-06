namespace EVIL.Grammar.AST.Constants;

using EVIL.CommonTypes.TypeSystem;
using EVIL.Grammar.AST.Base;

public class TypeCodeConstant : ConstantExpression
{
    public DynamicValueType Value { get; }

    public TypeCodeConstant(DynamicValueType value)
    {
        Value = value;
    }
}