namespace EVIL.Ceres.RuntimeTests;

using System.IO;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.TranslationEngine;
using EVIL.Grammar;
using NUnit.Framework;

public class Serialization
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
}