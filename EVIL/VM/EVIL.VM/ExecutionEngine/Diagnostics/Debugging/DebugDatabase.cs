namespace EVIL.VM.ExecutionEngine.Diagnostics.Debugging;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EVIL.VM.ExecutionEngine;
using EVIL.VM.ExecutionEngine.Serialization;

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

    public IReadOnlyDictionary<int, StatementDebugRecord> StatementRecords 
        => _statementRecords;

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
        _parameterRwStates.Clear();
        _localNames.Clear();
        _localRwStates.Clear();
        DefinedOnLine = -1;
        DefinedInFile = string.Empty;
    }

    internal void Serialize(
        BinaryWriter bw,
        Action<BinaryWriter, string> writeString,
        Action<BinaryWriter, int> writeCount)
    {
        bw.Write(DefinedOnLine);
        writeString(bw, DefinedInFile);

        writeCount(bw, _records.Count);
        foreach (var record in _records)
        {
            bw.Write(record.Key);
            writeCount(bw, record.Value.Count);

            foreach (var addr in record.Value)
                bw.Write(addr);
        }

        writeCount(bw, _statementRecords.Count);
        foreach (var (ip, stmt) in _statementRecords)
        {
            bw.Write(ip);
            bw.Write(stmt.Line);
            bw.Write(stmt.Column);
            bw.Write(stmt.EndLine);
            bw.Write(stmt.EndColumn);
            bw.Write(stmt.IsStepBoundary);
        }

        writeCount(bw, _localNames.Count);
        foreach (var localKvp in _localNames)
        {
            bw.Write(localKvp.Key);
            writeString(bw, localKvp.Value);
            bw.Write(_localRwStates[localKvp.Key]);
        }

        writeCount(bw, _parameterNames.Count);
        foreach (var paramKvp in _parameterNames)
        {
            bw.Write(paramKvp.Key);
            writeString(bw, paramKvp.Value);
            bw.Write(_parameterRwStates[paramKvp.Key]);
        }
    }

    internal void Deserialize(
        BinaryChunkReader reader,
        int parameterCount,
        int localCount,
        int codeLength)
    {
        Strip();

        DefinedOnLine = reader.ReadInt32();
        if (DefinedOnLine < -1)
        {
            throw new ChunkDeserializationException("Invalid debug definition line.");
        }

        DefinedInFile = reader.ReadString();

        var recordCount = reader.ReadCount(8);
        var seenLines = new HashSet<int>();
        for (var i = 0; i < recordCount; i++)
        {
            var line = reader.ReadInt32();
            var addrCount = reader.ReadCount(sizeof(int));
            if (line < 0 || addrCount == 0 || !seenLines.Add(line))
            {
                throw new ChunkDeserializationException("Invalid or duplicate debug line record.");
            }

            var seenAddresses = new HashSet<int>();
            for (var j = 0; j < addrCount; j++)
            {
                var address = reader.ReadInt32();
                ValidateCodeAddress(address, codeLength);
                if (!seenAddresses.Add(address))
                {
                    throw new ChunkDeserializationException(
                        "Debug line record contains a duplicate code address."
                    );
                }

                AddDebugRecord(line, address);
            }
        }

        var stmtCount = reader.ReadCount(21);
        for (var i = 0; i < stmtCount; i++)
        {
            var ip = reader.ReadInt32();
            var line = reader.ReadInt32();
            var col = reader.ReadInt32();
            var endLine = reader.ReadInt32();
            var endCol = reader.ReadInt32();
            var isStep = reader.ReadBoolean();

            ValidateCodeAddress(ip, codeLength);
            if (line < 0 || !_records.TryGetValue(line, out var addresses)
                || !addresses.Contains(ip) || _statementRecords.ContainsKey(ip))
            {
                throw new ChunkDeserializationException(
                    "Invalid or duplicate statement debug record."
                );
            }

            AddStatementRecord(ip, line, col, endLine, endCol, isStep);
        }

        var localNameCount = reader.ReadCount(9);
        var seenLocals = new HashSet<int>();
        for (var i = 0; i < localNameCount; i++)
        {
            var id = reader.ReadInt32();
            var name = reader.ReadString();
            var rw = reader.ReadBoolean();
            if (id < 0 || id >= localCount || !seenLocals.Add(id))
            {
                throw new ChunkDeserializationException(
                    $"Debug local name references invalid local '{id}'."
                );
            }

            SetLocalName(id, name, rw);
        }

        var paramNameCount = reader.ReadCount(9);
        var seenParameters = new HashSet<int>();
        for (var i = 0; i < paramNameCount; i++)
        {
            var id = reader.ReadInt32();
            var name = reader.ReadString();
            var rw = reader.ReadBoolean();
            if (id < 0 || id >= parameterCount || !seenParameters.Add(id))
            {
                throw new ChunkDeserializationException(
                    $"Debug parameter name references invalid parameter '{id}'."
                );
            }

            SetParameterName(id, name, rw);
        }

        reader.EnsureFullyConsumed("debug database");

        static void ValidateCodeAddress(int address, int codeLength)
        {
            if (address < 0 || address > codeLength)
            {
                throw new ChunkDeserializationException(
                    $"Invalid debug code address '{address}'."
                );
            }
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
