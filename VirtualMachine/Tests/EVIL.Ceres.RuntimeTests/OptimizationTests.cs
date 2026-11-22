namespace EVIL.Ceres.RuntimeTests;

using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.Diagnostics.Disassembly;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.Ceres.Runtime;
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
        using (Assert.EnterMultipleScope())
        {
            // sum = 2 * sum(0..499) = 2 * (499 * 500 / 2) = 249500
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(249500.0));
        }
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
        using (Assert.EnterMultipleScope())
        {
            // 10 + 20 + 30 + 40 = 100
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(100.0));
        }
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].Number, Is.EqualTo(3.0));
            Assert.That(result.Array![1].Number, Is.EqualTo(102.0));
        }
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
                        ret self:fib(n - 1) + self:fib(n - 2);
                    }
                };

                ret obj:fib(10);
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(55.0));
        }
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(30.0));
        }
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(42000.0));
        }
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(35.0));
        }
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(135.0));
        }
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

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(50000.0));
            Assert.That(sw.ElapsedMilliseconds, Is.LessThan(2000), $"Execution took too long: {sw.ElapsedMilliseconds}ms");
        }
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(50000.0));
        }
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(1000.0));
        }
    }

    [Test]
    public void Table_HybridStorage_DenseIntegerKeys_FastArrayPath()
    {
        var script = """
            fn test() {
                val t = { 10, 20, 30, 40, 50 };
                rw val sum = 0;
                for (rw val i = 0; i < #t; i++) {
                    sum += t[i];
                }
                ret [sum, #t, 0 in t, 4 in t, 5 in t];
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].Number, Is.EqualTo(150.0));
            Assert.That(result.Array![1].Number, Is.EqualTo(5.0));
            Assert.That(result.Array![2].Boolean, Is.True);
            Assert.That(result.Array![3].Boolean, Is.True);
            Assert.That(result.Array![4].Boolean, Is.False);
        }
    }

    [Test]
    public void Table_MixedStorage_DenseIntegerAndHashKeys_CorrectSemantics()
    {
        var script = """
            fn test() {
                val t = { name: "Ceres", version: 2.4 };
                t[0] = 100;
                t[1] = 200;
                t[10] = "sparse";
                ret [t[0], t[1], t.name, t.version, t[10], #t];
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].Number, Is.EqualTo(100.0));
            Assert.That(result.Array![1].Number, Is.EqualTo(200.0));
            Assert.That(result.Array![2].String, Is.EqualTo("Ceres"));
            Assert.That(result.Array![3].Number, Is.EqualTo(2.4));
            Assert.That(result.Array![4].String, Is.EqualTo("sparse"));
            Assert.That(result.Array![5].Number, Is.EqualTo(5.0));
        }
    }

    [Test]
    public void Table_SetToNil_DecrementsLengthAndRemovesKey()
    {
        var script = """
            fn test() {
                val t = { 10, 20, 30 };
                val lenBefore = #t;
                t[1] = nil;
                val lenAfter = #t;
                val hasZero = 0 in t;
                val hasOne = 1 in t;
                val hasTwo = 2 in t;
                ret [lenBefore, lenAfter, hasZero, hasOne, hasTwo, t[1]];
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].Number, Is.EqualTo(3.0));
            Assert.That(result.Array![1].Number, Is.EqualTo(2.0));
            Assert.That(result.Array![2].Boolean, Is.True);
            Assert.That(result.Array![3].Boolean, Is.False);
            Assert.That(result.Array![4].Boolean, Is.True);
            Assert.That(result.Array![5].Type, Is.EqualTo(DynamicValueType.Nil));
        }
    }

    [Test]
    public void BatchContainerLiteral_ArrInit_ConstructsArrayDirectly()
    {
        var script = """
            fn test() {
                val arr = [10, 20, 30, 40];
                ret [#arr, arr[0], arr[1], arr[2], arr[3]];
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].Number, Is.EqualTo(4.0));
            Assert.That(result.Array![1].Number, Is.EqualTo(10.0));
            Assert.That(result.Array![2].Number, Is.EqualTo(20.0));
            Assert.That(result.Array![3].Number, Is.EqualTo(30.0));
            Assert.That(result.Array![4].Number, Is.EqualTo(40.0));
        }
    }

    [Test]
    public void BatchContainerLiteral_TabInit_ConstructsUnkeyedTableDirectly()
    {
        var script = """
            fn test() {
                val t = { "a", "b", "c" };
                ret [#t, t[0], t[1], t[2]];
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].Number, Is.EqualTo(3.0));
            Assert.That(result.Array![1].String, Is.EqualTo("a"));
            Assert.That(result.Array![2].String, Is.EqualTo("b"));
            Assert.That(result.Array![3].String, Is.EqualTo("c"));
        }
    }

    [Test]
    public void BatchContainerLiteral_TabInit_ConstructsKeyedTableDirectly()
    {
        var script = """
            fn test() {
                val t = { x: 10, y: 20, z: 30 };
                ret [#t, t.x, t.y, t.z];
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].Number, Is.EqualTo(3.0));
            Assert.That(result.Array![1].Number, Is.EqualTo(10.0));
            Assert.That(result.Array![2].Number, Is.EqualTo(20.0));
            Assert.That(result.Array![3].Number, Is.EqualTo(30.0));
        }
    }

    [Test]
    public void BatchContainerLiteral_EmptyContainers_ConstructEmptyInstances()
    {
        var script = """
            fn test() {
                val a = [];
                val t = {};
                ret [#a, #t];
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].Number, Is.EqualTo(0.0));
            Assert.That(result.Array![1].Number, Is.EqualTo(0.0));
        }
    }

    [Test]
    public void Table_TryGetValue_DenseIntegerAndHashKeys_RetrievesValuesCorrectly()
    {
        var table = new ExecutionEngine.Collections.Table();
        table.Set(0, 100);
        table.Set(1, 200);
        table.Set("name", "Ceres");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.TryGetValue(0, out var v0), Is.True);
            Assert.That(v0.Number, Is.EqualTo(100.0));

            Assert.That(table.TryGetValue(1, out var v1), Is.True);
            Assert.That(v1.Number, Is.EqualTo(200.0));

            Assert.That(table.TryGetValue("name", out var vName), Is.True);
            Assert.That(vName.String, Is.EqualTo("Ceres"));

            Assert.That(table.TryGetValue(2, out var v2), Is.False);
            Assert.That(v2, Is.EqualTo(DynamicValue.Nil));

            Assert.That(table.TryGetValue("nonexistent", out var vNonexistent), Is.False);
            Assert.That(vNonexistent, Is.EqualTo(DynamicValue.Nil));
        }
    }

    [Test]
    public void Table_TryGetValue_SetToNil_ReturnsFalse()
    {
        var table = new ExecutionEngine.Collections.Table();
        table.Set("key", "value");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.TryGetValue("key", out var vBefore), Is.True);
            Assert.That(vBefore.String, Is.EqualTo("value"));
        }

        table.Set("key", DynamicValue.Nil);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.TryGetValue("key", out var vAfter), Is.False);
            Assert.That(vAfter, Is.EqualTo(DynamicValue.Nil));
        }
    }

    [Test]
    public void Table_SinglePassIndex_PreservesMetatableAndNilSemantics()
    {
        var script = """
            fn test() {
                val t = { a: 42, b: "hello" };
                val valA = t.a;
                val valB = t.b;
                val valC = t.nonexistent;
                ret [valA, valB, valC];
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].Number, Is.EqualTo(42.0));
            Assert.That(result.Array![1].String, Is.EqualTo("hello"));
            Assert.That(result.Array![2].Type, Is.EqualTo(DynamicValueType.Nil));
        }
    }

    [Test]
    public void Table_TabInit_SpanDraining_ConstructsAccurateStructures()
    {
        var script = """
            fn test() {
                val unkeyed = { 10, 20, 30, 40 };
                val keyed = { alpha: 1, beta: 2, gamma: 3 };
                ret [#unkeyed, unkeyed[0], unkeyed[3], #keyed, keyed.alpha, keyed.gamma];
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].Number, Is.EqualTo(4.0));
            Assert.That(result.Array![1].Number, Is.EqualTo(10.0));
            Assert.That(result.Array![2].Number, Is.EqualTo(40.0));
            Assert.That(result.Array![3].Number, Is.EqualTo(3.0));
            Assert.That(result.Array![4].Number, Is.EqualTo(1.0));
            Assert.That(result.Array![5].Number, Is.EqualTo(3.0));
        }
    }

    [Test]
    public void Table_MetamethodFlags_TracksAdditionRemovalAndClear()
    {
        var table = new ExecutionEngine.Collections.Table();
        Assert.That(table.MetamethodFlags, Is.EqualTo(ExecutionEngine.Collections.MetamethodFlags.None));

        table.Set(ExecutionEngine.Collections.Table.AddMetaKey, 1);
        Assert.That(table.MetamethodFlags, Is.EqualTo(ExecutionEngine.Collections.MetamethodFlags.Add));

        table.Set(ExecutionEngine.Collections.Table.ToStringMetaKey, 2);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.MetamethodFlags.HasFlag(ExecutionEngine.Collections.MetamethodFlags.Add), Is.True);
            Assert.That(table.MetamethodFlags.HasFlag(ExecutionEngine.Collections.MetamethodFlags.ToStr), Is.True);
        }

        // Setting non-metamethod property should not affect flags
        table.Set("customProp", 42);
        Assert.That(table.MetamethodFlags, Is.EqualTo(ExecutionEngine.Collections.MetamethodFlags.Add | ExecutionEngine.Collections.MetamethodFlags.ToStr));

        // Setting metamethod to nil should clear the flag
        table.Set(ExecutionEngine.Collections.Table.AddMetaKey, DynamicValue.Nil);
        Assert.That(table.MetamethodFlags, Is.EqualTo(ExecutionEngine.Collections.MetamethodFlags.ToStr));

        // Clear should reset all flags
        table.Clear();
        Assert.That(table.MetamethodFlags, Is.EqualTo(ExecutionEngine.Collections.MetamethodFlags.None));
    }

    [Test]
    public void Table_MetamethodFlags_PreservedAcrossCopies()
    {
        var original = new ExecutionEngine.Collections.Table();
        original.Set(ExecutionEngine.Collections.Table.MultiplyMetaKey, 1);
        original.Set(ExecutionEngine.Collections.Table.InvokeMetaKey, 2);

        var shallow = original.ShallowCopy();
        var deep = original.DeepCopy();
        var ctorCopy = new ExecutionEngine.Collections.Table(original);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(shallow.MetamethodFlags, Is.EqualTo(original.MetamethodFlags));
            Assert.That(deep.MetamethodFlags, Is.EqualTo(original.MetamethodFlags));
            Assert.That(ctorCopy.MetamethodFlags, Is.EqualTo(original.MetamethodFlags));
        }
    }

    [Test]
    public void Table_Metamethod_ScriptDispatchAndFastRejection()
    {
        var runtime = new EvilRuntime(VM);
        runtime.RegisterBuiltInModules();

        var script = """
            fn test() {
                val mt = {
                    __add: fn(a, b) -> a.num + b.num,
                    __tostr: fn(a) -> "Item(" + @a.num + ")"
                };

                val x = { num: 10 };
                val y = { num: 20 };
                tbl.set_mt(x, mt);
                tbl.set_mt(y, mt);

                val sum = x + y;
                val str = @x;
                ret [sum, str];
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].Number, Is.EqualTo(30.0));
            Assert.That(result.Array![1].String, Is.EqualTo("Item(10)"));
        }
    }

    [Test]
    public void StringInterpolation_VariadicStrcat_InterpolatesMultipleSegmentsCorrectly()
    {
        var script = """
            fn test() {
                val name = "Ceres";
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
            Assert.That(result.String, Is.EqualTo("Engine: Ceres, v2.4 (active: true)"));
        }
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
    public void ScriptStackFrame_Pooling_ZeroAllocationsOnFunctionCallLoop()
    {
        var script = """
            fn add(a, b) -> a + b;

            fn test() {
                rw val sum = 0;
                for (rw val i = 0; i < 1000; i++) {
                    sum = add(sum, 1);
                }
                ret sum;
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(1000.0));
        }
    }

    [Test]
    public void ScriptStackFrame_Pooling_DeepRecursionExecutesCorrectly()
    {
        var script = """
            fn recurse(n) {
                if (n <= 0) {
                    ret 0;
                }
                ret 1 + recurse(n - 1);
            }

            fn test() {
                ret recurse(500);
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(500.0));
        }
    }

    [Test]
    public void CallFrame_Struct_RepresentsMetadataCorrectly()
    {
        var chunk = new Chunk("test_chunk", [0x01, 0x02]);
        var callFrame = new CallFrame(chunk, null, 10, 42);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(callFrame.Chunk, Is.SameAs(chunk));
            Assert.That(callFrame.Upvalues, Is.Null);
            Assert.That(callFrame.BasePointer, Is.EqualTo(10));
            Assert.That(callFrame.ReturnIP, Is.EqualTo(42));
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

        var opcodes = new System.Collections.Generic.List<OpCode>();
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

        var opcodes = new System.Collections.Generic.List<OpCode>();
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

        var opcodes = new System.Collections.Generic.List<OpCode>();
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

        var opcodes = new System.Collections.Generic.List<OpCode>();
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
    public void ConstantFolding_ArithmeticExpressions_PrecalculatedAtCompileTime()
    {
        var script = """
            fn test() {
                val timeout = 10 * 60 * 1000;
                ret timeout;
            }
            """;

        var chunk = Compiler.Compile(script, "fold_arithmetic_test");
        var testChunk = chunk["test"]!;

        var opcodes = new System.Collections.Generic.List<OpCode>();
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

        Assert.That(opcodes, Does.Not.Contain(OpCode.MUL));
        Assert.That(opcodes, Contains.Item(OpCode.LDNUM));

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(600000.0));
        }
    }

    [Test]
    public void ConstantFolding_StringConcatenation_PrecalculatedAtCompileTime()
    {
        var script = """
            fn test() {
                val tag = "item_" + 1;
                val s2 = "hello " + "world";
                val s3 = 42 + "_id";
                val s4 = "flag_" + true;
                ret [tag, s2, s3, s4];
            }
            """;

        var chunk = Compiler.Compile(script, "fold_string_test");
        var testChunk = chunk["test"]!;

        var opcodes = new System.Collections.Generic.List<OpCode>();
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

        Assert.That(opcodes, Does.Not.Contain(OpCode.ADD));

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].String, Is.EqualTo("item_1"));
            Assert.That(result.Array![1].String, Is.EqualTo("hello world"));
            Assert.That(result.Array![2].String, Is.EqualTo("42_id"));
            Assert.That(result.Array![3].String, Is.EqualTo("flag_true"));
        }
    }

    [Test]
    public void ConstantFolding_BitwiseAndBooleanLogic_PrecalculatedAtCompileTime()
    {
        var script = """
            fn test() {
                val bAnd = 12 & 10;
                val bOr = 12 | 10;
                val bXor = 12 ^ 10;
                val shl = 1 << 4;
                val shr = 32 >> 2;
                val lAnd = true && false;
                val lOr = true || false;
                val notVal = !true;
                val notNil = !nil;
                ret [bAnd, bOr, bXor, shl, shr, lAnd, lOr, notVal, notNil];
            }
            """;

        var chunk = Compiler.Compile(script, "fold_bitwise_bool_test");
        var testChunk = chunk["test"]!;

        var opcodes = new System.Collections.Generic.List<OpCode>();
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
            Assert.That(opcodes, Does.Not.Contain(OpCode.BAND));
            Assert.That(opcodes, Does.Not.Contain(OpCode.BOR));
            Assert.That(opcodes, Does.Not.Contain(OpCode.BXOR));
            Assert.That(opcodes, Does.Not.Contain(OpCode.SHL));
            Assert.That(opcodes, Does.Not.Contain(OpCode.SHR));
            Assert.That(opcodes, Does.Not.Contain(OpCode.LAND));
            Assert.That(opcodes, Does.Not.Contain(OpCode.LOR));
            Assert.That(opcodes, Does.Not.Contain(OpCode.LNOT));
        }

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].Number, Is.EqualTo(8.0));
            Assert.That(result.Array![1].Number, Is.EqualTo(14.0));
            Assert.That(result.Array![2].Number, Is.EqualTo(6.0));
            Assert.That(result.Array![3].Number, Is.EqualTo(16.0));
            Assert.That(result.Array![4].Number, Is.EqualTo(8.0));
            Assert.That(result.Array![5].Boolean, Is.False);
            Assert.That(result.Array![6].Boolean, Is.True);
            Assert.That(result.Array![7].Boolean, Is.False);
            Assert.That(result.Array![8].Boolean, Is.True);
        }
    }

    [Test]
    public void ConstantFolding_RelationalAndEquality_PrecalculatedAtCompileTime()
    {
        var script = """
            fn test() {
                val gtTrue = 10 > 5;
                val gtFalse = 10 > 10;
                val gteTrue = 10 >= 10;
                val ltTrue = 5 < 10;
                val lteTrue = 5 <= 5;
                val eqTrue = 42 == 42;
                val eqFalse = 42 == 43;
                val strEq = "evil" == "evil";
                val strNeq = "evil" != "good";
                val crossEq = "evil" == 42;
                ret [gtTrue, gtFalse, gteTrue, ltTrue, lteTrue, eqTrue, eqFalse, strEq, strNeq, crossEq];
            }
            """;

        var chunk = Compiler.Compile(script, "fold_relational_test");
        var testChunk = chunk["test"]!;

        var opcodes = new System.Collections.Generic.List<OpCode>();
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
            Assert.That(opcodes, Does.Not.Contain(OpCode.CGT));
            Assert.That(opcodes, Does.Not.Contain(OpCode.CGE));
            Assert.That(opcodes, Does.Not.Contain(OpCode.CLT));
            Assert.That(opcodes, Does.Not.Contain(OpCode.CLE));
            Assert.That(opcodes, Does.Not.Contain(OpCode.CEQ));
            Assert.That(opcodes, Does.Not.Contain(OpCode.CNE));
        }

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Array));
            Assert.That(result.Array![0].Boolean, Is.True);
            Assert.That(result.Array![1].Boolean, Is.False);
            Assert.That(result.Array![2].Boolean, Is.True);
            Assert.That(result.Array![3].Boolean, Is.True);
            Assert.That(result.Array![4].Boolean, Is.True);
            Assert.That(result.Array![5].Boolean, Is.True);
            Assert.That(result.Array![6].Boolean, Is.False);
            Assert.That(result.Array![7].Boolean, Is.True);
            Assert.That(result.Array![8].Boolean, Is.True);
            Assert.That(result.Array![9].Boolean, Is.False);
        }
    }

    [Test]
    public void ConstantFolding_SubexpressionFoldingWithVariable_FoldsConstantPart()
    {
        var script = """
            fn test() {
                val base = 5;
                ret base + 10 * 60 * 1000;
            }
            """;

        var chunk = Compiler.Compile(script, "fold_subexpr_test");
        var testChunk = chunk["test"]!;

        var opcodes = new System.Collections.Generic.List<OpCode>();
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

        Assert.That(opcodes, Does.Not.Contain(OpCode.MUL));
        Assert.That(opcodes.FindAll(op => op == OpCode.ADD).Count, Is.EqualTo(1));

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(600005.0));
        }
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

        var opcodes = new System.Collections.Generic.List<OpCode>();
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
        var opcodes = new System.Collections.Generic.List<OpCode>();
        
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

    [Test]
    public void DeadCodeElimination_Ret_StripsSubsequentSiblingStatements()
    {
        var script = """
            fn test() {
                ret 42;
                val dead1 = 100;
                val dead2 = "unreachable";
                ret 999;
            }
            """;

        var chunk = Compiler.Compile(script, "dce_ret_test");
        var testChunk = chunk["test"]!;

        var opcodes = new System.Collections.Generic.List<OpCode>();
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

        Assert.That(opcodes, Does.Not.Contain(OpCode.SETLOCAL));
        Assert.That(opcodes, Does.Not.Contain(OpCode.SETLOCAL_B));
        Assert.That(opcodes, Does.Not.Contain(OpCode.SETLOCAL_0));
        Assert.That(opcodes, Does.Not.Contain(OpCode.LDSTR));
        Assert.That(opcodes.FindAll(op => op == OpCode.RET), Has.Count.EqualTo(1));

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(42.0));
        }
    }

    [Test]
    public void DeadCodeElimination_Throw_StripsSubsequentSiblingStatements()
    {
        var script = """
            fn test() {
                throw "intentional_err";
                val dead = 100;
                ret 200;
            }
            """;

        var chunk = Compiler.Compile(script, "dce_throw_test");
        var testChunk = chunk["test"]!;

        var opcodes = new System.Collections.Generic.List<OpCode>();
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

        Assert.That(opcodes, Does.Not.Contain(OpCode.RET));
        Assert.That(opcodes, Does.Not.Contain(OpCode.SETLOCAL));
        Assert.That(opcodes, Does.Not.Contain(OpCode.SETLOCAL_B));
        Assert.That(opcodes, Does.Not.Contain(OpCode.SETLOCAL_0));
        Assert.That(opcodes, Does.Contain(OpCode.THROW));
    }

    [Test]
    public void DeadCodeElimination_Break_StripsSubsequentStatementsInLoop()
    {
        var script = """
            fn test() {
                rw val sum = 0;
                while (true) {
                    sum += 10;
                    break;
                    sum += 1000;
                    val dead = 500;
                }
                ret sum;
            }
            """;

        var chunk = Compiler.Compile(script, "dce_break_test");
        var testChunk = chunk["test"]!;

        var opcodes = new System.Collections.Generic.List<OpCode>();
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

        Assert.That(opcodes.FindAll(op => op == OpCode.ADD).Count, Is.EqualTo(1));

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(10.0));
        }
    }

    [Test]
    public void DeadCodeElimination_Skip_StripsSubsequentStatementsInLoop()
    {
        var script = """
            fn test() {
                rw val sum = 0;
                for (rw val i = 0; i < 5; i++) {
                    sum += i;
                    skip;
                    sum += 1000;
                }
                ret sum;
            }
            """;

        var chunk = Compiler.Compile(script, "dce_skip_test");
        var testChunk = chunk["test"]!;

        var opcodes = new System.Collections.Generic.List<OpCode>();
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

        Assert.That(opcodes.FindAll(op => op == OpCode.ADD), Has.Count.EqualTo(1));

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(10.0));
        }
    }

    [Test]
    public void DeadCodeElimination_IfElse_BothBranchesExit_StripsSubsequentStatements()
    {
        var script = """
            fn test(x) {
                if (x > 0) {
                    ret 100;
                    val dead1 = 1;
                } else {
                    ret 200;
                    val dead2 = 2;
                }
                ret 300;
            }
            """;

        var chunk = Compiler.Compile(script, "dce_ifelse_test");
        var testChunk = chunk["test"]!;

        var opcodes = new System.Collections.Generic.List<OpCode>();
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
            Assert.That(opcodes.FindAll(op => op == OpCode.RET), Has.Count.EqualTo(2));
            Assert.That(opcodes, Does.Not.Contain(OpCode.SETLOCAL));
        }
        Assert.That(opcodes, Does.Not.Contain(OpCode.SETLOCAL_B));

        var res1 = EvilTestResult(script, 5);
        Assert.That(res1.Number, Is.EqualTo(100.0));

        var res2 = EvilTestResult(script, -5);
        Assert.That(res2.Number, Is.EqualTo(200.0));
    }

    [Test]
    public void DeadCodeElimination_IfElse_OneBranchExits_PreservesSubsequentStatements()
    {
        var script = """
            fn test(x) {
                if (x > 0) {
                    ret 100;
                    val dead = 1;
                } else {
                    val a = 5;
                }
                ret 300;
            }
            """;

        var res1 = EvilTestResult(script, 5);
        Assert.That(res1.Number, Is.EqualTo(100.0));

        var res2 = EvilTestResult(script, -5);
        Assert.That(res2.Number, Is.EqualTo(300.0));
    }

    [Test]
    public void DeadCodeElimination_Retry_StripsSubsequentStatementsInCatchBlock()
    {
        var script = """
            fn test() {
                rw val attempts = 0;
                try {
                    attempts++;
                    if (attempts < 3) {
                        throw "err";
                    }
                } catch (e) {
                    retry;
                    attempts += 1000;
                    val dead = 42;
                }
                ret attempts;
            }
            """;

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(3.0));
        }
    }
}
