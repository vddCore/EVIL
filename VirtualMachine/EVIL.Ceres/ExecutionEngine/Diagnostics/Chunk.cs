namespace EVIL.Ceres.ExecutionEngine.Diagnostics;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using EVIL.Ceres.ExecutionEngine.Diagnostics.Debugging;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.CommonTypes.TypeSystem;

public sealed partial class Chunk : IDisposable, IEquatable<Chunk>
{
    private byte[]? _cachedCode;
    private readonly MemoryStream _code;
    private bool _labelsBackpatched;
    private List<int> _labels;
    private List<ChunkAttribute> _attributes;
    private Dictionary<int, DynamicValue> _parameterInitializers;
    private List<ClosureInfo> _closures;
    private Dictionary<string, ClosureContext> _closureContexts;
    private List<Chunk> _subChunks;
    private Dictionary<string, int> _namedSubChunkLookup;
    private List<BlockProtectionInfo> _protectedBlocks;
    private readonly ConcurrentDictionary<int, SwitchTable> _switchTables = new();
    private readonly HashSet<int> _targetedLabels = [];
    private PropertyInlineCache[]? _propertyInlineCaches;
    private readonly Serializer _serializer;

    public const byte FormatVersion = 4;

    public string Name { get; set; }
    internal Guid ClosureOwnerId { get; private set; }
    public Chunk? Parent { get; private set; }

    public StringPool StringPool { get; }
    public CodeGenerator CodeGenerator { get; }
    public DebugDatabase DebugDatabase { get; private init; }

    public int ParameterCount { get; private set; }
    public int LocalCount { get; private set; }
    public int ClosureCount => Closures.Count;
    public int SubChunkCount => SubChunks.Count;
    public bool IsSelfAware { get; private set; }
    public bool IsSpecialName { get; private set; }
    public bool MayThrow { get; private set; }

    public IReadOnlyList<int> Labels => _labels;
    public IReadOnlyList<ChunkAttribute> Attributes => _attributes;
    public IReadOnlyDictionary<int, DynamicValue> ParameterInitializers => _parameterInitializers;
    public IReadOnlyList<ClosureInfo> Closures => _closures;
    public IReadOnlyDictionary<string, ClosureContext> ClosureContexts => _closureContexts;
    public IReadOnlyList<Chunk> SubChunks => _subChunks;
    public IReadOnlyDictionary<string, int> NamedSubChunkLookup => _namedSubChunkLookup;
    public IReadOnlyList<BlockProtectionInfo> ProtectedBlocks => _protectedBlocks;

    public Upvalue[]? Upvalues { get; set; }
    public byte[] Code => _cachedCode ??= FinalizeAndGetCode();

    public bool HasParameters => Flags.HasFlag(ChunkFlags.HasParameters);
    public bool HasLocals => Flags.HasFlag(ChunkFlags.HasLocals);
    public bool HasAttributes => Flags.HasFlag(ChunkFlags.HasAttributes);
    public bool HasDebugInfo => Flags.HasFlag(ChunkFlags.HasDebugInfo);
    public bool HasClosures => Flags.HasFlag(ChunkFlags.HasClosures);
    public bool HasSubChunks => Flags.HasFlag(ChunkFlags.HasSubChunks);
    public bool IsSubChunk => Flags.HasFlag(ChunkFlags.IsSubChunk);

    public Chunk? this[string name] 
        => _namedSubChunkLookup.TryGetValue(name, out var id) 
         ? _subChunks[id] 
         : null;

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

            if (IsSpecialName)
                ret |= ChunkFlags.IsSpecialName;

            if (ProtectedBlocks.Count > 0)
                ret |= ChunkFlags.HasProtectedBlocks;

            if (MayThrow)
                ret |= ChunkFlags.MayThrow;
                
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
        _closureContexts = new Dictionary<string, ClosureContext>();
        _subChunks = new List<Chunk>();
        _namedSubChunkLookup = new Dictionary<string, int>();
        _protectedBlocks = new List<BlockProtectionInfo>();
        _serializer = new Serializer(this);

        Name = name;
        ClosureOwnerId = Guid.NewGuid();
        StringPool = new StringPool();
        CodeGenerator = new CodeGenerator(_code, _labels)
        {
            CodeModified = () =>
            {
                _cachedCode = null;
                _labelsBackpatched = false;
                _switchTables.Clear();
            },
            LabelTargetEmitted = MarkLabelTargeted
        };
        DebugDatabase = new DebugDatabase();
    }

    public Chunk(string name, byte[] code)
    {
        _cachedCode = code;
        _code = new MemoryStream(code, 0, code.Length, true, true);
        _labels = new List<int>();
        _attributes = new List<ChunkAttribute>();
        _parameterInitializers = new Dictionary<int, DynamicValue>();
        _closures = new List<ClosureInfo>();
        _closureContexts = new Dictionary<string, ClosureContext>();
        _subChunks = new List<Chunk>();
        _namedSubChunkLookup = new Dictionary<string, int>();
        _protectedBlocks = new List<BlockProtectionInfo>();
        _serializer = new Serializer(this);

        Name = name;
        ClosureOwnerId = Guid.NewGuid();
        StringPool = new StringPool();
        CodeGenerator = new CodeGenerator(_code, _labels)
        {
            CodeModified = () =>
            {
                _cachedCode = null;
                _labelsBackpatched = false;
                _switchTables.Clear();
            },
            LabelTargetEmitted = MarkLabelTargeted
        };
        DebugDatabase = new DebugDatabase();
    }

    public Chunk(string name, byte[] code, string[] stringConstants)
    {
        _cachedCode = code;
        _code = new MemoryStream(code, 0, code.Length, true, true);
        _labels = new List<int>();
        _attributes = new List<ChunkAttribute>();
        _parameterInitializers = new Dictionary<int, DynamicValue>();
        _closures = new List<ClosureInfo>();
        _closureContexts = new Dictionary<string, ClosureContext>();
        _subChunks = new List<Chunk>();
        _namedSubChunkLookup = new Dictionary<string, int>();
        _protectedBlocks = new List<BlockProtectionInfo>();
        _serializer = new Serializer(this);

        Name = name;
        ClosureOwnerId = Guid.NewGuid();
        StringPool = new StringPool(stringConstants);
        CodeGenerator = new CodeGenerator(_code, _labels)
        {
            CodeModified = () =>
            {
                _cachedCode = null;
                _labelsBackpatched = false;
                _switchTables.Clear();
            },
            LabelTargetEmitted = MarkLabelTargeted
        };
        DebugDatabase = new DebugDatabase();
    }

    public Chunk(string name, byte[] code, StringPool stringPool)
    {
        _cachedCode = code;
        _code = new MemoryStream(code, 0, code.Length, true, true);
        _labels = new List<int>();
        _attributes = new List<ChunkAttribute>();
        _parameterInitializers = new Dictionary<int, DynamicValue>();
        _closures = new List<ClosureInfo>();
        _closureContexts = new Dictionary<string, ClosureContext>();
        _subChunks = new List<Chunk>();
        _namedSubChunkLookup = new Dictionary<string, int>();
        _protectedBlocks = new List<BlockProtectionInfo>();
        _serializer = new Serializer(this);

        Name = name;
        ClosureOwnerId = Guid.NewGuid();
        StringPool = stringPool;
        CodeGenerator = new CodeGenerator(_code, _labels)
        {
            CodeModified = () =>
            {
                _cachedCode = null;
                _labelsBackpatched = false;
                _switchTables.Clear();
            },
            LabelTargetEmitted = MarkLabelTargeted
        };
        DebugDatabase = new DebugDatabase();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void EnsurePropertyInlineCaches(int opOffset)
    {
        var newLen = Math.Max(opOffset + 1, Math.Max(Code.Length, 16));
        Array.Resize(ref _propertyInlineCaches, newLen);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref PropertyInlineCache GetPropertyInlineCache(int opOffset)
    {
        if (_propertyInlineCaches == null || (uint)opOffset >= (uint)_propertyInlineCaches.Length)
        {
            EnsurePropertyInlineCaches(opOffset);
        }

        return ref _propertyInlineCaches![opOffset];
    }

    private SwitchTable BuildSwitchTable(int opCodeIp)
    {
        BackpatchLabels();

        var code = Code;
        
        using var ms = new MemoryStream(code);
        ms.Seek(opCodeIp + 1, SeekOrigin.Begin);
        
        using var reader = new BinaryReader(ms, Encoding.UTF8);
        var defaultIp = reader.ReadInt32();
        var count = reader.ReadInt32();
        var cases = new Dictionary<DynamicValue, int>(count);

        for (var i = 0; i < count; i++)
        {
            var type = (DynamicValueType)reader.ReadByte();
            DynamicValue val;
            
            switch (type)
            {
                case DynamicValueType.Nil:
                    val = DynamicValue.Nil;
                    break;
                
                case DynamicValueType.Boolean:
                    val = new DynamicValue(reader.ReadBoolean());
                    break;
                
                case DynamicValueType.Number:
                    val = new DynamicValue(reader.ReadDouble());
                    break;
                
                case DynamicValueType.String:
                    var strId = reader.ReadInt32();
                    val = new DynamicValue(StringPool[strId]!);
                    break;
                
                case DynamicValueType.TypeCode:
                    val = new DynamicValue((DynamicValueType)reader.ReadByte());
                    break;
                
                default:
                    throw new InvalidOperationException($"Unexpected constant type in SWITCH: {type}");
            }

            var targetIp = reader.ReadInt32();
            cases.TryAdd(val, targetIp);
        }

        return new SwitchTable(defaultIp, cases);
    }
    
    public SwitchTable GetSwitchTable(int opCodeIp)
        => _switchTables.GetOrAdd(opCodeIp, BuildSwitchTable);

    public void BackpatchLabels()
    {
        if (_labelsBackpatched)
        {
            return;
        }

        _labelsBackpatched = true;

        if (_labels.Count == 0 || CodeGenerator.JumpFixups.Count == 0)
        {
            return;
        }

        var currentPos = _code.Position;
        using var reader = new BinaryReader(_code, Encoding.UTF8, true);
        using var writer = new BinaryWriter(_code, Encoding.UTF8, true);

        foreach (var fixupOffset in CodeGenerator.JumpFixups)
        {
            _code.Seek(fixupOffset, SeekOrigin.Begin);
            
            var labelId = reader.ReadInt32();
            if (labelId >= 0 && labelId < _labels.Count)
            {
                var targetIp = _labels[labelId];
                _code.Seek(fixupOffset, SeekOrigin.Begin);
                writer.Write(targetIp);
            }
        }

        _code.Seek(currentPos, SeekOrigin.Begin);
    }

    private byte[] FinalizeAndGetCode()
    {
        BackpatchLabels();
        return _code.ToArray();
    }

    public ChunkReader SpawnCodeReader()
    {
        return new ChunkReader(Code);
    }

    public void MarkThrowing()
        => MayThrow = true;

    public void MarkSelfAware()
        => IsSelfAware = true;

    public int AllocateParameter()
        => ParameterCount++;

    public int AllocateLocal()
        => LocalCount++;
        
    public ClosureContext SetClosureContext(string chunkName)
    {
        if (!_closureContexts.TryGetValue(chunkName, out var value))
        {
            value = _closureContexts[chunkName] = new ClosureContext(chunkName);
        }

        return value;
    }
        
    public (int Id, ClosureInfo Closure) AllocateClosure(
        int nestingLevel,
        int enclosedId,
        string enclosedFunctionName,
        ClosureType closureType,
        bool isSharedScope)
        => AllocateClosure(
            nestingLevel,
            enclosedId,
            enclosedFunctionName,
            closureType,
            isSharedScope,
            default
        );

    internal (int Id, ClosureInfo Closure) AllocateClosure(
        int nestingLevel,
        int enclosedId,
        string enclosedFunctionName,
        ClosureType closureType,
        bool isSharedScope,
        Guid enclosedOwnerId)
    {
        var id = ClosureCount;
        var ret = new ClosureInfo(
            nestingLevel,
            enclosedId,
            enclosedFunctionName,
            closureType,
            isSharedScope,
            enclosedOwnerId
        );

        _closures.Add(ret);

        return (id, ret);
    }

    public (int Id, Chunk SubChunk) AllocateAnonymousSubChunk()
    {
        var id = SubChunkCount;

        var chunk = new Chunk(BuildName())
        {
            IsSpecialName = true,
            Parent = this
        };

        _subChunks.Add(chunk);
        return (id, chunk);

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
    }
        
    public (int Id, Chunk SubChunk) AllocateNamedSubChunk(
        string name,
        out bool wasReplaced,
        out Chunk replacedChunk
    ) => AllocateNamedSubChunk(name, false, out wasReplaced, out replacedChunk);

    internal (int Id, Chunk SubChunk) AllocateNamedSubChunk(
        string name,
        bool allocateNewVersionOnRedefinition,
        out bool wasReplaced,
        out Chunk replacedChunk
    )
    {
        var id = SubChunkCount;
        wasReplaced = false;
        replacedChunk = null!;
            
        var chunk = new Chunk(name) { Parent = this };

        if (_namedSubChunkLookup.TryGetValue(name, out var existingId))
        {
            replacedChunk = _subChunks[existingId];
            wasReplaced = true;

            if (allocateNewVersionOnRedefinition)
            {
                _subChunks.Add(chunk);
                _namedSubChunkLookup[name] = id;
            }
            else
            {
                id = existingId;
                _subChunks[existingId] = chunk;
            }
        }
        else
        {
            _namedSubChunkLookup.Add(name, id);
            _subChunks.Add(chunk);
        }

        return (id, chunk);
    }

    public void InitializeParameter(int parameterId, DynamicValue constant)
    {
        if (!_parameterInitializers.TryAdd(parameterId, constant))
        {
            throw new InvalidOperationException(
                "Internal compiler error: Attempt to initialize the same parameter symbol twice."
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

    public BlockProtectionInfo AllocateBlockProtector()
    {
        var info = new BlockProtectionInfo();
        _protectedBlocks.Add(info);
        return info;
    }

    public Action<int, int>? LabelUpdated { get; set; }

    public void MarkLabelTargeted(int labelId)
        => _targetedLabels.Add(labelId);

    public bool IsLabelTargeted(int labelId)
        => _targetedLabels.Contains(labelId);

    public void AddAttribute(ChunkAttribute attribute)
        => _attributes.Add(attribute);

    public void UpdateLabel(int id, int ip)
    {
        _labels[id] = ip;
        LabelUpdated?.Invoke(id, ip);
    }

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

    public byte[] ComputeChecksum()
    {
        var bytes = new List<byte>();
        bytes.AddRange(Code);
        foreach (var subChunk in _subChunks)
        {
            bytes.AddRange(subChunk.ComputeChecksum());
        }

        return SHA256.HashData(bytes.ToArray());
    }

    public string ComputeChecksumString()
    {
        var bytes = ComputeChecksum();
        var sb = new StringBuilder();

        foreach (var b in bytes)
        {
            sb.Append($"{b:X2}");
        }

        return sb.ToString();
    }

    public void Serialize(Stream stream)
    {
        BackpatchLabels();
        _serializer.Write(stream);
    }

    public static Chunk Deserialize(Stream stream, out byte version, out long timestamp)
        => Deserializer.Deserialize(stream, out version, out timestamp);

    public Chunk Clone()
    {
        var contexts = new Dictionary<string, ClosureContext>(_closureContexts.Count);
        foreach (var kvp in _closureContexts)
        {
            var ctx = new ClosureContext(kvp.Value.EnclosedFunctionName);
            foreach (var kvp2 in kvp.Value.Values)
            {
                ctx.Values[kvp2.Key] = kvp2.Value;
            }
            contexts[kvp.Key] = ctx;
        }
        
        var clone = new Chunk(
            Name,
            Code,
            StringPool
        )
        {
            Parent = Parent,
            Name = Name,
            ClosureOwnerId = ClosureOwnerId,
            LocalCount = LocalCount,
            ParameterCount = ParameterCount,
            DebugDatabase = DebugDatabase,
            IsSelfAware = IsSelfAware,
            IsSpecialName = IsSpecialName,
            MayThrow = MayThrow,
            Upvalues = Upvalues != null 
                ? (Upvalue[])Upvalues.Clone() 
                : null,
            _subChunks = _subChunks,
            _namedSubChunkLookup = _namedSubChunkLookup,
            _closures = _closures,
            _closureContexts = contexts,
            _labels = _labels,
            _attributes = _attributes,
            _parameterInitializers = _parameterInitializers,
            _protectedBlocks = _protectedBlocks,
            _propertyInlineCaches = _propertyInlineCaches
        };

        return clone;
    }

    public void Dispose()
    {
        _code.Dispose();
        CodeGenerator.Dispose();
        _propertyInlineCaches = null;
    }

    public bool Equals(Chunk? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Name == other.Name
               && ClosureOwnerId == other.ClosureOwnerId
               && IsSelfAware == other.IsSelfAware
               && ParameterCount == other.ParameterCount
               && _parameterInitializers.SequenceEqual(other._parameterInitializers)
               && LocalCount == other.LocalCount
               && _closures.SequenceEqual(other._closures)
               && _labels.SequenceEqual(other._labels)
               && _attributes.SequenceEqual(other._attributes)
               && _subChunks.SequenceEqual(other._subChunks)
               && _namedSubChunkLookup.SequenceEqual(other._namedSubChunkLookup)
               && _protectedBlocks.SequenceEqual(other._protectedBlocks)
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
        hashCode.Add(ClosureOwnerId);
        hashCode.Add(IsSelfAware);
        hashCode.Add(ParameterCount);
        hashCode.Add(_parameterInitializers);
        hashCode.Add(LocalCount);
        hashCode.Add(_closures);
        hashCode.Add(_closureContexts);
        hashCode.Add(_labels);
        hashCode.Add(_attributes);
        hashCode.Add(_subChunks);
        hashCode.Add(_namedSubChunkLookup);
        hashCode.Add(_protectedBlocks);
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