namespace EVIL.Ceres.ExecutionEngine;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
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

    public void Step()
    {
        var frame = _callStack.Peek().As<ScriptStackFrame>();
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
                var clone = frame.Chunk.SubChunks[frame.FetchInt32()].Clone();

                InitializeClosures(clone, frame);

                PushValue(clone);
                break;
            }

            case OpCode.LDTYPE:
            {
                PushValue((DynamicValueType)frame.FetchInt32());
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

                if (a.Type == DynamicValueType.Chunk)
                {
                    InvokeChunk(a.Chunk!, args);
                    break;
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
                var args = frame.Arguments;

                for (var i = 0; i < frame.Arguments.Length; i++)
                {
                    args[args.Length - i - 1] = PopValue();
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

            case OpCode.SETLOCAL:
            {
                a = PopValue();
                var localId = frame.FetchInt32();
                
                frame.Locals![localId] = a;
                PropagateValueToSubClosures(frame, ClosureType.Local, localId, a);
                break;
            }

            case OpCode.GETLOCAL:
            {
                PushValue(frame.Locals![frame.FetchInt32()]);
                break;
            }

            case OpCode.SETARG:
            {
                a = PopValue();
                var argId = frame.FetchInt32();

                frame.Arguments[argId] = a;
                PropagateValueToSubClosures(frame, ClosureType.Parameter, argId, a);
                break;
            }

            case OpCode.GETARG:
            {
                PushValue(frame.Arguments[frame.FetchInt32()]);
                break;
            }

            case OpCode.SETCLOSURE:
            {
                SetClosure(frame);
                break;
            }

            case OpCode.GETCLOSURE:
            {
                GetClosure(frame);
                break;
            }

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

                PushValue(
                    a.Type != DynamicValueType.NativeObject
                        ? DynamicValue.Nil
                        : a.NativeObject!.GetType().FullName!
                );

                break;
            }

            case OpCode.NJMP:
            {
                var labelId = frame.FetchInt32();

                if (PopValue() == DynamicValue.Nil)
                {
                    frame.JumpAbsolute(
                        frame.Chunk.Labels[labelId]
                    );
                }

                break;
            }

            case OpCode.VJMP:
            {
                var labelId = frame.FetchInt32();

                if (PopValue() != DynamicValue.Nil)
                {
                    frame.JumpAbsolute(
                        frame.Chunk.Labels[labelId]
                    );
                }

                break;
            }

            case OpCode.FJMP:
            {
                var labelId = frame.FetchInt32();

                if (!DynamicValue.IsTruth(PopValue()))
                {
                    frame.JumpAbsolute(
                        frame.Chunk.Labels[labelId]
                    );
                }

                break;
            }

            case OpCode.TJMP:
            {
                var labelId = frame.FetchInt32();

                if (DynamicValue.IsTruth(PopValue()))
                {
                    frame.JumpAbsolute(
                        frame.Chunk.Labels[labelId]
                    );
                }

                break;
            }

            case OpCode.JUMP:
            {
                frame.JumpAbsolute(
                    frame.Chunk.Labels[
                        frame.FetchInt32()
                    ]
                );
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
                _callStack.Pop();
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

    private ClosureInfo ResolveInnermostClosure(ClosureInfo closureInfo, ref ScriptStackFrame? frame)
    {
        while (closureInfo.Type == ClosureType.Closure)
        {
            closureInfo = frame!.Chunk.Closures[closureInfo.EnclosedId];
            frame = FindClosureSourceFrame(closureInfo.EnclosedFunctionName);
        }

        return closureInfo;
    }

    private DynamicValue GetClosureValue(ScriptStackFrame currentFrame, ClosureInfo info)
    {
        var sourceFrame = FindClosureSourceFrame(info.EnclosedFunctionName);

        if (sourceFrame != null)
        {
            if (info.Type == ClosureType.Parameter)
            {
                return sourceFrame.Arguments[info.EnclosedId];
            }

            if (info.Type == ClosureType.Closure)
            {
                var resolved = ResolveInnermostClosure(info, ref sourceFrame);
                var ctx = resolved.IsSharedScope
                    ? sourceFrame!.Fiber.ClosureContexts[resolved.EnclosedFunctionName]
                    : sourceFrame!.Chunk.ClosureContexts[resolved.EnclosedFunctionName];

                return ctx.Values[resolved.EnclosedId];
            }

            return sourceFrame.Locals![info.EnclosedId];
        }
        else
        {
            if (info.Type == ClosureType.Closure)
            {
                var chunk = currentFrame.Chunk;
                while (chunk.Name != info.EnclosedFunctionName)
                {
                    chunk = chunk.Parent!;
                }

                info = chunk.Closures[info.EnclosedId];
            }

            var ctx = info.IsSharedScope
                ? currentFrame.Fiber.ClosureContexts[info.EnclosedFunctionName]
                : currentFrame.Chunk.ClosureContexts[info.EnclosedFunctionName];

            return ctx.Values[info.EnclosedId];
        }
    }

    private void SetClosureValue(ScriptStackFrame currentFrame, ClosureInfo info, DynamicValue value)
    {
        var targetFrame = FindClosureSourceFrame(info.EnclosedFunctionName);

        if (targetFrame != null)
        {
            if (info.Type == ClosureType.Parameter)
            {
                targetFrame.Arguments[info.EnclosedId] = value;
                return;
            }

            if (info.Type == ClosureType.Closure)
            {
                var resolved = ResolveInnermostClosure(info, ref targetFrame);
                var ctx = resolved.IsSharedScope
                    ? targetFrame!.Fiber.ClosureContexts[resolved.EnclosedFunctionName]
                    : targetFrame!.Chunk.ClosureContexts[resolved.EnclosedFunctionName];

                ctx.Values[resolved.EnclosedId] = value;
                return;
            }

            targetFrame.Locals![info.EnclosedId] = value;
            return;
        }

        if (info.Type == ClosureType.Closure)
        {
            var chunk = currentFrame.Chunk;
            while (chunk.Name != info.EnclosedFunctionName)
            {
                chunk = chunk.Parent!;
            }

            info = chunk.Closures[info.EnclosedId];
        }

        var context = info.IsSharedScope
            ? currentFrame.Fiber.ClosureContexts[info.EnclosedFunctionName]
            : currentFrame.Chunk.ClosureContexts[info.EnclosedFunctionName];

        context.Values[info.EnclosedId] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void InitializeClosures(Chunk chunk, ScriptStackFrame stackFrame)
    {
        var subChunkQueue = new Queue<Chunk>();
        subChunkQueue.Enqueue(chunk);

        while (subChunkQueue.Count > 0)
        {
            var current = subChunkQueue.Dequeue();

            foreach (var closure in current.Closures)
            {
                ClosureContext ctx = closure.IsSharedScope
                    ? stackFrame.Fiber.SetClosureContext(closure.EnclosedFunctionName)
                    : current.SetClosureContext(closure.EnclosedFunctionName);

                var sourceFrame = FindClosureSourceFrame(closure.EnclosedFunctionName);
                if (sourceFrame == null)
                    continue;

                DynamicValue value = closure.Type switch
                {
                    ClosureType.Parameter => sourceFrame.Arguments[closure.EnclosedId],
                    ClosureType.Closure => GetClosureValue(stackFrame, sourceFrame.Chunk.Closures[closure.EnclosedId]),
                    _ => sourceFrame.Locals![closure.EnclosedId]
                };

                ctx.Values[closure.EnclosedId] = value;
            }

            for (var i = 0; i < current.SubChunks.Count; i++) 
                subChunkQueue.Enqueue(current.SubChunks[i].Clone());
        }
    }

    private void PropagateValueToSubClosures(ScriptStackFrame frame, ClosureType closureType, int slotId, DynamicValue value)
    {
        var subChunkQueue = new Queue<Chunk>(frame.Chunk.SubChunks);

        while (subChunkQueue.Count > 0)
        {
            var current = subChunkQueue.Dequeue();

            foreach (var closure in current.Closures)
            {
                if (closure.Type != closureType || closure.EnclosedId != slotId)
                    continue;

                var context = closure.IsSharedScope
                    ? frame.Fiber.SetClosureContext(closure.EnclosedFunctionName)
                    : current.SetClosureContext(closure.EnclosedFunctionName);

                var sourceFrame = FindClosureSourceFrame(closure.EnclosedFunctionName);
                if (sourceFrame != null)
                {
                    context.Values[closure.EnclosedId] = value;
                }
            }

            for (var i = 0; i < current.SubChunks.Count; i++) 
                subChunkQueue.Enqueue(current.SubChunks[i].Clone());
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GetClosure(ScriptStackFrame frame)
    {
        var info = frame.Chunk.Closures[frame.FetchInt32()];
        PushValue(GetClosureValue(frame, info));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetClosure(ScriptStackFrame frame)
    {
        var id = frame.FetchInt32();
        var info = frame.Chunk.Closures[id];
        var value = PopValue();

        SetClosureValue(frame, info, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void InvokeChunk(Chunk chunk, params DynamicValue[] args)
    {
        _callStack.Push(new ScriptStackFrame(_fiber, chunk, args));
        _fiber.OnChunkInvoke?.Invoke(
            _fiber, 
            chunk, 
            false
        );
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
        var args = new DynamicValue[count];
        for (var i = 0; i < count; i++)
        {
            args[count - i - 1] = PopValue();
        }

        return args;
    }
}