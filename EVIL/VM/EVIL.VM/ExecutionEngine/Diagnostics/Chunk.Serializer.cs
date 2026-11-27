namespace EVIL.VM.ExecutionEngine.Diagnostics;

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using EVIL.Core.Common.TypeSystem;
using EVIL.VM.ExecutionEngine.Serialization;
using EVIL.VM.ExecutionEngine.TypeSystem;

public sealed partial class Chunk
{
    private class Serializer
    {
        private const ushort MetadataSection = 1;
        private const ushort StringPoolSection = 2;
        private const ushort CodeSection = 3;
        private const ushort DebugSection = 4;
        private const ushort ChildrenSection = 5;
        private const ushort RequiredSection = 1;

        private static readonly Encoding Utf8 = new UTF8Encoding(false, true);

        private readonly Chunk _chunk;

        public Serializer(Chunk chunk)
        {
            _chunk = chunk;
        }

        private static void WriteCount(BinaryWriter bw, int count)
        {
            if (count < 0 || count > BinaryChunkLimits.MaximumItemCount)
            {
                throw new InvalidDataException($"Chunk item count '{count}' exceeds the v6 limit.");
            }

            bw.Write((uint)count);
        }

        private static void WriteString(BinaryWriter bw, string value)
        {
            var bytes = Utf8.GetBytes(value);
            if (bytes.Length > BinaryChunkLimits.MaximumStringSize)
            {
                throw new InvalidDataException(
                    $"Chunk string length '{bytes.Length}' exceeds the v6 limit."
                );
            }

            bw.Write((uint)bytes.Length);
            bw.Write(bytes);
        }

        private static void WriteGuid(BinaryWriter bw, Guid value)
            => bw.Write(value.ToByteArray(bigEndian: true));

        private static void WriteConstValue(BinaryWriter bw, DynamicValue value)
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
                    WriteString(bw, value.String!);
                    break;

                case DynamicValueType.Boolean:
                    bw.Write(value.Boolean);
                    break;

                case DynamicValueType.TypeCode:
                    bw.Write((byte)value.TypeCode);
                    break;

                default:
                    throw new InvalidOperationException("Invalid constant value type.");
            }
        }

        private static BinaryChunkSection CreateSection(ushort tag, Action<BinaryWriter> writeSection)
        {
            using var stream = new MemoryStream();
            using (var bw = new BinaryWriter(stream, Utf8, true))
            {
                writeSection(bw);
            }

            var data = stream.ToArray();
            if (data.Length > BinaryChunkLimits.MaximumFrameSize)
            {
                throw new InvalidDataException($"Chunk section '{tag}' exceeds the v6 limit.");
            }

            return new BinaryChunkSection(tag, data);
        }

        private void WriteMetadata(BinaryWriter bw)
        {
            WriteString(bw, _chunk.Name);
            WriteGuid(bw, _chunk.ClosureOwnerId);
            WriteCount(bw, _chunk.ParameterCount);
            WriteCount(bw, _chunk.LocalCount);

            WriteCount(bw, _chunk.ParameterInitializers.Count);
            foreach (var (parameterId, value) in _chunk.ParameterInitializers)
            {
                bw.Write(parameterId);
                WriteConstValue(bw, value);
            }

            WriteCount(bw, _chunk.Closures.Count);
            foreach (var closure in _chunk.Closures)
            {
                bw.Write(closure.NestingLevel);
                bw.Write(closure.EnclosedId);
                WriteString(bw, closure.EnclosedFunctionName);
                bw.Write((int)closure.Type);
                bw.Write(closure.IsSharedScope);
                WriteGuid(bw, closure.EnclosedOwnerId);
            }

            WriteCount(bw, _chunk.ClosureContexts.Count);
            foreach (var (name, context) in _chunk.ClosureContexts)
            {
                WriteString(bw, name);
                WriteString(bw, context.EnclosedFunctionName);
                
                WriteCount(bw, context.Values.Count);
                foreach (var (id, value) in context.Values)
                {
                    bw.Write(id);
                    WriteConstValue(bw, value);
                }
            }

            WriteCount(bw, _chunk.Labels.Count);
            foreach (var label in _chunk.Labels)
            {
                bw.Write(label);
            }

            WriteCount(bw, _chunk.Attributes.Count);
            foreach (var attribute in _chunk.Attributes)
            {
                WriteString(bw, attribute.Name);
                WriteCount(bw, attribute.Values.Count);
                foreach (var value in attribute.Values)
                {
                    WriteConstValue(bw, value);
                }

                WriteCount(bw, attribute.Properties.Count);
                foreach (var (key, value) in attribute.Properties)
                {
                    WriteString(bw, key);
                    WriteConstValue(bw, value);
                }
            }

            WriteCount(bw, _chunk.ProtectedRegions.Count);
            foreach (var region in _chunk.ProtectedRegions)
            {
                bw.Write(region.BodyStartAddress);
                bw.Write(region.BodyEndAddress);
                bw.Write(region.HandlerAddress);
            }
        }

        private void WriteStringPool(BinaryWriter bw)
        {
            var strings = _chunk.StringPool.ToArray();
            WriteCount(bw, strings.Length);
            foreach (var value in strings)
            {
                WriteString(bw, value);
            }
        }

        private void WriteChildren(BinaryWriter bw, int depth)
        {
            WriteCount(bw, _chunk.SubChunks.Count);
            foreach (var subChunk in _chunk.SubChunks)
            {
                subChunk.BackpatchLabels();
                subChunk._serializer.WriteFrame(bw.BaseStream, depth + 1);
            }

            WriteCount(bw, _chunk.NamedSubChunkLookup.Count);
            foreach (var (name, id) in _chunk.NamedSubChunkLookup)
            {
                WriteString(bw, name);
                bw.Write(id);
            }
        }

        private List<BinaryChunkSection> BuildSections(int depth)
        {
            var sections = new List<BinaryChunkSection>
            {
                CreateSection(MetadataSection, WriteMetadata),
                CreateSection(StringPoolSection, WriteStringPool),
                CreateSection(CodeSection, bw => bw.Write(_chunk.Code))
            };

            if (_chunk.Flags.HasFlag(ChunkFlags.HasDebugInfo))
            {
                sections.Add(
                    CreateSection(
                        DebugSection,
                        bw => _chunk.DebugDatabase.Serialize(bw, WriteString, WriteCount)
                    )
                );
            }

            if (_chunk.Flags.HasFlag(ChunkFlags.HasSubChunks))
            {
                sections.Add(CreateSection(ChildrenSection, bw => WriteChildren(bw, depth)));
            }

            return sections;
        }

        private void WriteFrame(Stream stream, int depth)
        {
            if (depth >= BinaryChunkLimits.MaximumNestingDepth)
            {
                throw new InvalidDataException("Chunk nesting exceeds the v6 limit.");
            }

            var sections = BuildSections(depth);
            if (sections.Count > ushort.MaxValue)
            {
                throw new InvalidDataException("Chunk section count exceeds the v6 limit.");
            }

            long payloadLength = 0;
            foreach (var section in sections)
            {
                payloadLength = checked(payloadLength + section.Data.Length);
            }

            var bodyLength = checked(
                BinaryChunkLimits.HeaderSize
                + sections.Count * BinaryChunkLimits.DirectoryEntrySize
                + payloadLength
            );
            var frameLength = checked(bodyLength + BinaryChunkLimits.DigestSize);
            if (frameLength > BinaryChunkLimits.MaximumFrameSize)
            {
                throw new InvalidDataException("Chunk frame exceeds the v6 size limit.");
            }

            using var frame = new MemoryStream((int)bodyLength);
            using (var bw = new BinaryWriter(frame, Utf8, true))
            {
                bw.Write(new[] { (byte)'E', (byte)'V', (byte)'C', (byte)0x06 });
                bw.Write(FormatVersion);
                bw.Write((byte)BinaryChunkLimits.HeaderSize);
                bw.Write((ushort)sections.Count);
                bw.Write((uint)frameLength);
                bw.Write(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                bw.Write((uint)_chunk.Flags);
                bw.Write(0u);

                var sectionOffset = checked(
                    BinaryChunkLimits.HeaderSize
                    + sections.Count * BinaryChunkLimits.DirectoryEntrySize
                );
                foreach (var section in sections)
                {
                    bw.Write(section.Tag);
                    bw.Write(RequiredSection);
                    bw.Write((uint)sectionOffset);
                    bw.Write((uint)section.Data.Length);
                    sectionOffset = checked(sectionOffset + section.Data.Length);
                }

                foreach (var section in sections)
                {
                    bw.Write(section.Data);
                }
            }

            var frameBytes = frame.ToArray();
            stream.Write(frameBytes);
            stream.Write(SHA256.HashData(frameBytes));
        }

        public void Write(Stream stream)
        {
            _chunk.BackpatchLabels();
            WriteFrame(stream, 0);
        }
    }
}