using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ceres.ExecutionEngine.TypeSystem;
using static Ceres.ExecutionEngine.TypeSystem.DynamicValue;

namespace Ceres.ExecutionEngine.Diagnostics
{
    public sealed partial class Chunk
    {
        private static class Deserializer
        {
            private static void ValidateSignature(byte[] signature, out byte version)
            {
                if (signature[0] != 'E'
                    || signature[1] != 'V'
                    || signature[2] != 'C')
                {
                    throw new ChunkDeserializationException(
                        "Expected signature 'E' 'V' 'C', found " +
                        $"'{(char)signature[0]}' '{(char)signature[1]}' '{(char)signature[2]}' instead."
                    );
                }

                version = signature[3];
            }

            private static void ReadHeader(
                BinaryReader br, 
                out byte version, 
                out long timestamp,
                out ChunkFlags flags)
            {
                ValidateSignature(
                    br.ReadBytes(4),
                    out version
                );

                timestamp = br.ReadInt64();
                flags = (ChunkFlags)br.ReadInt32();
            }

            private static DynamicValue ReadConstValue(BinaryReader br)
            {
                var type = (DynamicValueType)br.ReadByte();

                return type switch
                {
                    DynamicValueType.Nil => Nil,
                    DynamicValueType.Number => br.ReadDouble(),
                    DynamicValueType.String => br.ReadString(),
                    DynamicValueType.Boolean => br.ReadBoolean(),
                    _ => throw new ChunkDeserializationException($"Unexpected constant value type '{type}'.")
                };
            }

            private static ChunkAttribute ReadAttribute(BinaryReader br)
            {
                var name = br.ReadString();
                var attribute = new ChunkAttribute(name);
                
                var valueCount = br.ReadInt32();
                for (var i = 0; i < valueCount; i++)
                {
                    attribute.Values.Add(ReadConstValue(br));
                }

                var propertyCount = br.ReadInt32();
                for (var i = 0; i < propertyCount; i++)
                {
                    attribute.Properties.Add(
                        br.ReadString(),
                        ReadConstValue(br)
                    );
                }

                return attribute;
            }

            private static string ReadName(BinaryReader br)
            {
                return br.ReadString();
            }
            
            public static Chunk Deserialize(Stream stream, out byte version, out long timestamp)
            {
                var chunk = new Chunk();
                
                using (var br = new BinaryReader(stream, Encoding.UTF8, true))
                {
                    ReadHeader(br, out version, out timestamp, out var flags);

                    if (flags.HasFlag(ChunkFlags.HasName))
                    {
                        chunk.Name = ReadName(br);
                    }

                    if (flags.HasFlag(ChunkFlags.HasParameters))
                    {
                        chunk.ParameterCount = br.ReadInt32();
                    }

                    if (flags.HasFlag(ChunkFlags.HasParameterInitializers))
                    {
                        var initializerCount = br.ReadInt32();

                        for (var i = 0; i < initializerCount; i++)
                        {
                            chunk._parameterInitializers.Add(
                                br.ReadInt32(),
                                ReadConstValue(br)
                            );
                        }
                    }

                    if (flags.HasFlag(ChunkFlags.HasLocals))
                    {
                        chunk.LocalCount = br.ReadInt32();
                    }

                    if (flags.HasFlag(ChunkFlags.HasLabels))
                    {
                        var labelCount = br.ReadInt32();
                        for (var i = 0; i < labelCount; i++)
                        {
                            chunk._labels.Add(br.ReadInt32());
                        }
                    }

                    if (flags.HasFlag(ChunkFlags.HasAttributes))
                    {
                        var attrCount = br.ReadInt32();

                        for (var i = 0; i < attrCount; i++)
                        {
                            chunk._attributes.Add(ReadAttribute(br));
                        }
                    }

                    if (flags.HasFlag(ChunkFlags.HasDebugInfo))
                    {
                        chunk.DebugDatabase.Deserialize(br);
                    }

                    var stringPoolLength = br.ReadInt32();
                    for (var i = 0; i < stringPoolLength; i++)
                    {
                        chunk.StringPool.FetchOrAdd(br.ReadString());
                    }

                    var codeAreaLength = br.ReadInt32();
                    var code = br.ReadBytes(codeAreaLength);

                    var md5sum = br.ReadBytes(16);
                    using (var md5 = MD5.Create())
                    {
                        var sum = md5.ComputeHash(code);

                        if (!md5sum.SequenceEqual(sum))
                        {
                            throw new ChunkDeserializationException("Code area checksum mismatch.");
                        }
                    }
                        
                    chunk._code.Write(code);
                    chunk._code.Seek(0, SeekOrigin.Begin);
                }

                return chunk;
            }
        }
    }
}