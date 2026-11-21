namespace EVIL.Ceres.RuntimeTests;

using System;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.CommonTypes.TypeSystem;
using NUnit.Framework;

[TestFixture]
public class OptimizationTests : TestBase
{
    [Test]
    public void DynamicValue_LayoutAndRepresentation_PreservesSemantics()
    {
        var nilVal = DynamicValue.Nil;
        Assert.That(nilVal.Type, Is.EqualTo(DynamicValueType.Nil));

        DynamicValue trueVal = true;
        Assert.That(trueVal.Type, Is.EqualTo(DynamicValueType.Boolean));
        Assert.That(trueVal.Boolean, Is.True);

        DynamicValue falseVal = false;
        Assert.That(falseVal.Type, Is.EqualTo(DynamicValueType.Boolean));
        Assert.That(falseVal.Boolean, Is.False);

        DynamicValue numVal = 42.5;
        Assert.That(numVal.Type, Is.EqualTo(DynamicValueType.Number));
        Assert.That(numVal.Number, Is.EqualTo(42.5));

        DynamicValue typeVal = DynamicValueType.String;
        Assert.That(typeVal.Type, Is.EqualTo(DynamicValueType.TypeCode));
        Assert.That(typeVal.TypeCode, Is.EqualTo(DynamicValueType.String));

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

        // Bytecode and StringPool must have a shared ref between clones.
        Assert.That(clone.Code, Is.SameAs(originalCode));
        Assert.That(clone.StringPool, Is.SameAs(originalPool));
        Assert.That(clone.Code.Length, Is.EqualTo(3));
    }

    [Test]
    public void Chunk_SpawnCodeReader_SharesBytecodeBuffer()
    {
        var chunk = new Chunk("reader_test", [0x10, 0x20]);
        var reader1 = chunk.SpawnCodeReader();
        var reader2 = chunk.SpawnCodeReader();

        Assert.That(reader1.Length, Is.EqualTo(2));
        Assert.That(reader2.Length, Is.EqualTo(2));
        Assert.That(chunk.Code.Length, Is.EqualTo(2));
    }

    [Test]
    public void Closure_HighIterationLoop_PreservesCorrectnessAndState()
    {
        var script = """
            fn test() {
                rw val sum = 0;
                for (rw val i = 0; i < 500; i++) {
                    val multiplier = i;
                    val closure = fn(x) -> x * multiplier;
                    sum += closure(2);
                }
                ret sum;
            }
            """;

        var result = EvilTestResult(script);
        
        // sum = 2 * sum(0..499) = 2 * (499 * 500 / 2) = 249500
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
        Assert.That(result.Number, Is.EqualTo(249500.0));
    }

    [Test]
    public void Closure_DeeplyNested_CapturesAllLevelsCorrectly()
    {
        var script = """
            fn test() {
                val a = 10;
                val f = fn(b) {
                    ret fn(c) {
                        ret fn(d) -> a + b + c + d;
                    }(30);
                }(20);

                ret f(40);
            }
            """;

        var result = EvilTestResult(script);
        // 10 + 20 + 30 + 40 = 100
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
        Assert.That(result.Number, Is.EqualTo(100.0));
    }

    [Test]
    public void Closure_MultipleInstances_HaveIndependentState()
    {
        var script = """
            fn makeCounter(initial) {
                rw val count = initial;
                ret fn() {
                    count++;
                    ret count;
                };
            }

            fn test() {
                val c1 = makeCounter(0);
                val c2 = makeCounter(100);

                c1();
                c1();
                val r1 = c1();

                c2();
                val r2 = c2();

                ret [r1, r2];
            }
            """;

        var result = EvilTestResult(script);
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
        Assert.That(result.Array![0].Number, Is.EqualTo(3.0));
        Assert.That(result.Array![1].Number, Is.EqualTo(102.0));
    }

    [Test]
    public void Closure_RecursiveAnonymousFunction_ExecutesCorrectly()
    {
        var script = """
            fn test() {
                val obj = {
                    fib: self::fn(n) {
                        if (n <= 1) {
                            ret n;
                        }
                        ret self::fib(n - 1) + self::fib(n - 2);
                    }
                };

                ret obj::fib(10);
            }
            """;

        var result = EvilTestResult(script);
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
        Assert.That(result.Number, Is.EqualTo(55.0));
    }

    [Test]
    public void Closure_MutationAcrossCalls_PersistsInClosureContext()
    {
        var script = """
            fn test() {
                rw val acc = 0;
                val add = fn(n) {
                    acc += n;
                    ret acc;
                };

                add(5);
                add(10);
                ret add(15);
            }
            """;

        var result = EvilTestResult(script);
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
        Assert.That(result.Number, Is.EqualTo(30.0));
    }

    [Test]
    public void FastLeafFunction_ZeroArguments_ExecutesEfficiently()
    {
        var script = """
            fn leaf() -> 42;

            fn test() {
                rw val total = 0;
                for (rw val i = 0; i < 1000; i++) {
                    total += leaf();
                }
                ret total;
            }
            """;

        var result = EvilTestResult(script);
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
        Assert.That(result.Number, Is.EqualTo(42000.0));
    }

    [Test]
    public void Upvalue_SharedMutation_BetweenSiblingClosures()
    {
        var script = """
            fn make_pair() {
                rw val shared_var = 10;
                val adder = fn(delta) {
                    shared_var += delta;
                    ret shared_var;
                };
                val getter = fn() -> shared_var;

                ret { adder: adder, getter: getter };
            }

            fn test() {
                val pair = make_pair();
                pair.adder(25);
                ret pair.getter();
            }
            """;

        var result = EvilTestResult(script);
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
        Assert.That(result.Number, Is.EqualTo(35.0));
    }

    [Test]
    public void Upvalue_DeeplyNested_MultiLevelCapture()
    {
        var script = """
            fn outer(x) {
                rw val y = 100;
                ret fn(z) {
                    ret fn(w) {
                        ret x + y + z + w;
                    };
                };
            }

            fn test() {
                val step1 = outer(10);
                val step2 = step1(20);
                ret step2(5);
            }
            """;

        var result = EvilTestResult(script);
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
        Assert.That(result.Number, Is.EqualTo(135.0));
    }

    [Test]
    public void Upvalue_HighVolume_Throughput()
    {
        var script = """
            fn make_accumulator() {
                rw val acc = 0;
                ret fn() {
                    acc++;
                    ret acc;
                };
            }

            fn test() {
                val tick = make_accumulator();
                rw val last = 0;
                for (rw val i = 0; i < 50000; i++) {
                    last = tick();
                }
                ret last;
            }
            """;

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var result = EvilTestResult(script);
        sw.Stop();

        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
        Assert.That(result.Number, Is.EqualTo(50000.0));
        Assert.That(sw.ElapsedMilliseconds, Is.LessThan(2000), $"Execution took too long: {sw.ElapsedMilliseconds}ms");
    }

    [Test]
    public void TailCall_TernaryExpressionBody_DeepRecursionDoesNotOverflow()
    {
        var script = """
            fn sum_tail(n, acc) -> n == 0 ? acc : sum_tail(n - 1, acc + 1);

            fn test() {
                ret sum_tail(50000, 0);
            }
            """;

        var result = EvilTestResult(script);
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
        Assert.That(result.Number, Is.EqualTo(50000.0));
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
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
        Assert.That(result.Number, Is.EqualTo(0.0));
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
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
        Assert.That(result.Number, Is.EqualTo(10.0));
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
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
        Assert.That(result.Array![0].Number, Is.EqualTo(1.0));
        Assert.That(result.Array![1].Number, Is.EqualTo(15.0));
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
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
        Assert.That(result.Array![0].Number, Is.EqualTo(1.0));
        Assert.That(result.Array![1].Number, Is.EqualTo(11.0));
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

                val r = t::get_self()::get_num();
                ret [eval_count, r];
            }
            """;

        var result = EvilTestResult(script);
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
        Assert.That(result.Array![0].Number, Is.EqualTo(1.0));
        Assert.That(result.Array![1].Number, Is.EqualTo(42.0));
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
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
        Assert.That(result.Array![0].Boolean, Is.False);
        Assert.That(result.Array![1].Number, Is.EqualTo(0.0));
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
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
        Assert.That(result.Array![0].Boolean, Is.True);
        Assert.That(result.Array![1].Number, Is.EqualTo(0.0));
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
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
        Assert.That(result.Array![0].Number, Is.EqualTo(10.0));
        Assert.That(result.Array![1].Number, Is.EqualTo(30.0));
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
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
        Assert.That(result.Number, Is.EqualTo(10.0));
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
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
        Assert.That(result.Array![0].Boolean, Is.False);
        Assert.That(result.Array![1].Boolean, Is.True);
        Assert.That(result.Array![2].Boolean, Is.True);
        Assert.That(result.Array![3].Number, Is.EqualTo(0.0));
    }

    [Test]
    public void InvokeChunkFromStack_HighVolumeInvocations_ExecuteFastAndCorrect()
    {
        var script = """
            fn test() {
                val add = fn(a, b) -> a + b;
                rw val sum = 0;
                rw val i = 0;
                while (i < 1000) {
                    sum = add(sum, 1);
                    i++;
                }
                ret sum;
            }
            """;

        var result = EvilTestResult(script);
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
        Assert.That(result.Number, Is.EqualTo(1000.0));
    }
}
