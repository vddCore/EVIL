namespace EVIL.VM.ExecutionEngine.Diagnostics;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Buffers.Binary;
using EVIL.Core.Common.TypeSystem;
using EVIL.VM.ExecutionEngine.Serialization;
using EVIL.VM.ExecutionEngine.TypeSystem;

public sealed partial class Chunk
{
    private static class Deserializer
    {
        private const ushort RequiredSectionFlag = 1;

        private readonly record struct FrameHeader(
            int SectionCount,
            uint FrameLength,
            long Timestamp,
            ChunkFlags Flags
        );

        private readonly record struct SectionRange(
            ushort Tag,
            ushort Flags,
            uint Offset,
            uint Length
        );

        private static void ValidateSignature(ReadOnlySpan<byte> signature, out byte version)
        {
            if (signature[0] != 'E'
                || signature[1] != 'V'
                || signature[2] != 'C'
                || signature[3] != 0x06)
            {
                throw new ChunkDeserializationException(
                    "Expected signature 'E' 'V' 'C' '\\x06', found " +
                    $"'{(char)signature[0]}' '{(char)signature[1]}' '{(char)signature[2]}' '\\x{signature[3]:X2}' instead."
                );
            }

            version = signature[4];
            if (version != FormatVersion)
            {
                throw new ChunkDeserializationException(
                    $"Unsupported chunk format version '{version}'. Expected version '{FormatVersion}'."
                );
            }
        }

        private static FrameHeader ReadHeader(ReadOnlySpan<byte> header)
        {
            if (header.Length != BinaryChunkLimits.HeaderSize)
            {
                throw new ChunkDeserializationException("Truncated v6 chunk header.");
            }

            ValidateSignature(header[..5], out _);

            if (header[5] != BinaryChunkLimits.HeaderSize)
            {
                throw new ChunkDeserializationException(
                    $"Invalid v6 header size '{header[5]}'."
                );
            }

            var sectionCount = BinaryPrimitives.ReadUInt16LittleEndian(header[6..8]);
            var frameLength = BinaryPrimitives.ReadUInt32LittleEndian(header[8..12]);
            var timestamp = BinaryPrimitives.ReadInt64LittleEndian(header[12..20]);
            var flagsValue = BinaryPrimitives.ReadUInt32LittleEndian(header[20..24]);
            var reserved = BinaryPrimitives.ReadUInt32LittleEndian(header[24..28]);

            if (sectionCount < 3 || sectionCount > BinaryChunkLimits.MaximumSectionCount)
            {
                throw new ChunkDeserializationException(
                    $"Invalid v6 section count '{sectionCount}'."
                );
            }

            var minimumFrameLength = (uint)(
                BinaryChunkLimits.HeaderSize
                + sectionCount * BinaryChunkLimits.DirectoryEntrySize
                + BinaryChunkLimits.DigestSize
            );
            if (frameLength < minimumFrameLength
                || frameLength > BinaryChunkLimits.MaximumFrameSize)
            {
                throw new ChunkDeserializationException(
                    $"Invalid v6 frame length '{frameLength}'."
                );
            }

            if ((flagsValue & ~BinaryChunkLimits.KnownChunkFlags) != 0)
            {
                throw new ChunkDeserializationException("Chunk header contains unknown flag bits.");
            }

            if (reserved != 0)
            {
                throw new ChunkDeserializationException("Chunk header reserved field must be zero.");
            }

            return new FrameHeader(
                sectionCount,
                frameLength,
                timestamp,
                (ChunkFlags)flagsValue
            );
        }

        private static ReadOnlyMemory<byte> ReadFrame(Stream stream, out byte version)
        {
            Span<byte> prefix = stackalloc byte[5];
            stream.ReadExactly(prefix);
            ValidateSignature(prefix, out version);

            var header = new byte[BinaryChunkLimits.HeaderSize];
            prefix.CopyTo(header);
            stream.ReadExactly(header.AsSpan(prefix.Length));

            var parsedHeader = ReadHeader(header);
            var frame = new byte[(int)parsedHeader.FrameLength];
            header.CopyTo(frame, 0);
            stream.ReadExactly(frame.AsSpan(BinaryChunkLimits.HeaderSize));
            return frame;
        }

        private static Dictionary<ushort, ReadOnlyMemory<byte>> ReadSections(
            ReadOnlyMemory<byte> frame,
            FrameHeader header)
        {
            var directoryLength = header.SectionCount * BinaryChunkLimits.DirectoryEntrySize;
            var payloadStart = BinaryChunkLimits.HeaderSize + directoryLength;
            var payloadEnd = frame.Length - BinaryChunkLimits.DigestSize;
            var directoryReader = new BinaryChunkReader(
                frame.Slice(BinaryChunkLimits.HeaderSize, directoryLength)
            );
            var ranges = new List<SectionRange>(header.SectionCount);
            var tags = new HashSet<ushort>();

            for (var i = 0; i < header.SectionCount; i++)
            {
                var range = new SectionRange(
                    directoryReader.ReadUInt16(),
                    directoryReader.ReadUInt16(),
                    directoryReader.ReadUInt32(),
                    directoryReader.ReadUInt32()
                );

                if (!tags.Add(range.Tag))
                {
                    throw new ChunkDeserializationException(
                        $"Duplicate chunk section tag '{range.Tag}'."
                    );
                }

                if ((range.Flags & ~RequiredSectionFlag) != 0)
                {
                    throw new ChunkDeserializationException(
                        $"Chunk section '{range.Tag}' contains unknown flag bits."
                    );
                }

                var isKnownTag = range.Tag is >= 1 and <= 5;
                if (isKnownTag && range.Flags != RequiredSectionFlag)
                {
                    throw new ChunkDeserializationException(
                        $"Known chunk section '{range.Tag}' must be marked required."
                    );
                }

                if (!isKnownTag && range.Flags == RequiredSectionFlag)
                {
                    throw new ChunkDeserializationException(
                        $"Unknown required chunk section '{range.Tag}'."
                    );
                }

                var rangeEnd = (ulong)range.Offset + range.Length;
                if (range.Offset < payloadStart || rangeEnd > (ulong)payloadEnd)
                {
                    throw new ChunkDeserializationException(
                        $"Chunk section '{range.Tag}' is outside its frame."
                    );
                }

                ranges.Add(range);
            }

            directoryReader.EnsureFullyConsumed("directory");

            var currentOffset = payloadStart;
            foreach (var range in ranges
                         .OrderBy(x => x.Offset)
                         .ThenBy(x => x.Length))
            {
                if (range.Offset != currentOffset)
                {
                    throw new ChunkDeserializationException(
                        "Chunk section ranges overlap or leave unassigned frame bytes."
                    );
                }

                currentOffset = checked(currentOffset + (int)range.Length);
            }

            if (currentOffset != payloadEnd)
            {
                throw new ChunkDeserializationException(
                    "Chunk section ranges do not cover the complete frame payload."
                );
            }

            var sections = new Dictionary<ushort, ReadOnlyMemory<byte>>(header.SectionCount);
            foreach (var range in ranges)
            {
                sections.Add(
                    range.Tag,
                    frame.Slice((int)range.Offset, (int)range.Length)
                );
            }

            RequireSection(sections, 1);
            RequireSection(sections, 2);
            RequireSection(sections, 3);
            return sections;
        }

        private static void RequireSection(
            IReadOnlyDictionary<ushort, ReadOnlyMemory<byte>> sections,
            ushort tag)
        {
            if (!sections.ContainsKey(tag))
            {
                throw new ChunkDeserializationException(
                    $"Required chunk section '{tag}' is missing."
                );
            }
        }

        private static DynamicValue ReadConstValue(BinaryChunkReader reader)
        {
            var type = (DynamicValueType)reader.ReadByte();
            return type switch
            {
                DynamicValueType.Nil => DynamicValue.Nil,
                DynamicValueType.Number => new DynamicValue(reader.ReadDouble()),
                DynamicValueType.String => new DynamicValue(reader.ReadString()),
                DynamicValueType.Boolean => new DynamicValue(reader.ReadBoolean()),
                DynamicValueType.TypeCode => ReadTypeCode(reader),
                _ => throw new ChunkDeserializationException(
                    $"Unexpected constant value type '{type}'."
                )
            };
        }

        private static DynamicValue ReadTypeCode(BinaryChunkReader reader)
        {
            var typeCode = (DynamicValueType)reader.ReadByte();
            if (!Enum.IsDefined(typeof(DynamicValueType), typeCode))
            {
                throw new ChunkDeserializationException(
                    $"Unexpected type-code value '{typeCode}'."
                );
            }

            return new DynamicValue(typeCode);
        }

        private static Chunk ReadMetadata(
            ReadOnlyMemory<byte> data,
            ChunkFlags flags,
            Chunk? parent,
            int codeLength)
        {
            var reader = new BinaryChunkReader(data);
            var name = reader.ReadString();
            var ownerId = reader.ReadGuid();
            var chunk = new Chunk(name)
            {
                ClosureOwnerId = ownerId,
                Parent = parent,
                IsSelfAware = flags.HasFlag(ChunkFlags.IsSelfAware),
                IsSpecialName = flags.HasFlag(ChunkFlags.IsSpecialName),
                MayThrow = flags.HasFlag(ChunkFlags.MayThrow)
            };

            var parameterCount = reader.ReadCount();
            var localCount = reader.ReadCount();
            chunk.ParameterCount = parameterCount;
            chunk.LocalCount = localCount;

            var initializerCount = reader.ReadCount(5);
            for (var i = 0; i < initializerCount; i++)
            {
                var parameterId = reader.ReadInt32();
                if (parameterId < 0 || parameterId >= parameterCount)
                {
                    throw new ChunkDeserializationException(
                        $"Parameter initializer references invalid parameter '{parameterId}'."
                    );
                }

                chunk._parameterInitializers.Add(parameterId, ReadConstValue(reader));
            }

            var closureCount = reader.ReadCount(33);
            for (var i = 0; i < closureCount; i++)
            {
                var nestingLevel = reader.ReadInt32();
                var enclosedId = reader.ReadInt32();
                var enclosedFunctionName = reader.ReadString();
                var closureType = (ClosureType)reader.ReadInt32();
                var isSharedScope = reader.ReadBoolean();
                var enclosedOwnerId = reader.ReadGuid();

                if (nestingLevel < 0 || enclosedId < 0
                    || !Enum.IsDefined(typeof(ClosureType), closureType))
                {
                    throw new ChunkDeserializationException("Invalid closure metadata.");
                }

                chunk._closures.Add(
                    new ClosureInfo(
                        nestingLevel,
                        enclosedId,
                        enclosedFunctionName,
                        closureType,
                        isSharedScope,
                        enclosedOwnerId
                    )
                );
            }

            var contextCount = reader.ReadCount(12);
            for (var i = 0; i < contextCount; i++)
            {
                var contextName = reader.ReadString();
                var context = new ClosureContext(reader.ReadString());
                var valueCount = reader.ReadCount(5);
                for (var j = 0; j < valueCount; j++)
                {
                    var valueId = reader.ReadInt32();
                    if (valueId < 0)
                    {
                        throw new ChunkDeserializationException(
                            $"Closure context contains invalid value ID '{valueId}'."
                        );
                    }

                    context.Values.Add(valueId, ReadConstValue(reader));
                }

                chunk._closureContexts.Add(contextName, context);
            }

            var labelCount = reader.ReadCount(sizeof(int));
            for (var i = 0; i < labelCount; i++)
            {
                var label = reader.ReadInt32();
                ValidateCodeAddress(label, codeLength, "label");
                chunk._labels.Add(label);
            }

            var attributeCount = reader.ReadCount(12);
            for (var i = 0; i < attributeCount; i++)
            {
                var attribute = new ChunkAttribute(reader.ReadString());
                var valueCount = reader.ReadCount(1);
                for (var j = 0; j < valueCount; j++)
                {
                    attribute.Values.Add(ReadConstValue(reader));
                }

                var propertyCount = reader.ReadCount(5);
                for (var j = 0; j < propertyCount; j++)
                {
                    attribute.Properties.Add(reader.ReadString(), ReadConstValue(reader));
                }

                chunk._attributes.Add(attribute);
            }

            var regionCount = reader.ReadCount(12);
            for (var i = 0; i < regionCount; i++)
            {
                var region = new BlockProtectionInfo
                {
                    BodyStartAddress = reader.ReadInt32(),
                    BodyEndAddress = reader.ReadInt32(),
                    HandlerAddress = reader.ReadInt32()
                };

                ValidateCodeAddress(region.BodyStartAddress, codeLength, "protected-region start");
                ValidateCodeAddress(region.BodyEndAddress, codeLength, "protected-region end");
                ValidateCodeAddress(region.HandlerAddress, codeLength, "protected-region handler");
                if (region.BodyStartAddress > region.BodyEndAddress)
                {
                    throw new ChunkDeserializationException("Invalid protected-region range.");
                }

                chunk._protectedRegions.Add(region);
            }

            reader.EnsureFullyConsumed("metadata");
            return chunk;
        }

        private static void ValidateCodeAddress(int address, int codeLength, string description)
        {
            if (address < 0 || address > codeLength)
            {
                throw new ChunkDeserializationException(
                    $"Invalid {description} code address '{address}'."
                );
            }
        }

        private static void ReadStringPool(ReadOnlyMemory<byte> data, Chunk chunk)
        {
            var reader = new BinaryChunkReader(data);
            var count = reader.ReadCount(sizeof(uint));
            var seen = new HashSet<string>();
            for (var i = 0; i < count; i++)
            {
                var value = reader.ReadString();
                if (!seen.Add(value))
                {
                    throw new ChunkDeserializationException("String pool contains duplicate values.");
                }

                chunk.StringPool.FetchOrAdd(value);
            }

            reader.EnsureFullyConsumed("string pool");
        }

        private static void ReadChildren(
            ReadOnlyMemory<byte> data,
            Chunk chunk,
            int depth)
        {
            var reader = new BinaryChunkReader(data);
            var childCount = reader.ReadCount(BinaryChunkLimits.MinimumFrameSize);
            for (var i = 0; i < childCount; i++)
            {
                if (reader.Remaining < BinaryChunkLimits.HeaderSize)
                {
                    throw new ChunkDeserializationException("Truncated child chunk frame.");
                }

                var childLength = reader.PeekUInt32(8);
                if (childLength > BinaryChunkLimits.MaximumFrameSize
                    || childLength > (uint)reader.Remaining
                    || childLength < BinaryChunkLimits.MinimumFrameSize)
                {
                    throw new ChunkDeserializationException("Invalid child chunk frame length.");
                }

                var childFrame = reader.ReadMemory((int)childLength);
                var child = DecodeFrame(childFrame, depth + 1, chunk, out _, out _);
                chunk._subChunks.Add(child);
            }

            var lookupCount = reader.ReadCount(8);
            for (var i = 0; i < lookupCount; i++)
            {
                var name = reader.ReadString();
                var id = reader.ReadInt32();
                if (id < 0 || id >= chunk._subChunks.Count
                    || chunk._subChunks[id].Name != name)
                {
                    throw new ChunkDeserializationException(
                        $"Named child lookup '{name}' references invalid child '{id}'."
                    );
                }

                chunk._namedSubChunkLookup.Add(name, id);
            }

            reader.EnsureFullyConsumed("children");
        }

        private static Chunk DecodeFrame(
            ReadOnlyMemory<byte> frame,
            int depth,
            Chunk? parent,
            out byte version,
            out long timestamp)
        {
            if (depth >= BinaryChunkLimits.MaximumNestingDepth)
            {
                throw new ChunkDeserializationException("Chunk nesting exceeds the v6 limit.");
            }

            if (frame.Length < BinaryChunkLimits.HeaderSize)
            {
                throw new ChunkDeserializationException("Truncated v6 chunk frame.");
            }

            var header = ReadHeader(frame.Span[..BinaryChunkLimits.HeaderSize]);
            version = FormatVersion;
            timestamp = header.Timestamp;

            if (header.FrameLength != frame.Length)
            {
                throw new ChunkDeserializationException(
                    "Chunk frame length does not match the bounded frame bytes."
                );
            }

            var digestOffset = frame.Length - BinaryChunkLimits.DigestSize;
            var expectedDigest = SHA256.HashData(frame.Span[..digestOffset]);
            if (!CryptographicOperations.FixedTimeEquals(
                    expectedDigest,
                    frame.Span[digestOffset..]
                ))
            {
                throw new ChunkDeserializationException("Chunk frame checksum mismatch.");
            }

            var sections = ReadSections(frame, header);
            var hasDebugSection = sections.ContainsKey(4);
            var hasChildrenSection = sections.ContainsKey(5);
            if (header.Flags.HasFlag(ChunkFlags.HasDebugInfo) != hasDebugSection
                || header.Flags.HasFlag(ChunkFlags.HasSubChunks) != hasChildrenSection)
            {
                throw new ChunkDeserializationException(
                    "Chunk flags disagree with the debug or child section directory."
                );
            }

            var code = sections[3];
            var chunk = ReadMetadata(sections[1], header.Flags, parent, code.Length);
            chunk._code.Write(code.Span);
            chunk._code.Position = 0;
            ReadStringPool(sections[2], chunk);

            if (hasDebugSection)
            {
                chunk.DebugDatabase.Deserialize(
                    new BinaryChunkReader(sections[4]),
                    chunk.ParameterCount,
                    chunk.LocalCount,
                    code.Length
                );
            }

            if (hasChildrenSection)
            {
                ReadChildren(sections[5], chunk, depth);
            }

            if (chunk.Flags != header.Flags)
            {
                throw new ChunkDeserializationException(
                    "Chunk flags disagree with decoded chunk metadata."
                );
            }

            return chunk;
        }

        public static Chunk Deserialize(Stream stream, out byte version, out long timestamp)
        {
            try
            {
                var frame = ReadFrame(stream, out version);
                return DecodeFrame(frame, 0, null, out version, out timestamp);
            }
            catch (ChunkDeserializationException)
            {
                throw;
            }
            catch (Exception exception) when (
                exception is IOException
                or ArgumentException
                or OverflowException
                or InvalidOperationException
            )
            {
                throw new ChunkDeserializationException(
                    $"Malformed chunk data: {exception.Message}"
                );
            }
        }
    }
}