using EVIL.Grammar.AST.Base;

namespace EVIL.Grammar.AST.Constants
{
    public sealed class NilConstant : ConstantExpression
    {
        public override bool CanBeNil => true;
    }
}