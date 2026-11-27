namespace EVIL.VM.Debugger;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EVIL.Core.Common.TypeSystem;
using EVIL.VM.ExecutionEngine.Concurrency;
using EVIL.VM.ExecutionEngine.Diagnostics;
using EVIL.VM.ExecutionEngine.Diagnostics.Debugging;

public sealed class BreakpointManager
{
    private int _nextBreakpointId = 1;
    
    private readonly Dictionary<int, Breakpoint> _breakpoints = [];
    private readonly Dictionary<(Chunk, int), List<Breakpoint>> _boundBreakpoints = [];
    private readonly HashSet<Chunk> _registeredChunks = [];

    public event Action<Breakpoint>? OnBreakpointBound;

    public Breakpoint AddBreakpoint(
        string file,
        int line,
        int column = 0,
        string? condition = null,
        string? hitCondition = null)
    {
        var normalizedFile = NormalizePath(file);
        var bp = new Breakpoint
        {
            Id = _nextBreakpointId++,
            FilePath = normalizedFile,
            Line = line,
            Column = column,
            Condition = condition,
            HitCondition = hitCondition,
            Enabled = true
        };

        _breakpoints[bp.Id] = bp;

        foreach (var chunk in _registeredChunks)
        {
            TryBindBreakpoint(bp, chunk);
        }

        return bp;
    }

    public Breakpoint AddAddressBreakpoint(
        Chunk chunk,
        int ip,
        string? condition = null,
        string? hitCondition = null)
    {
        var line = chunk.HasDebugInfo ? chunk.DebugDatabase.GetLineForIP(ip) : 0;
        var file = chunk.HasDebugInfo ? chunk.DebugDatabase.DefinedInFile : string.Empty;

        var bp = new Breakpoint
        {
            Id = _nextBreakpointId++,
            FilePath = file,
            Line = line,
            Column = 0,
            Condition = condition,
            HitCondition = hitCondition,
            Enabled = true,
            BoundChunk = chunk,
            BoundIP = ip,
            IsVerified = true
        };

        _breakpoints[bp.Id] = bp;

        var key = (chunk, ip);
        if (!_boundBreakpoints.TryGetValue(key, out var list))
        {
            list = [];
            _boundBreakpoints[key] = list;
        }

        if (!list.Contains(bp))
        {
            list.Add(bp);
        }

        OnBreakpointBound?.Invoke(bp);
        return bp;
    }

    public bool RemoveBreakpoint(int id)
    {
        if (_breakpoints.Remove(id, out var bp))
        {
            if (bp.BoundChunk != null && bp.BoundIP >= 0)
            {
                var key = (bp.BoundChunk, bp.BoundIP);
                if (_boundBreakpoints.TryGetValue(key, out var list))
                {
                    list.Remove(bp);
                    if (list.Count == 0)
                    {
                        _boundBreakpoints.Remove(key);
                    }
                }
            }
            return true;
        }

        return false;
    }

    public void ClearBreakpoints(string? file = null)
    {
        if (file == null)
        {
            _breakpoints.Clear();
            _boundBreakpoints.Clear();
            
            return;
        }

        var normalizedFile = NormalizePath(file);
        
        var toRemove = _breakpoints.Values.Where(
            b => IsFileMatch(
                b.FilePath,
                normalizedFile
            )
        ).Select(b => b.Id).ToList();
        
        foreach (var id in toRemove)
        {
            RemoveBreakpoint(id);
        }
    }

    public IReadOnlyList<Breakpoint> GetBreakpoints()
        => [.._breakpoints.Values];

    public void BindBreakpoints(Chunk rootChunk)
    {
        RegisterAndBindChunkTree(rootChunk);
    }

    private void RegisterAndBindChunkTree(Chunk chunk)
    {
        _registeredChunks.Add(chunk);

        foreach (var bp in _breakpoints.Values)
        {
            if (!bp.IsVerified)
            {
                TryBindBreakpoint(bp, chunk);
            }
        }

        for (var i = 0; i < chunk.SubChunkCount; i++)
        {
            RegisterAndBindChunkTree(chunk.SubChunks[i]);
        }
    }

    private bool TryBindBreakpoint(Breakpoint bp, Chunk chunk)
    {
        var chunkFile = NormalizePath(chunk.DebugDatabase.DefinedInFile);
        if (string.IsNullOrEmpty(chunkFile) || !IsFileMatch(chunkFile, bp.FilePath))
        {
            return false;
        }

        var targetIP = chunk.DebugDatabase.GetFirstIPForLine(bp.Line);
        if (targetIP < 0)
        {
            return false;
        }

        bp.BoundChunk = chunk;
        bp.BoundIP = targetIP;
        bp.IsVerified = true;

        var key = (chunk, targetIP);
        if (!_boundBreakpoints.TryGetValue(key, out var list))
        {
            list = [];
            _boundBreakpoints[key] = list;
        }

        if (!list.Contains(bp))
        {
            list.Add(bp);
        }

        OnBreakpointBound?.Invoke(bp);
        return true;
    }

    public bool ShouldBreakAt(
        Chunk chunk,
        int ip,
        Fiber fiber,
        ScriptStackFrame frame,
        IDebuggerSession session,
        out Breakpoint? hitBreakpoint)
    {
        hitBreakpoint = null;

        if (!_boundBreakpoints.TryGetValue((chunk, ip), out var list) || list.Count == 0)
        {
            return false;
        }

        foreach (var bp in list)
        {
            if (!bp.Enabled)
            {
                continue;
            }

            bp.HitCount++;

            if (!bp.MatchesHitCondition())
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(bp.Condition))
            {
                try
                {
                    var result = session.EvaluateInFrame(frame, bp.Condition);
                    
                    switch (result.Type)
                    {
                        case DynamicValueType.Nil:
                        case DynamicValueType.Boolean when !result.Boolean:
                            continue;
                    }
                }
                catch
                {
                    /* break if eval throws. */
                }
            }

            hitBreakpoint = bp;
            return true;
        }

        return false;
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }
        
        return path.Replace('\\', '/').Trim();
    }

    private static bool IsFileMatch(string path1, string path2)
    {
        if (string.Equals(path1, path2, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var name1 = Path.GetFileName(path1);
        var name2 = Path.GetFileName(path2);

        return !string.IsNullOrEmpty(name1) 
            && string.Equals(name1, name2, StringComparison.OrdinalIgnoreCase);
    }
}
