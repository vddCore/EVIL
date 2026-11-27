namespace EVIL.VM.ExecutionEngine;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EVIL.Core.Common.TypeSystem;
using EVIL.VM.ExecutionEngine.Collections;
using EVIL.VM.ExecutionEngine.Concurrency;
using EVIL.VM.ExecutionEngine.Diagnostics;
using EVIL.VM.ExecutionEngine.Marshaling;
using EVIL.VM.ExecutionEngine.TypeSystem;
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Add, Table.AddMetaKeyVal, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, MetamethodFlags.Add, Table.AddMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Sub, Table.SubtractMetaKeyVal, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, MetamethodFlags.Sub, Table.SubtractMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Mul, Table.MultiplyMetaKeyVal, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, MetamethodFlags.Mul, Table.MultiplyMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Div, Table.DivideMetaKeyVal, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, MetamethodFlags.Div, Table.DivideMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Mod, Table.ModuloMetaKeyVal, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, MetamethodFlags.Mod, Table.ModuloMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Shl, Table.ShiftLeftMetaKeyVal, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, MetamethodFlags.Shl, Table.ShiftLeftMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Shr, Table.ShiftRightMetaKeyVal, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, MetamethodFlags.Shr, Table.ShiftRightMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Aneg, Table.ArithmeticNegateMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Lnot, Table.LogicalNotMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Lor, Table.LogicalOrMetaKeyVal, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, MetamethodFlags.Lor, Table.LogicalOrMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Land, Table.LogicalAndMetaKeyVal, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, MetamethodFlags.Land, Table.LogicalAndMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Bor, Table.BitwiseOrMetaKeyVal, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, MetamethodFlags.Bor, Table.BitwiseOrMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Bxor, Table.BitwiseXorMetaKeyVal, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, MetamethodFlags.Bxor, Table.BitwiseXorMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Band, Table.BitwiseAndMetaKeyVal, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, MetamethodFlags.Band, Table.BitwiseAndMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Bnot, Table.BitwiseNotMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Deq, Table.DeepEqualMetaKeyVal, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, MetamethodFlags.Deq, Table.DeepEqualMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Dne, Table.DeepNotEqualMetaKeyVal, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, MetamethodFlags.Dne, Table.DeepNotEqualMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Eq, Table.EqualMetaKeyVal, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, MetamethodFlags.Eq, Table.EqualMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Ne, Table.NotEqualMetaKeyVal, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, MetamethodFlags.Ne, Table.NotEqualMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Gt, Table.GreaterThanMetaKeyVal, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, MetamethodFlags.Gt, Table.GreaterThanMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Gte, Table.GreaterEqualMetaKeyVal, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, MetamethodFlags.Gte, Table.GreaterEqualMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Lt, Table.LessThanMetaKeyVal, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, MetamethodFlags.Lt, Table.LessThanMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Lte, Table.LessEqualMetaKeyVal, out var chunk))
                        {
                            InvokeChunk(chunk, a, b);
                            break;
                        }
                    }

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, MetamethodFlags.Lte, Table.LessEqualMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Invoke, Table.InvokeMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Len, Table.LengthMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.ToStr, Table.ToStringMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.ToNum, Table.ToNumberMetaKeyVal, out var chunk))
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

                    frame.JumpAbsolute(switchTable.Cases.TryGetValue(qualifier, out var targetIp)
                        ? targetIp
                        : switchTable.DefaultIp);
                    
                    break;
                }

                case OpCode.EXISTS:
                {
                    b = PopValue();
                    a = PopValue();

                    if (b.Type == DynamicValueType.Table)
                    {
                        if (FindMetaFunction(b.Table!, MetamethodFlags.Exists, Table.ExistsMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Inc, Table.IncrementMetaKeyVal, out var chunk))
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
                        if (FindMetaFunction(a.Table!, MetamethodFlags.Dec, Table.DecrementMetaKeyVal, out var chunk))
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

                case OpCode.ARRDEC:
                {
                    var count = frame.FetchInt32();
                    var source = PopValue();

                    if (count < 0)
                    {
                        throw new UnsupportedDynamicValueOperationException(
                            $"Invalid array deconstruction size '{count}'."
                        );
                    }

                    if (source.Type != DynamicValueType.Array)
                    {
                        throw new UnsupportedDynamicValueOperationException(
                            $"Array deconstruction requires an Array, but received a {source.Type}."
                        );
                    }

                    var array = source.Array!;
                    if (array.Length < count)
                    {
                        throw new ArrayException(
                            $"Array deconstruction requires at least {count} elements, but the array contains {array.Length}."
                        );
                    }

                    for (var i = 0; i < count; i++)
                    {
                        PushValue(array[i]);
                    }

                    break;
                }

                case OpCode.TABINIT:
                {
                    var count = frame.FetchInt32();
                    if (count >= 0)
                    {
                        var elements = _evaluationStack.PopSpan(count);
                        PushValue(new Table(elements));
                    }
                    else
                    {
                        var pairCount = -count;
                        var items = _evaluationStack.PopSpan(pairCount * 2);
                        PushValue(Table.CreateKeyed(items));
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
                        if (FindMetaFunction(c.Table!, MetamethodFlags.Set, Table.SetMetaKeyVal, out var chunk))
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
                        IndexTable(c, a);
                        break;
                    }

                    PushValue(c.Index(a));
                    break;
                }

                case OpCode.TABGET:
                {
                    var key = frame.Chunk.StringPool.GetDynamicValue(
                        frame.FetchInt32()
                    );
                    var source = PopValue();

                    if (source.Type != DynamicValueType.Table)
                    {
                        throw new UnsupportedDynamicValueOperationException(
                            $"Table deconstruction requires a Table, but received a {source.Type}."
                        );
                    }

                    IndexTable(source, key);
                    break;
                }

                case OpCode.SLICE:
                {
                    var flags = (SliceFlags)frame.FetchByte();
                    DynamicValue? end = null;
                    DynamicValue? start = null;
                    
                    if (flags.HasFlag(SliceFlags.EndPresent))
                    {
                        end = PopValue();
                    }
                    
                    if (flags.HasFlag(SliceFlags.StartPresent))
                    {
                        start = PopValue();
                    }

                    c = PopValue();
                    PushValue(c.Slice(start, end, flags));
                    break;
                }

                case OpCode.GETPROP_B:
                case OpCode.GETPROP:
                {
                    var opOffset = (int)(frame.IP - 1);
                    var strId = opCode == OpCode.GETPROP_B 
                        ? frame.FetchByte() 
                        : frame.FetchInt32();
                    
                    var key = frame.Chunk.StringPool.GetDynamicValue(strId);
                    c = PopValue(); // target

                    switch (c.Type)
                    {
                        case DynamicValueType.String:
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

                        case DynamicValueType.Array:
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
                        var tbl = c.Table!;
                        ref var ic = ref frame.Chunk.GetPropertyInlineCache(opOffset);

                        // monomorph cache hit.
                        if (tbl.Shape != null && tbl.Shape == ic.Shape0)
                        {
                            PushValue(tbl.ShapeSlots![ic.Slot0]);
                            break;
                        }

                        // bimorph cache hit.
                        if (tbl.Shape != null && tbl.Shape == ic.Shape1)
                        {
                            PushValue(tbl.ShapeSlots![ic.Slot1]);
                            break;
                        }

                        // cache miss.
                        if (tbl.TryGetValue(key, out var val))
                        {
                            if (tbl.Shape != null && key.String != null && tbl.Shape.TryGetSlot(key.String, out var slot))
                            {
                                if (ic.Shape0 == null)
                                {
                                    ic.Shape0 = tbl.Shape;
                                    ic.Slot0 = slot;
                                }
                                else if (ic.Shape1 == null && tbl.Shape != ic.Shape0)
                                {
                                    ic.Shape1 = tbl.Shape;
                                    ic.Slot1 = slot;
                                }
                            }

                            PushValue(val);
                            break;
                        }

                        var mt = tbl.MetaTable;
                        if (mt != null 
                            && (mt.MetamethodFlags & MetamethodFlags.Get) != 0
                            && mt.TryGetValue(Table.GetMetaKeyVal, out var value))
                        {
                            if (value.Type == DynamicValueType.Chunk)
                            {
                                InvokeChunk(
                                    value.Chunk!,
                                    c,
                                    key
                                );
                                
                                break;
                            }

                            if (value.Type == DynamicValueType.Table)
                            {
                                PushValue(
                                    value.Table!.Index(key)
                                );
                                
                                break;
                            }
                        }

                        PushValue(DynamicValue.Nil);
                        break;
                    }

                    PushValue(c.Index(key));
                    break;
                }

                case OpCode.SETPROP_B:
                case OpCode.SETPROP:
                {
                    var opOffset = (int)(frame.IP - 1);
                    var strId = opCode == OpCode.SETPROP_B 
                        ? frame.FetchByte()
                        : frame.FetchInt32();
                    
                    var key = frame.Chunk.StringPool.GetDynamicValue(strId);
                    c = PopValue(); // table
                    a = PopValue(); // value

                    if (c.Type == DynamicValueType.Table)
                    {
                        var tbl = c.Table!;
                        ref var ic = ref frame.Chunk.GetPropertyInlineCache(opOffset);

                        // cache hit, no __set.
                        if (!tbl.IsFrozen
                            && a != DynamicValue.Nil 
                            && tbl.Shape != null
                            && tbl.Shape == ic.Shape0 
                            && !FindMetaFunction(tbl, MetamethodFlags.Set, Table.SetMetaKeyVal, out _))
                        {
                            tbl.ShapeSlots![ic.Slot0] = a;
                            tbl.IncrementVersion();
                            break;
                        }

                        if (!tbl.IsFrozen 
                            && a != DynamicValue.Nil
                            && tbl.Shape != null 
                            && tbl.Shape == ic.Shape1
                            && !FindMetaFunction(tbl, MetamethodFlags.Set, Table.SetMetaKeyVal, out _))
                        {
                            tbl.ShapeSlots![ic.Slot1] = a;
                            tbl.IncrementVersion();
                            break;
                        }

                        if (FindMetaFunction(tbl, MetamethodFlags.Set, Table.SetMetaKeyVal, out var chunk))
                        {
                            InvokeChunk(chunk, c, key, a);
                            break;
                        }

                        tbl.Set(key, a);
                        if (tbl.Shape != null && key.String != null && tbl.Shape.TryGetSlot(key.String, out var slot))
                        {
                            if (ic.Shape0 == null)
                            {
                                ic.Shape0 = tbl.Shape;
                                ic.Slot0 = slot;
                            }
                            else if (ic.Shape1 == null && tbl.Shape != ic.Shape0)
                            {
                                ic.Shape1 = tbl.Shape;
                                ic.Slot1 = slot;
                            }
                        }
                        break;
                    }

                    c.SetEntry(key, a);
                    break;
                }

                case OpCode.INVOKEMETHOD:
                {
                    var strId = frame.FetchInt32();
                    var argumentCount = frame.FetchInt32();
                    var key = frame.Chunk.StringPool.GetDynamicValue(strId);

                    var args = PopArguments(argumentCount);
                    var receiver = PopValue();

                    var method = receiver.Index(key);
                    if (method.Type == DynamicValueType.Chunk)
                    {
                        var callArgs = new DynamicValue[argumentCount + 1];
                        callArgs[0] = receiver;
                        
                        Array.Copy(args, 0, callArgs, 1, argumentCount);
                        InvokeChunk(method.Chunk!, callArgs);
  
                        break;
                    }

                    if (method.Type == DynamicValueType.NativeFunction)
                    {
                        var callArgs = new DynamicValue[argumentCount + 1];
                        callArgs[0] = receiver;
                        
                        Array.Copy(args, 0, callArgs, 1, argumentCount);
                        
                        _callStack.Push(new NativeStackFrame(
                            method.NativeFunction!
                        ));
                        
                        _fiber.OnNativeFunctionInvoke?.Invoke(
                            _fiber, 
                            method.NativeFunction!
                        );
                        
                        var value = method.NativeFunction!.Invoke(
                            _fiber, 
                            callArgs
                        );
                        
                        if (_callStack.Peek() is NativeStackFrame)
                        {
                            PushValue(value);
                            _callStack.Pop();
                        }
                        
                        break;
                    }

                    throw new UnsupportedDynamicValueOperationException(
                        $"Attempt to invoke method '{key.String}', but member is not a function."
                    );
                }

                case OpCode.INOCT:
                {
                    a = PopValue(); // key
                    c = PopValue(); // table

                    if (c.Type != DynamicValueType.Table)
                    {
                        throw new UnsupportedDynamicValueOperationException(
                            $"Attempt to index or create a table on an unsupported type '{c.Type}'"
                        );
                    }

                    var tbl = c.Table!;
                    if (!tbl.TryGetValue(a, out var value) 
                        || value == DynamicValue.Nil)
                    {
                        value = new Table();
                        tbl.Set(a, value);
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

                    PushValue(isDeepClone
                        ? a.Table!.DeepCopy()
                        : a.Table!.ShallowCopy()
                    );

                    break;
                }

                case OpCode.TABSPRD:
                {
                    var isNilTolerant = frame.FetchByte() > 0;
                    a = PopValue();
                    c = PeekValue();

                    if (c.Type != DynamicValueType.Table)
                    {
                        throw new UnsupportedDynamicValueOperationException(
                            $"Attempt to spread into a {c.Type} value."
                        );
                    }

                    if (a.Type == DynamicValueType.Nil)
                    {
                        if (!isNilTolerant)
                        {
                            throw new UnsupportedDynamicValueOperationException(
                                "Attempt to spread a Nil value."
                            );
                        }

                        break;
                    }

                    if (a.Type != DynamicValueType.Table)
                    {
                        throw new UnsupportedDynamicValueOperationException(
                            $"Attempt to spread a {a.Type} value."
                        );
                    }

                    var dest = c.Table!;
                    var src = a.Table!;

                    foreach (var (key, value) in src)
                    {
                        dest.Set(key, value);
                    }

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
                    ref var enumerator = ref frame.CurrentEnumerator;

                    var next = enumerator.MoveNext();

                    if (next)
                    {
                        PushValue(enumerator.CurrentValue);

                        if (isKeyValue)
                        {
                            PushValue(enumerator.CurrentKey);
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
                    var regionId = frame.FetchInt32();
                    frame.EnterProtectedBlock(regionId);
                    break;
                }

                case OpCode.RETRY:
                {
                    var regionId = frame.FetchInt32();
                    frame.RetryProtectedBlock(regionId);
                    break;
                }

                case OpCode.THROW:
                {
                    _fiber.UnwindTryHandle();
                    break;
                }

                case OpCode.LEAVE:
                {
                    var regionId = frame.FetchInt32();
                    frame.ExitProtectedBlock(regionId, ProtectedRegionPhase.Body);
                    break;
                }

                case OpCode.LEAVE_H:
                {
                    var regionId = frame.FetchInt32();
                    frame.ExitProtectedBlock(regionId, ProtectedRegionPhase.Handler);
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
                currentDepth = _callStack.Count;
            }
        }
    }

    private void IndexTable(DynamicValue tableValue, DynamicValue key)
    {
        var table = tableValue.Table!;
        if (table.TryGetValue(key, out var value))
        {
            PushValue(value);
            return;
        }

        var metaTable = table.MetaTable;
        if (metaTable != null
            && (metaTable.MetamethodFlags & MetamethodFlags.Get) != 0
            && metaTable.TryGetValue(Table.GetMetaKeyVal, out var getValue))
        {
            if (getValue.Type == DynamicValueType.Chunk)
            {
                InvokeChunk(getValue.Chunk!, tableValue, key);
                return;
            }

            if (getValue.Type == DynamicValueType.Table)
            {
                PushValue(getValue.Table!.Index(key));
                return;
            }
        }

        PushValue(DynamicValue.Nil);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool FindMetaFunction(Table table, MetamethodFlags flag, DynamicValue opKeyVal, [MaybeNullWhen(false)] out Chunk chunk)
    {
        var mt = table.MetaTable;
        if (mt == null || (mt.MetamethodFlags & flag) == 0)
        {
            chunk = null;
            return false;
        }

        if (mt.TryGetValue(opKeyVal, out var value) && value.Type == DynamicValueType.Chunk)
        {
            chunk = value.Chunk!;
            return true;
        }

        chunk = null;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool FindMetaFunction(Table table, string op, [MaybeNullWhen(false)] out Chunk chunk)
    {
        var mt = table.MetaTable;
        if (mt == null)
        {
            chunk = null;
            return false;
        }

        var flag = Table.GetMetamethodFlag(op);
        if (flag != MetamethodFlags.None && (mt.MetamethodFlags & flag) == 0)
        {
            chunk = null;
            return false;
        }

        if (mt.TryGetValue(op, out var value) && value.Type == DynamicValueType.Chunk)
        {
            chunk = value.Chunk!;
            return true;
        }

        chunk = null;
        return false;
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

    private static bool IsClosureOwner(Chunk chunk, ClosureInfo closure)
        => chunk.ClosureOwnerId == closure.EnclosedOwnerId;

    private ScriptStackFrame? FindClosureSourceFrame(ClosureInfo closure)
    {
        var currentFiber = _fiber;

        while (currentFiber != null)
        {
            var callStack = currentFiber.CallStack;

            for (var i = callStack.Count - 1; i >= 0; i--)
            {
                var frame = callStack[i].As<ScriptStackFrame>();
                if (IsClosureOwner(frame.Chunk, closure))
                {
                    return frame;
                }
            }

            currentFiber = currentFiber.Parent;
        }

        return null;
    }

    private bool TryGetSharedUpvalue(ClosureInfo closure, out Upvalue upvalue)
        => _fiber.TryGetSharedUpvalue(
            closure.EnclosedOwnerId,
            closure.EnclosedId,
            closure.Type == ClosureType.Local,
            out upvalue
        );

    private bool TryGetClosedUpvalue(ClosureInfo closure, out Upvalue upvalue)
        => _fiber.TryGetClosedUpvalue(
            closure.EnclosedOwnerId,
            closure.EnclosedId,
            closure.Type == ClosureType.Local,
            out upvalue
        );

    private void RegisterSharedUpvalue(ClosureInfo closure, Upvalue upvalue)
        => _fiber.RegisterSharedUpvalue(
            closure.EnclosedOwnerId,
            closure.EnclosedId,
            closure.Type == ClosureType.Local,
            upvalue
        );

    private Upvalue ResolveUpvalue(ScriptStackFrame frame, ClosureInfo closure)
    {
        if (closure.IsSharedScope)
        {
            if (TryGetSharedUpvalue(closure, out var shared))
            {
                return shared;
            }

            Upvalue? upval = null;
            if (IsClosureOwner(frame.Chunk, closure))
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
                var targetFrame = FindClosureSourceFrame(closure);
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
                else if (TryGetClosedUpvalue(closure, out var closed))
                {
                    upval = closed;
                }
            }

            upval ??= new Upvalue(DynamicValue.Nil);
            RegisterSharedUpvalue(closure, upval);

            return upval;
        }

        if (IsClosureOwner(frame.Chunk, closure))
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

        var sourceFrame = FindClosureSourceFrame(closure);
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

        if (TryGetClosedUpvalue(closure, out var closedVal))
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