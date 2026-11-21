namespace EVIL.Ceres.ExecutionEngine.Diagnostics;

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
    internal Action? BytecodeModified { get; set; }

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
        BytecodeModified?.Invoke();
        _opCodeAddresses.Push(IP);
        _writer.Write((byte)value);

        if (value is OpCode.JUMP or OpCode.FJMP or OpCode.TJMP or OpCode.VJMP or OpCode.NJMP)
        {
            _nextOperandIsJumpFixup = true;
        }
        else
        {
            _nextOperandIsJumpFixup = false;
        }

        OpCodeEmitted?.Invoke(_opCodeAddresses.Peek(), value);
    }

    public void Emit(OpCode value, double operand)
    {
        Emit(value);
        Emit(operand);
    }

    public void Emit(OpCode value, long operand)
    {
        Emit(value);
        Emit(operand);
    }
        
    public void Emit(OpCode value, int operand)
    {
        Emit(value);
        Emit(operand);
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
        }

        _writer.Write(value);
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
        _opCodeAddresses.Clear();
            
        _writer.Dispose();
    }
}