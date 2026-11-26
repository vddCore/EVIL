namespace EVIL.VirtualMachine.TranslationEngine;

using EVIL.Core.Grammar.AST.Expressions;

public partial class Compiler
{
    protected override void Visit(SymbolReferenceExpression symbolReferenceExpression)
    {
        EmitVarGet(symbolReferenceExpression.Identifier);
    }
}