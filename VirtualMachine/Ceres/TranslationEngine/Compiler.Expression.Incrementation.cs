using Ceres.ExecutionEngine.Diagnostics;
using Ceres.TranslationEngine.Diagnostics;
using EVIL.Grammar.AST.Expressions;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(IncrementationExpression incrementationExpression)
        {
            if (incrementationExpression.Target is SymbolReferenceExpression vre)
            {
                ThrowIfVarReadOnly(vre.Identifier);

                EmitVarGet(vre.Identifier);
                if (incrementationExpression.IsPrefix)
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
                Visit(ie);

                if (incrementationExpression.IsPrefix)
                {
                    Chunk.CodeGenerator.Emit(OpCode.INC);
                    Chunk.CodeGenerator.Emit(OpCode.DUP);
                }
                else
                {
                    Chunk.CodeGenerator.Emit(OpCode.DUP);
                    Chunk.CodeGenerator.Emit(OpCode.INC);
                }

                Visit(ie.Indexable);
                Visit(ie.KeyExpression);
                Chunk.CodeGenerator.Emit(OpCode.TABSET);
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
}