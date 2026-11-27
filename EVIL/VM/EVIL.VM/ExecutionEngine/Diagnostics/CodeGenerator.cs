namespace EVIL.VM.ExecutionEngine.Diagnostics;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public sealed class CodeGenerator : IDisposable
{
    private readonly MemoryStream _code;
    private readonly BinaryWriter _writer;
    private readonly List<int> _labels;
    private readonly Stack<int> _opCodeAddresses;
    private readonly List<int> _jumpFixups;
    private bool _nextOperandIsJumpFixup;

    public Action<int, OpCode>? OpCodeEmitted { get; set; }
    internal Action? CodeModified { get; set; }
    public Action<int>? LabelTargetEmitted { get; set; }

    public int IP
    {
        get => (int)_code.Position;
        set => _code.Seek(value, SeekOrigin.Begin);
    }

    public IReadOnlyList<int> Labels => _labels;
    public IReadOnlyCollection<int> OpCodeAddresses => _opCodeAddresses;
    public IReadOnlyList<int> JumpFixups => _jumpFixups;
        
    internal CodeGenerator(MemoryStream code, List<int> labels)
    {
        _code = code;
        _labels = labels;
        _opCodeAddresses = new();
        _jumpFixups = new();
            
        _writer = new BinaryWriter(_code, Encoding.UTF8, true);
    }

    public OpCode PeekOpCode()
    {
        if (_code.Length == 0 || _opCodeAddresses.Count == 0)
        {
            throw new InvalidOperationException("Chunk contains no instructions.");
        }

        var position = _code.Position;
        _code.Seek(_opCodeAddresses.Peek(), SeekOrigin.Begin);
        var opCode = (OpCode)_code.ReadByte();
        _code.Seek(position, SeekOrigin.Begin);
        return opCode;
    }

    public bool TryPeekOpCode(out OpCode? opCode)
    {
        opCode = null;

        try
        {
            opCode = PeekOpCode();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Emit(OpCode value)
    {
        CodeModified?.Invoke();
        _opCodeAddresses.Push(IP);
        _writer.Write((byte)value);

        _nextOperandIsJumpFixup = value is OpCode.JUMP 
                                        or OpCode.FJMP
                                        or OpCode.TJMP 
                                        or OpCode.VJMP
                                        or OpCode.NJMP;
        
        OpCodeEmitted?.Invoke(_opCodeAddresses.Peek(), value);
    }

    public void Emit(OpCode value, double operand)
    {
        if (value == OpCode.LDNUM)
        {
            EmitNumber(operand);
            return;
        }

        Emit(value);
        Emit(operand);
    }

    public void EmitNumber(double value)
    {
        if (value == 0.0 && !double.IsNegative(value))
        {
            Emit(OpCode.LDZERO);
        }
        else if (value == 1.0)
        {
            Emit(OpCode.LDONE);
        }
        else if (value is >= sbyte.MinValue and <= sbyte.MaxValue && Math.Truncate(value) == value && !(value == 0.0 && double.IsNegative(value)))
        {
            Emit(OpCode.LDNUM_I8, (byte)(sbyte)value);
        }
        else
        {
            Emit(OpCode.LDNUM);
            Emit(value);
        }
    }

    public void Emit(OpCode value, long operand)
    {
        Emit(value);
        Emit(operand);
    }
        
    public void Emit(OpCode value, int operand)
    {
        switch (value)
        {
            case OpCode.GETLOCAL:
                EmitGetLocal(operand);
                return;
            
            case OpCode.SETLOCAL:
                EmitSetLocal(operand);
                return;
            
            case OpCode.GETARG:
                EmitGetArg(operand);
                return;
            
            case OpCode.SETARG:
                EmitSetArg(operand);
                return;
            
            case OpCode.GETCLOSURE:
                EmitGetClosure(operand);
                return;
            
            case OpCode.SETCLOSURE:
                EmitSetClosure(operand);
                return;
            
            case OpCode.GETPROP:
                EmitGetProp(operand);
                return;

            case OpCode.SETPROP:
                EmitSetProp(operand);
                return;

            case OpCode.LDTYPE:
                Emit(OpCode.LDTYPE, (byte)operand);
                return;
        }

        Emit(value);
        Emit(operand);
    }

    public void EmitGetLocal(int operand)
    {
        switch (operand)
        {
            case 0: Emit(OpCode.GETLOCAL_0); break;
            case 1: Emit(OpCode.GETLOCAL_1); break;
            case 2: Emit(OpCode.GETLOCAL_2); break;
            case 3: Emit(OpCode.GETLOCAL_3); break;

            default:
            {
                if ((uint)operand <= byte.MaxValue)
                {
                    Emit(OpCode.GETLOCAL_B, (byte)operand);
                }
                else
                {
                    Emit(OpCode.GETLOCAL);
                    Emit(operand);
                }

                break;
            }
        }
    }

    public void EmitSetLocal(int operand)
    {
        switch (operand)
        {
            case 0: Emit(OpCode.SETLOCAL_0); break;
            case 1: Emit(OpCode.SETLOCAL_1); break;
            case 2: Emit(OpCode.SETLOCAL_2); break;
            case 3: Emit(OpCode.SETLOCAL_3); break;

            default:
            {
                if ((uint)operand <= byte.MaxValue)
                {
                    Emit(OpCode.SETLOCAL_B, (byte)operand);
                }
                else
                {
                    Emit(OpCode.SETLOCAL);
                    Emit(operand);
                }

                break;
            }
        }
    }

    public void EmitGetArg(int operand)
    {
        switch (operand)
        {
            case 0: Emit(OpCode.GETARG_0); break;
            case 1: Emit(OpCode.GETARG_1); break;
            case 2: Emit(OpCode.GETARG_2); break;
            case 3: Emit(OpCode.GETARG_3); break;

            default:
            {
                if ((uint)operand <= byte.MaxValue)
                {
                    Emit(OpCode.GETARG_B, (byte)operand);
                }
                else
                {
                    Emit(OpCode.GETARG);
                    Emit(operand);
                }

                break;
            }
        }
    }

    public void EmitSetArg(int operand)
    {
        switch (operand)
        {
            case 0: Emit(OpCode.SETARG_0); break;
            case 1: Emit(OpCode.SETARG_1); break;
            case 2: Emit(OpCode.SETARG_2); break;
            case 3: Emit(OpCode.SETARG_3); break;

            default:
            {
                if ((uint)operand <= byte.MaxValue)
                {
                    Emit(OpCode.SETARG_B, (byte)operand);
                }
                else
                {
                    Emit(OpCode.SETARG);
                    Emit(operand);
                }

                break;
            }
        }
    }

    public void EmitGetClosure(int operand)
    {
        switch (operand)
        {
            case 0: Emit(OpCode.GETCLOSURE_0); break;
            case 1: Emit(OpCode.GETCLOSURE_1); break;
            case 2: Emit(OpCode.GETCLOSURE_2); break;
            case 3: Emit(OpCode.GETCLOSURE_3); break;

            default:
            {
                if ((uint)operand <= byte.MaxValue)
                {
                    Emit(OpCode.GETCLOSURE_B, (byte)operand);
                }
                else
                {
                    Emit(OpCode.GETCLOSURE);
                    Emit(operand);
                }

                break;
            }
        }
    }

    public void EmitSetClosure(int operand)
    {
        switch (operand)
        {
            case 0: Emit(OpCode.SETCLOSURE_0); break;
            case 1: Emit(OpCode.SETCLOSURE_1); break;
            case 2: Emit(OpCode.SETCLOSURE_2); break;
            case 3: Emit(OpCode.SETCLOSURE_3); break;

            default:
            {
                if ((uint)operand <= byte.MaxValue)
                {
                    Emit(OpCode.SETCLOSURE_B, (byte)operand);
                }
                else
                {
                    Emit(OpCode.SETCLOSURE);
                    Emit(operand);
                }

                break;
            }
        }
    }
        
    public void EmitGetProp(int operand)
    {
        if ((uint)operand <= byte.MaxValue)
        {
            Emit(OpCode.GETPROP_B, (byte)operand);
        }
        else
        {
            Emit(OpCode.GETPROP);
            Emit(operand);
        }
    }

    public void EmitGetTableField(int stringPoolIndex)
    {
        Emit(OpCode.TABGET);
        Emit(stringPoolIndex);
    }

    public void EmitSetProp(int operand)
    {
        if ((uint)operand <= byte.MaxValue)
        {
            Emit(OpCode.SETPROP_B, (byte)operand);
        }
        else
        {
            Emit(OpCode.SETPROP);
            Emit(operand);
        }
    }

    public void Emit(OpCode value, int operand1, int operand2)
    {
        Emit(value);
        Emit(operand1);
        Emit(operand2);
    }
        
    public void Emit(OpCode value, byte operand)
    {
        Emit(value);
        Emit(operand);
    }

    public void Emit(OpCode value, bool operand)
    {
        Emit(value);
        Emit(operand);
    }
        
    public void Emit(double value)
        => _writer.Write(value);

    public void Emit(long value) 
        => _writer.Write(value);

    public void Emit(int value)
    {
        if (_nextOperandIsJumpFixup)
        {
            _jumpFixups.Add(IP);
            _nextOperandIsJumpFixup = false;
            LabelTargetEmitted?.Invoke(value);
        }

        _writer.Write(value);
    }

    public void EmitJumpTarget(int labelId)
    {
        _jumpFixups.Add(IP);
        LabelTargetEmitted?.Invoke(labelId);
        _writer.Write(labelId);
    }

    public void Emit(bool value)
        => _writer.Write(value);

    public void Emit(byte value)
        => _writer.Write(value);

    public long Label(int offset = 0)
    {
        _labels.Add(IP + offset);
        return _labels[^1];
    }

    public void Dispose()
    {
        OpCodeEmitted = null;
        CodeModified = null;
        LabelTargetEmitted = null;
        _opCodeAddresses.Clear();
            
        _writer.Dispose();
    }
}