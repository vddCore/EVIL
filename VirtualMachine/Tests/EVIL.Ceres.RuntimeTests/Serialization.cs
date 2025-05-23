﻿namespace EVIL.Ceres.RuntimeTests;

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
}