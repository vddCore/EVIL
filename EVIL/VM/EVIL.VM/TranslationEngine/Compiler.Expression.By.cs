namespace EVIL.VM.TranslationEngine;

using System;
using System.Collections.Generic;
using EVIL.Core.Common.TypeSystem;
using EVIL.Core.Grammar.AST.Base;
using EVIL.Core.Grammar.AST.Constants;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Grammar.AST.Miscellaneous;
using EVIL.VM.ExecutionEngine.Diagnostics;
using EVIL.VM.ExecutionEngine.TypeSystem;

public partial class Compiler
{
    protected override void Visit(ByExpression byExpression)
    {
        var arms = byExpression.Arms;

        if (arms.Count > 0 && TryExtractConstantArms(arms, out var constantValues))
        {
            EmitSwitchBy(byExpression, constantValues);
        }
        else
        {
            EmitLinearBy(byExpression);
        }
    }

    private static bool TryExtractConstantArms(List<ByArmNode> arms, out List<DynamicValue> constantValues)
    {
        constantValues = new List<DynamicValue>(arms.Count);

        for (var i = 0; i < arms.Count; i++)
        {
            var selector = arms[i].Selector.Reduce();
            if (TryGetConstantValue(selector, out var val))
            {
                constantValues.Add(val);
            }
            else
            {
                constantValues = null!;
                return false;
            }
        }

        return true;
    }

    private static bool TryGetConstantValue(Expression expression, out DynamicValue value)
    {
        switch (expression)
        {
            case NumberConstant num:
                value = new DynamicValue(num.Value);
                return true;

            case StringConstant str:
                value = new DynamicValue(str.Value);
                return true;

            case BooleanConstant b:
                value = new DynamicValue(b.Value);
                return true;

            case NilConstant:
                value = DynamicValue.Nil;
                return true;

            case TypeCodeConstant tc:
                value = new DynamicValue(tc.Value);
                return true;

            default:
                value = default(DynamicValue);
                return false;
        }
    }

    private void EmitSwitchBy(ByExpression byExpression, List<DynamicValue> constantValues)
    {
        Visit(byExpression.Qualifier);

        var arms = byExpression.Arms;
        var elseArm = byExpression.ElseArm;

        var caseLabels = new int[arms.Count];
        for (var i = 0; i < arms.Count; i++)
        {
            caseLabels[i] = Chunk.CreateLabel();
        }

        var defaultLabel = Chunk.CreateLabel();
        var endLabel = Chunk.CreateLabel();

        // duplicate keys: first come, first served.
        var uniqueCases = new List<(DynamicValue Value, int Label)>(arms.Count);
        var seen = new HashSet<DynamicValue>();

        for (var i = 0; i < arms.Count; i++)
        {
            var val = constantValues[i];
            if (seen.Add(val))
            {
                uniqueCases.Add((val, caseLabels[i]));
            }
        }

        Chunk.CodeGenerator.Emit(OpCode.SWITCH);
        Chunk.CodeGenerator.EmitJumpTarget(defaultLabel);
        Chunk.CodeGenerator.Emit(uniqueCases.Count);

        foreach (var (val, labelId) in uniqueCases)
        {
            Chunk.CodeGenerator.Emit((byte)val.Type);
            switch (val.Type)
            {
                case DynamicValueType.Nil:
                    break;

                case DynamicValueType.Boolean:
                    Chunk.CodeGenerator.Emit(val.Boolean);
                    break;

                case DynamicValueType.Number:
                    Chunk.CodeGenerator.Emit(val.Number);
                    break;

                case DynamicValueType.String:
                    var strId = (int)Chunk.StringPool.FetchOrAdd(val.String!);
                    Chunk.CodeGenerator.Emit(strId);
                    break;

                case DynamicValueType.TypeCode:
                    Chunk.CodeGenerator.Emit((byte)val.TypeCode);
                    break;

                default:
                    throw new InvalidOperationException($"Unexpected constant type in switch: {val.Type}");
            }

            Chunk.CodeGenerator.EmitJumpTarget(labelId);
        }

        for (var i = 0; i < arms.Count; i++)
        {
            Chunk.UpdateLabel(caseLabels[i], Chunk.CodeGenerator.IP);

            Visit(arms[i].ValueArm);

            if (arms[i].ValueArm is Expression)
            {
                Chunk.CodeGenerator.Emit(OpCode.JUMP, endLabel);
            }
        }

        Chunk.UpdateLabel(defaultLabel, Chunk.CodeGenerator.IP);

        if (elseArm != null)
        {
            Visit(elseArm);
        }
        else
        {
            Chunk.CodeGenerator.Emit(OpCode.LDNIL);
        }

        Chunk.UpdateLabel(endLabel, Chunk.CodeGenerator.IP);
    }

    private void EmitLinearBy(ByExpression byExpression)
    {
        Visit(byExpression.Qualifier);

        var arms = byExpression.Arms;
        var elseArm = byExpression.ElseArm;

        var labels = new int[arms.Count];
        for (var i = 0; i < arms.Count; i++)
        {
            labels[i] = Chunk.CreateLabel();
        }

        var endLabel = Chunk.CreateLabel();

        for (var i = 0; i < arms.Count; i++)
        {
            if (i < arms.Count - 1)
            {
                Chunk.CodeGenerator.Emit(OpCode.DUP);
            }

            Visit(arms[i].Selector);

            Chunk.CodeGenerator.Emit(arms[i].DeepEquality ? OpCode.DEQ : OpCode.CEQ);
            Chunk.CodeGenerator.Emit(OpCode.FJMP, labels[i]);

            if (i < arms.Count - 1)
            {
                Chunk.CodeGenerator.Emit(OpCode.POP);
            }

            Visit(arms[i].ValueArm);

            if (arms[i].ValueArm is Expression)
            {
                Chunk.CodeGenerator.Emit(OpCode.JUMP, endLabel);
            }

            Chunk.UpdateLabel(labels[i], Chunk.CodeGenerator.IP);
        }

        if (elseArm != null)
        {
            Visit(elseArm);
        }
        else
        {
            Chunk.CodeGenerator.Emit(OpCode.LDNIL);
        }

        Chunk.UpdateLabel(endLabel, Chunk.CodeGenerator.IP);
    }
}
