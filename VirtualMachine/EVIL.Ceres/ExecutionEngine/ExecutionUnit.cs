namespace EVIL.Ceres.ExecutionEngine;

using Array = System.Array;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.CommonTypes.TypeSystem;

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
                var clone = frame.Chunk.SubChunks[
                    frame.FetchInt32()
                ].Clone();
                    
                var subChunkStack = new Queue<Chunk>();
                subChunkStack.Enqueue(clone);

                while (subChunkStack.Any())
                {
                    var currentChunk = subChunkStack.Dequeue();

                    for (var i = 0; i < currentChunk.ClosureCount; i++)
                    {
                        var closure = currentChunk.Closures[i];

                        ClosureContext closureContext;

                        if (closure.IsSharedScope)
                        {
                            closureContext = frame.Fiber.SetClosureContext(closure.EnclosedFunctionName);
                        }
                        else
                        {
                            closureContext = currentChunk.SetClosureContext(closure.EnclosedFunctionName);
                        }

                        ScriptStackFrame? sourceFrame = null;
                        for (var j = _callStack.Count - 1; j >= 0; j--)
                        {
                            var tmpFrame = _callStack[j].As<ScriptStackFrame>();
                            if (tmpFrame.Chunk.Name == closure.EnclosedFunctionName)
                            {
                                sourceFrame = tmpFrame;
                                break;
                            }
                        }

                        if (sourceFrame == null)
                        {
                            continue;
                        }

                        if (closure.IsParameter)
                        {
                            closureContext.Values[closure.EnclosedId] = sourceFrame.Arguments[closure.EnclosedId];
                        }
                        else if (closure.IsClosure)
                        {
                            // This is probably going to fuck up at some point :skull:
                            // I just don't know when.
                            //
                            // This is pain. First-class functions are pain.
                            // Curing testicular cancer was easier than implementing  this shit.
                            //
                            var innerClosure = sourceFrame.Chunk.Closures[closure.EnclosedId];
                                
                            if (innerClosure.IsSharedScope)
                            {
                                closureContext.Values[innerClosure.EnclosedId] = sourceFrame.Fiber
                                    .ClosureContexts[innerClosure.EnclosedFunctionName].Values[innerClosure.EnclosedId];
                            }
                            else
                            {
                                    
                                closureContext.Values[innerClosure.EnclosedId] = sourceFrame.Chunk
                                    .ClosureContexts[innerClosure.EnclosedFunctionName].Values[innerClosure.EnclosedId];
                            }
                        }
                        else
                        {
                            closureContext.Values[closure.EnclosedId] = sourceFrame.Locals![closure.EnclosedId];
                        }
                    }
                        
                    foreach (var child in currentChunk.SubChunks)
                    {
                        subChunkStack.Enqueue(child.Clone());
                    }
                }

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
                var closureId = frame.FetchInt32();
                var closureInfo = frame.Chunk.Closures[closureId];

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
                    else if (closureInfo.IsClosure)
                    {
                        while (closureInfo.IsClosure)
                        {
                            closureInfo = targetFrame.Chunk.Closures[closureInfo.EnclosedId];
                                
                            for (var i = 0; i < _callStack.Count; i++)
                            {
                                var tmpScriptFrame = _callStack[i].As<ScriptStackFrame>();

                                if (tmpScriptFrame.Chunk.Name == closureInfo.EnclosedFunctionName)
                                {
                                    targetFrame = tmpScriptFrame;
                                    break;
                                }
                            }
                        }
                            
                        ClosureContext closureContext;

                        if (closureInfo.IsSharedScope)
                        {
                            closureContext = targetFrame.Fiber.ClosureContexts[closureInfo.EnclosedFunctionName];
                        }
                        else
                        {
                            closureContext = targetFrame.Chunk.ClosureContexts[closureInfo.EnclosedFunctionName];
                        }

                        closureContext.Values[closureInfo.EnclosedId] = value;
                    }
                    else
                    {
                        targetFrame.Locals![closureInfo.EnclosedId] = value;
                    }
                }
                else
                {
                    ClosureContext closureContext;
                        
                    if (closureInfo.IsSharedScope)
                    {
                        closureContext = frame.Fiber.ClosureContexts[closureInfo.EnclosedFunctionName];
                    }
                    else
                    {
                        if (closureInfo.IsClosure)
                        {
                            var currentChunk = frame.Chunk;
                            while (currentChunk.Name != closureInfo.EnclosedFunctionName)
                            {
                                currentChunk = currentChunk.Parent!;
                            }

                            closureInfo = currentChunk.Closures[closureInfo.EnclosedId];
                            closureContext = frame.Fiber.ClosureContexts[closureInfo.EnclosedFunctionName];
                        }
                        else
                        {
                            closureContext = frame.Chunk.ClosureContexts[closureInfo.EnclosedFunctionName];
                        }
                    }
                        
                    closureContext.Values[closureInfo.EnclosedId] = value;
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
                    else if (closureInfo.IsClosure)
                    {
                        while (closureInfo.IsClosure)
                        {
                            closureInfo = targetFrame.Chunk.Closures[closureInfo.EnclosedId];
                                
                            for (var i = 0; i < _callStack.Count; i++)
                            {
                                var tmpScriptFrame = _callStack[i].As<ScriptStackFrame>();

                                if (tmpScriptFrame.Chunk.Name == closureInfo.EnclosedFunctionName)
                                {
                                    targetFrame = tmpScriptFrame;
                                    break;
                                }
                            }
                        }
                            
                        ClosureContext closureContext;

                        if (closureInfo.IsSharedScope)
                        {
                            closureContext = targetFrame.Fiber.ClosureContexts[closureInfo.EnclosedFunctionName];
                        }
                        else
                        {
                            closureContext = targetFrame.Chunk.ClosureContexts[closureInfo.EnclosedFunctionName];
                        }

                        PushValue(closureContext.Values[closureInfo.EnclosedId]);
                    }
                    else
                    {
                        PushValue(targetFrame.Locals![closureInfo.EnclosedId]);
                    }
                }
                else
                {
                    ClosureContext closureContext;
                        
                    if (closureInfo.IsSharedScope)
                    {
                        closureContext = frame.Fiber.ClosureContexts[closureInfo.EnclosedFunctionName];
                    }
                    else /* Probably called from somewhere *outside* EVIL. */
                    {
                        if (closureInfo.IsClosure)
                        {
                            var currentChunk = frame.Chunk;
                            while (currentChunk.Name != closureInfo.EnclosedFunctionName)
                            {
                                currentChunk = currentChunk.Parent!;
                            }

                            closureInfo = currentChunk.Closures[closureInfo.EnclosedId];
                            closureContext = frame.Fiber.ClosureContexts[closureInfo.EnclosedFunctionName];
                        }
                        else
                        {
                            closureContext = frame.Chunk.ClosureContexts[closureInfo.EnclosedFunctionName];
                        }
                    }   

                    PushValue(closureContext.Values[closureInfo.EnclosedId]);
                }

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

                if (a.Type != DynamicValueType.NativeObject)
                {
                    PushValue(DynamicValue.Nil);
                }
                else
                {
                    PushValue(a.NativeObject!.GetType().FullName!);
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
                frame.Dispose();

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
                    
                if (c.Type == DynamicValueType.String && a.Type == DynamicValueType.String)
                {
                    c = _global.Index("str");

                    if (c.Type != DynamicValueType.Table)
                    {
                        throw new UnsupportedDynamicValueOperationException(
                            "Attempt to index a String using a String, but no `str' support table found."
                        );
                    }
                }
                else if (c.Type == DynamicValueType.Array && a.Type == DynamicValueType.String)
                {
                    c = _global.Index("arr");

                    if (c.Type != DynamicValueType.Table)
                    {
                        throw new UnsupportedDynamicValueOperationException(
                            "Attempt to index an Array using a String, but no `arr' support table found."
                        );
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

                var fiber = _fiber.VirtualMachine.Scheduler.CreateFiber(
                    false, 
                    closureContexts: (Dictionary<string, ClosureContext>)frame.Fiber.ClosureContexts
                );
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
                        PushValue(enumerator.Current.Key);
                    }
                    else
                    {
                        PushValue(enumerator.Current.Value);
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

    private bool FindMetaFunction(Table table, string op, [MaybeNullWhen(false)] out Chunk chunk)
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