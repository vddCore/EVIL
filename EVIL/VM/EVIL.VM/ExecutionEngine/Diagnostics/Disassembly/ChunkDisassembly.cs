namespace EVIL.VM.ExecutionEngine.Diagnostics.Disassembly;

using System;
using System.Collections.Generic;

public sealed class ChunkDisassembly
{
    private readonly List<Instruction> _instructions;
    private readonly Dictionary<int, Instruction> _byOffset;
    private readonly HashSet<int> _jumpTargets;

    public Chunk Chunk { get; }
    public IReadOnlyList<Instruction> Instructions => _instructions;
    public IReadOnlyDictionary<int, Instruction> ByOffset => _byOffset;
    public IReadOnlySet<int> JumpTargets => _jumpTargets;
    public int InstructionCount => _instructions.Count;

    public ChunkDisassembly(Chunk chunk, List<Instruction> instructions, HashSet<int> jumpTargets)
    {
        Chunk = chunk ?? throw new ArgumentNullException(
            nameof(chunk)
        );
        
        _instructions = instructions ?? throw new ArgumentNullException(
            nameof(instructions)
        );
        
        _jumpTargets = jumpTargets ?? throw new ArgumentNullException(
            nameof(jumpTargets)
        );
        
        _byOffset = new Dictionary<int, Instruction>(
            instructions.Count
        );

        for (var i = 0; i < instructions.Count; i++)
        {
            _byOffset[instructions[i].Offset] = instructions[i];
        }
    }

    public Instruction? FindAtOffset(int ip)
        => _byOffset.GetValueOrDefault(ip);

    public int IndexOf(int ip)
    {
        for (var i = 0; i < _instructions.Count; i++)
        {
            if (_instructions[i].Offset == ip)
            {
                return i;
            }
        }
        return -1;
    }

    public IReadOnlyList<Instruction> GetWindow(int centerIp, int radiusBefore = 5, int radiusAfter = 5)
    {
        if (_instructions.Count == 0)
        {
            return [];
        }

        var idx = IndexOf(centerIp);
        if (idx < 0)
        {
            for (var i = _instructions.Count - 1; i >= 0; i--)
            {
                if (_instructions[i].Offset <= centerIp)
                {
                    idx = i;
                    break;
                }
            }
            if (idx < 0)
            {
                idx = 0;
            }
        }

        var start = Math.Max(0, idx - radiusBefore);
        var end = Math.Min(_instructions.Count - 1, idx + radiusAfter);
        var count = end - start + 1;

        var result = new List<Instruction>(count);
        
        for (var i = start; i <= end; i++)
        {
            result.Add(_instructions[i]);
        }
        
        return result;
    }

    public IReadOnlyList<Instruction> GetRange(int startIp, int count)
    {
        if (_instructions.Count == 0 || count <= 0)
        {
            return [];
        }

        var idx = IndexOf(startIp);
        if (idx < 0)
        {
            for (var i = 0; i < _instructions.Count; i++)
            {
                if (_instructions[i].Offset >= startIp)
                {
                    idx = i;
                    break;
                }
            }
            if (idx < 0)
            {
                return [];
            }
        }

        var takeCount = Math.Min(count, _instructions.Count - idx);
        var result = new List<Instruction>(takeCount);
        
        for (var i = idx; i < idx + takeCount; i++)
        {
            result.Add(_instructions[i]);
        }
        
        return result;
    }

    public IReadOnlyList<Instruction> GetForLine(int line)
    {
        var result = new List<Instruction>();
        
        for (var i = 0; i < _instructions.Count; i++)
        {
            if (_instructions[i].Line == line)
            {
                result.Add(_instructions[i]);
            }
        }
        
        return result;
    }
}
