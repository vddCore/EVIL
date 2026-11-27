namespace EVIL.VM.TranslationEngine;

using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.VM.ExecutionEngine.Diagnostics;

public partial class Compiler
{
    protected override void Visit(NonNilAssertionExpression nonNilAssertionExpression)
    {
        Visit(nonNilAssertionExpression.Operand);

        var passLabel = Chunk.CreateLabel();
        Chunk.CodeGenerator.Emit(OpCode.DUP);
        Chunk.CodeGenerator.Emit(OpCode.VJMP, passLabel);

        Chunk.CodeGenerator.Emit(OpCode.POP);
        Chunk.MarkThrowing();

        var exprText = GetExpressionStringRepresentation(
            nonNilAssertionExpression.Operand
        );
        
        var fileInfo = !string.IsNullOrEmpty(CurrentFileName) ? $" in {CurrentFileName}" : "";
        var message = $"NilAssertionException: Non-nil assertion '!' failed on expression '{exprText}' " +
                      $"at line {nonNilAssertionExpression.Line}, col {nonNilAssertionExpression.Column}{fileInfo}.";

        var strId = (int)Chunk.StringPool.FetchOrAdd(message);
        Chunk.CodeGenerator.Emit(OpCode.LDSTR, strId);
        Chunk.CodeGenerator.Emit(OpCode.THROW);

        Chunk.UpdateLabel(passLabel, Chunk.CodeGenerator.IP);
    }

    private static string GetExpressionStringRepresentation(Expression expr) => expr switch
    {
        SymbolReferenceExpression sym => sym.Identifier,
        IndexerExpression ie => ie.BuildChainStringRepresentation(),
        InvocationExpression inv => $"{GetExpressionStringRepresentation(inv.Callee)}()",
        SelfInvocationExpression selfInv => $"{GetExpressionStringRepresentation(selfInv.Indexable)}:{selfInv.Identifier.Name}()",
        NonNilAssertionExpression nna => $"{GetExpressionStringRepresentation(nna.Operand)}!",
        ConstantExpression c => c.ToString() ?? "constant",
        _ => "expression"
    };
}
