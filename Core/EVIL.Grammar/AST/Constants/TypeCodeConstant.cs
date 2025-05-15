namespace EVIL.Grammar.AST.Constants;

using EVIL.CommonTypes.TypeSystem;
using EVIL.Grammar.AST.Base;

public class TypeCodeConstant(DynamicValueType value) 
    : ConstantExpression
{
    public DynamicValueType Value { get; } = value;
}