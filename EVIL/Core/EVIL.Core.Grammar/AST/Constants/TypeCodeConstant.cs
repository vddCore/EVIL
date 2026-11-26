namespace EVIL.Core.Grammar.AST.Constants;

using EVIL.Core.Common.TypeSystem;
using EVIL.Core.Grammar.AST.Base;

public class TypeCodeConstant(DynamicValueType value) 
    : ConstantExpression
{
    public DynamicValueType Value { get; } = value;
}