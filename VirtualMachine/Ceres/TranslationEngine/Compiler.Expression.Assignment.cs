using Ceres.ExecutionEngine.Diagnostics;
using Ceres.TranslationEngine.Diagnostics;
using EVIL.Grammar;
using EVIL.Grammar.AST.Expressions;

namespace Ceres.TranslationEngine
{
    public partial class Compiler
    {
        public override void Visit(AssignmentExpression assignmentExpression)
        {
            if (assignmentExpression.Left is SymbolReferenceExpression symRef)
            {
                ThrowIfVarReadOnly(symRef.Identifier);

                if (assignmentExpression.OperationType == AssignmentOperationType.Coalesce)
                {
                    var valueNotNilLabel = Chunk.CreateLabel();

                    EmitVarGet(symRef.Identifier);
                    Chunk.CodeGenerator.Emit(OpCode.DUP);
                    Chunk.CodeGenerator.Emit(OpCode.LDNIL);
                    Chunk.CodeGenerator.Emit(OpCode.CNE);
                    Chunk.CodeGenerator.Emit(OpCode.TJMP, valueNotNilLabel);
                    Chunk.CodeGenerator.Emit(OpCode.POP);
                    Visit(assignmentExpression.Right);
                    Chunk.CodeGenerator.Emit(OpCode.DUP);
                    EmitVarSet(symRef.Identifier);
                    Chunk.UpdateLabel(valueNotNilLabel, Chunk.CodeGenerator.IP);

                    return;
                }

                if (assignmentExpression.OperationType != AssignmentOperationType.Direct)
                {
                    EmitVarGet(symRef.Identifier);
                    Visit(assignmentExpression.Right);

                    Chunk.CodeGenerator.Emit(
                        assignmentExpression.OperationType switch
                        {
                            AssignmentOperationType.Add => OpCode.ADD,
                            AssignmentOperationType.Subtract => OpCode.SUB,
                            AssignmentOperationType.Divide => OpCode.DIV,
                            AssignmentOperationType.Modulo => OpCode.MOD,
                            AssignmentOperationType.Multiply => OpCode.MUL,
                            AssignmentOperationType.BitwiseAnd => OpCode.BAND,
                            AssignmentOperationType.BitwiseOr => OpCode.BOR,
                            AssignmentOperationType.BitwiseXor => OpCode.BXOR,
                            AssignmentOperationType.ShiftLeft => OpCode.SHL,
                            AssignmentOperationType.ShiftRight => OpCode.SHR,
                            _ => Log.TerminateWithInternalFailure(
                                $"Invalid assignment operation type '{assignmentExpression.OperationType}'.",
                                CurrentFileName,
                                line: Line,
                                column: Column,
                                dummyReturn: OpCode.NOOP
                            )
                        }
                    );
                }
                else
                {
                    Visit(assignmentExpression.Right);
                }

                Chunk.CodeGenerator.Emit(OpCode.DUP);
                EmitVarSet(symRef.Identifier);
            }
            else if (assignmentExpression.Left is IndexerExpression ie)
            {
                if (assignmentExpression.OperationType == AssignmentOperationType.Coalesce)
                {
                    var valueNotNilLabel = Chunk.CreateLabel();

                    Visit(ie);
                    Chunk.CodeGenerator.Emit(OpCode.DUP);
                    Chunk.CodeGenerator.Emit(OpCode.LDNIL);
                    Chunk.CodeGenerator.Emit(OpCode.CNE);
                    Chunk.CodeGenerator.Emit(OpCode.TJMP, valueNotNilLabel);
                    Chunk.CodeGenerator.Emit(OpCode.POP);
                    Visit(assignmentExpression.Right);
                    Chunk.CodeGenerator.Emit(OpCode.DUP);
                    Visit(ie.Indexable);
                    Visit(ie.KeyExpression);
                    Chunk.CodeGenerator.Emit(OpCode.ELSET);
                    Chunk.UpdateLabel(valueNotNilLabel, Chunk.CodeGenerator.IP);

                    return;
                }

                if (assignmentExpression.OperationType != AssignmentOperationType.Direct)
                {
                    Visit(ie);
                    Visit(assignmentExpression.Right);

                    Chunk.CodeGenerator.Emit(
                        assignmentExpression.OperationType switch
                        {
                            AssignmentOperationType.Add => OpCode.ADD,
                            AssignmentOperationType.Subtract => OpCode.SUB,
                            AssignmentOperationType.Divide => OpCode.DIV,
                            AssignmentOperationType.Modulo => OpCode.MOD,
                            AssignmentOperationType.Multiply => OpCode.MUL,
                            AssignmentOperationType.BitwiseAnd => OpCode.BAND,
                            AssignmentOperationType.BitwiseOr => OpCode.BOR,
                            AssignmentOperationType.BitwiseXor => OpCode.BXOR,
                            AssignmentOperationType.ShiftLeft => OpCode.SHL,
                            AssignmentOperationType.ShiftRight => OpCode.SHR,
                            _ => Log.TerminateWithInternalFailure(
                                $"Invalid assignment operation type '{assignmentExpression.OperationType}'.",
                                CurrentFileName,
                                line: Line,
                                column: Column,
                                dummyReturn: OpCode.NOOP
                            )
                        }
                    );
                }
                else
                {
                    Visit(assignmentExpression.Right);
                }

                Chunk.CodeGenerator.Emit(OpCode.DUP);

                Visit(ie.Indexable);
                Visit(ie.KeyExpression);
                Chunk.CodeGenerator.Emit(OpCode.ELSET);
            }
            else
            {
                Log.TerminateWithFatal(
                    "Illegal assignment target.",
                    CurrentFileName,
                    EvilMessageCode.IllegalAssignmentTarget,
                    Line,
                    Column
                );
            }
        }
    }
}