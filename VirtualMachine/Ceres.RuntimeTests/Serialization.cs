using System.IO;
using System.Linq;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.TranslationEngine;
using EVIL.Grammar.Parsing;
using NUnit.Framework;

namespace Ceres.RuntimeTests
{
    public class Serialization
    {
        private Parser _parser = new();
        private Compiler _compiler = new();

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
                         "  var f = 33.3, " +
                         "      s = \"teststring\";" +
                         "" +
                         "  for (var i = 0; i < 10; i++)" +
                         "      f += 2;" +
                         "" +
                         "  if (false) -> 10; " +
                         "  elif (false_too_just_nil) -> 20;" +
                         "  elif (0) -> 30;" +
                         "  else -> \"haha you will never get a 4\";" +
                         "" +
                         "  ret 2+2;" +
                         "}";

            var program = _parser.Parse(source);
            var script = _compiler.Compile(program);
            var originalChunk = script.Chunks[0];

            using (var ms = new MemoryStream())
            {
                originalChunk.Serialize(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var chunk = Chunk.Deserialize(ms, out var version, out _);

                Assert.That(version, Is.EqualTo(Chunk.FormatVersion));
                Assert.That(chunk.Name, Is.EqualTo(originalChunk.Name));
                Assert.That(chunk.ParameterCount, Is.EqualTo(originalChunk.ParameterCount));
                Assert.That(chunk.ParameterInitializers.SequenceEqual(originalChunk.ParameterInitializers));
                Assert.That(chunk.LocalCount, Is.EqualTo(originalChunk.LocalCount));
                Assert.That(chunk.Labels.SequenceEqual(originalChunk.Labels));
                
                Assert.That(chunk.Attributes.Count, Is.EqualTo(originalChunk.Attributes.Count));
                for (var i = 0; i < originalChunk.Attributes.Count; i++)
                {
                    var originalAttr = originalChunk.Attributes[i];
                    var chunkAttr = chunk.Attributes[i];
                    
                    Assert.That(chunkAttr, Is.EqualTo(originalAttr));
                }
                
                Assert.That(chunk.StringPool, Is.EqualTo(originalChunk.StringPool));
                Assert.That(chunk.Code.SequenceEqual(originalChunk.Code));
            }
        }
    }
}