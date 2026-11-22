namespace EVIL.Ceres.ExecutionEngine;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.Marshaling;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.CommonTypes.TypeSystem;
using Array = System.Array;

internal class ExecutionUnit
{
    private readonly Table _global;
    private readonly Fiber _fiber;
    private readonly ValueStack _evaluationStack;
    private readonly CallStack _callStack;

    public ExecutionUnit(Table global,
        Fiber fiber,
        ValueStack evaluationStack,
        CallStack callStack)
    {
        _global = global;
        _fiber = fiber;
        _evaluationStack = evaluationStack;
        _callStack = callStack;
    }

    public void Step() => Step(1);

    public void Step(int maxInstructions)
    {
        if (_callStack.Count == 0 || _fiber.State != FiberState.Running)
        {
            return;
        }

        if (_callStack.Peek() is not ScriptStackFrame frame)
        {
            return;
        }

        var currentDepth = _callStack.Count;

        for (var stepIndex = 0; stepIndex < maxInstructions; stepIndex++)
        {
            var debugger = _fiber.DebuggerSession;
            if (debugger != null)
            {
                var ip = (int)frame.IP;
                var nextOp = frame.PeekOpCode();
                if (debugger.ShouldBreakAt(_fiber, frame, ip, nextOp, out var reason))
                {
                    debugger.OnBreakEncountered(_fiber, frame, ip, nextOp, reason);
                    if (_fiber.State != FiberState.Running || _callStack.Count == 0)
                    {
                        return;
                    }
                    if (_callStack.Peek() is not ScriptStackFrame activeFrame)
                    {
                        return;
                    }
                    frame = activeFrame;
                }
            }

            var opCode = frame.FetchOpCode();

            DynamicValue a;
            DynamicValue b;
            DynamicValue c;

            switch (opCode)
            {
                case OpCode.NOOP:
                {
                    break;
                }

                case OpCode.DUP:
                {
                    PushValue(PeekValue());
                    break;
                }

                case OpCode.LDNUM:
                {
                    PushValue(frame.FetchDouble());
                    break;
                }

                case OpCode.LDNUM_I8:
                {
                    PushValue((double)(sbyte)frame.FetchByte());
                    break;
                }

                case OpCode.LDNIL:
                {
                    PushValue(DynamicValue.Nil);
                    break;
                }

                case OpCode.LDONE:
                {
                    PushValue(DynamicValue.One);
                    break;
                }

                case OpCode.LDZERO:
                {
                    PushValue(DynamicValue.Zero);
                    break;
                }

                case OpCode.LDTRUE:
                {
                    PushValue(DynamicValue.True);
                    break;
                }

                case OpCode.LDFALSE:
                {
                    PushValue(DynamicValue.False);
                    break;
                }

                case OpCode.LDSTR:
                {
                    PushValue(frame.Chunk.StringPool[
                        frame.FetchInt32()
                    ]!);

                    break;
                }

                case OpCode.LDCNK:
                {
                    var subChunk = frame.Chunk.SubChunks[frame.FetchInt32()];
                    Chunk chunkToPush;

                    if (subChunk.ClosureCount == 0)
                    {
                        chunkToPush = subChunk;
                    }
                    else
                    {
                        var clone = subChunk.Clone();
                        var upvalues = new Upvalue[clone.ClosureCount];
                        for (var i = 0; i < clone.ClosureCount; i++)
                        {
                            upvalues[i] = ResolveUpvalue(frame, clone.Closures[i]);
                        }

                        clone.Upvalues = upvalues;
                        chunkToPush = clone;
                    }

                    PushValue(chunkToPush);
                    break;
                }

                case OpCode.LDTYPE:
                {
                    PushValue((DynamicValueType)frame.FetchByte());
                    break;
                }

                case OpCode.ADD:
                {
                    b = PopValue();
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.AddMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, Table.AddMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    PushValue(a.Add(b));
                    break;
                }

                case OpCode.SUB:
                {
                    b = PopValue();
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.SubtractMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, Table.SubtractMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    PushValue(a.Subtract(b));
                    break;
                }

                case OpCode.MUL:
                {
                    b = PopValue();
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.MultiplyMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, Table.MultiplyMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    PushValue(a.Multiply(b));
                    break;
                }

                case OpCode.DIV:
                {
                    b = PopValue();
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.DivideMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, Table.DivideMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    PushValue(a.DivideBy(b));
                    break;
                }

                case OpCode.MOD:
                {
                    b = PopValue();
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.ModuloMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, Table.ModuloMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    PushValue(a.Modulo(b));
                    break;
                }

                case OpCode.SHL:
                {
                    b = PopValue();
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.ShiftLeftMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, Table.ShiftLeftMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    PushValue(a.ShiftLeft(b));
                    break;
                }

                case OpCode.SHR:
                {
                    b = PopValue();
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.ShiftRightMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, Table.ShiftRightMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    PushValue(a.ShiftRight(b));
                    break;
                }

                case OpCode.POP:
                {
                    PopValue();
                    break;
                }

                case OpCode.ANEG:
                {
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.ArithmeticNegateMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a);
                            break;
                        }
                    }

                    PushValue(a.ArithmeticallyNegate());
                    break;
                }

                case OpCode.LNOT:
                {
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.LogicalNotMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a);
                            break;
                        }
                    }

                    PushValue(a.LogicallyNegate());
                    break;
                }

                case OpCode.LOR:
                {
                    b = PopValue();
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.LogicalOrMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, Table.LogicalOrMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    PushValue(a.LogicalOr(b));
                    break;
                }

                case OpCode.LAND:
                {
                    b = PopValue();
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.LogicalAndMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, Table.LogicalAndMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    PushValue(a.LogicalAnd(b));
                    break;
                }

                case OpCode.BOR:
                {
                    b = PopValue();
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.BitwiseOrMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, Table.BitwiseOrMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    PushValue(a.BitwiseOr(b));
                    break;
                }

                case OpCode.BXOR:
                {
                    b = PopValue();
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.BitwiseXorMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, Table.BitwiseXorMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    PushValue(a.BitwiseXor(b));
                    break;
                }

                case OpCode.BAND:
                {
                    b = PopValue();
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.BitwiseAndMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, Table.BitwiseAndMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    PushValue(a.BitwiseAnd(b));
                    break;
                }

                case OpCode.BNOT:
                {
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.BitwiseNotMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a);
                            break;
                        }
                    }

                    PushValue(a.BitwiseNegate());
                    break;
                }

                case OpCode.DEQ:
                {
                    b = PopValue();
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.DeepEqualMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, Table.DeepEqualMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    PushValue(a.IsDeeplyEqualTo(b));
                    break;
                }

                case OpCode.DNE:
                {
                    b = PopValue();
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.DeepNotEqualMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, Table.DeepNotEqualMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    PushValue(a.IsDeeplyNotEqualTo(b));
                    break;
                }

                case OpCode.CEQ:
                {
                    b = PopValue();
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.EqualMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, Table.EqualMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    PushValue(a.IsEqualTo(b));
                    break;
                }

                case OpCode.CNE:
                {
                    b = PopValue();
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.NotEqualMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, Table.NotEqualMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    PushValue(a.IsNotEqualTo(b));
                    break;
                }

                case OpCode.CGT:
                {
                    b = PopValue();
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.GreaterThanMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, Table.GreaterThanMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    PushValue(a.IsGreaterThan(b));
                    break;
                }

                case OpCode.CGE:
                {
                    b = PopValue();
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.GreaterEqualMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, Table.GreaterEqualMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    PushValue(a.IsGreaterThanOrEqualTo(b));
                    break;
                }

                case OpCode.CLT:
                {
                    b = PopValue();
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.LessThanMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, Table.LessThanMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    PushValue(a.IsLessThan(b));
                    break;
                }

                case OpCode.CLE:
                {
                    b = PopValue();
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.LessEqualMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, Table.LessEqualMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    PushValue(a.IsLessThanOrEqualTo(b));
                    break;
                }

                case OpCode.INVOKE:
                {
                    a = PopValue();

                    var argumentCount = frame.FetchInt32();
                    var isVariadic = frame.FetchByte() == 1;

                    if (isVariadic)
                    {
                        argumentCount += frame.ExtraArguments.Length - 1;
                        // -1 because variadic specifier is treated as an argument
                    }

                    if (a.Type == DynamicValueType.Chunk)
                    {
                        InvokeChunkFromStack(a.Chunk!, argumentCount);
                        break;
                    }

                    var args = PopArguments(argumentCount);

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.InvokeMetaKey, out var chunk))
                        {
                            var overrideArgs = new DynamicValue[args.Length + 1];
                            overrideArgs[0] = a;
                            Array.Copy(
                                args, 0,
                                overrideArgs, 1,
                                args.Length
                            );

                            InvokeChunk(chunk, overrideArgs);
                            break;
                        }
                    }

                    if (a.Type == DynamicValueType.NativeFunction)
                    {
                        _callStack.Push(new NativeStackFrame(a.NativeFunction!));

                        _fiber.OnNativeFunctionInvoke?.Invoke(
                            _fiber,
                            a.NativeFunction!
                        );

                        var value = a.NativeFunction!.Invoke(_fiber, args);

                        if (_callStack.Peek() is NativeStackFrame)
                        {
                            PushValue(value);
                            _callStack.Pop();
                        }
                        else
                        {
                            /* We probably threw from native, nothing else to be done. */
                        }

                        break;
                    }

                    throw new UnsupportedDynamicValueOperationException(
                        $"Attempt to invoke a {a.Type} value."
                    );
                }

                case OpCode.TAILINVOKE:
                {
                    frame.CloseUpvalues();
                    var args = frame.Arguments;

                    _evaluationStack.PopTo(args);

                    if (frame.Locals != null)
                    {
                        Array.Clear(frame.Locals, 0, frame.Locals.Length);
                    }

                    _fiber.OnChunkInvoke?.Invoke(
                        _fiber,
                        _callStack.Peek().As<ScriptStackFrame>().Chunk,
                        true
                    );

                    frame.JumpAbsolute(0);
                    break;
                }

                case OpCode.SETGLOBAL:
                {
                    b = PopValue();
                    a = PopValue();
                    _global[b] = a;

                    break;
                }

                case OpCode.GETGLOBAL:
                {
                    PushValue(_global[PopValue()]);
                    break;
                }

                case OpCode.SETLOCAL_0:
                    SetLocal(frame, 0, PopValue());
                    break;
                case OpCode.SETLOCAL_1:
                    SetLocal(frame, 1, PopValue());
                    break;
                case OpCode.SETLOCAL_2:
                    SetLocal(frame, 2, PopValue());
                    break;
                case OpCode.SETLOCAL_3:
                    SetLocal(frame, 3, PopValue());
                    break;
                case OpCode.SETLOCAL_B:
                    SetLocal(frame, frame.FetchByte(), PopValue());
                    break;
                case OpCode.SETLOCAL:
                    SetLocal(frame, frame.FetchInt32(), PopValue());
                    break;

                case OpCode.GETLOCAL_0:
                    PushValue(frame.Locals![0]);
                    break;
                case OpCode.GETLOCAL_1:
                    PushValue(frame.Locals![1]);
                    break;
                case OpCode.GETLOCAL_2:
                    PushValue(frame.Locals![2]);
                    break;
                case OpCode.GETLOCAL_3:
                    PushValue(frame.Locals![3]);
                    break;
                case OpCode.GETLOCAL_B:
                    PushValue(frame.Locals![frame.FetchByte()]);
                    break;
                case OpCode.GETLOCAL:
                    PushValue(frame.Locals![frame.FetchInt32()]);
                    break;

                case OpCode.SETARG_0:
                    SetArg(frame, 0, PopValue());
                    break;
                case OpCode.SETARG_1:
                    SetArg(frame, 1, PopValue());
                    break;
                case OpCode.SETARG_2:
                    SetArg(frame, 2, PopValue());
                    break;
                case OpCode.SETARG_3:
                    SetArg(frame, 3, PopValue());
                    break;
                case OpCode.SETARG_B:
                    SetArg(frame, frame.FetchByte(), PopValue());
                    break;
                case OpCode.SETARG:
                    SetArg(frame, frame.FetchInt32(), PopValue());
                    break;

                case OpCode.GETARG_0:
                    PushValue(frame.Arguments[0]);
                    break;
                case OpCode.GETARG_1:
                    PushValue(frame.Arguments[1]);
                    break;
                case OpCode.GETARG_2:
                    PushValue(frame.Arguments[2]);
                    break;
                case OpCode.GETARG_3:
                    PushValue(frame.Arguments[3]);
                    break;
                case OpCode.GETARG_B:
                    PushValue(frame.Arguments[frame.FetchByte()]);
                    break;
                case OpCode.GETARG:
                    PushValue(frame.Arguments[frame.FetchInt32()]);
                    break;

                case OpCode.SETCLOSURE_0:
                    frame.Upvalues![0].Set(PopValue());
                    break;
                case OpCode.SETCLOSURE_1:
                    frame.Upvalues![1].Set(PopValue());
                    break;
                case OpCode.SETCLOSURE_2:
                    frame.Upvalues![2].Set(PopValue());
                    break;
                case OpCode.SETCLOSURE_3:
                    frame.Upvalues![3].Set(PopValue());
                    break;
                case OpCode.SETCLOSURE_B:
                    frame.Upvalues![frame.FetchByte()].Set(PopValue());
                    break;
                case OpCode.SETCLOSURE:
                    frame.Upvalues![frame.FetchInt32()].Set(PopValue());
                    break;

                case OpCode.GETCLOSURE_0:
                    PushValue(frame.Upvalues![0].Get());
                    break;
                case OpCode.GETCLOSURE_1:
                    PushValue(frame.Upvalues![1].Get());
                    break;
                case OpCode.GETCLOSURE_2:
                    PushValue(frame.Upvalues![2].Get());
                    break;
                case OpCode.GETCLOSURE_3:
                    PushValue(frame.Upvalues![3].Get());
                    break;
                case OpCode.GETCLOSURE_B:
                    PushValue(frame.Upvalues![frame.FetchByte()].Get());
                    break;
                case OpCode.GETCLOSURE:
                    PushValue(frame.Upvalues![frame.FetchInt32()].Get());
                    break;

                case OpCode.LENGTH:
                {
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.LengthMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a);
                            break;
                        }
                    }

                    PushValue(a.GetLength());
                    break;
                }

                case OpCode.TOSTRING:
                {
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.ToStringMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a);
                            break;
                        }
                    }

                    PushValue(a.ConvertToString());
                    break;
                }

                case OpCode.TONUMBER:
                {
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.ToNumberMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a);
                            break;
                        }
                    }

                    PushValue(a.ConvertToNumber());
                    break;
                }

                case OpCode.TYPE:
                {
                    PushValue(PopValue().Type);
                    break;
                }

                case OpCode.NTYPE:
                {
                    a = PopValue();

                    if (a.Type != DynamicValueType.NativeObject)
                    {
                        PushValue(DynamicValue.Nil);
                    }
                    else
                    {
                        var obj = a.NativeObject!;

                        if (obj is INativeTypeProvider nativeTypeProvider)
                        {
                            PushValue(nativeTypeProvider.ProvideType());
                        }
                        else
                        {
                            PushValue(a.NativeObject!.GetType().FullName!);
                        }
                    }

                    break;
                }

                case OpCode.NJMP:
                {
                    var targetIp = frame.FetchInt32();

                    if (PopValue() == DynamicValue.Nil)
                    {
                        frame.JumpAbsolute(targetIp);
                    }

                    break;
                }

                case OpCode.VJMP:
                {
                    var targetIp = frame.FetchInt32();

                    if (PopValue() != DynamicValue.Nil)
                    {
                        frame.JumpAbsolute(targetIp);
                    }

                    break;
                }

                case OpCode.FJMP:
                {
                    var targetIp = frame.FetchInt32();

                    if (!DynamicValue.IsTruth(PopValue()))
                    {
                        frame.JumpAbsolute(targetIp);
                    }

                    break;
                }

                case OpCode.TJMP:
                {
                    var targetIp = frame.FetchInt32();

                    if (DynamicValue.IsTruth(PopValue()))
                    {
                        frame.JumpAbsolute(targetIp);
                    }

                    break;
                }

                case OpCode.JUMP:
                {
                    frame.JumpAbsolute(frame.FetchInt32());
                    break;
                }

                case OpCode.SWITCH:
                {
                    var switchTable = frame.Chunk.GetSwitchTable((int)frame.PreviousOpCodeIP);
                    var qualifier = PopValue();
                    
                    if (switchTable.Cases.TryGetValue(qualifier, out var targetIp))
                    {
                        frame.JumpAbsolute(targetIp);
                    }
                    else
                    {
                        frame.JumpAbsolute(switchTable.DefaultIp);
                    }
                    break;
                }

                case OpCode.EXISTS:
                {
                    b = PopValue();
                    a = PopValue();

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, Table.ExistsMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, b, a);
                            break;
                        }
                    }

                    PushValue(b.Contains(a));
                    break;
                }

                case OpCode.RET:
                {
                    frame.CloseUpvalues();
                    _callStack.Pop();
                    _fiber.ReturnScriptFrame(frame);
                    break;
                }

                case OpCode.INC:
                {
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.IncrementMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a);
                            break;
                        }
                    }

                    PushValue(a.Increment());
                    break;
                }

                case OpCode.DEC:
                {
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(a.Table!, Table.DecrementMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, a);
                            break;
                        }
                    }

                    PushValue(a.Decrement());
                    break;
                }

                case OpCode.TABNEW:
                {
                    PushValue(new Table());
                    break;
                }

                case OpCode.ARRNEW:
                {
                    a = PopValue();

                    if (a.Type != DynamicValueType.Number)
                    {
                        throw new UnsupportedDynamicValueOperationException("Array size must be a Number.");
                    }

                    PushValue(new Collections.Array((int)a.Number));
                    break;
                }

                case OpCode.ELINIT:
                {
                    b = PopValue();
                    a = PopValue();
                    c = PeekValue();

                    c.SetEntry(b, a);
                    break;
                }

                case OpCode.ARRINIT:
                {
                    var count = frame.FetchInt32();
                    var elements = PopArguments(count);
                    PushValue(new Collections.Array(elements));
                    break;
                }

                case OpCode.TABINIT:
                {
                    var count = frame.FetchInt32();
                    if (count >= 0)
                    {
                        var elements = PopArguments(count);
                        PushValue(new Table(elements));
                    }
                    else
                    {
                        var pairCount = -count;
                        var items = PopArguments(pairCount * 2);
                        var table = new Table(pairCount);
                        for (var i = 0; i < pairCount; i++)
                        {
                            var val = items[i * 2];
                            var key = items[i * 2 + 1];
                            table.Set(key, val);
                        }

                        PushValue(table);
                    }

                    break;
                }

                case OpCode.STRCAT:
                {
                    var count = frame.FetchInt32();
                    var parts = PopArguments(count);
                    var strings = new string[count];
                    for (var i = 0; i < count; i++)
                    {
                        strings[i] = parts[i].Type == DynamicValueType.String ? parts[i].String! : parts[i].ConvertToString().String ?? string.Empty;
                    }

                    PushValue(string.Concat(strings));
                    break;
                }

                case OpCode.ELSET:
                {
                    b = PopValue(); // Key
                    c = PopValue(); // Table
                    a = PopValue(); // Value

                    if (c.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(c.Table!, Table.SetMetaKey, out var chunk))
                        {
                            InvokeChunk(chunk, c, b, a);
                            break;
                        }
                    }

                    c.SetEntry(b, a);
                    break;
                }

                case OpCode.INDEX:
                {
                    a = PopValue(); // Key
                    c = PopValue(); // Table

                    switch (c.Type)
                    {
                        case DynamicValueType.String when a.Type == DynamicValueType.String:
                        {
                            c = _global.Index("str");

                            if (c.Type != DynamicValueType.Table)
                            {
                                throw new UnsupportedDynamicValueOperationException(
                                    "Attempt to index a String using a String, but no `str' support table found."
                                );
                            }

                            break;
                        }

                        case DynamicValueType.Array when a.Type == DynamicValueType.String:
                        {
                            c = _global.Index("arr");

                            if (c.Type != DynamicValueType.Table)
                            {
                                throw new UnsupportedDynamicValueOperationException(
                                    "Attempt to index an Array using a String, but no `arr' support table found."
                                );
                            }

                            break;
                        }
                    }

                    if (c.Type == DynamicValueType.Table)
                    {
                        if (!c.Table!.Contains(a))
                        {
                            var mt = c.Table!.MetaTable;

                            if (mt != null)
                            {
                                var value = mt[Table.GetMetaKey];

                                if (value.Type == DynamicValueType.Chunk)
                                {
                                    InvokeChunk(value.Chunk!, c, a);
                                    break;
                                }

                                if (value.Type == DynamicValueType.Table)
                                {
                                    PushValue(value.Table!.Index(a));
                                    break;
                                }
                            }
                        }
                    }

                    PushValue(c.Index(a));
                    break;
                }

                case OpCode.INOCT:
                {
                    a = PopValue(); // Key
                    c = PopValue(); // Table

                    if (c.Type != DynamicValueType.Table)
                    {
                        throw new UnsupportedDynamicValueOperationException(
                            $"Attempt to index or create a table on an unsupported type '{c.Type}'"
                        );
                    }

                    var value = c.Index(a);

                    if (value == DynamicValue.Nil)
                    {
                        value = new Table();
                        c.SetEntry(a, value);
                    }

                    PushValue(value);
                    break;
                }

                case OpCode.TABCLN:
                {
                    var isDeepClone = frame.FetchByte() > 0;
                    a = PopValue();

                    if (a.Type != DynamicValueType.Table)
                    {
                        throw new UnsupportedDynamicValueOperationException(
                            $"Attempt to clone a {a.Type} value."
                        );
                    }

                    PushValue(
                        isDeepClone
                            ? a.Table!.DeepCopy()
                            : a.Table!.ShallowCopy()
                    );

                    break;
                }

                case OpCode.YIELD:
                {
                    var argumentCount = frame.FetchInt32();

                    a = PopValue(); // chunk
                    var args = PopArguments(argumentCount);

                    if (a.Type != DynamicValueType.Chunk)
                    {
                        throw new UnsupportedDynamicValueOperationException(
                            $"Attempt to yield to a {a.Type} value."
                        );
                    }

                    var awaitee = _fiber.SpawnChild(
                        false,
                        closureContexts: (Dictionary<string, ClosureContext>)frame.Fiber.ClosureContexts
                    );
                    awaitee.Schedule(a.Chunk!, args);
                    awaitee.Resume();

                    PushValue(awaitee);

                    _fiber.WaitFor(awaitee);
                    break;
                }

                case OpCode.YRET:
                {
                    a = PopValue();

                    if (a.Type != DynamicValueType.Fiber)
                    {
                        throw new UnsupportedDynamicValueOperationException(
                            $"Attempt to resume from a {a.Type} value. " +
                            $"Messing with code generation now, are we?"
                        );
                    }

                    PushValue(a.Fiber!.PopValue());
                    break;
                }

                case OpCode.XARGS:
                {
                    var mode = frame.FetchByte();

                    if (mode == 0)
                    {
                        PushValue(frame.ExtraArguments);
                    }
                    else if (mode == 1)
                    {
                        for (var i = 0; i < frame.ExtraArguments.Length; i++)
                        {
                            PushValue(frame.ExtraArguments[i]);
                        }
                    }

                    break;
                }

                case OpCode.EACH:
                {
                    a = PopValue();

                    if (a.Type == DynamicValueType.String)
                    {
                        a = Collections.Array.FromString(a.String!);
                    }

                    switch (a.Type)
                    {
                        case DynamicValueType.Table:
                        {
                            frame.PushEnumerator(a.Table!);
                            break;
                        }

                        case DynamicValueType.Array:
                        {
                            frame.PushEnumerator(a.Array!);
                            break;
                        }

                        default:
                        {
                            throw new UnsupportedDynamicValueOperationException(
                                $"Attempt to iterate over a {a.Type} value."
                            );
                        }
                    }

                    break;
                }

                case OpCode.NEXT:
                {
                    var isKeyValue = frame.FetchInt32() != 0;
                    var enumerator = frame.CurrentEnumerator ?? throw new VirtualMachineException(
                        "Attempt to iterate without an active iterator."
                    );

                    var next = enumerator.MoveNext();

                    if (next)
                    {
                        PushValue(enumerator.Current.Value);

                        if (isKeyValue)
                        {
                            PushValue(enumerator.Current.Key);
                        }
                    }

                    PushValue(next);
                    break;
                }

                case OpCode.EEND:
                {
                    frame.PopEnumerator();
                    break;
                }

                case OpCode.ENTER:
                {
                    var blockId = frame.FetchInt32();
                    frame.EnterProtectedBlock(blockId);
                    break;
                }

                case OpCode.THROW:
                {
                    _fiber.UnwindTryHandle(
                        _callStack.ToArray()
                    );
                    break;
                }

                case OpCode.LEAVE:
                {
                    frame.ExitProtectedBlock();
                    break;
                }

                case OpCode.ERRNEW:
                {
                    a = PopValue();

                    if (a.Type != DynamicValueType.Table)
                    {
                        throw new UnsupportedDynamicValueOperationException(
                            $"Attempt to create an Error out of a value type '{a.Type}'."
                        );
                    }

                    PushValue(new Error(a.Table!));
                    break;
                }

                default:
                {
                    throw new VirtualMachineException($"Invalid opcode '{opCode}'.");
                }
            }

            if (_fiber.State != FiberState.Running || _callStack.Count == 0)
            {
                return;
            }

            if (_callStack.Count != currentDepth)
            {
                frame = (_callStack.Peek() as ScriptStackFrame)!;

                if (frame == null)
                {
                    return;
                }

                currentDepth = _callStack.Count;
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool FindMetaFunction(Table table, string op, [MaybeNullWhen(false)] out Chunk chunk)
    {
        if (!table.HasMetaTable)
        {
            chunk = null;
            return false;
        }

        var value = table.MetaTable![op];
        if (value.Type != DynamicValueType.Chunk)
        {
            chunk = null;
            return false;
        }

        chunk = value.Chunk!;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void InvokeChunk(Chunk chunk)
    {
        _callStack.Push(_fiber.RentScriptFrame(chunk));
        _fiber.OnChunkInvoke?.Invoke(_fiber, chunk, false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void InvokeChunk(Chunk chunk, DynamicValue a)
    {
        _callStack.Push(_fiber.RentScriptFrame(chunk, a));
        _fiber.OnChunkInvoke?.Invoke(_fiber, chunk, false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void InvokeChunk(Chunk chunk, DynamicValue a, DynamicValue b)
    {
        _callStack.Push(_fiber.RentScriptFrame(chunk, a, b));
        _fiber.OnChunkInvoke?.Invoke(_fiber, chunk, false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void InvokeChunk(Chunk chunk, DynamicValue a, DynamicValue b, DynamicValue c)
    {
        _callStack.Push(_fiber.RentScriptFrame(chunk, a, b, c));
        _fiber.OnChunkInvoke?.Invoke(_fiber, chunk, false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void InvokeChunk(Chunk chunk, DynamicValue a, DynamicValue b, DynamicValue c, DynamicValue d)
    {
        _callStack.Push(_fiber.RentScriptFrame(chunk, a, b, c, d));
        _fiber.OnChunkInvoke?.Invoke(_fiber, chunk, false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void InvokeChunk(Chunk chunk, DynamicValue[] args)
    {
        _callStack.Push(_fiber.RentScriptFrame(chunk, args));
        _fiber.OnChunkInvoke?.Invoke(_fiber, chunk, false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void InvokeChunkFromStack(Chunk chunk, int argumentCount)
    {
        _callStack.Push(_fiber.RentScriptFrame(chunk, _evaluationStack, argumentCount));
        _fiber.OnChunkInvoke?.Invoke(_fiber, chunk, false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private DynamicValue PopValue()
    {
        return _evaluationStack.Pop();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private DynamicValue PeekValue()
    {
        return _evaluationStack.Peek();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void PushValue(DynamicValue value)
    {
        _evaluationStack.Push(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private DynamicValue[] PopArguments(int count)
    {
        return _evaluationStack.PopArguments(count);
    }

    private ScriptStackFrame? FindClosureSourceFrame(string enclosedFunctionName)
    {
        var currentFiber = _fiber;

        while (currentFiber != null)
        {
            var callStack = currentFiber.CallStack;

            for (var i = callStack.Count - 1; i >= 0; i--)
            {
                var frame = callStack[i].As<ScriptStackFrame>();
                if (frame.Chunk.Name == enclosedFunctionName)
                {
                    return frame;
                }
            }

            currentFiber = currentFiber.Parent;
        }

        return null;
    }

    private Upvalue ResolveUpvalue(ScriptStackFrame frame, ClosureInfo closure)
    {
        if (closure.IsSharedScope)
        {
            if (_fiber.TryGetSharedUpvalue(
                    closure.EnclosedFunctionName,
                    closure.EnclosedId,
                    closure.Type == ClosureType.Local,
                    out var shared))
            {
                return shared;
            }

            Upvalue? upval = null;
            if (frame.Chunk.Name == closure.EnclosedFunctionName)
            {
                if (closure.Type == ClosureType.Local)
                {
                    upval = frame.GetOrCreateLocalUpvalue(closure.EnclosedId);
                }
                else if (closure.Type == ClosureType.Parameter)
                {
                    upval = frame.GetOrCreateArgumentUpvalue(closure.EnclosedId);
                }
                else if (frame.Upvalues != null && closure.EnclosedId < frame.Upvalues.Length)
                {
                    upval = frame.Upvalues[closure.EnclosedId];
                }
            }
            else
            {
                var targetFrame = FindClosureSourceFrame(closure.EnclosedFunctionName);
                if (targetFrame != null)
                {
                    if (closure.Type == ClosureType.Local)
                    {
                        upval = targetFrame.GetOrCreateLocalUpvalue(closure.EnclosedId);
                    }
                    else if (closure.Type == ClosureType.Parameter)
                    {
                        upval = targetFrame.GetOrCreateArgumentUpvalue(closure.EnclosedId);
                    }
                    else if (targetFrame.Upvalues != null && closure.EnclosedId < targetFrame.Upvalues.Length)
                    {
                        upval = targetFrame.Upvalues[closure.EnclosedId];
                    }
                }
                else if (
                    _fiber.TryGetClosedUpvalue(
                        closure.EnclosedFunctionName,
                        closure.EnclosedId,
                        closure.Type == ClosureType.Local,
                        out var closed))
                {
                    upval = closed;
                }
            }

            upval ??= new Upvalue(DynamicValue.Nil);
            _fiber.RegisterSharedUpvalue(
                closure.EnclosedFunctionName,
                closure.EnclosedId,
                closure.Type == ClosureType.Local,
                upval
            );

            return upval;
        }

        if (frame.Chunk.Name == closure.EnclosedFunctionName)
        {
            if (closure.Type == ClosureType.Local)
            {
                return frame.GetOrCreateLocalUpvalue(closure.EnclosedId);
            }

            if (closure.Type == ClosureType.Parameter)
            {
                return frame.GetOrCreateArgumentUpvalue(closure.EnclosedId);
            }

            if (closure.Type == ClosureType.Closure
                && frame.Upvalues != null
                && closure.EnclosedId < frame.Upvalues.Length)
            {
                return frame.Upvalues[closure.EnclosedId];
            }
        }

        if (closure.Type == ClosureType.Closure
            && frame.Upvalues != null
            && closure.EnclosedId < frame.Upvalues.Length)
        {
            return frame.Upvalues[closure.EnclosedId];
        }

        var sourceFrame = FindClosureSourceFrame(closure.EnclosedFunctionName);
        if (sourceFrame != null)
        {
            if (closure.Type == ClosureType.Local)
            {
                return sourceFrame.GetOrCreateLocalUpvalue(closure.EnclosedId);
            }

            if (closure.Type == ClosureType.Parameter)
            {
                return sourceFrame.GetOrCreateArgumentUpvalue(closure.EnclosedId);
            }

            if (closure.Type == ClosureType.Closure && sourceFrame.Upvalues != null &&
                closure.EnclosedId < sourceFrame.Upvalues.Length)
            {
                return sourceFrame.Upvalues[closure.EnclosedId];
            }
        }

        if (_fiber.TryGetClosedUpvalue(
                closure.EnclosedFunctionName,
                closure.EnclosedId,
                closure.Type == ClosureType.Local,
                out var closedVal))
        {
            return closedVal;
        }

        return new Upvalue(DynamicValue.Nil);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetLocal(ScriptStackFrame frame, int localId, DynamicValue a)
    {
        if (frame.Locals![localId].Type == DynamicValueType.Nil && frame.HasOpenLocalUpvalue(localId))
        {
            frame.UpdateOpenLocalUpvalue(localId, a);
        }
        else
        {
            frame.InvalidateLocalUpvalue(localId);
        }

        frame.Locals![localId] = a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void SetArg(ScriptStackFrame frame, int argId, DynamicValue a)
    {
        frame.InvalidateArgumentUpvalue(argId);
        frame.Arguments[argId] = a;
    }
}