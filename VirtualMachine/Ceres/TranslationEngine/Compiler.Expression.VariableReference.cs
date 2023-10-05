using EVIL.Grammar.AST.Expressions;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(VariableReferenceExpression variableReferenceExpression)
        {
            EmitVarGet(variableReferenceExpression.Identifier);
        }
    }
}