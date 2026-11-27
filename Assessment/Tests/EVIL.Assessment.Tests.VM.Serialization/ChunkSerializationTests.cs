namespace EVIL.Assessment.Tests.VM.Serialization;

using System;
using System.Buffers.Binary;
using System.IO;
using System.Security.Cryptography;
using EVIL.Core.Grammar;
using EVIL.VM.ExecutionEngine;
using EVIL.VM.ExecutionEngine.Diagnostics;
using EVIL.VM.ExecutionEngine.Diagnostics.Disassembly;
using EVIL.VM.TranslationEngine;
using NUnit.Framework;

public class ChunkSerializationTests
{
    private readonly Parser _parser = new();
    private readonly Compiler _compiler = new();

    [SetUp]
    public void Setup()
    {
    }

    [TearDown]
    public void Teardown()
    {
    }

    [Test]
    public void ChunkSerializeDeserialize()
    {
        var source = "#[jp2;gmd(2137)] fn main(a, b = 123, c = \"brokuł\") { " +
                     "  rw val f = 33.3, " +
                     "      s = \"teststring\";" +
                     "" +
                     "  for (rw val i = 0; i < 10; i++)" +
                     "      f += 2;" +
                     "" +
                     "  val f1 = fn(a, b) -> a + b * f;" +
                     "" +
                     "  if (false) -> 10; " +
                     "  elif (false_too_just_nil) -> 20;" +
                     "  elif (0) -> 30;" +
                     "  else -> \"haha you will never get a 4\";" +
                     "" +
                     "  ret 2+2;" +
                     "}";   

        var program = _parser.Parse(source);
        var rootChunk = _compiler.Compile(program);
        var originalChunk = rootChunk;

        using (var ms = new MemoryStream())
        {
            originalChunk.Serialize(ms);
            ms.Seek(0, SeekOrigin.Begin);
            var chunk = Chunk.Deserialize(ms, out var version, out _);

            Assert.That(version, Is.EqualTo(Chunk.FormatVersion));
            Assert.That(chunk, Is.EqualTo(originalChunk));
        }
    }

    [Test]
    public void ChunkSerialize_UsesV6SectionDirectoryAndWholeFrameDigest()
    {
        var chunk = new Chunk("directory");
        using var stream = new MemoryStream();
        chunk.Serialize(stream);

        var frame = stream.ToArray();
        var sectionCount = BinaryPrimitives.ReadUInt16LittleEndian(frame.AsSpan(6, 2));
        var frameLength = BinaryPrimitives.ReadUInt32LittleEndian(frame.AsSpan(8, 4));
        var sectionStart = 28 + sectionCount * 12;
        var currentOffset = sectionStart;
        var tags = new ushort[sectionCount];

        using (Assert.EnterMultipleScope())
        {
            Assert.That(frame.AsSpan(0, 4).ToArray(), Is.EqualTo(new byte[] { 0x45, 0x56, 0x43, 0x06 }));
            Assert.That(frame[4], Is.EqualTo(Chunk.FormatVersion));
            Assert.That(frame[5], Is.EqualTo(28));
            Assert.That(frameLength, Is.EqualTo((uint)frame.Length));
            Assert.That(sectionCount, Is.EqualTo(3));
        }

        for (var i = 0; i < sectionCount; i++)
        {
            var entryOffset = 28 + i * 12;
            var tag = BinaryPrimitives.ReadUInt16LittleEndian(frame.AsSpan(entryOffset, 2));
            var flags = BinaryPrimitives.ReadUInt16LittleEndian(frame.AsSpan(entryOffset + 2, 2));
            var sectionOffset = BinaryPrimitives.ReadUInt32LittleEndian(frame.AsSpan(entryOffset + 4, 4));
            var sectionLength = BinaryPrimitives.ReadUInt32LittleEndian(frame.AsSpan(entryOffset + 8, 4));

            tags[i] = tag;
            Assert.That(flags, Is.EqualTo(1));
            Assert.That(sectionOffset, Is.EqualTo((uint)currentOffset));
            currentOffset += checked((int)sectionLength);
        }

        Assert.That(tags, Is.EquivalentTo(new ushort[] { 1, 2, 3 }));
        Assert.That(currentOffset, Is.EqualTo(frame.Length - 32));
        Assert.That(
            frame.AsSpan(frame.Length - 32, 32).ToArray(),
            Is.EqualTo(SHA256.HashData(frame.AsSpan(0, frame.Length - 32)))
        );
    }

    [Test]
    public void ChunkDeserialize_RejectsTruncatedFrame()
    {
        var frame = Serialize(new Chunk("truncated"));
        using var stream = new MemoryStream(frame, 0, frame.Length - 1, false, true);

        Assert.Throws<ChunkDeserializationException>(
            () => Chunk.Deserialize(stream, out _, out _)
        );
    }

    [Test]
    public void ChunkDeserialize_RejectsFrameDigestMismatch()
    {
        var frame = Serialize(new Chunk("digest"));
        frame[^1] ^= 0x80;

        var exception = Assert.Throws<ChunkDeserializationException>(
            () => Deserialize(frame)
        );

        Assert.That(exception!.Message, Does.Contain("checksum mismatch"));
    }

    [Test]
    public void ChunkDeserialize_RejectsOverlappingSectionRanges()
    {
        var frame = Serialize(new Chunk("overlap"));
        var metadataOffset = BinaryPrimitives.ReadUInt32LittleEndian(frame.AsSpan(32, 4));
        BinaryPrimitives.WriteUInt32LittleEndian(frame.AsSpan(44, 4), metadataOffset);
        RecomputeDigest(frame);

        var exception = Assert.Throws<ChunkDeserializationException>(
            () => Deserialize(frame)
        );

        Assert.That(exception!.Message, Does.Contain("overlap"));
    }

    [Test]
    public void ChunkDeserialize_RejectsUnknownHeaderFlags()
    {
        var frame = Serialize(new Chunk("flags"));
        BinaryPrimitives.WriteUInt32LittleEndian(frame.AsSpan(20, 4), 1u << 31);
        RecomputeDigest(frame);

        var exception = Assert.Throws<ChunkDeserializationException>(
            () => Deserialize(frame)
        );

        Assert.That(exception!.Message, Does.Contain("unknown flag bits"));
    }

    [Test]
    public void ChunkDeserialize_RejectsExcessiveSectionCounts()
    {
        var frame = Serialize(new Chunk("count"));
        var sectionCount = BinaryPrimitives.ReadUInt16LittleEndian(frame.AsSpan(6, 2));
        var stringPoolOffset = 0u;
        for (var i = 0; i < sectionCount; i++)
        {
            var entryOffset = 28 + i * 12;
            if (BinaryPrimitives.ReadUInt16LittleEndian(frame.AsSpan(entryOffset, 2)) == 2)
            {
                stringPoolOffset = BinaryPrimitives.ReadUInt32LittleEndian(
                    frame.AsSpan(entryOffset + 4, 4)
                );
                break;
            }
        }

        BinaryPrimitives.WriteUInt32LittleEndian(frame.AsSpan((int)stringPoolOffset, 4), uint.MaxValue);
        RecomputeDigest(frame);

        var exception = Assert.Throws<ChunkDeserializationException>(
            () => Deserialize(frame)
        );

        Assert.That(exception!.Message, Does.Contain("item count"));
    }

    [Test]
    public void ChunkDeserialize_RejectsDuplicateSectionTags()
    {
        var frame = Serialize(new Chunk("duplicate-tag"));
        var stringPoolEntry = FindSectionEntry(frame, 2);
        BinaryPrimitives.WriteUInt16LittleEndian(frame.AsSpan(stringPoolEntry, 2), 1);
        RecomputeDigest(frame);

        var exception = Assert.Throws<ChunkDeserializationException>(
            () => Deserialize(frame)
        );

        Assert.That(exception!.Message, Does.Contain("Duplicate chunk section tag"));
    }

    [Test]
    public void ChunkDeserialize_RejectsMissingRequiredSection()
    {
        var frame = Serialize(new Chunk("missing-section"));
        var metadataEntry = FindSectionEntry(frame, 1);
        BinaryPrimitives.WriteUInt16LittleEndian(frame.AsSpan(metadataEntry, 2), 6);
        BinaryPrimitives.WriteUInt16LittleEndian(frame.AsSpan(metadataEntry + 2, 2), 0);
        RecomputeDigest(frame);

        var exception = Assert.Throws<ChunkDeserializationException>(
            () => Deserialize(frame)
        );

        Assert.That(exception!.Message, Does.Contain("Required chunk section '1' is missing"));
    }

    [Test]
    public void ChunkDeserialize_RejectsUnknownRequiredSection()
    {
        var frame = Serialize(new Chunk("unknown-required"));
        var metadataEntry = FindSectionEntry(frame, 1);
        BinaryPrimitives.WriteUInt16LittleEndian(frame.AsSpan(metadataEntry, 2), 6);
        RecomputeDigest(frame);

        var exception = Assert.Throws<ChunkDeserializationException>(
            () => Deserialize(frame)
        );

        Assert.That(exception!.Message, Does.Contain("Unknown required chunk section '6'"));
    }

    [Test]
    public void ChunkDeserialize_RejectsOutOfFrameSectionRanges()
    {
        var frame = Serialize(new Chunk("out-of-frame"));
        var codeEntry = FindSectionEntry(frame, 3);
        BinaryPrimitives.WriteUInt32LittleEndian(frame.AsSpan(codeEntry + 4, 4), uint.MaxValue);
        RecomputeDigest(frame);

        Assert.Throws<ChunkDeserializationException>(() => Deserialize(frame));
    }

    [Test]
    public void ChunkDeserialize_RejectsTrailingBytesInKnownSections()
    {
        var frame = Serialize(new Chunk("section-trailing"));
        var metadataEntry = FindSectionEntry(frame, 1);
        var metadataOffset = (int)BinaryPrimitives.ReadUInt32LittleEndian(
            frame.AsSpan(metadataEntry + 4, 4)
        );
        var metadataLength = (int)BinaryPrimitives.ReadUInt32LittleEndian(
            frame.AsSpan(metadataEntry + 8, 4)
        );
        var sectionEnd = metadataOffset + metadataLength;
        var payloadEnd = frame.Length - 32;
        var extendedFrame = new byte[frame.Length + 1];

        Array.Copy(frame, 0, extendedFrame, 0, sectionEnd);
        extendedFrame[sectionEnd] = 0xA5;
        Array.Copy(frame, sectionEnd, extendedFrame, sectionEnd + 1, payloadEnd - sectionEnd);
        BinaryPrimitives.WriteUInt32LittleEndian(
            extendedFrame.AsSpan(8, 4),
            checked((uint)extendedFrame.Length)
        );
        BinaryPrimitives.WriteUInt32LittleEndian(
            extendedFrame.AsSpan(metadataEntry + 8, 4),
            checked((uint)(metadataLength + 1))
        );

        var sectionCount = BinaryPrimitives.ReadUInt16LittleEndian(extendedFrame.AsSpan(6, 2));
        for (var i = 0; i < sectionCount; i++)
        {
            var entryOffset = 28 + i * 12;
            if (entryOffset != metadataEntry)
            {
                var offset = BinaryPrimitives.ReadUInt32LittleEndian(
                    extendedFrame.AsSpan(entryOffset + 4, 4)
                );
                if (offset >= sectionEnd)
                {
                    BinaryPrimitives.WriteUInt32LittleEndian(
                        extendedFrame.AsSpan(entryOffset + 4, 4),
                        checked(offset + 1)
                    );
                }
            }
        }

        RecomputeDigest(extendedFrame);

        var exception = Assert.Throws<ChunkDeserializationException>(
            () => Deserialize(extendedFrame)
        );

        Assert.That(exception!.Message, Does.Contain("metadata section contains"));
    }

    [Test]
    public void ChunkDeserialize_RejectsInvalidMetadataReferences()
    {
        var chunk = new Chunk("invalid-reference");
        chunk.AllocateParameter();
        chunk.InitializeParameter(0, new EVIL.VM.ExecutionEngine.TypeSystem.DynamicValue(1.0));
        var frame = Serialize(chunk);
        var metadataEntry = FindSectionEntry(frame, 1);
        var metadataOffset = (int)BinaryPrimitives.ReadUInt32LittleEndian(
            frame.AsSpan(metadataEntry + 4, 4)
        );
        var nameLength = (int)BinaryPrimitives.ReadUInt32LittleEndian(
            frame.AsSpan(metadataOffset, 4)
        );
        var initializerIdOffset = metadataOffset + 4 + nameLength + 16 + 4 + 4 + 4;
        BinaryPrimitives.WriteInt32LittleEndian(frame.AsSpan(initializerIdOffset, 4), 1);
        RecomputeDigest(frame);

        var exception = Assert.Throws<ChunkDeserializationException>(
            () => Deserialize(frame)
        );

        Assert.That(exception!.Message, Does.Contain("invalid parameter"));
    }

    [Test]
    public void ChunkReader_ReadsMultiByteOperandsAsLittleEndian()
    {
        var reader = new CodeReader([
            0x78, 0x56, 0x34, 0x12,
            0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F
        ]);

        Assert.That(reader.ReadInt32(), Is.EqualTo(0x12345678));
        Assert.That(reader.ReadInt64(), Is.EqualTo(0x0102030405060708));
        Assert.That(reader.ReadDouble(), Is.EqualTo(1.0));
        Assert.That(reader.IP, Is.EqualTo(reader.Length));
    }

    [Test]
    public void ChunkSerializeDeserialize_PreservesArrayDeconstructionOperand()
    {
        var originalChunk = _compiler.Compile("val [first, second] <- [1, 2];");

        using var stream = new MemoryStream();
        originalChunk.Serialize(stream);
        stream.Position = 0;
        var chunk = Chunk.Deserialize(stream, out var version, out _);

        Assert.That(version, Is.EqualTo(Chunk.FormatVersion));
        Assert.That(chunk, Is.EqualTo(originalChunk));

        Instruction? deconstruction = null;
        foreach (var instruction in chunk.Disassemble().Instructions)
        {
            if (instruction.OpCode == OpCode.ARRDEC)
            {
                deconstruction = instruction;
                break;
            }
        }

        Assert.That(deconstruction, Is.Not.Null);
        Assert.That(deconstruction!.Operands, Has.Count.EqualTo(1));
        Assert.That(deconstruction.Operands[0].Kind, Is.EqualTo(OperandKind.ImmediateInt32));
        Assert.That(deconstruction.Operands[0].RawValue, Is.EqualTo(2));
    }

    [Test]
    public void ChunkSerializeDeserialize_PreservesTableFieldKeys()
    {
        var originalChunk = _compiler.Compile(
            "val {name, points: score, ['total points']: total} <- source;"
        );

        using var stream = new MemoryStream();
        originalChunk.Serialize(stream);
        stream.Position = 0;
        var chunk = Chunk.Deserialize(stream, out var version, out _);

        Assert.That(version, Is.EqualTo(Chunk.FormatVersion));
        Assert.That(chunk, Is.EqualTo(originalChunk));

        var expectedKeys = new[] { "name", "points", "total points" };
        var keyIndex = 0;
        foreach (var instruction in chunk.Disassemble().Instructions)
        {
            if (instruction.OpCode != OpCode.TABGET)
            {
                continue;
            }

            Assert.That(instruction.Operands, Has.Count.EqualTo(1));
            Assert.That(instruction.Operands[0].Kind, Is.EqualTo(OperandKind.StringPoolIndex));
            Assert.That(instruction.Operands[0].ResolvedSymbol, Is.EqualTo(expectedKeys[keyIndex]));
            keyIndex++;
        }

        Assert.That(keyIndex, Is.EqualTo(expectedKeys.Length));
    }

    [Test]
    public void ChunkSerializeDeserialize_PreservesProtectedRegions()
    {
        var originalChunk = new Compiler().Compile("try { throw 1; } catch (e) { }");

        using var stream = new MemoryStream();
        originalChunk.Serialize(stream);
        stream.Position = 0;
        var deserializedChunk = Chunk.Deserialize(stream, out var version, out _);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(version, Is.EqualTo(Chunk.FormatVersion));
            Assert.That(deserializedChunk, Is.EqualTo(originalChunk));
            Assert.That(deserializedChunk.ProtectedRegions, Has.Count.EqualTo(1));
            Assert.That(
                deserializedChunk.ProtectedRegions[0],
                Is.EqualTo(originalChunk.ProtectedRegions[0])
            );
        }
    }

    [Test]
    public void ChunkSerializeDeserialize_PreservesGeneratedNamesAndDebugLocations()
    {
        var compiler = new Compiler();
        var source = """
            val obj = {};
            loc fn obj.bar() -> 1;
            val anonymous = fn() -> 2;
            loc fn obj::baz() -> 3;
            loc fn obj.bar() -> 4;
            """;

        var originalChunk = compiler.Compile(source, "generated_chunk_names.vil");

        using var ms = new MemoryStream();
        originalChunk.Serialize(ms);
        ms.Seek(0, SeekOrigin.Begin);
        var chunk = Chunk.Deserialize(ms, out var version, out _);

        Assert.That(version, Is.EqualTo(Chunk.FormatVersion));
        Assert.That(chunk, Is.EqualTo(originalChunk));
        Assert.That(chunk.NamedSubChunkLookup["obj.bar"], Is.EqualTo(3));
        Assert.That(chunk.NamedSubChunkLookup["obj::baz"], Is.EqualTo(2));
        Assert.That(chunk.SubChunks[0].Name, Is.EqualTo("obj.bar"));
        Assert.That(chunk.SubChunks[2].Name, Is.EqualTo("obj::baz"));
        Assert.That(chunk.SubChunks[0].DebugDatabase.DefinedOnLine, Is.EqualTo(2));
        Assert.That(chunk.SubChunks[2].DebugDatabase.DefinedOnLine, Is.EqualTo(4));
        Assert.That(chunk.SubChunks[3].Name, Is.EqualTo("obj.bar"));
        Assert.That(chunk.SubChunks[3].DebugDatabase.DefinedOnLine, Is.EqualTo(5));
        Assert.That(chunk.SubChunks[3].DebugDatabase.DefinedInFile, Is.EqualTo("generated_chunk_names.vil"));
        Assert.That(chunk.SubChunks[0].DebugDatabase.DefinedInFile, Is.EqualTo("generated_chunk_names.vil"));
        Assert.That(chunk.SubChunks[2].DebugDatabase.DefinedInFile, Is.EqualTo("generated_chunk_names.vil"));
    }

    [TestCase(0)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    public void ChunkDeserialize_RejectsUnsupportedVersions(int version)
    {
        using var stream = new MemoryStream([
            (byte)'E', (byte)'V', (byte)'C', 0x06, (byte)version
        ]);

        var exception = Assert.Throws<ChunkDeserializationException>(
            () => Chunk.Deserialize(stream, out _, out _)
        );

        Assert.That(exception!.Message, Does.Contain($"Unsupported chunk format version '{version}'"));
    }

    [Test]
    public void RootChunk_UsesReadableSourceBasedName()
    {
        const string fileName = "src/demo.vil";
        var chunk = new Compiler().Compile("val value = 1;", fileName);
        var sameSourceName = new Compiler().Compile("val value = 2;", fileName);
        var unnamedChunk = new Compiler().Compile("val value = 3;", string.Empty);
        var windowsPathChunk = new Compiler().Compile("val value = 4;", @"C:\src\windows_demo.vil");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(chunk.Name, Is.EqualTo("!_root_chunk!demo.vil"));
            Assert.That(sameSourceName.Name, Is.EqualTo(chunk.Name));
            Assert.That(sameSourceName.ClosureOwnerId, Is.Not.EqualTo(chunk.ClosureOwnerId));
            Assert.That(unnamedChunk.Name, Is.EqualTo("!_root_chunk"));
            Assert.That(windowsPathChunk.Name, Is.EqualTo("!_root_chunk!windows_demo.vil"));
            Assert.That(chunk.DebugDatabase.DefinedInFile, Is.EqualTo(fileName));
        }
    }

    [Test]
    public void ChunkSerializeDeserialize_PreservesClosureOwnerIdentities()
    {
        var originalChunk = new Compiler().Compile(
            "val value = 1; val closure = fn() -> value;",
            "identity_round_trip.vil"
        );

        using var stream = new MemoryStream();
        originalChunk.Serialize(stream);
        stream.Position = 0;
        var deserializedChunk = Chunk.Deserialize(stream, out _, out _);

        Assert.That(deserializedChunk.ClosureOwnerId, Is.EqualTo(originalChunk.ClosureOwnerId));
        Assert.That(
            deserializedChunk.SubChunks[0].Closures[0].EnclosedOwnerId,
            Is.EqualTo(originalChunk.SubChunks[0].Closures[0].EnclosedOwnerId)
        );
    }

    [Test]
    public void ChunkSerializeDeserialize_PreservesClosureContexts()
    {
        var originalChunk = new Compiler().Compile("val value = 1;");
        var context = originalChunk.SetClosureContext("owner");
        context.Values.Add(2, "captured");
        context.Values.Add(4, 3.14);

        var frame = Serialize(originalChunk);
        var deserializedChunk = Deserialize(frame);

        Assert.That(deserializedChunk, Is.EqualTo(originalChunk));
        Assert.That(deserializedChunk.ClosureContexts["owner"].EnclosedFunctionName,
            Is.EqualTo(context.EnclosedFunctionName));
        Assert.That(deserializedChunk.ClosureContexts["owner"].Values,
            Is.EqualTo(context.Values));
    }

    private static byte[] Serialize(Chunk chunk)
    {
        using var stream = new MemoryStream();
        chunk.Serialize(stream);
        return stream.ToArray();
    }

    private static Chunk Deserialize(byte[] frame)
    {
        using var stream = new MemoryStream(frame);
        return Chunk.Deserialize(stream, out _, out _);
    }

    private static void RecomputeDigest(byte[] frame)
    {
        SHA256.HashData(frame.AsSpan(0, frame.Length - 32))
              .CopyTo(frame.AsSpan(frame.Length - 32));
    }

    private static int FindSectionEntry(byte[] frame, ushort tag)
    {
        var sectionCount = BinaryPrimitives.ReadUInt16LittleEndian(frame.AsSpan(6, 2));
        for (var i = 0; i < sectionCount; i++)
        {
            var entryOffset = 28 + i * 12;
            if (BinaryPrimitives.ReadUInt16LittleEndian(frame.AsSpan(entryOffset, 2)) == tag)
            {
                return entryOffset;
            }
        }

        throw new AssertionException($"Section '{tag}' was not found.");
    }
}