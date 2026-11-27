namespace EVIL.Assessment.Tests.VM.Optimization;

using System.Collections.Generic;
using EVIL.Assessment.Tests.Base;
using EVIL.Core.Common.TypeSystem;
using EVIL.VM.ExecutionEngine.Diagnostics;
using EVIL.VM.ExecutionEngine.Diagnostics.Disassembly;
using NUnit.Framework;

[TestFixture]
public class CodeGenerationTests : EvilTest
{

    [Test]
    public void FunctionGeneration_NamedRedefinitionWithMixedChildren_InvokesReplacement()
    {
        var script = """
            fn foo() -> 1;
            val anonymous = fn() -> 2;
            fn other() -> 3;
            fn test() -> foo();
            fn foo() -> 4;
            fn padding() -> 99;
            """;

        var result = EvilTestResult(script);

        Assert.That(result.Number, Is.EqualTo(4.0));
    }

    [Test]
    public void FunctionGeneration_IndexedRedefinition_InvokesReplacement()
    {
        var script = """
            fn test() {
                val obj = {};
                loc fn obj.bar() -> 1;
                loc fn obj.bar() -> 2;
                ret obj.bar();
            }
            """;

        var result = EvilTestResult(script);

        Assert.That(result.Number, Is.EqualTo(2.0));
    }

    [Test]
    public void FunctionGeneration_IndexedRedefinition_PreservesEarlierFunctionBody()
    {
        var script = """
            fn test() {
                val obj = {};
                loc fn obj.bar() -> "first";
                val first = obj.bar();
                loc fn obj.bar() -> "second";
                ret first + obj.bar();
            }
            """;

        var result = EvilTestResult(script);

        Assert.That(result.String, Is.EqualTo("firstsecond"));
    }

    [Test]
    public void FunctionGeneration_TargetedRedefinition_InvokesReplacement()
    {
        var script = """
            fn test() {
                val obj = {};
                loc fn obj::bar() -> 1;
                loc fn obj::bar() -> 2;
                ret obj:bar();
            }
            """;

        var result = EvilTestResult(script);

        Assert.That(result.Number, Is.EqualTo(2.0));
    }

    [Test]
    public void FunctionGeneration_TargetedRedefinition_PreservesEarlierFunctionBody()
    {
        var script = """
            fn test() {
                val obj = {};
                loc fn obj::bar() -> "first";
                val first = obj:bar();
                loc fn obj::bar() -> "second";
                ret first + obj:bar();
            }
            """;

        var result = EvilTestResult(script);

        Assert.That(result.String, Is.EqualTo("firstsecond"));
    }

    [Test]
    public void FunctionGeneration_IndexedPrefixAndSuffixNames_PreserveAssignments()
    {
        var script = """
            fn test() {
                val obj = {};
                loc fn obj.barExtra() -> 1;
                loc fn obj.bar() -> 2;
                loc fn obj.bar_1() -> 3;
                loc fn obj.bar() -> 4;
                ret obj.bar() + obj.barExtra() + obj.bar_1();
            }
            """;

        var result = EvilTestResult(script);

        Assert.That(result.Number, Is.EqualTo(8.0));
    }

    [Test]
    public void FunctionGeneration_TargetedPrefixAndSuffixNames_PreserveAssignments()
    {
        var script = """
            fn test() {
                val obj = {};
                loc fn obj::barExtra() -> 1;
                loc fn obj::bar() -> 2;
                loc fn obj::bar_1() -> 3;
                loc fn obj::bar() -> 4;
                ret obj:bar() + obj:barExtra() + obj:bar_1();
            }
            """;

        var result = EvilTestResult(script);

        Assert.That(result.Number, Is.EqualTo(8.0));
    }

    [Test]
    public void ShortCircuit_And_DoesNotEvaluateRightHandSideWhenLeftIsFalse()
    {
        var script = """
            fn test() {
                rw val side_effect = 0;
                val bump = fn() {
                    side_effect++;
                    ret true;
                };

                if (false && bump()) {
                    side_effect += 100;
                }

                ret side_effect;
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(0.0));
        }
    }

    [Test]
    public void ShortCircuit_Or_DoesNotEvaluateRightHandSideWhenLeftIsTrue()
    {
        var script = """
            fn test() {
                rw val side_effect = 0;
                val bump = fn() {
                    side_effect++;
                    ret false;
                };

                if (true || bump()) {
                    side_effect += 10;
                }

                ret side_effect;
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(10.0));
        }
    }

    [Test]
    public void CompoundAssignment_IndexerEvaluatedOnce()
    {
        var script = """
            fn test() {
                rw val eval_count = 0;
                val get_idx = fn() {
                    eval_count++;
                    ret "k";
                };

                val t = { k: 10 };
                t[get_idx()] += 5;

                ret [eval_count, t.k];
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].Number, Is.EqualTo(1.0));
            Assert.That(result.Array![1].Number, Is.EqualTo(15.0));
        }
    }

    [Test]
    public void Incrementation_IndexerEvaluatedOnce()
    {
        var script = """
            fn test() {
                rw val eval_count = 0;
                val get_idx = fn() {
                    eval_count++;
                    ret "k";
                };

                val t = { k: 10 };
                t[get_idx()]++;

                ret [eval_count, t.k];
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].Number, Is.EqualTo(1.0));
            Assert.That(result.Array![1].Number, Is.EqualTo(11.0));
        }
    }

    [Test]
    public void SelfInvocation_TargetEvaluatedOnce()
    {
        var script = """
            fn test() {
                rw val eval_count = 0;
                val t = {
                    get_self: self::fn() {
                        eval_count++;
                        ret self;
                    },
                    num: 42,
                    get_num: self::fn() -> self.num
                };

                val r = t:get_self():get_num();
                ret [eval_count, r];
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].Number, Is.EqualTo(1.0));
            Assert.That(result.Array![1].Number, Is.EqualTo(42.0));
        }
    }

    [Test]
    public void GeneralExpression_And_ShortCircuitsInVariableAssignment()
    {
        var script = """
            fn test() {
                rw val side_effect = 0;
                val bump = fn() {
                    side_effect++;
                    ret true;
                };

                val result = false && bump();
                ret [result, side_effect];
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].Boolean, Is.False);
            Assert.That(result.Array![1].Number, Is.EqualTo(0.0));
        }
    }

    [Test]
    public void GeneralExpression_Or_ShortCircuitsInVariableAssignment()
    {
        var script = """
            fn test() {
                rw val side_effect = 0;
                val bump = fn() {
                    side_effect++;
                    ret false;
                };

                val result = true || bump();
                ret [result, side_effect];
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].Boolean, Is.True);
            Assert.That(result.Array![1].Number, Is.EqualTo(0.0));
        }
    }

    [Test]
    public void CoalesceAssignment_Statement_DoesNotLeakEvaluationStack()
    {
        var script = """
            fn test() {
                rw val x = nil;
                x ??= 10;
                x ??= 20;
                
                val t = { a: nil };
                t.a ??= 30;
                t.a ??= 40;

                ret [x, t.a];
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].Number, Is.EqualTo(10.0));
            Assert.That(result.Array![1].Number, Is.EqualTo(30.0));
        }
    }

    [Test]
    public void Jump_Backpatching_EncodesDirectTargetAddressesInBytecode()
    {
        var script = """
            fn test() {
                rw val sum = 0;
                rw val i = 0;
                while (i < 5) {
                    sum += i;
                    i++;
                }
                ret sum;
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(10.0));
        }
    }

    [Test]
    public void Chained_LogicalExpressions_ShortCircuitCorrectly()
    {
        var script = """
            fn test() {
                rw val side_effects = 0;
                val err = fn() {
                    side_effects++;
                    ret true;
                };

                val a = false && err() && err();
                val b = true || err() || err();
                val c = (false && err()) || (true && 42);

                ret [a, b, c, side_effects];
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].Boolean, Is.False);
            Assert.That(result.Array![1].Boolean, Is.True);
            Assert.That(result.Array![2].Boolean, Is.True);
            Assert.That(result.Array![3].Number, Is.EqualTo(0.0));
        }
    }

    [Test]
    public void StringInterpolation_VariadicStrcat_InterpolatesMultipleSegmentsCorrectly()
    {
        var script = """
            fn test() {
                val name = "EVIL VM";
                val version = 2.4;
                val active = true;
                val result = "Engine: $name, v$version (active: $active)";
                ret result;
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.String));
            Assert.That(result.String, Is.EqualTo("Engine: EVIL VM, v2.4 (active: true)"));
        }
    }

    [Test]
    public void StringInterpolation_NestedStringConcatenation_EvaluatesCorrectly()
    {
        var script = """
            fn test() {
                val result = "${"string"+"string"}." + " suffix";
                ret result;
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.String));
            Assert.That(result.String, Is.EqualTo("stringstring. suffix"));
        }
    }

    [Test]
    public void StringInterpolation_ConstantConcatenation_PreservesMultipleEmbeddedExpressions()
    {
        var script = """
            fn test() {
                val name = "EVIL";
                val both = "first $name" + " and second $name";
                val right = "first $name" + 42;
                val left = 42 + " and $name";
                ret [both, right, left];
            }
            """;

        var result = EvilTestResult(script);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].String, Is.EqualTo("first EVIL and second EVIL"));
            Assert.That(result.Array![1].String, Is.EqualTo("first EVIL42"));
            Assert.That(result.Array![2].String, Is.EqualTo("42 and EVIL"));
        }
    }

    [Test]
    public void StringInterpolation_UnaryStringConversion_PreservesSegments()
    {
        var script = """
            fn test() {
                val name = "EVIL";
                ret @"name: $name";
            }
            """;

        var result = EvilTestResult(script);

        Assert.That(result.Type, Is.EqualTo(DynamicValueType.String));
        Assert.That(result.String, Is.EqualTo("name: EVIL"));
    }

    [Test]
    public void StringInterpolation_SingleExpression_ConvertsToStringDirectly()
    {
        var script = """
            fn test() {
                val num = 42;
                val str = "$num";
                ret [str, str is String];
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].String, Is.EqualTo("42"));
            Assert.That(result.Array![1].Boolean, Is.True);
        }
    }

    [Test]
    public void StringInterpolation_NoExpressions_EmitsSingleStringConstant()
    {
        var script = """
            fn test() {
                val str = "Plain literal string without vars";
                ret str;
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.String));
            Assert.That(result.String, Is.EqualTo("Plain literal string without vars"));
        }
    }

    [Test]
    public void CompactBytecode_NumericConstants_EncodingsAndExecution()
    {
        var script = """
            fn test() {
                val a = 0;
                val b = 1;
                val c = 2;
                val d = -1;
                val e = 42;
                val f = -128;
                val g = 127;
                val h = 1000;
                val i = 3.14;
                ret [a, b, c, d, e, f, g, h, i];
            }
            """;

        var chunk = Compiler.Compile(script, "num_test");
        var testChunk = chunk["test"]!;

        var opcodes = new List<OpCode>();
        var reader = testChunk.SpawnCodeReader();
        while (reader.IP < reader.Length)
        {
            var op = (OpCode)reader.ReadByte();
            opcodes.Add(op);
            switch (op)
            {
                case OpCode.LDNUM:
                    reader.ReadDouble();
                    break;
                
                case OpCode.LDNUM_I8:
                case OpCode.LDTYPE:
                case OpCode.GETLOCAL_B:
                case OpCode.SETLOCAL_B:
                case OpCode.GETARG_B:
                case OpCode.SETARG_B:
                case OpCode.GETCLOSURE_B:
                case OpCode.SETCLOSURE_B:
                    reader.ReadByte();
                    break;
                
                case OpCode.GETLOCAL:
                case OpCode.SETLOCAL:
                case OpCode.GETARG:
                case OpCode.SETARG:
                case OpCode.GETCLOSURE:
                case OpCode.SETCLOSURE:
                case OpCode.LDSTR:
                case OpCode.LDCNK:
                case OpCode.ENTER:
                case OpCode.ARRINIT:
                case OpCode.TABINIT:
                case OpCode.STRCAT:
                case OpCode.FJMP:
                case OpCode.TJMP:
                case OpCode.JUMP:
                case OpCode.NJMP:
                case OpCode.VJMP:
                    reader.ReadInt32();
                    break;
            }
        }

        Assert.That(opcodes, Contains.Item(OpCode.LDZERO), "0 should compile to LDZERO");
        Assert.That(opcodes, Contains.Item(OpCode.LDONE), "1 should compile to LDONE");
        Assert.That(opcodes, Contains.Item(OpCode.LDNUM_I8), "Small numbers should compile to LDNUM_I8");
        Assert.That(opcodes, Contains.Item(OpCode.LDNUM), "Large or non-integer numbers should compile to LDNUM");

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].Number, Is.EqualTo(0.0));
            Assert.That(result.Array![1].Number, Is.EqualTo(1.0));
            Assert.That(result.Array![2].Number, Is.EqualTo(2.0));
            Assert.That(result.Array![3].Number, Is.EqualTo(-1.0));
            Assert.That(result.Array![4].Number, Is.EqualTo(42.0));
            Assert.That(result.Array![5].Number, Is.EqualTo(-128.0));
            Assert.That(result.Array![6].Number, Is.EqualTo(127.0));
            Assert.That(result.Array![7].Number, Is.EqualTo(1000.0));
            Assert.That(result.Array![8].Number, Is.EqualTo(3.14));
        }
    }

    [Test]
    public void CompactBytecode_DynamicValueType_EncodingAndExecution()
    {
        var script = """
            fn test() {
                val t = String;
                val isStr = "hello" is String;
                val isNum = 42 is Number;
                ret [t == String, isStr, isNum];
            }
            """;

        var chunk = Compiler.Compile(script, "type_test");
        var testChunk = chunk["test"]!;

        var foundLdType = false;
        var reader = testChunk.SpawnCodeReader();
        while (reader.IP < reader.Length)
        {
            var op = (OpCode)reader.ReadByte();
            if (op == OpCode.LDTYPE)
            {
                foundLdType = true;
                var byteVal = reader.ReadByte();
                Assert.That(byteVal, Is.LessThan(20), "LDTYPE operand should be a compact byte");
                continue;
            }

            switch (op)
            {
                case OpCode.LDNUM:
                    reader.ReadDouble();
                    break;
                
                case OpCode.LDNUM_I8:
                case OpCode.GETLOCAL_B:
                case OpCode.SETLOCAL_B:
                case OpCode.GETARG_B:
                case OpCode.SETARG_B:
                case OpCode.GETCLOSURE_B:
                case OpCode.SETCLOSURE_B:
                    reader.ReadByte();
                    break;
                
                case OpCode.GETLOCAL:
                case OpCode.SETLOCAL:
                case OpCode.GETARG:
                case OpCode.SETARG:
                case OpCode.GETCLOSURE:
                case OpCode.SETCLOSURE:
                case OpCode.LDSTR:
                case OpCode.LDCNK:
                case OpCode.ENTER:
                case OpCode.ARRINIT:
                case OpCode.TABINIT:
                case OpCode.STRCAT:
                case OpCode.FJMP:
                case OpCode.TJMP:
                case OpCode.JUMP:
                case OpCode.NJMP:
                case OpCode.VJMP:
                    reader.ReadInt32();
                    break;
            }
        }

        Assert.That(foundLdType, Is.True, "Expected OpCode.LDTYPE to be emitted");

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].Boolean, Is.True);
            Assert.That(result.Array![1].Boolean, Is.True);
            Assert.That(result.Array![2].Boolean, Is.True);
        }
    }

    [Test]
    public void CompactBytecode_SpecializedLocals_EncodingAndExecution()
    {
        var script = """
            fn test() {
                rw val v0 = 10;
                rw val v1 = 20;
                rw val v2 = 30;
                rw val v3 = 40;
                rw val v4 = 50;
                v0 += 1;
                v1 += 2;
                v2 += 3;
                v3 += 4;
                v4 += 5;
                ret [v0, v1, v2, v3, v4];
            }
            """;

        var chunk = Compiler.Compile(script, "locals_test");
        var testChunk = chunk["test"]!;

        var opcodes = new List<OpCode>();
        var reader = testChunk.SpawnCodeReader();
        while (reader.IP < reader.Length)
        {
            var op = (OpCode)reader.ReadByte();
            opcodes.Add(op);
            switch (op)
            {
                case OpCode.LDNUM:
                    reader.ReadDouble();
                    break;
                
                case OpCode.LDNUM_I8:
                case OpCode.LDTYPE:
                case OpCode.GETLOCAL_B:
                case OpCode.SETLOCAL_B:
                case OpCode.GETARG_B:
                case OpCode.SETARG_B:
                case OpCode.GETCLOSURE_B:
                case OpCode.SETCLOSURE_B:
                    reader.ReadByte();
                    break;
                
                case OpCode.GETLOCAL:
                case OpCode.SETLOCAL:
                case OpCode.GETARG:
                case OpCode.SETARG:
                case OpCode.GETCLOSURE:
                case OpCode.SETCLOSURE:
                case OpCode.LDSTR:
                case OpCode.LDCNK:
                case OpCode.ENTER:
                case OpCode.ARRINIT:
                case OpCode.TABINIT:
                case OpCode.STRCAT:
                case OpCode.FJMP:
                case OpCode.TJMP:
                case OpCode.JUMP:
                case OpCode.NJMP:
                case OpCode.VJMP:
                    reader.ReadInt32();
                    break;
            }
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(opcodes, Contains.Item(OpCode.SETLOCAL_0));
            Assert.That(opcodes, Contains.Item(OpCode.SETLOCAL_1));
            Assert.That(opcodes, Contains.Item(OpCode.SETLOCAL_2));
            Assert.That(opcodes, Contains.Item(OpCode.SETLOCAL_3));
            Assert.That(opcodes, Contains.Item(OpCode.SETLOCAL_B));

            Assert.That(opcodes, Contains.Item(OpCode.GETLOCAL_0));
            Assert.That(opcodes, Contains.Item(OpCode.GETLOCAL_1));
            Assert.That(opcodes, Contains.Item(OpCode.GETLOCAL_2));
            Assert.That(opcodes, Contains.Item(OpCode.GETLOCAL_3));
            Assert.That(opcodes, Contains.Item(OpCode.GETLOCAL_B));
        }

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].Number, Is.EqualTo(11.0));
            Assert.That(result.Array![1].Number, Is.EqualTo(22.0));
            Assert.That(result.Array![2].Number, Is.EqualTo(33.0));
            Assert.That(result.Array![3].Number, Is.EqualTo(44.0));
            Assert.That(result.Array![4].Number, Is.EqualTo(55.0));
        }
    }

    [Test]
    public void CompactBytecode_SpecializedParameters_EncodingAndExecution()
    {
        var script = """
            fn worker(p0, p1, p2, p3, p4) {
                ret p0 + p1 + p2 + p3 + p4;
            }

            fn test() {
                ret worker(1, 2, 3, 4, 5);
            }
            """;

        var chunk = Compiler.Compile(script, "params_test");
        var workerChunk = chunk["worker"]!;

        var opcodes = new List<OpCode>();
        var reader = workerChunk.SpawnCodeReader();
        while (reader.IP < reader.Length)
        {
            var op = (OpCode)reader.ReadByte();
            opcodes.Add(op);
            switch (op)
            {
                case OpCode.LDNUM:
                    reader.ReadDouble();
                    break;
                
                case OpCode.LDNUM_I8:
                case OpCode.LDTYPE:
                case OpCode.GETLOCAL_B:
                case OpCode.SETLOCAL_B:
                case OpCode.GETARG_B:
                case OpCode.SETARG_B:
                case OpCode.GETCLOSURE_B:
                case OpCode.SETCLOSURE_B:
                    reader.ReadByte();
                    break;
                
                case OpCode.GETLOCAL:
                case OpCode.SETLOCAL:
                case OpCode.GETARG:
                case OpCode.SETARG:
                case OpCode.GETCLOSURE:
                case OpCode.SETCLOSURE:
                case OpCode.LDSTR:
                case OpCode.LDCNK:
                case OpCode.ENTER:
                case OpCode.ARRINIT:
                case OpCode.TABINIT:
                case OpCode.STRCAT:
                case OpCode.FJMP:
                case OpCode.TJMP:
                case OpCode.JUMP:
                case OpCode.NJMP:
                case OpCode.VJMP:
                    reader.ReadInt32();
                    break;
            }
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(opcodes, Contains.Item(OpCode.GETARG_0));
            Assert.That(opcodes, Contains.Item(OpCode.GETARG_1));
            Assert.That(opcodes, Contains.Item(OpCode.GETARG_2));
            Assert.That(opcodes, Contains.Item(OpCode.GETARG_3));
            Assert.That(opcodes, Contains.Item(OpCode.GETARG_B));
        }

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(15.0));
        }
    }

    [Test]
    public void CompactBytecode_SpecializedClosures_EncodingAndExecution()
    {
        var script = """
            fn test() {
                rw val a = 1;
                rw val b = 2;
                rw val c = 3;
                rw val d = 4;
                rw val e = 5;

                val f = fn() {
                    a += 10;
                    b += 20;
                    c += 30;
                    d += 40;
                    e += 50;
                    ret a + b + c + d + e;
                };

                ret f();
            }
            """;

        var chunk = Compiler.Compile(script, "closure_compact_test");
        var closureChunk = chunk["test"]!.SubChunks[0];

        var opcodes = new List<OpCode>();
        var reader = closureChunk.SpawnCodeReader();
        while (reader.IP < reader.Length)
        {
            var op = (OpCode)reader.ReadByte();
            opcodes.Add(op);
            switch (op)
            {
                case OpCode.LDNUM:
                    reader.ReadDouble();
                    break;
                
                case OpCode.LDNUM_I8:
                case OpCode.LDTYPE:
                case OpCode.GETLOCAL_B:
                case OpCode.SETLOCAL_B:
                case OpCode.GETARG_B:
                case OpCode.SETARG_B:
                case OpCode.GETCLOSURE_B:
                case OpCode.SETCLOSURE_B:
                    reader.ReadByte();
                    break;
                
                case OpCode.GETLOCAL:
                case OpCode.SETLOCAL:
                case OpCode.GETARG:
                case OpCode.SETARG:
                case OpCode.GETCLOSURE:
                case OpCode.SETCLOSURE:
                case OpCode.LDSTR:
                case OpCode.LDCNK:
                case OpCode.ENTER:
                case OpCode.ARRINIT:
                case OpCode.TABINIT:
                case OpCode.STRCAT:
                case OpCode.FJMP:
                case OpCode.TJMP:
                case OpCode.JUMP:
                case OpCode.NJMP:
                case OpCode.VJMP:
                    reader.ReadInt32();
                    break;
            }
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(opcodes, Contains.Item(OpCode.GETCLOSURE_0));
            Assert.That(opcodes, Contains.Item(OpCode.GETCLOSURE_1));
            Assert.That(opcodes, Contains.Item(OpCode.GETCLOSURE_2));
            Assert.That(opcodes, Contains.Item(OpCode.GETCLOSURE_3));
            Assert.That(opcodes, Contains.Item(OpCode.GETCLOSURE_B));

            Assert.That(opcodes, Contains.Item(OpCode.SETCLOSURE_0));
            Assert.That(opcodes, Contains.Item(OpCode.SETCLOSURE_1));
            Assert.That(opcodes, Contains.Item(OpCode.SETCLOSURE_2));
            Assert.That(opcodes, Contains.Item(OpCode.SETCLOSURE_3));
            Assert.That(opcodes, Contains.Item(OpCode.SETCLOSURE_B));
        }

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            
            // (1 + 10) + (2 + 20) + (3 + 30) + (4 + 40) + (5 + 50) = 11 + 22 + 33 + 44 + 55 = 165
            Assert.That(result.Number, Is.EqualTo(165.0));
        }
    }

    [Test]
    public void CompactBytecode_Disassembler_FormatsAllNewOpcodes()
    {
        var script = """
            fn worker(p0, p1, p2, p3, p4) {
                rw val l0 = 0;
                rw val l1 = 1;
                rw val l2 = 2;
                rw val l3 = 3;
                rw val l4 = 4;
                val t = String;
                val closure = fn() {
                    ret p0 + p1 + p2 + p3 + p4;
                };
                ret l0 + l1 + l2 + l3 + l4 + p0 + closure();
            }

            fn test() {
                ret worker(10, 20, 30, 40, 50);
            }
            """;

        var chunk = Compiler.Compile(script, "disasm_test");
        var sw = new System.IO.StringWriter();
        Disassembler.Disassemble(chunk, sw);
        var disasm = sw.ToString();

        Assert.That(disasm, Does.Contain("LDNUM_I8"));
        Assert.That(disasm, Does.Contain("LDTYPE"));
        Assert.That(disasm, Does.Contain("SETLOCAL_0"));
        Assert.That(disasm, Does.Contain("GETLOCAL_0"));
        Assert.That(disasm, Does.Contain("GETARG_0"));
        Assert.That(disasm, Does.Contain("GETCLOSURE_0"));
    }

    [Test]
    public void ByExpression_ConstantCases_EmitsSwitchOpCodeAndNoLinearCeqChain()
    {
        var script = """
            fn match_fn(x) {
                ret by x {
                    1 -> "one",
                    2 -> "two",
                    3 -> "three",
                    else: "other"
                };
            }
            fn test() {
                ret [match_fn(1), match_fn(2), match_fn(3), match_fn(99)];
            }
            """;

        var chunk = Compiler.Compile(script, "by_switch_test");
        var matchChunk = chunk["match_fn"]!;

        var opcodes = new List<OpCode>();
        var reader = matchChunk.SpawnCodeReader();
        while (reader.IP < reader.Length)
        {
            var op = (OpCode)reader.ReadByte();
            opcodes.Add(op);
            switch (op)
            {
                case OpCode.LDNUM:
                    reader.ReadDouble();
                    break;
                
                case OpCode.LDNUM_I8:
                case OpCode.LDTYPE:
                case OpCode.GETLOCAL_B:
                case OpCode.SETLOCAL_B:
                case OpCode.GETARG_B:
                case OpCode.SETARG_B:
                case OpCode.GETCLOSURE_B:
                case OpCode.SETCLOSURE_B:
                    reader.ReadByte();
                    break;
                
                case OpCode.GETLOCAL:
                case OpCode.SETLOCAL:
                case OpCode.GETARG:
                case OpCode.SETARG:
                case OpCode.GETCLOSURE:
                case OpCode.SETCLOSURE:
                case OpCode.LDSTR:
                case OpCode.LDCNK:
                case OpCode.ENTER:
                case OpCode.ARRINIT:
                case OpCode.TABINIT:
                case OpCode.STRCAT:
                case OpCode.FJMP:
                case OpCode.TJMP:
                case OpCode.JUMP:
                case OpCode.NJMP:
                case OpCode.VJMP:
                    reader.ReadInt32();
                    break;
                
                case OpCode.SWITCH:
                {
                    reader.ReadInt32(); // defaultIp
                    var count = reader.ReadInt32();
                    for (var i = 0; i < count; i++)
                    {
                        var type = (DynamicValueType)reader.ReadByte();
                        switch (type)
                        {
                            case DynamicValueType.Nil:
                                break;
                            
                            case DynamicValueType.Boolean:
                                reader.ReadBoolean();
                                break;
                            
                            case DynamicValueType.Number:
                                reader.ReadDouble();
                                break;
                            
                            case DynamicValueType.String:
                                reader.ReadInt32();
                                break;
                            
                            case DynamicValueType.TypeCode:
                                reader.ReadByte();
                                break;
                        }
                        reader.ReadInt32(); // targetIp
                    }
                    break;
                }
            }
        }

        Assert.That(opcodes, Contains.Item(OpCode.SWITCH));
        Assert.That(opcodes, Does.Not.Contain(OpCode.CEQ));
        Assert.That(opcodes, Does.Not.Contain(OpCode.FJMP));

        var res = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(res.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(res.Array![0].String, Is.EqualTo("one"));
            Assert.That(res.Array![1].String, Is.EqualTo("two"));
            Assert.That(res.Array![2].String, Is.EqualTo("three"));
            Assert.That(res.Array![3].String, Is.EqualTo("other"));
        }
    }

    [Test]
    public void ByExpression_ManyCases_Dispatches20CasesCorrectly()
    {
        var script = """
            fn dispatch_test(k) {
                ret by k {
                    0 -> "c0",
                    1 -> "c1",
                    2 -> "c2",
                    3 -> "c3",
                    4 -> "c4",
                    5 -> "c5",
                    6 -> "c6",
                    7 -> "c7",
                    8 -> "c8",
                    9 -> "c9",
                    10 -> "c10",
                    11 -> "c11",
                    12 -> "c12",
                    13 -> "c13",
                    14 -> "c14",
                    15 -> "c15",
                    16 -> "c16",
                    17 -> "c17",
                    18 -> "c18",
                    19 -> "c19",
                    else: "default"
                };
            }
            fn test() {
                val arr = array(21);
                for (rw val i = 0; i < 20; i++) {
                    arr[i] = dispatch_test(i);
                }
                arr[20] = dispatch_test(999);
                ret arr;
            }
            """;

        var chunk = Compiler.Compile(script, "by_20_cases");
        var matchChunk = chunk["dispatch_test"]!;
        var reader = matchChunk.SpawnCodeReader();
        var hasSwitch = false;
        
        while (reader.IP < reader.Length)
        {
            var op = (OpCode)reader.ReadByte();
            if (op == OpCode.SWITCH)
            {
                hasSwitch = true;
                break;
            }
        }
        Assert.That(hasSwitch, Is.True);

        var res = EvilTestResult(script);
        Assert.That(res.Type, Is.EqualTo(DynamicValueType.Array));
        for (var i = 0; i < 20; i++)
        {
            Assert.That(res.Array![i].String, Is.EqualTo($"c{i}"));
        }
        Assert.That(res.Array![20].String, Is.EqualTo("default"));
    }

    [Test]
    public void ByExpression_MixedConstantTypes_DispatchesAllTypes()
    {
        var script = """
            fn match_types(x) {
                ret by x {
                    nil -> "it_nil",
                    true -> "it_true",
                    false -> "it_false",
                    "hello" -> "it_str",
                    42 -> "it_num",
                    String -> "it_type_str",
                    Number -> "it_type_num",
                    else: "unknown"
                };
            }
            fn test() {
                ret [
                    match_types(nil),
                    match_types(true),
                    match_types(false),
                    match_types("hello"),
                    match_types(42),
                    match_types(String),
                    match_types(Number),
                    match_types("other")
                ];
            }
            """;

        var res = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(res.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(res.Array![0].String, Is.EqualTo("it_nil"));
            Assert.That(res.Array![1].String, Is.EqualTo("it_true"));
            Assert.That(res.Array![2].String, Is.EqualTo("it_false"));
            Assert.That(res.Array![3].String, Is.EqualTo("it_str"));
            Assert.That(res.Array![4].String, Is.EqualTo("it_num"));
            Assert.That(res.Array![5].String, Is.EqualTo("it_type_str"));
            Assert.That(res.Array![6].String, Is.EqualTo("it_type_num"));
            Assert.That(res.Array![7].String, Is.EqualTo("unknown"));
        }
    }

    [Test]
    public void ByExpression_NonConstantArms_FallsBackToLinearSearch()
    {
        var script = """
            loc fn get_dynamic_key() -> 42;
            fn match_dynamic(x) {
                ret by x {
                    get_dynamic_key() -> "dynamic_match",
                    else: "no_match"
                };
            }
            fn test() {
                ret [match_dynamic(42), match_dynamic(99)];
            }
            """;

        var chunk = Compiler.Compile(script, "by_fallback");
        var matchChunk = chunk["match_dynamic"]!;
        var reader = matchChunk.SpawnCodeReader();
        var opcodes = new List<OpCode>();
        
        while (reader.IP < reader.Length)
        {
            var op = (OpCode)reader.ReadByte();
            opcodes.Add(op);
            switch (op)
            {
                case OpCode.LDNUM:
                    reader.ReadDouble();
                    break;
                
                case OpCode.LDNUM_I8:
                case OpCode.LDTYPE:
                case OpCode.GETLOCAL_B:
                case OpCode.SETLOCAL_B:
                case OpCode.GETARG_B:
                case OpCode.SETARG_B:
                case OpCode.GETCLOSURE_B:
                case OpCode.SETCLOSURE_B:
                    reader.ReadByte();
                    break;
                
                case OpCode.GETLOCAL:
                case OpCode.SETLOCAL:
                case OpCode.GETARG:
                case OpCode.SETARG:
                case OpCode.GETCLOSURE:
                case OpCode.SETCLOSURE:
                case OpCode.LDSTR:
                case OpCode.LDCNK:
                case OpCode.ENTER:
                case OpCode.ARRINIT:
                case OpCode.TABINIT:
                case OpCode.STRCAT:
                case OpCode.FJMP:
                case OpCode.TJMP:
                case OpCode.JUMP:
                case OpCode.NJMP:
                case OpCode.VJMP:
                    reader.ReadInt32();
                    break;
            }
        }

        Assert.That(opcodes, Does.Not.Contain(OpCode.SWITCH));
        Assert.That(opcodes, Contains.Item(OpCode.CEQ));
        Assert.That(opcodes, Contains.Item(OpCode.FJMP));

        var res = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(res.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(res.Array![0].String, Is.EqualTo("dynamic_match"));
            Assert.That(res.Array![1].String, Is.EqualTo("no_match"));
        }
    }

    [Test]
    public void ByExpression_Serialization_PreservesSwitchDispatch()
    {
        var script = """
            fn test() {
                val f = fn(x) -> by x {
                    1 -> "one",
                    2 -> "two",
                    "hello" -> "world",
                    else: "none"
                };
                ret [f(1), f(2), f("hello"), f(999)];
            }
            """;

        var originalChunk = Compiler.Compile(script, "by_serialize_test");
        using var ms = new System.IO.MemoryStream();
        originalChunk.Serialize(ms);
        ms.Seek(0, System.IO.SeekOrigin.Begin);
        var deserializedChunk = Chunk.Deserialize(ms, out _, out _);

        Assert.That(deserializedChunk, Is.EqualTo(originalChunk));

        VM.MainFiber.Schedule(deserializedChunk);
        VM.MainFiber.Schedule(deserializedChunk["test"]!, false);
        VM.MainFiber.Resume();
        VM.MainFiber.BlockUntilFinished();

        var res = VM.MainFiber.PopValue();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(res.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(res.Array![0].String, Is.EqualTo("one"));
            Assert.That(res.Array![1].String, Is.EqualTo("two"));
            Assert.That(res.Array![2].String, Is.EqualTo("world"));
            Assert.That(res.Array![3].String, Is.EqualTo("none"));
        }
    }
}
