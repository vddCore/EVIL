using Ceres.ExecutionEngine.Diagnostics;
using Ceres.TranslationEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(DecrementationExpression decrementationExpression)
        {
            if (decrementationExpression.Target is SymbolReferenceExpression vre)
            {
                ThrowIfVarReadOnly(vre.Identifier);

                EmitVarGet(vre.Identifier);
                {
                    if (decrementationExpression.IsPrefix)
                    {
                        Chunk.CodeGenerator.Emit(OpCode.DEC);
                        Chunk.CodeGenerator.Emit(OpCode.DUP);
                    }
                    else
                    {
                        Chunk.CodeGenerator.Emit(OpCode.DUP);
                        Chunk.CodeGenerator.Emit(OpCode.DEC);
                    }
                }
                EmitVarSet(vre.Identifier);
            }
            else if (decrementationExpression.Target is IndexerExpression ie)
            {
                Visit(ie);

                if (decrementationExpression.IsPrefix)
                {
                    Chunk.CodeGenerator.Emit(OpCode.DEC);
                    Chunk.CodeGenerator.Emit(OpCode.DUP);
                }
                else
                {
                    Chunk.CodeGenerator.Emit(OpCode.DUP);
                    Chunk.CodeGenerator.Emit(OpCode.DEC);
                }

                Visit(ie.Indexable);
                Visit(ie.KeyExpression);
                Chunk.CodeGenerator.Emit(OpCode.ELSET);
            }
            else
            {
                Log.TerminateWithFatal(
                    "Illegal decrementation target.",
                    CurrentFileName,
                    EvilMessageCode.IllegalDecrementationTarget,
                    Line,
                    Column
                );
            }
        }
    }
}