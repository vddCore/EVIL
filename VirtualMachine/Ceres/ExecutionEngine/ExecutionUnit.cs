using System.Collections.Generic;
using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine
{
    internal class ExecutionUnit
    {
        public const int CallStackLimit = 128;

        private readonly Table _global;
        private readonly Fiber _fiber;
        private readonly Stack<DynamicValue> _evaluationStack;
        private readonly Stack<StackFrame> _callStack;

        public ExecutionUnit(Table global, Fiber fiber, Stack<DynamicValue> evaluationStack,
            Stack<StackFrame> callStack)
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

                case OpCode.ADD:
                {
                    b = PopValue();
                    a = PopValue();
                    PushValue(a.Add(b));

                    break;
                }

                case OpCode.SUB:
                {
                    b = PopValue();
                    a = PopValue();

                    PushValue(a.Subtract(b));

                    break;
                }

                case OpCode.MUL:
                {
                    b = PopValue();
                    a = PopValue();

                    PushValue(a.Multiply(b));

                    break;
                }

                case OpCode.DIV:
                {
                    b = PopValue();
                    a = PopValue();

                    PushValue(a.DivideBy(b));

                    break;
                }

                case OpCode.MOD:
                {
                    b = PopValue();
                    a = PopValue();

                    PushValue(a.Modulo(b));

                    break;
                }

                case OpCode.SHL:
                {
                    b = PopValue();
                    a = PopValue();

                    PushValue(a.ShiftLeft(b));

                    break;
                }

                case OpCode.SHR:
                {
                    b = PopValue();
                    a = PopValue();

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
                    PushValue(PopValue().ArithmeticallyNegate());
                    break;
                }

                case OpCode.LNOT:
                {
                    PushValue(PopValue().LogicallyNegate());
                    break;
                }

                case OpCode.LOR:
                {
                    b = PopValue();
                    a = PopValue();

                    PushValue(a.LogicalOr(b));
                    break;
                }

                case OpCode.LAND:
                {
                    b = PopValue();
                    a = PopValue();

                    PushValue(a.LogicalAnd(b));
                    break;
                }

                case OpCode.BOR:
                {
                    b = PopValue();
                    a = PopValue();

                    PushValue(a.BitwiseOr(b));
                    break;
                }

                case OpCode.BXOR:
                {
                    b = PopValue();
                    a = PopValue();

                    PushValue(a.BitwiseXor(b));
                    break;
                }

                case OpCode.BAND:
                {
                    b = PopValue();
                    a = PopValue();

                    PushValue(a.BitwiseAnd(b));
                    break;
                }

                case OpCode.BNOT:
                {
                    a = PopValue();

                    PushValue(a.BitwiseNegate());
                    break;
                }

                case OpCode.DEQ:
                {
                    b = PopValue();
                    a = PopValue();

                    PushValue(a.IsDeeplyEqualTo(b));
                    break;
                }

                case OpCode.DNE:
                {
                    b = PopValue();
                    a = PopValue();

                    PushValue(a.IsDeeplyNotEqualTo(b));
                    break;
                }

                case OpCode.CEQ:
                {
                    b = PopValue();
                    a = PopValue();

                    PushValue(a.IsEqualTo(b));
                    break;
                }

                case OpCode.CNE:
                {
                    b = PopValue();
                    a = PopValue();

                    PushValue(a.IsNotEqualTo(b));
                    break;
                }

                case OpCode.CGT:
                {
                    b = PopValue();
                    a = PopValue();

                    PushValue(a.IsGreaterThan(b));
                    break;
                }

                case OpCode.CGE:
                {
                    b = PopValue();
                    a = PopValue();

                    PushValue(a.IsGreaterThanOrEqualTo(b));
                    break;
                }

                case OpCode.CLT:
                {
                    b = PopValue();
                    a = PopValue();

                    PushValue(a.IsLessThan(b));
                    break;
                }

                case OpCode.CLE:
                {
                    b = PopValue();
                    a = PopValue();

                    PushValue(a.IsLessThanOrEqualTo(b));
                    break;
                }

                case OpCode.INVOKE:
                {
                    if (_callStack.Count >= CallStackLimit)
                    {
                        throw new VirtualMachineException("Call stack overflow.");
                    }
                    
                    a = PopValue();

                    var argumentCount = frame.FetchInt32();
                    var args = PopArguments(argumentCount);

                    if (a.Type == DynamicValue.DynamicValueType.Chunk)
                    {
                        _callStack.Push(new ScriptStackFrame(_fiber, a.Chunk!, args));

                        _fiber.VirtualMachine.OnChunkInvoke?.Invoke(
                            _fiber,
                            a.Chunk!,
                            false
                        );

                        break;
                    }

                    if (a.Type == DynamicValue.DynamicValueType.NativeFunction)
                    {
                        _callStack.Push(new NativeStackFrame(a.NativeFunction!));
                        {
                            PushValue(a.NativeFunction!.Invoke(_fiber, args));
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

                    _fiber.VirtualMachine.OnChunkInvoke?.Invoke(
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

                case OpCode.LENGTH:
                {
                    PushValue(PopValue().GetLength());
                    break;
                }

                case OpCode.TOSTRING:
                {
                    PushValue(PopValue().ConvertToString());
                    break;
                }

                case OpCode.TONUMBER:
                {
                    PushValue(PopValue().ConvertToNumber());
                    break;
                }

                case OpCode.TYPE:
                {
                    PushValue(PopValue().Type.ToString());
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

                    PushValue(b.Contains(a));
                    break;
                }

                case OpCode.RET:
                {
                    _callStack.Pop();
                    frame.Dispose();

                    break;
                }

                case OpCode.INC:
                {
                    a = PopValue();
                    PushValue(a.Increment());

                    break;
                }

                case OpCode.DEC:
                {
                    a = PopValue();
                    PushValue(a.Decrement());

                    break;
                }

                case OpCode.TABNEW:
                {
                    PushValue(new Table());
                    break;
                }

                case OpCode.TABINIT:
                {
                    b = PopValue();
                    a = PopValue();
                    c = PeekValue();

                    c.SetEntry(b, a);
                    break;
                }

                case OpCode.TABSET:
                {
                    b = PopValue();
                    c = PopValue();
                    a = PopValue();

                    c.SetEntry(b, a);
                    break;
                }

                case OpCode.INDEX:
                {
                    a = PopValue();
                    c = PopValue();

                    PushValue(c.Index(a));
                    break;
                }

                case OpCode.YIELD:
                {
                    var argumentCount = frame.FetchInt32();

                    a = PopValue(); // chunk
                    var args = PopArguments(argumentCount);

                    if (a.Type != DynamicValue.DynamicValueType.Chunk)
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

                    if (a.Type != DynamicValue.DynamicValueType.Fiber)
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

                    if (a.Type == DynamicValue.DynamicValueType.String)
                    {
                        a = Table.FromString(a.String!);
                    }
                    else if (a.Type != DynamicValue.DynamicValueType.Table)
                    {
                        throw new UnsupportedDynamicValueOperationException(
                            $"Attempt to iterate over a {a.Type} value."
                        );
                    }

                    frame.PushEnumerator(a.Table!);
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

                default:
                {
                    throw new VirtualMachineException($"Invalid opcode '{opCode}'.");
                }
            }
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