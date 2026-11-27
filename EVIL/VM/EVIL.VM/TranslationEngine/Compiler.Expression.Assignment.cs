namespace EVIL.VM.TranslationEngine;

using EVIL.Core.Diagnostics;
using EVIL.Core.Grammar;
using EVIL.Core.Grammar.AST.Constants;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Grammar.AST.Statements;
using EVIL.VM.ExecutionEngine.Diagnostics;

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
            if (ie.IsConditional)
            {
                var isStatement = assignmentExpression.Parent is ExpressionStatement;
                var notNilTargetLabel = Chunk.CreateLabel();
                var endConditionalAssignLabel = Chunk.CreateLabel();
                var tempTarget = Chunk.AllocateLocal();

                Visit(ie.Indexable);
                Chunk.CodeGenerator.Emit(OpCode.DUP);
                Chunk.CodeGenerator.Emit(OpCode.VJMP, notNilTargetLabel);

                if (isStatement)
                {
                    Chunk.CodeGenerator.Emit(OpCode.POP);
                }

                Chunk.CodeGenerator.Emit(OpCode.JUMP, endConditionalAssignLabel);
                Chunk.UpdateLabel(notNilTargetLabel, Chunk.CodeGenerator.IP);
                Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, tempTarget);

                if (assignmentExpression.OperationType == AssignmentOperationType.Coalesce)
                {
                    if (ie.KeyExpression is StringConstant { IsInterpolated: false } sc 
                        && !ie.CreateTableIfNonExistent)
                    {
                        var strId = (int)Chunk.StringPool.FetchOrAdd(sc.Value);
                        var valueNotNilLabel = Chunk.CreateLabel();

                        Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempTarget);
                        Chunk.CodeGenerator.EmitGetProp(strId);

                        Chunk.CodeGenerator.Emit(OpCode.DUP);
                        Chunk.CodeGenerator.Emit(OpCode.VJMP, valueNotNilLabel);
                        Chunk.CodeGenerator.Emit(OpCode.POP);
                        Visit(assignmentExpression.Right);
                        
                        if (!isStatement)
                        {
                            Chunk.CodeGenerator.Emit(OpCode.DUP);
                        }
                        
                        Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempTarget);
                        Chunk.CodeGenerator.EmitSetProp(strId);

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
                    }
                    else
                    {
                        var tempKey = Chunk.AllocateLocal();

                        Visit(ie.KeyExpression);
                        Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, tempKey);

                        var valueNotNilLabelInner = Chunk.CreateLabel();

                        Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempTarget);
                        Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempKey);
                        Chunk.CodeGenerator.Emit(ie.CreateTableIfNonExistent 
                            ? OpCode.INOCT 
                            : OpCode.INDEX
                        );

                        Chunk.CodeGenerator.Emit(OpCode.DUP);
                        Chunk.CodeGenerator.Emit(OpCode.VJMP, valueNotNilLabelInner);
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
                            Chunk.UpdateLabel(valueNotNilLabelInner, Chunk.CodeGenerator.IP);
                            Chunk.CodeGenerator.Emit(OpCode.POP);
                            Chunk.UpdateLabel(endLabel, Chunk.CodeGenerator.IP);
                        }
                        else
                        {
                            Chunk.UpdateLabel(valueNotNilLabelInner, Chunk.CodeGenerator.IP);
                        }
                    }
                }
                else if (assignmentExpression.OperationType != AssignmentOperationType.Direct)
                {
                    if (ie.KeyExpression is StringConstant { IsInterpolated: false } sc 
                        && !ie.CreateTableIfNonExistent)
                    {
                        var strId = (int)Chunk.StringPool.FetchOrAdd(sc.Value);

                        Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempTarget);
                        Chunk.CodeGenerator.EmitGetProp(strId);

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

                        if (!isStatement)
                        {
                            Chunk.CodeGenerator.Emit(OpCode.DUP);
                        }

                        Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempTarget);
                        Chunk.CodeGenerator.EmitSetProp(strId);
                    }
                    else
                    {
                        var tempKey = Chunk.AllocateLocal();

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

                        if (!isStatement)
                        {
                            Chunk.CodeGenerator.Emit(OpCode.DUP);
                        }

                        Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempTarget);
                        Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempKey);
                        Chunk.CodeGenerator.Emit(OpCode.ELSET);
                    }
                }
                else
                {
                    if (ie.KeyExpression is StringConstant { IsInterpolated: false } sc
                        && !ie.CreateTableIfNonExistent)
                    {
                        var strId = (int)Chunk.StringPool.FetchOrAdd(sc.Value);

                        Visit(assignmentExpression.Right);

                        if (!isStatement)
                        {
                            Chunk.CodeGenerator.Emit(OpCode.DUP);
                        }

                        Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempTarget);
                        Chunk.CodeGenerator.EmitSetProp(strId);
                    }
                    else
                    {
                        var tempKey = Chunk.AllocateLocal();

                        Visit(ie.KeyExpression);
                        Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, tempKey);

                        Visit(assignmentExpression.Right);

                        if (!isStatement)
                        {
                            Chunk.CodeGenerator.Emit(OpCode.DUP);
                        }

                        Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempTarget);
                        Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempKey);
                        Chunk.CodeGenerator.Emit(OpCode.ELSET);
                    }
                }

                Chunk.UpdateLabel(endConditionalAssignLabel, Chunk.CodeGenerator.IP);
                return;
            }

            if (assignmentExpression.OperationType == AssignmentOperationType.Coalesce)
            {
                var isStatement = assignmentExpression.Parent is ExpressionStatement;
                var tempTarget = Chunk.AllocateLocal();

                /* yeye i know patterns exist. but they look fucking UGLY here. */
                if (ie.KeyExpression is StringConstant { IsInterpolated: false } sc 
                    && !ie.CreateTableIfNonExistent)
                {
                    var strId = (int)Chunk.StringPool.FetchOrAdd(sc.Value);

                    Visit(ie.Indexable);
                    Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, tempTarget);

                    var valueNotNilLabel = Chunk.CreateLabel();

                    Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempTarget);
                    Chunk.CodeGenerator.EmitGetProp(strId);

                    Chunk.CodeGenerator.Emit(OpCode.DUP);
                    Chunk.CodeGenerator.Emit(OpCode.VJMP, valueNotNilLabel);
                    Chunk.CodeGenerator.Emit(OpCode.POP);
                    Visit(assignmentExpression.Right);
                    
                    if (!isStatement)
                    {
                        Chunk.CodeGenerator.Emit(OpCode.DUP);
                    }
                    
                    Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempTarget);
                    Chunk.CodeGenerator.EmitSetProp(strId);

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

                var tempKey = Chunk.AllocateLocal();

                Visit(ie.Indexable);
                Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, tempTarget);
                Visit(ie.KeyExpression);
                Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, tempKey);

                var valueNotNilLabelInner = Chunk.CreateLabel();

                Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempTarget);
                Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempKey);
                Chunk.CodeGenerator.Emit(ie.CreateTableIfNonExistent ? OpCode.INOCT : OpCode.INDEX);

                Chunk.CodeGenerator.Emit(OpCode.DUP);
                Chunk.CodeGenerator.Emit(OpCode.VJMP, valueNotNilLabelInner);
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
                    Chunk.UpdateLabel(valueNotNilLabelInner, Chunk.CodeGenerator.IP);
                    Chunk.CodeGenerator.Emit(OpCode.POP);
                    Chunk.UpdateLabel(endLabel, Chunk.CodeGenerator.IP);
                }
                else
                {
                    Chunk.UpdateLabel(valueNotNilLabelInner, Chunk.CodeGenerator.IP);
                }

                return;
            }

            if (assignmentExpression.OperationType != AssignmentOperationType.Direct)
            {
                if (ie.KeyExpression is StringConstant { IsInterpolated: false } sc 
                    && !ie.CreateTableIfNonExistent)
                {
                    var strId = (int)Chunk.StringPool.FetchOrAdd(sc.Value);
                    var tempTarget = Chunk.AllocateLocal();

                    Visit(ie.Indexable);
                    Chunk.CodeGenerator.Emit(OpCode.SETLOCAL, tempTarget);

                    Chunk.CodeGenerator.Emit(OpCode.GETLOCAL, tempTarget);
                    Chunk.CodeGenerator.EmitGetProp(strId);

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
                    Chunk.CodeGenerator.EmitSetProp(strId);
                }
                else
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
            }
            else
            {
                if (ie.KeyExpression is StringConstant { IsInterpolated: false } sc
                    && !ie.CreateTableIfNonExistent)
                {
                    var strId = (int)Chunk.StringPool.FetchOrAdd(sc.Value);

                    Visit(assignmentExpression.Right);

                    if (assignmentExpression.Parent is not ExpressionStatement)
                    {
                        Chunk.CodeGenerator.Emit(OpCode.DUP);
                    }

                    Visit(ie.Indexable);
                    Chunk.CodeGenerator.EmitSetProp(strId);
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