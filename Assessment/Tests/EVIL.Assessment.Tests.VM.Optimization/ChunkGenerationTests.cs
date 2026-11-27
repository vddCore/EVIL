namespace EVIL.Assessment.Tests.VM.Optimization;

using EVIL.Assessment.Tests.Base;
using EVIL.Core.Common.TypeSystem;
using EVIL.VM.ExecutionEngine.Diagnostics;
using EVIL.VM.ExecutionEngine.TypeSystem;
using NUnit.Framework;

[TestFixture]
public class ChunkGenerationTests : EvilTest
{
    
    [Test]
    public void DynamicValue_LayoutAndRepresentation_PreservesSemantics()
    {
        var nilVal = DynamicValue.Nil;
        Assert.That(nilVal.Type, Is.EqualTo(DynamicValueType.Nil));

        DynamicValue trueVal = true;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(trueVal.Type, Is.EqualTo(DynamicValueType.Boolean));
            Assert.That(trueVal.Boolean, Is.True);
        }

        DynamicValue falseVal = false;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(falseVal.Type, Is.EqualTo(DynamicValueType.Boolean));
            Assert.That(falseVal.Boolean, Is.False);
        }

        DynamicValue numVal = 42.5;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(numVal.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(numVal.Number, Is.EqualTo(42.5));
        }

        DynamicValue typeVal = DynamicValueType.String;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(typeVal.Type, Is.EqualTo(DynamicValueType.TypeCode));
            Assert.That(typeVal.TypeCode, Is.EqualTo(DynamicValueType.String));
        }

        DynamicValue strVal1 = "hello";
        DynamicValue strVal2 = "hello";
        Assert.That(strVal1, Is.EqualTo(strVal2));
        Assert.That(strVal1.GetHashCode(), Is.EqualTo(strVal2.GetHashCode()));
    }

    [Test]
    public void Chunk_BytecodeAndPoolSharing_AvoidsDuplicationOnClone()
    {
        var chunk = new Chunk("test_chunk", [0x01, 0x02, 0x03], ["str1", "str2"]);
        var originalCode = chunk.Code;
        var originalPool = chunk.StringPool;

        var clone = chunk.Clone();

        using (Assert.EnterMultipleScope())
        {
            // Code and StringPool must have a shared ref between clones.
            Assert.That(clone.Code, Is.SameAs(originalCode));
            Assert.That(clone.StringPool, Is.SameAs(originalPool));
            Assert.That(clone.ClosureOwnerId, Is.EqualTo(chunk.ClosureOwnerId));
        }
        Assert.That(clone.Code, Has.Length.EqualTo(3));
    }

    [Test]
    public void Chunk_SpawnCodeReader_SharesBytecodeBuffer()
    {
        var chunk = new Chunk("reader_test", [0x10, 0x20]);
        var reader1 = chunk.SpawnCodeReader();
        var reader2 = chunk.SpawnCodeReader();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(reader1.Length, Is.EqualTo(2));
            Assert.That(reader2.Length, Is.EqualTo(2));
            Assert.That(chunk.Code, Has.Length.EqualTo(2));
        }
    }

    [Test]
    public void StringPool_GetDynamicValue_PreInternsAndCachesEntries()
    {
        var pool = new StringPool(["first", "second", "third"]);
        var v0 = pool.GetDynamicValue(0);
        var v1 = pool.GetDynamicValue(1);
        var v0Again = pool.GetDynamicValue(0);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(v0.Type, Is.EqualTo(DynamicValueType.String));
            Assert.That(v0.String, Is.EqualTo("first"));
            Assert.That(v1.String, Is.EqualTo("second"));
            Assert.That(v0.String, Is.SameAs(v0Again.String));
            Assert.That(pool.GetDynamicValue(-1), Is.EqualTo(DynamicValue.Nil));
            Assert.That(pool.GetDynamicValue(999), Is.EqualTo(DynamicValue.Nil));
        }
    }
}
