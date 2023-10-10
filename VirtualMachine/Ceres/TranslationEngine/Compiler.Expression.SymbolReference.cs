using EVIL.Grammar.AST.Expressions;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(SymbolReferenceExpression symbolReferenceExpression)
        {
            EmitVarGet(symbolReferenceExpression.Identifier);
        }
    }
}