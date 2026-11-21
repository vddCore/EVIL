namespace EVIL.Ceres.TranslationEngine;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.CommonTypes.Diagnostics;
using EVIL.Grammar;
using EVIL.Grammar.AST.Expressions;
using EVIL.Grammar.AST.Statements;

public partial class Compiler
{
    protected override void Visit(AssignmentExpression assignmentExpression)
    {
        if (assignmentExpression.Left is SymbolReferenceExpression symRef)
        {
            ThrowIfVarReadOnly(symRef.Identifier);

            if (assignmentExpression.OperationType == AssignmentOperationType.Coalesce)
            {
                var isStatement = assignmentExpression.Parent is ExpressionStatement;
                var valueNotNilLabel = Chunk.CreateLabel();

                EmitVarGet(symRef.Identifier);
                Chunk.CodeGenerator.Emit(OpCode.DUP);
                Chunk.CodeGenerator.Emit(OpCode.VJMP, valueNotNilLabel);
                Chunk.CodeGenerator.Emit(OpCode.POP);
                Visit(assignmentExpression.Right);
                
                if (!isStatement)
                {
                    Chunk.CodeGenerator.Emit(OpCode.DUP);
                }
                
                EmitVarSet(symRef.Identifier);

                if (isStatement)
                {
                    var endLabel = Chunk.CreateLabel();
                    
                    Chunk.CodeGenerator.Emit(OpCode.JUMP, endLabel);
                    Chunk.UpdateLabel(valueNotNilLabel, Chunk.CodeGenerator.IP);
                    
                    Chunk.CodeGenerator.Emit(OpCode.POP);
                    Chunk.UpdateLabel(endLabel, Chunk.CodeGenerator.IP);
                }
                else
                {
                    Chunk.UpdateLabel(valueNotNilLabel, Chunk.CodeGenerator.IP);
                }

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
                            CompilerDiagnostics.InternalCompilerError.Create(
                                GetLocation(Line, Column),
                                $"Invalid assignment operation type '{assignmentExpression.OperationType}'."
                            ),
                            dummyReturn: OpCode.NOOP
                        )
                    }
                );
            }
            else
            {
                Visit(assignmentExpression.Right);
            }

            if (assignmentExpression.Parent is not ExpressionStatement)
            {
                Chunk.CodeGenerator.Emit(OpCode.DUP);
            }

            EmitVarSet(symRef.Identifier);
        }
        else if (assignmentExpression.Left is IndexerExpression ie)
        {
            if (assignmentExpression.OperationType == AssignmentOperationType.Coalesce)
            {
                var isStatement = assignmentExpression.Parent is ExpressionStatement;
                var tempTarget = Chunk.AllocateLocal();
                var tempKey = Chunk.AllocateLocal();

                Visit(ie.Indexable);
                Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, tempTarget);
                Visit(ie.KeyExpression);
                Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, tempKey);

                var valueNotNilLabel = Chunk.CreateLabel();

                Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempTarget);
                Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempKey);
                Chunk.CodeGenerator.Emit(ie.CreateTableIfNonExistent ? OpCode.INOCT : OpCode.INDEX);

                Chunk.CodeGenerator.Emit(OpCode.DUP);
                Chunk.CodeGenerator.Emit(OpCode.VJMP, valueNotNilLabel);
                Chunk.CodeGenerator.Emit(OpCode.POP);
                Visit(assignmentExpression.Right);
                if (!isStatement)
                {
                    Chunk.CodeGenerator.Emit(OpCode.DUP);
                }
                Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempTarget);
                Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempKey);
                Chunk.CodeGenerator.Emit(OpCode.ELSET);

                if (isStatement)
                {
                    var endLabel = Chunk.CreateLabel();
                    Chunk.CodeGenerator.Emit(OpCode.JUMP, endLabel);
                    Chunk.UpdateLabel(valueNotNilLabel, Chunk.CodeGenerator.IP);
                    Chunk.CodeGenerator.Emit(OpCode.POP);
                    Chunk.UpdateLabel(endLabel, Chunk.CodeGenerator.IP);
                }
                else
                {
                    Chunk.UpdateLabel(valueNotNilLabel, Chunk.CodeGenerator.IP);
                }

                return;
            }

            if (assignmentExpression.OperationType != AssignmentOperationType.Direct)
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
                            CompilerDiagnostics.InternalCompilerError.Create(
                                GetLocation(Line, Column),
                                $"Invalid assignment operation type '{assignmentExpression.OperationType}'."
                            ),
                            dummyReturn: OpCode.NOOP
                        )
                    }
                );

                if (assignmentExpression.Parent is not ExpressionStatement)
                {
                    Chunk.CodeGenerator.Emit(OpCode.DUP);
                }

                Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempTarget);
                Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempKey);
                Chunk.CodeGenerator.Emit(OpCode.ELSET);
            }
            else
            {
                Visit(assignmentExpression.Right);

                if (assignmentExpression.Parent is not ExpressionStatement)
                {
                    Chunk.CodeGenerator.Emit(OpCode.DUP);
                }

                Visit(ie.Indexable);
                Visit(ie.KeyExpression);
                Chunk.CodeGenerator.Emit(OpCode.ELSET);
            }
        }
        else
        {
            Log.TerminateWithFatal(
                CompilerDiagnostics.IllegalAssignmentTarget.Create(
                    GetLocation(Line, Column)
                )
            );
        }
    }
}