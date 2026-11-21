namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.TranslationEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(IncrementationExpression incrementationExpression)
    {
        if (incrementationExpression.Target is SymbolReferenceExpression vre)
        {
            ThrowIfVarReadOnly(vre.Identifier);

            EmitVarGet(vre.Identifier);
            if (incrementationExpression.Parent is ExpressionStatement)
            {
                Chunk.CodeGenerator.Emit(OpCode.INC);
                EmitVarSet(vre.Identifier);
            }
            else if (incrementationExpression.IsPrefix)
            {
                Chunk.CodeGenerator.Emit(OpCode.INC);
                Chunk.CodeGenerator.Emit(OpCode.DUP);
                EmitVarSet(vre.Identifier);
            }
            else
            {
                Chunk.CodeGenerator.Emit(OpCode.DUP);
                Chunk.CodeGenerator.Emit(OpCode.INC);
                EmitVarSet(vre.Identifier);
            }
        }
        else if (incrementationExpression.Target is IndexerExpression ie)
        {
            var tempTarget = Chunk.AllocateLocal();
            var tempKey = Chunk.AllocateLocal();

            Visit(ie.Indexable);
            Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, tempTarget);
            Visit(ie.KeyExpression);
            Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, tempKey);

            Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempTarget);
            Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempKey);
            Chunk.CodeGenerator.Emit(ie.CreateTableIfNonExistent ? OpCode.INOCT : OpCode.INDEX);

            if (incrementationExpression.Parent is ExpressionStatement)
            {
                Chunk.CodeGenerator.Emit(OpCode.INC);
            }
            else if (incrementationExpression.IsPrefix)
            {
                Chunk.CodeGenerator.Emit(OpCode.INC);
                Chunk.CodeGenerator.Emit(OpCode.DUP);
            }
            else
            {
                Chunk.CodeGenerator.Emit(OpCode.DUP);
                Chunk.CodeGenerator.Emit(OpCode.INC);
            }

            Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempTarget);
            Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempKey);
            Chunk.CodeGenerator.Emit(OpCode.ELSET);
        }
        else
        {
            Log.TerminateWithFatal(
                "Illegal incrementation target.",
                CurrentFileName,
                EvilMessageCode.IllegalIncrementationTarget,
                Line,
                Column
            );
        }
    }
}