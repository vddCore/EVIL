using EVIL.CommonTypes.TypeSystem;
using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Constants
{
    public class TypeCodeConstant : ConstantExpression
    {
        public DynamicValueType Value { get; }

        public TypeCodeConstant(DynamicValueType value)
        {
            Value = value;
        }
    }
}