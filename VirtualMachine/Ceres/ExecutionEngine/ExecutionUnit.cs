using System.Collections.Generic;
using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using EVIL.CommonTypes.TypeSystem;

namespace Ceres.ExecutionEngine
{
    internal class ExecutionUnit
    {
        private readonly Table _global;
        private readonly Fiber _fiber;
        private readonly CallStack _callStack;
        private readonly Stack<DynamicValue> _evaluationStack;

        public ExecutionUnit(
            Table global,
            Fiber fiber,
            Stack<DynamicValue> evaluationStack,
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
                    var chunk = frame.Chunk.SubChunks[
                        frame.FetchInt32()
                    ].Clone();

                    for (var i = 0; i < chunk.ClosureCount; i++)
                    {
                        var closure = chunk.Closures[i];
                        var sourceFrame = _callStack[closure.NestingLevel - 1].As<ScriptStackFrame>();

                        if (closure.IsParameter)
                        {
                            closure.Value = sourceFrame.Arguments[closure.EnclosedId];
                        }
                        else if (closure.IsClosure)
                        {
                            closure.Value = sourceFrame.Chunk.Closures[closure.EnclosedId].Value;
                        }
                        else
                        {
                            closure.Value = sourceFrame.Locals![closure.EnclosedId];
                        }
                    }

                    PushValue(chunk);
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
                        if (a.Table!.TryGetOverride(TableOverride.Add, out var chunk))
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
                        if (a.Table!.TryGetOverride(TableOverride.Subtract, out var chunk))
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
                        if (a.Table!.TryGetOverride(TableOverride.Multiply, out var chunk))
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
                        if (a.Table!.TryGetOverride(TableOverride.Divide, out var chunk))
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
                        if (a.Table!.TryGetOverride(TableOverride.Modulo, out var chunk))
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
                        if (a.Table!.TryGetOverride(TableOverride.ShiftLeft, out var chunk))
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
                        if (a.Table!.TryGetOverride(TableOverride.ShiftRight, out var chunk))
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
                        if (a.Table!.TryGetOverride(TableOverride.ArithmeticNegate, out var chunk))
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
                        if (a.Table!.TryGetOverride(TableOverride.LogicalNot, out var chunk))
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
                        if (a.Table!.TryGetOverride(TableOverride.LogicalOr, out var chunk))
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
                        if (a.Table!.TryGetOverride(TableOverride.LogicalAnd, out var chunk))
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
                        if (a.Table!.TryGetOverride(TableOverride.BitwiseOr, out var chunk))
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
                        if (a.Table!.TryGetOverride(TableOverride.BitwiseXor, out var chunk))
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
                        if (a.Table!.TryGetOverride(TableOverride.BitwiseAnd, out var chunk))
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
                        if (a.Table!.TryGetOverride(TableOverride.BitwiseNot, out var chunk))
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
                        if (a.Table!.TryGetOverride(TableOverride.DeepEqual, out var chunk))
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
                        if (a.Table!.TryGetOverride(TableOverride.DeepNotEqual, out var chunk))
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
                        if (a.Table!.TryGetOverride(TableOverride.Equal, out var chunk))
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
                        if (a.Table!.TryGetOverride(TableOverride.NotEqual, out var chunk))
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
                        if (a.Table!.TryGetOverride(TableOverride.GreaterThan, out var chunk))
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
                        if (a.Table!.TryGetOverride(TableOverride.GreaterThanOrEqual, out var chunk))
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
                        if (a.Table!.TryGetOverride(TableOverride.LessThan, out var chunk))
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
                        if (a.Table!.TryGetOverride(TableOverride.LessThanOrEqual, out var chunk))
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
                    var args = PopArguments(argumentCount);
                    
                    if (a.Type == DynamicValueType.Table)
                    {
                        if (a.Table!.TryGetOverride(TableOverride.Invoke, out var chunk))
                        {
                            var overrideArgs = new DynamicValue[args.Length + 1];
                            overrideArgs[0] = a;
                            System.Array.Copy(
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
                        {
                            PushValue(a.NativeFunction!.Invoke(_fiber, args));

                            _fiber.OnNativeFunctionInvoke?.Invoke(
                                _fiber,
                                a.NativeFunction!
                            );
                        }
                        _callStack.Pop();
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
                    frame.Locals![frame.FetchInt32()] = PopValue();
                    break;
                }

                case OpCode.GETLOCAL:
                {
                    PushValue(frame.Locals![frame.FetchInt32()]);
                    break;
                }

                case OpCode.SETARG:
                {
                    frame.Arguments[frame.FetchInt32()] = PopValue();
                    break;
                }

                case OpCode.GETARG:
                {
                    PushValue(frame.Arguments[frame.FetchInt32()]);
                    break;
                }

                case OpCode.SETCLOSURE:
                {
                    var closureInfo = frame.Chunk.Closures[frame.FetchInt32()];

                    ScriptStackFrame? targetFrame = null;

                    for (var i = 0; i < _callStack.Count; i++)
                    {
                        var tmpScriptFrame = _callStack[i].As<ScriptStackFrame>();

                        if (tmpScriptFrame.Chunk.Name == closureInfo.EnclosedFunctionName)
                        {
                            targetFrame = tmpScriptFrame;
                            break;
                        }
                    }

                    var value = PopValue();

                    if (targetFrame != null)
                    {
                        if (closureInfo.IsParameter)
                        {
                            targetFrame.Arguments[closureInfo.EnclosedId] = value;
                        }
                        else
                        {
                            targetFrame.Locals![closureInfo.EnclosedId] = value;
                        }
                    }
                    else
                    {
                        closureInfo.Value = value;
                    }

                    break;
                }

                case OpCode.GETCLOSURE:
                {
                    var closureInfo = frame.Chunk.Closures[frame.FetchInt32()];

                    ScriptStackFrame? targetFrame = null;
                    for (var i = 0; i < _callStack.Count; i++)
                    {
                        var tmpScriptFrame = _callStack[i].As<ScriptStackFrame>();

                        if (tmpScriptFrame.Chunk.Name == closureInfo.EnclosedFunctionName)
                        {
                            targetFrame = tmpScriptFrame;
                            break;
                        }
                    }

                    if (targetFrame != null)
                    {
                        if (closureInfo.IsParameter)
                        {
                            PushValue(targetFrame.Arguments[closureInfo.EnclosedId]);
                        }
                        else
                        {
                            PushValue(targetFrame.Locals![closureInfo.EnclosedId]);
                        }
                    }
                    else
                    {
                        PushValue(closureInfo.Value);
                    }

                    break;
                }

                case OpCode.LENGTH:
                {
                    a = PopValue();
                    
                    if (a.Type == DynamicValueType.Table)
                    {
                        if (a.Table!.TryGetOverride(TableOverride.Length, out var chunk))
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
                        if (a.Table!.TryGetOverride(TableOverride.ToString, out var chunk))
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
                        if (a.Table!.TryGetOverride(TableOverride.ToNumber, out var chunk))
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
                        if (b.Table!.TryGetOverride(TableOverride.Exists, out var chunk))
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
                    frame.Dispose();

                    break;
                }

                case OpCode.CRET:
                {
                    _callStack.Pop();
                    frame.Chunk.Dispose();
                    frame.Dispose();

                    break;
                }

                case OpCode.INC:
                {
                    a = PopValue();

                    if (a.Type == DynamicValueType.Table)
                    {
                        if (a.Table!.TryGetOverride(TableOverride.Increment, out var chunk))
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
                        if (a.Table!.TryGetOverride(TableOverride.Decrement, out var chunk))
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

                    PushValue(new Array((int)a.Number));
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
                    b = PopValue();
                    c = PopValue();
                    a = PopValue();
                    
                    if (c.Type == DynamicValueType.Table)
                    {
                        if (c.Table!.TryGetOverride(TableOverride.Set, out var chunk))
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
                    a = PopValue();
                    c = PopValue();
                    
                    if (c.Type == DynamicValueType.Table)
                    {
                        if (c.Table!.TryGetOverride(TableOverride.Get, out var chunk))
                        {
                            InvokeChunk(chunk, c, a);
                            break;
                        }
                    }

                    PushValue(c.Index(a));
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

                    var fiber = _fiber.VirtualMachine.Scheduler.CreateFiber(false);
                    fiber.Schedule(a.Chunk!, args);
                    fiber.Resume();

                    PushValue(fiber);
                    _fiber.WaitFor(fiber);
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
                    PushValue(frame.ExtraArguments);
                    break;
                }

                case OpCode.EACH:
                {
                    a = PopValue();

                    if (a.Type == DynamicValueType.String)
                    {
                        a = Array.FromString(a.String!);
                    }

                    if (a.Type == DynamicValueType.Table)
                    {
                        frame.PushEnumerator(a.Table!);
                    }
                    else if (a.Type == DynamicValueType.Array)
                    {
                        frame.PushEnumerator(a.Array!);
                    }
                    else
                    {
                        throw new UnsupportedDynamicValueOperationException(
                            $"Attempt to iterate over a {a.Type} value."
                        );
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
                        if (isKeyValue)
                        {
                            PushValue(enumerator.Current.Value);
                        }

                        PushValue(enumerator.Current.Key);
                    }

                    PushValue(next);
                    break;
                }

                case OpCode.EEND:
                {
                    frame.PopEnumerator();
                    break;
                }

                case OpCode.OVERRIDE:
                {
                    var tableOverride = (TableOverride)frame.FetchByte();
                    a = PopValue();
                    b = PopValue();

                    if (a.Type != DynamicValueType.Table)
                    {
                        throw new VirtualMachineException(
                            $"Attempt to override an operator on a {a.Type} value."
                        );
                    }

                    a.Table!.SetOverride(tableOverride, b.Chunk!);
                    break;
                }

                default:
                {
                    throw new VirtualMachineException($"Invalid opcode '{opCode}'.");
                }
            }
        }

        private void InvokeChunk(Chunk chunk, params DynamicValue[] args)
        {
            _callStack.Push(new ScriptStackFrame(_fiber, chunk, args));
            _fiber.OnChunkInvoke?.Invoke(
                _fiber, 
                chunk, 
                false
            );
        }

        private DynamicValue PopValue()
        {
            lock (_evaluationStack)
            {
                return _evaluationStack.Pop();
            }
        }

        private DynamicValue PeekValue()
        {
            lock (_evaluationStack)
            {
                return _evaluationStack.Peek();
            }
        }

        private void PushValue(DynamicValue value)
        {
            lock (_evaluationStack)
            {
                _evaluationStack.Push(value);
            }
        }

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
}