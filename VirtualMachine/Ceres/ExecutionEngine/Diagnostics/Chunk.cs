using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ceres.ExecutionEngine.Diagnostics.Debug;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine.Diagnostics
{
    public sealed partial class Chunk : IDisposable, IEquatable<Chunk>
    {        
        private readonly MemoryStream _code;
        private readonly List<int> _labels;
        private readonly List<ChunkAttribute> _attributes;
        private readonly Dictionary<int, DynamicValue> _parameterInitializers;
        
        private readonly Serializer _serializer;

        public const byte FormatVersion = 1;
        
        public string? Name { get; set; }

        public StringPool StringPool { get; }
        public CodeGenerator CodeGenerator { get; }
        public DebugDatabase DebugDatabase { get; }

        public int ParameterCount { get; private set; }
        public int LocalCount { get; private set; }
        
        public IReadOnlyList<int> Labels => _labels;
        public IReadOnlyList<ChunkAttribute> Attributes => _attributes;
        public IReadOnlyDictionary<int, DynamicValue> ParameterInitializers => _parameterInitializers;

        public byte[] Code => _code.GetBuffer();
        
        public bool IsAnonymous => !Flags.HasFlag(ChunkFlags.HasName);
        public bool HasParameters => Flags.HasFlag(ChunkFlags.HasParameters);
        public bool HasLocals => Flags.HasFlag(ChunkFlags.HasLocals);
        public bool HasAttributes => Flags.HasFlag(ChunkFlags.HasAttributes);
        public bool HasDebugInfo => Flags.HasFlag(ChunkFlags.HasDebugInfo);
        
        public ChunkFlags Flags
        {
            get
            {
                var ret = ChunkFlags.Empty;

                if (Name != null) 
                    ret |= ChunkFlags.HasName;
                
                if (ParameterCount > 0) 
                    ret |= ChunkFlags.HasParameters;
                
                if (ParameterInitializers.Count > 0) 
                    ret |= ChunkFlags.HasParameterInitializers;
                
                if (LocalCount > 0)
                    ret |= ChunkFlags.HasLocals;
                
                if (Attributes.Count > 0)
                    ret |= ChunkFlags.HasAttributes;

                if (Labels.Count > 0)
                    ret |= ChunkFlags.HasLabels;

                if (!DebugDatabase.IsEmpty)
                    ret |= ChunkFlags.HasDebugInfo;
                
                return ret;
            }
        }

        public Chunk()
        {
            _code = new MemoryStream(0);
            _labels = new List<int>();
            _attributes = new List<ChunkAttribute>();
            _parameterInitializers = new Dictionary<int, DynamicValue>();
            _serializer = new Serializer(this);
            
            StringPool = new StringPool();
            CodeGenerator = new CodeGenerator(_code, _labels);
            DebugDatabase = new DebugDatabase();
        }

        public Chunk(byte[] code)
        {
            _code = new MemoryStream(code, 0, code.Length, true, true);
            _labels = new List<int>();
            _attributes = new List<ChunkAttribute>();
            _parameterInitializers = new Dictionary<int, DynamicValue>();
            _serializer = new Serializer(this);

            StringPool = new StringPool();
            CodeGenerator = new CodeGenerator(_code, _labels);
            DebugDatabase = new DebugDatabase();
        }

        public Chunk(byte[] code, string[] stringConstants)
        {
            _code = new MemoryStream(code, 0, code.Length, true, true);
            _labels = new List<int>();
            _attributes = new List<ChunkAttribute>();
            _parameterInitializers = new Dictionary<int, DynamicValue>();
            _serializer = new Serializer(this);

            StringPool = new StringPool(stringConstants);
            CodeGenerator = new CodeGenerator(_code, _labels);
            DebugDatabase = new DebugDatabase();
        }

        public BinaryReader SpawnCodeReader()
        {
            return new BinaryReader(
                new MemoryStream(_code.ToArray()),
                Encoding.UTF8,
                false
            );
        }

        public int AllocateParameter()
            => ParameterCount++;

        public int AllocateLocal()
            => LocalCount++;

        public void InitializeParameter(int parameterId, DynamicValue constant)
        {
            if (!_parameterInitializers.TryAdd(parameterId, constant))
            {
                throw new InvalidOperationException(
                    $"Internal compiler error: Attempt to initialize the same parameter symbol twice."
                );
            }
        }

        public int CreateLabel()
            => CreateLabel(CodeGenerator.IP);
        
        public int CreateLabel(int ip)
        {
            _labels.Add(ip);
            return _labels.Count - 1;
        }

        public void AddAttribute(ChunkAttribute attribute) 
            => _attributes.Add(attribute);

        public void UpdateLabel(int id, int ip) 
            => _labels[id] = ip;

        public ChunkAttribute[] GetAttributes(string name)
            => _attributes.Where(x => x.Name == name).ToArray();
        
        public ChunkAttribute GetAttribute(string name)
            => _attributes.First(x => x.Name == name);

        public bool TryGetAttribute(string name, out ChunkAttribute value)
        {
            var ret = _attributes.FirstOrDefault(x => x.Name == name);
            value = ret!;
            
            return ret != null;
        }

        public bool HasAttribute(string name)
            => TryGetAttribute(name, out _);

        public void Serialize(Stream stream)
            => _serializer.Write(stream);

        public static Chunk Deserialize(Stream stream, out byte version, out long timestamp)
            => Deserializer.Deserialize(stream, out version, out timestamp);
        
        public void Dispose()
        {
            _code.Dispose();
            _labels.Clear();
            
            CodeGenerator.Dispose();
        }

        public bool Equals(Chunk? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Name == other.Name
                   && ParameterCount == other.ParameterCount
                   && _parameterInitializers.SequenceEqual(other._parameterInitializers)
                   && LocalCount == other.LocalCount
                   && _labels.SequenceEqual(other._labels)
                   && _attributes.SequenceEqual(other._attributes)
                   && StringPool.Equals(other.StringPool)
                   && Code.SequenceEqual(other.Code)
                   && DebugDatabase.Equals(other.DebugDatabase);
        }

        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) 
                   || obj is Chunk other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            
            hashCode.Add(Name);
            hashCode.Add(ParameterCount);
            hashCode.Add(_parameterInitializers);
            hashCode.Add(LocalCount);
            hashCode.Add(_labels);
            hashCode.Add(_attributes);
            hashCode.Add(StringPool);
            hashCode.Add(Code);
            hashCode.Add(DebugDatabase);
            
            return hashCode.ToHashCode();
        }

        public static bool operator ==(Chunk? left, Chunk? right)
            => Equals(left, right);

        public static bool operator !=(Chunk? left, Chunk? right)
            => !Equals(left, right);
    }
}