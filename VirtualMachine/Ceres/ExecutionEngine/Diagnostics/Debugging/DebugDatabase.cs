using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace Ceres.ExecutionEngine.Diagnostics.Debugging
{
    public sealed class DebugDatabase : IEquatable<DebugDatabase>
    {
        private readonly Dictionary<int, HashSet<int>> _records = new();
        private readonly Dictionary<int, string> _parameterNames = new();
        private readonly Dictionary<int, string> _localNames = new();

        public int DefinedOnLine { get; internal set; } = -1;
        public string DefinedInFile { get; internal set; } = string.Empty;

        public void AddDebugRecord(int line, int ip)
        {
            if (!_records.TryGetValue(line, out var addresses))
            {
                addresses = new HashSet<int>();
                _records.Add(line, addresses);
            }

            addresses.Add(ip);
        }

        public int GetLineForIP(int ip)
        {
            foreach (var kvp in _records)
            {
                if (kvp.Value.Contains(ip))
                    return kvp.Key;
            }

            return -1;
        }

        public void SetParameterName(int id, string name)
        {
            if (!_parameterNames.TryAdd(id, name))
                _parameterNames[id] = name;
        }

        public bool RemoveParameterName(int id) 
            => _parameterNames.Remove(id);

        public bool HasParameterNameFor(int id)
            => _parameterNames.ContainsKey(id);

        public bool TryGetParameterName(int id, out string parameterName)
            => _parameterNames.TryGetValue(id, out parameterName!);

        public void SetLocalName(int id, string name)
        {
            if (!_localNames.TryAdd(id, name))
                _localNames[id] = name;
        }

        public bool RemoveLocalName(int id)
            => _localNames.Remove(id);

        public bool HasLocalNameFor(int id)
            => _localNames.ContainsKey(id);

        public bool TryGetLocalName(int id, out string localName)
            => _localNames.TryGetValue(id, out localName!);

        public bool IsEmpty => !_records.Any()
                               && !_parameterNames.Any()
                               && !_localNames.Any()
                               && DefinedOnLine == -1;

        public void Strip()
        {
            _records.Clear();
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

            bw.Write(_localNames.Count);
            foreach (var localKvp in _localNames)
            {
                bw.Write(localKvp.Key);
                bw.Write(localKvp.Value);
            }

            bw.Write(_parameterNames.Count);
            foreach (var paramKvp in _parameterNames)
            {
                bw.Write(paramKvp.Key);
                bw.Write(paramKvp.Value);
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

            var localNameCount = br.ReadInt32();
            for (var i = 0; i < localNameCount; i++)
            {
                SetLocalName(
                    br.ReadInt32(),
                    br.ReadString()
                );
            }

            var paramNameCount = br.ReadInt32();
            for (var i = 0; i < paramNameCount; i++)
            {
                SetParameterName(
                    br.ReadInt32(),
                    br.ReadString()
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
            
            return recordsEqual
                && _parameterNames.SequenceEqual(other._parameterNames) 
                && _localNames.SequenceEqual(other._localNames)
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
}