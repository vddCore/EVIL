using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Ceres.ExecutionEngine.TypeSystem;
using EVIL.CommonTypes.TypeSystem;

namespace Ceres.ExecutionEngine.Diagnostics
{
    public sealed partial class Chunk
    {
        private class Serializer
        {
            private readonly Chunk _chunk;

            public Serializer(Chunk chunk)
            {
                _chunk = chunk;
            }

            private void WriteHeader(BinaryWriter bw)
            {
                bw.Write(new[]
                {
                    (byte)'E',
                    (byte)'V',
                    (byte)'C',
                    Chunk.FormatVersion
                });

                bw.Write(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                bw.Write((int)_chunk.Flags);
            }

            private void WriteConstValue(BinaryWriter bw, DynamicValue value)
            {
                bw.Write((byte)value.Type);
                switch (value.Type)
                {
                    case DynamicValueType.Nil:
                        break;

                    case DynamicValueType.Number:
                        bw.Write(value.Number);
                        break;

                    case DynamicValueType.String:
                        bw.Write(value.String!);
                        break;

                    case DynamicValueType.Boolean:
                        bw.Write(value.Boolean);
                        break;
                    
                    case DynamicValueType.TypeCode:
                        bw.Write((byte)value.TypeCode);
                        break;
                    
                    default: throw new InvalidOperationException("Invalid constant value type.");
                }
            }

            private void WriteAttribute(BinaryWriter bw, ChunkAttribute attribute)
            {
                bw.Write(attribute.Name);

                bw.Write(attribute.Values.Count);
                foreach (var value in attribute.Values)
                {
                    WriteConstValue(bw, value);
                }

                bw.Write(attribute.Properties.Count);
                foreach (var property in attribute.Properties)
                {
                    bw.Write(property.Key);
                    WriteConstValue(bw, property.Value);
                }
            }

            private void WriteName(BinaryWriter bw)
                => bw.Write(_chunk.Name);

            private void WriteParameterInfo(BinaryWriter bw)
            {
                if (_chunk.Flags.HasFlag(ChunkFlags.HasParameters))
                {
                    bw.Write(_chunk.ParameterCount);
                }

                if (_chunk.Flags.HasFlag(ChunkFlags.HasParameterInitializers))
                {
                    bw.Write(_chunk.ParameterInitializers.Count);

                    foreach (var kvp in _chunk.ParameterInitializers)
                    {
                        bw.Write(kvp.Key);
                        WriteConstValue(bw, kvp.Value);
                    }
                }
            }

            private void WriteClosureInfo(BinaryWriter bw)
            {
                if (_chunk.Flags.HasFlag(ChunkFlags.HasClosures))
                {
                    bw.Write(_chunk.ClosureCount);

                    for (var i = 0; i < _chunk.ClosureCount; i++)
                    {
                        var closure = _chunk.Closures[i];

                        bw.Write(closure.NestingLevel);
                        bw.Write(closure.EnclosedId);
                        bw.Write(closure.EnclosedFunctionName);
                        bw.Write(closure.IsParameter);
                        bw.Write(closure.IsClosure);
                        bw.Write(closure.IsSharedScope);
                    }
                }
            }

            private void WriteSubChunks(BinaryWriter bw)
            {
                if (_chunk.Flags.HasFlag(ChunkFlags.HasSubChunks))
                {   
                    bw.Write(_chunk.SubChunkCount);
                    for (var i = 0; i < _chunk.SubChunkCount; i++)
                    {
                        var subChunk = _chunk.SubChunks[i];
                        subChunk.Serialize(bw.BaseStream);
                    }
                    
                    bw.Write(_chunk.NamedSubChunkLookup.Count);
                    foreach (var (name, id) in _chunk.NamedSubChunkLookup)
                    {
                        bw.Write(name);
                        bw.Write(id);
                    }         
                }
            }

            private void WriteLocalInfo(BinaryWriter bw)
            {
                if (_chunk.Flags.HasFlag(ChunkFlags.HasLocals))
                {
                    bw.Write(_chunk.LocalCount);
                }
            }

            private void WriteLabelInfo(BinaryWriter bw)
            {
                if (_chunk.Flags.HasFlag(ChunkFlags.HasLabels))
                {
                    bw.Write(_chunk.Labels.Count);
                    foreach (var label in _chunk.Labels)
                    {
                        bw.Write(label);
                    }
                }
            }

            private void WriteChunkAttributes(BinaryWriter bw)
            {
                if (_chunk.Flags.HasFlag(ChunkFlags.HasAttributes))
                {
                    bw.Write(_chunk.Attributes.Count);

                    foreach (var attr in _chunk.Attributes)
                    {
                        WriteAttribute(bw, attr);
                    }
                }
            }

            private void WriteDebugDatabase(BinaryWriter bw)
            {
                if (_chunk.Flags.HasFlag(ChunkFlags.HasDebugInfo))
                {
                    _chunk.DebugDatabase.Serialize(bw);
                }
            }

            private void WriteStringPool(BinaryWriter bw)
            {
                bw.Write(_chunk.StringPool.Count);

                foreach (var str in _chunk.StringPool.ToArray())
                {
                    bw.Write(str);
                }
            }

            private void WriteCodeArea(BinaryWriter bw)
            {
                bw.Write(_chunk.Code.Length);
                bw.Write(_chunk.Code);

                using (var md5 = MD5.Create())
                {
                    bw.Write(md5.ComputeHash(_chunk.Code));
                }
            }

            public void Write(Stream stream)
            {
                using (var bw = new BinaryWriter(stream, Encoding.UTF8, true))
                {
                    WriteHeader(bw);
                    WriteName(bw);
                    WriteParameterInfo(bw);
                    WriteClosureInfo(bw);
                    WriteSubChunks(bw);
                    WriteLocalInfo(bw);
                    WriteLabelInfo(bw);
                    WriteChunkAttributes(bw);
                    WriteDebugDatabase(bw);
                    WriteStringPool(bw);
                    WriteCodeArea(bw);
                }
            }
        }
    }
}