using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ceres.ExecutionEngine.Diagnostics.Debugging;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine.Diagnostics
{
    public sealed partial class Chunk : IDisposable, IEquatable<Chunk>
    {
        private readonly MemoryStream _code;
        private List<int> _labels;
        private List<ChunkAttribute> _attributes;
        private Dictionary<int, DynamicValue> _parameterInitializers;
        private List<ClosureInfo> _closures;
        private List<Chunk> _subChunks;

        private readonly Serializer _serializer;

        public const byte FormatVersion = 3;

        public string Name { get; set; }
        public Chunk? Parent { get; private set; }

        public StringPool StringPool { get; }
        public CodeGenerator CodeGenerator { get; }
        public DebugDatabase DebugDatabase { get; private set; }

        public int ParameterCount { get; private set; }
        public int LocalCount { get; private set; }
        public int ClosureCount => Closures.Count;
        public int SubChunkCount => SubChunks.Count;
        public bool IsSelfAware { get; private set; }

        public IReadOnlyList<int> Labels => _labels;
        public IReadOnlyList<ChunkAttribute> Attributes => _attributes;
        public IReadOnlyDictionary<int, DynamicValue> ParameterInitializers => _parameterInitializers;
        public IReadOnlyList<ClosureInfo> Closures => _closures;
        public IReadOnlyList<Chunk> SubChunks => _subChunks;

        public byte[] Code => _code.GetBuffer();

        public bool HasParameters => Flags.HasFlag(ChunkFlags.HasParameters);
        public bool HasLocals => Flags.HasFlag(ChunkFlags.HasLocals);
        public bool HasAttributes => Flags.HasFlag(ChunkFlags.HasAttributes);
        public bool HasDebugInfo => Flags.HasFlag(ChunkFlags.HasDebugInfo);
        public bool HasClosures => Flags.HasFlag(ChunkFlags.HasClosures);
        public bool HasSubChunks => Flags.HasFlag(ChunkFlags.HasSubChunks);
        public bool IsSubChunk => Flags.HasFlag(ChunkFlags.IsSubChunk);

        public ChunkFlags Flags
        {
            get
            {
                var ret = ChunkFlags.Empty;

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

                if (ClosureCount > 0)
                    ret |= ChunkFlags.HasClosures;

                if (SubChunkCount > 0)
                    ret |= ChunkFlags.HasSubChunks;

                if (Parent != null)
                    ret |= ChunkFlags.IsSubChunk;

                if (IsSelfAware)
                    ret |= ChunkFlags.IsSelfAware;
                
                return ret;
            }
        }

        public Chunk(string name)
        {
            _code = new MemoryStream(0);
            _labels = new List<int>();
            _attributes = new List<ChunkAttribute>();
            _parameterInitializers = new Dictionary<int, DynamicValue>();
            _closures = new List<ClosureInfo>();
            _subChunks = new List<Chunk>();
            _serializer = new Serializer(this);

            Name = name;
            StringPool = new StringPool();
            CodeGenerator = new CodeGenerator(_code, _labels);
            DebugDatabase = new DebugDatabase();
        }

        public Chunk(string name, byte[] code)
        {
            _code = new MemoryStream(code, 0, code.Length, true, true);
            _labels = new List<int>();
            _attributes = new List<ChunkAttribute>();
            _parameterInitializers = new Dictionary<int, DynamicValue>();
            _closures = new List<ClosureInfo>();
            _subChunks = new List<Chunk>();
            _serializer = new Serializer(this);

            Name = name;
            StringPool = new StringPool();
            CodeGenerator = new CodeGenerator(_code, _labels);
            DebugDatabase = new DebugDatabase();
        }

        public Chunk(string name, byte[] code, string[] stringConstants)
        {
            _code = new MemoryStream(code, 0, code.Length, true, true);
            _labels = new List<int>();
            _attributes = new List<ChunkAttribute>();
            _parameterInitializers = new Dictionary<int, DynamicValue>();
            _closures = new List<ClosureInfo>();
            _subChunks = new List<Chunk>();
            _serializer = new Serializer(this);

            Name = name;
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

        public void MarkSelfAware()
            => IsSelfAware = true;

        public int AllocateParameter()
            => ParameterCount++;

        public int AllocateLocal()
            => LocalCount++;

        public (int Id, ClosureInfo Closure) AllocateClosure(int nestingLevel, int enclosedId, string enclosedFunctionName, bool isParameter, bool isClosure)
        {
            var id = ClosureCount;
            var ret = new ClosureInfo(
                nestingLevel,
                enclosedId,
                enclosedFunctionName,
                isParameter,
                isClosure
            );

            _closures.Add(ret);

            return (id, ret);
        }

        public (int Id, Chunk SubChunk) AllocateSubChunk()
        {
            var id = SubChunkCount;

            string BuildName()
            {
                var list = new List<string>();

                if (Parent == null)
                {
                    return $"{Name}$?{id}";
                }
                else
                {
                    list.AddRange(
                        Name.Split('$')
                            .Where(x => !string.IsNullOrEmpty(x))
                    );
                }

                list.Add($"?{id}");
                return string.Join("$", list);
            }

            var chunk = new Chunk(BuildName()) { Parent = this };

            _subChunks.Add(chunk);
            return (id, chunk);
        }

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

        public Chunk Clone()
        {
            var clone = new Chunk(
                Name + "$clone",
                _code.ToArray(),
                StringPool.ToArray()
            )
            {
                Parent = Parent,
                Name = Name,
                LocalCount = LocalCount,
                ParameterCount = ParameterCount,
                DebugDatabase = DebugDatabase,
                IsSelfAware = IsSelfAware,
                _labels = new(_labels),
                _attributes = new(_attributes),
                _parameterInitializers = new(_parameterInitializers),
                _subChunks = new(_subChunks)
            };

            foreach (var closure in _closures)
            {
                clone._closures.Add(
                    new(
                        closure.NestingLevel,
                        closure.EnclosedId,
                        closure.EnclosedFunctionName,
                        closure.IsParameter,
                        closure.IsClosure
                    )
                );
            }

            return clone;
        }

        public void Dispose()
        {
            _code.Dispose();
            CodeGenerator.Dispose();
        }

        public bool Equals(Chunk? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Name == other.Name
                   && Parent == other.Parent
                   && IsSelfAware == other.IsSelfAware
                   && ParameterCount == other.ParameterCount
                   && _parameterInitializers.SequenceEqual(other._parameterInitializers)
                   && LocalCount == other.LocalCount
                   && _closures.SequenceEqual(other._closures)
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
            hashCode.Add(IsSelfAware);
            hashCode.Add(ParameterCount);
            hashCode.Add(_parameterInitializers);
            hashCode.Add(LocalCount);
            hashCode.Add(_closures);
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