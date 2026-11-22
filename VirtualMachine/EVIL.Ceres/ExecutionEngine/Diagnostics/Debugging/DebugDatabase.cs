namespace EVIL.Ceres.ExecutionEngine.Diagnostics.Debugging;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public sealed class DebugDatabase : IEquatable<DebugDatabase>
{
    private readonly Dictionary<int, HashSet<int>> _records = new();
    private readonly Dictionary<int, StatementDebugRecord> _statementRecords = new();
    private readonly Dictionary<int, List<int>> _lineRecords = new();

    private readonly Dictionary<int, string> _parameterNames = new();
    private readonly Dictionary<int, bool> _parameterRwStates = new();

    private readonly Dictionary<int, string> _localNames = new();
    private readonly Dictionary<int, bool> _localRwStates = new();

    public int DefinedOnLine { get; internal set; } = -1;
    public string DefinedInFile { get; internal set; } = string.Empty;

    public IReadOnlyDictionary<int, StatementDebugRecord> StatementRecords => _statementRecords;

    public void AddDebugRecord(int line, int ip)
    {
        if (!_records.TryGetValue(line, out var addresses))
        {
            addresses = [];
            _records.Add(line, addresses);
        }

        addresses.Add(ip);

        if (!_lineRecords.TryGetValue(line, out var list))
        {
            list = [];
            _lineRecords.Add(line, list);
        }

        if (!list.Contains(ip))
        {
            list.Add(ip);
        }
    }

    public void AddStatementRecord(
        int ip,
        int line,
        int column,
        int endLine,
        int endColumn,
        bool isStepBoundary = true)
    {
        var record = new StatementDebugRecord(
            ip, 
            line,
            column,
            endLine,
            endColumn,
            isStepBoundary
        );
        
        _statementRecords[ip] = record;
        AddDebugRecord(line, ip);
    }

    public bool TryGetStatementAtIP(int ip, out StatementDebugRecord record)
        => _statementRecords.TryGetValue(ip, out record!);

    public int GetFirstIPForLine(int line)
    {
        if (_lineRecords.TryGetValue(line, out var ips) && ips.Count > 0)
        {
            return ips[0];
        }

        if (_records.TryGetValue(line, out var set) && set.Count > 0)
        {
            return set.Min();
        }

        return -1;
    }

    public IReadOnlyList<StatementDebugRecord> GetStatementsForLine(int line)
    {
        if (!_lineRecords.TryGetValue(line, out var ips))
        {
            return [];
        }

        var result = new List<StatementDebugRecord>(ips.Count);
        foreach (var ip in ips)
        {
            if (_statementRecords.TryGetValue(ip, out var rec))
            {
                result.Add(rec);
            }
        }

        return result;
    }

    public int GetLineForIP(int ip)
    {
        if (_statementRecords.TryGetValue(ip, out var stmt))
        {
            return stmt.Line;
        }

        foreach (var kvp in _records)
        {
            if (kvp.Value.Contains(ip))
            {
                return kvp.Key;
            }
        }

        return -1;
    }

    public void SetParameterName(int id, string name, bool rw)
    {
        if (!_parameterNames.TryAdd(id, name))
        {
            _parameterNames[id] = name;
        }

        if (!_parameterRwStates.TryAdd(id, rw))
        {
            _parameterRwStates[id] = rw;
        }
    }

    public bool RemoveParameterName(int id)
    {
        _parameterRwStates.Remove(id);
        return _parameterNames.Remove(id);
    }

    public bool HasParameterNameFor(int id)
        => _parameterNames.ContainsKey(id);

    public bool TryGetParameterName(int id, out string parameterName)
        => _parameterNames.TryGetValue(id, out parameterName!);

    public bool TryGetParameterRwState(int id, out bool rw)
        => _parameterRwStates.TryGetValue(id, out rw);

    public void SetLocalName(int id, string name, bool rw)
    {
        if (!_localNames.TryAdd(id, name))
        {
            _localNames[id] = name;
        }

        if (!_localRwStates.TryAdd(id, rw))
        {
            _localRwStates[id] = rw;
        }
    }

    public bool RemoveLocalName(int id)
    {
        _localRwStates.Remove(id);
        return _localNames.Remove(id);
    }

    public bool HasLocalNameFor(int id)
        => _localNames.ContainsKey(id);

    public bool TryGetLocalName(int id, out string localName)
        => _localNames.TryGetValue(id, out localName!);

    public bool TryGetLocalRwState(int id, out bool rw)
        => _localRwStates.TryGetValue(id, out rw);

    public bool IsEmpty => _records.Count == 0
                           && _statementRecords.Count == 0
                           && _parameterNames.Count == 0
                           && _localNames.Count == 0
                           && DefinedOnLine == -1;

    public void Strip()
    {
        _records.Clear();
        _statementRecords.Clear();
        _lineRecords.Clear();
        _parameterNames.Clear();
        _localNames.Clear();
        DefinedOnLine = -1;
        DefinedInFile = string.Empty;
    }

    internal void Serialize(BinaryWriter bw)
    {
        bw.Write(DefinedOnLine);
        bw.Write(DefinedInFile);

        bw.Write(_records.Count);
        foreach (var record in _records)
        {
            bw.Write(record.Key);
            bw.Write(record.Value.Count);

            foreach (var addr in record.Value)
                bw.Write(addr);
        }

        bw.Write(_statementRecords.Count);
        foreach (var (ip, stmt) in _statementRecords)
        {
            bw.Write(ip);
            bw.Write(stmt.Line);
            bw.Write(stmt.Column);
            bw.Write(stmt.EndLine);
            bw.Write(stmt.EndColumn);
            bw.Write(stmt.IsStepBoundary);
        }

        bw.Write(_localNames.Count);
        foreach (var localKvp in _localNames)
        {
            bw.Write(localKvp.Key);
            bw.Write(localKvp.Value);
            bw.Write(_localRwStates[localKvp.Key]);
        }

        bw.Write(_parameterNames.Count);
        foreach (var paramKvp in _parameterNames)
        {
            bw.Write(paramKvp.Key);
            bw.Write(paramKvp.Value);
            bw.Write(_parameterRwStates[paramKvp.Key]);
        }
    }

    internal void Deserialize(BinaryReader br)
    {
        Strip();

        DefinedOnLine = br.ReadInt32();
        DefinedInFile = br.ReadString();

        var recordCount = br.ReadInt32();
        for (var i = 0; i < recordCount; i++)
        {
            var line = br.ReadInt32();
            var addrCount = br.ReadInt32();

            for (var j = 0; j < addrCount; j++)
            {
                AddDebugRecord(
                    line,
                    br.ReadInt32()
                );
            }
        }

        var stmtCount = br.ReadInt32();
        for (var i = 0; i < stmtCount; i++)
        {
            var ip = br.ReadInt32();
            var line = br.ReadInt32();
            var col = br.ReadInt32();
            var endLine = br.ReadInt32();
            var endCol = br.ReadInt32();
            var isStep = br.ReadBoolean();
            AddStatementRecord(ip, line, col, endLine, endCol, isStep);
        }

        var localNameCount = br.ReadInt32();
        for (var i = 0; i < localNameCount; i++)
        {
            SetLocalName(
                br.ReadInt32(),
                br.ReadString(),
                br.ReadBoolean()
            );
        }

        var paramNameCount = br.ReadInt32();
        for (var i = 0; i < paramNameCount; i++)
        {
            SetParameterName(
                br.ReadInt32(),
                br.ReadString(),
                br.ReadBoolean()
            );
        }
    }

    public bool Equals(DebugDatabase? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        var recordsEqual = true;
        recordsEqual &= _records.Keys.SequenceEqual(other._records.Keys);
        foreach (var kvp in _records)
        {
            recordsEqual &= kvp.Value.SequenceEqual(other._records[kvp.Key]);
        }

        var stmtEqual = _statementRecords.Count == other._statementRecords.Count
                        && _statementRecords.Keys.All(k => other._statementRecords.TryGetValue(k, out var v) && v == _statementRecords[k]);

        return recordsEqual
               && stmtEqual
               && _parameterNames.SequenceEqual(other._parameterNames)
               && _parameterRwStates.SequenceEqual(other._parameterRwStates)
               && _localNames.SequenceEqual(other._localNames)
               && _localRwStates.SequenceEqual(other._localRwStates)
               && DefinedOnLine == other.DefinedOnLine
               && DefinedInFile == other.DefinedInFile;
    }

    public override bool Equals(object? obj)
        => ReferenceEquals(this, obj)
           || obj is DebugDatabase other
           && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(
            _records,
            _statementRecords,
            _parameterNames,
            _localNames,
            DefinedOnLine,
            DefinedInFile
        );

    public static bool operator ==(DebugDatabase? left, DebugDatabase? right)
        => Equals(left, right);

    public static bool operator !=(DebugDatabase? left, DebugDatabase? right)
        => !Equals(left, right);
}
