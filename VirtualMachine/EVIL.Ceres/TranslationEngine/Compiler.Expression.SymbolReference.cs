namespace EVIL.Ceres.TranslationEngine;

using EVIL.Grammar.AST.Expressions;

public partial class Compiler
{
    public override void Visit(SymbolReferenceExpression symbolReferenceExpression)
    {
        EmitVarGet(symbolReferenceExpression.Identifier);
    }
}