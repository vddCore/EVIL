namespace EVIL.Assessment.Tests.VM.Optimization;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EVIL.Assessment.Tests.Base;
using EVIL.Core.Common.TypeSystem;
using EVIL.VM.ExecutionEngine.Collections;
using EVIL.VM.ExecutionEngine.Diagnostics;
using EVIL.VM.ExecutionEngine.Diagnostics.Disassembly;
using EVIL.VM.ExecutionEngine.TypeSystem;
using EVIL.VM.Runtime;
using NUnit.Framework;

[TestFixture]
public class TableTests : EvilTest
{

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
                val t = { name: "EVIL VM", version: 2.4 };
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
            Assert.That(result.Array![2].String, Is.EqualTo("EVIL VM"));
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
        var table = new Table();
        table.Set(0, 100);
        table.Set(1, 200);
        table.Set("name", "EVIL VM");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.TryGetValue(0, out var v0), Is.True);
            Assert.That(v0.Number, Is.EqualTo(100.0));

            Assert.That(table.TryGetValue(1, out var v1), Is.True);
            Assert.That(v1.Number, Is.EqualTo(200.0));

            Assert.That(table.TryGetValue("name", out var vName), Is.True);
            Assert.That(vName.String, Is.EqualTo("EVIL VM"));

            Assert.That(table.TryGetValue(2, out var v2), Is.False);
            Assert.That(v2, Is.EqualTo(DynamicValue.Nil));

            Assert.That(table.TryGetValue("nonexistent", out var vNonexistent), Is.False);
            Assert.That(vNonexistent, Is.EqualTo(DynamicValue.Nil));
        }
    }

    [Test]
    public void Table_TryGetValue_SetToNil_ReturnsFalse()
    {
        var table = new Table();
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
        var table = new Table();
        Assert.That(table.MetamethodFlags, Is.EqualTo(MetamethodFlags.None));

        table.Set(Table.AddMetaKey, 1);
        Assert.That(table.MetamethodFlags, Is.EqualTo(MetamethodFlags.Add));

        table.Set(Table.ToStringMetaKey, 2);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.MetamethodFlags.HasFlag(MetamethodFlags.Add), Is.True);
            Assert.That(table.MetamethodFlags.HasFlag(MetamethodFlags.ToStr), Is.True);
        }

        table.Set("customProp", 42);
        Assert.That(table.MetamethodFlags, Is.EqualTo(MetamethodFlags.Add | MetamethodFlags.ToStr));

        table.Set(Table.AddMetaKey, DynamicValue.Nil);
        Assert.That(table.MetamethodFlags, Is.EqualTo(MetamethodFlags.ToStr));

        table.Clear();
        Assert.That(table.MetamethodFlags, Is.EqualTo(MetamethodFlags.None));
    }

    [Test]
    public void Table_MetamethodFlags_PreservedAcrossCopies()
    {
        var original = new Table();
        original.Set(Table.MultiplyMetaKey, 1);
        original.Set(Table.InvokeMetaKey, 2);

        var shallow = original.ShallowCopy();
        var deep = original.DeepCopy();
        var ctorCopy = new Table(original);

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
    public void Table_LazyStorage_PureArrayAndEmptyTables_PreserveZeroHashPart()
    {
        var table = new Table();
        Assert.That(table.Length, Is.EqualTo(0));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table.TryGetValue("nonexistent", out var val), Is.False);
            Assert.That(val, Is.EqualTo(DynamicValue.Nil));
            Assert.That(table.Contains("nonexistent"), Is.False);
        }

        table.Set(0, 10);
        table.Set(1, 20);
        table.Set(2, 30);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(table, Has.Length.EqualTo(3));
            Assert.That(table.TryGetValue(0, out var v0), Is.True);
            Assert.That(v0.Number, Is.EqualTo(10.0));
            Assert.That(table.TryGetValue(1, out var v1), Is.True);
            Assert.That(v1.Number, Is.EqualTo(20.0));
            Assert.That(table.TryGetValue(2, out var v2), Is.True);
            Assert.That(v2.Number, Is.EqualTo(30.0));
        }

        var keys = table.GetKeys();
        var values = table.GetValues();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(keys, Has.Length.EqualTo(3));
            Assert.That(values, Has.Length.EqualTo(3));
            Assert.That(values[0].Number, Is.EqualTo(10.0));
            Assert.That(values[1].Number, Is.EqualTo(20.0));
            Assert.That(values[2].Number, Is.EqualTo(30.0));
        }

        var enumerated = new List<KeyValuePair<DynamicValue, DynamicValue>>(table);
        Assert.That(enumerated, Has.Count.EqualTo(3));

        table.Set("foo", "bar");
        using (Assert.EnterMultipleScope())
        {
            Assert.That(table, Has.Length.EqualTo(4));
            Assert.That(table.TryGetValue("foo", out var vFoo), Is.True);
            Assert.That(vFoo.String, Is.EqualTo("bar"));
        }
    }

    [Test]
    public void Table_ScalableArrayPart_PowerOfTwoGrowthUpTo65536()
    {
        var table = new Table();
        const int elementCount = 4096;

        for (var i = 0; i < elementCount; i++)
        {
            table.Set(i, i * 3);
        }

        Assert.That(table, Has.Length.EqualTo(elementCount));

        for (var i = 0; i < elementCount; i++)
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(table.TryGetValue(i, out var v), Is.True);
                Assert.That(v.Number, Is.EqualTo(i * 3.0));
            }
        }

        table.Set(100, DynamicValue.Nil);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(table, Has.Length.EqualTo(elementCount - 1));
            Assert.That(table.TryGetValue(100, out _), Is.False);
            Assert.That(table.TryGetValue(101, out var v101), Is.True);
            Assert.That(v101.Number, Is.EqualTo(101 * 3.0));
        }
    }

    [Test]
    public void Table_MemoryFootprint_EmptyTables_SignificantReduction()
    {
        const int tableCount = 100_000;
        var tables = new Table[tableCount];

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var memBefore = GC.GetTotalMemory(true);

        for (var i = 0; i < tableCount; i++)
        {
            tables[i] = new Table();
        }

        var memAfter = GC.GetTotalMemory(false);
        var bytesUsed = memAfter - memBefore;

        GC.KeepAlive(tables);
        
        // lazy storage yay.
        Assert.That(bytesUsed, Is.LessThan(12 * 1024 * 1024), $"Used {bytesUsed / (1024.0 * 1024.0):F2} MB");
    }

    [Test]
    public void Table_BytecodeSpecialization_GetPropAndSetProp_EmittedAndExecuted()
    {
        var script = """
            fn test() {
                val point = { x: 10, y: 20 };
                point.x = 42;
                point.y += 8;
                ret point.x + point.y;
            }
            """;

        var chunk = Compiler.Compile(script, "getprop_setprop_test");
        var testChunk = chunk["test"]!;

        var sw = new StringWriter();
        Disassembler.Disassemble(testChunk, sw);
        var disasm = sw.ToString();

        Assert.That(disasm, Does.Contain("GETPROP_B").Or.Contain("GETPROP"));
        Assert.That(disasm, Does.Contain("SETPROP_B").Or.Contain("SETPROP"));

        var disassembly = InstructionDecoder.Decode(testChunk);
        var getPropInstr = disassembly.Instructions.FirstOrDefault(i => i.OpCode is OpCode.GETPROP_B or OpCode.GETPROP);
        Assert.That(getPropInstr, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(getPropInstr!.Operands, Has.Count.EqualTo(1));
            Assert.That(getPropInstr.Operands[0].Kind, Is.EqualTo(OperandKind.StringPoolIndex));
        }

        var setPropInstr = disassembly.Instructions.FirstOrDefault(i => i.OpCode is OpCode.SETPROP_B or OpCode.SETPROP);
        Assert.That(setPropInstr, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(setPropInstr!.Operands, Has.Count.EqualTo(1));
            Assert.That(setPropInstr.Operands[0].Kind, Is.EqualTo(OperandKind.StringPoolIndex));
        }

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(70.0));
        }
    }

    [Test]
    public void Table_BytecodeSpecialization_SelfInvocation_EmitsGetProp()
    {
        var script = """
            fn test() {
                val calculator = {
                    baseVal: 100,
                    add: fn(self, amount) {
                        ret self.baseVal + amount;
                    }
                };
                ret calculator:add(50);
            }
            """;

        var chunk = Compiler.Compile(script, "self_inv_test");
        var testChunk = chunk["test"]!;

        var sw = new StringWriter();
        Disassembler.Disassemble(testChunk, sw);
        var disasm = sw.ToString();

        Assert.That(disasm, Does.Contain("GETPROP_B").Or.Contain("GETPROP"));

        var result = EvilTestResult(script);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(150.0));
        }
    }

    [Test]
    public void Table_BytecodeSpecialization_MetatableInterception_Preserved()
    {
        var script = """
            fn test(t) {
                ret t.customProp;
            }
            """;

        var mt = new Table();
        var handlerChunk = Compiler.Compile("fn get_handler(t, k) -> 999;", "handler_chunk");
        mt.Set(Table.GetMetaKeyVal, handlerChunk["get_handler"]!);

        var t = new Table
        {
            MetaTable = mt
        };

        var result = EvilTestResult(script, t);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Type, Is.EqualTo(DynamicValueType.Number));
            Assert.That(result.Number, Is.EqualTo(999.0));
        }
    }

    [Test]
    public void TableShape_TransitionsAndSlotResolution_SharedDescriptor()
    {
        var empty = TableShape.Empty;
        Assert.That(empty.PropertyCount, Is.EqualTo(0));

        var shapeX1 = empty.Transition("x", out var slotX1);
        var shapeX2 = empty.Transition("x", out var slotX2);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(shapeX1, Is.SameAs(shapeX2));
            Assert.That(slotX1, Is.EqualTo(0));
            Assert.That(slotX2, Is.EqualTo(0));
        }

        var shapeXY = shapeX1.Transition("y", out var slotY);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(shapeXY.PropertyCount, Is.EqualTo(2));
            Assert.That(slotY, Is.EqualTo(1));

            Assert.That(shapeXY.TryGetSlot("x", out var resolvedX), Is.True);
            Assert.That(resolvedX, Is.EqualTo(0));
            Assert.That(shapeXY.TryGetSlot("y", out var resolvedY), Is.True);
            Assert.That(resolvedY, Is.EqualTo(1));
            Assert.That(shapeXY.TryGetSlot("z", out _), Is.False);

            Assert.That(shapeXY.PropertyKeys, Has.Length.EqualTo(2));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(shapeXY.PropertyKeys[0].String, Is.EqualTo("x"));
            Assert.That(shapeXY.PropertyKeys[1].String, Is.EqualTo("y"));
        }
    }

    [Test]
    public void Table_InlineCache_MonomorphicHit_FastPath()
    {
        var script = """
            fn test() {
                val pt = { x: 10, y: 20 };
                rw val sum = 0;
                for (rw val i = 0; i < 1000; i++) {
                    sum += pt.x;
                }
                ret sum;
            }
            """;

        var chunk = Compiler.Compile(script, "ic_mono_test");
        var testChunk = chunk["test"]!;

        VM.MainFiber.Schedule(chunk);
        VM.MainFiber.Schedule(testChunk, false);
        VM.MainFiber.Resume();
        VM.MainFiber.BlockUntilFinished();
        var result = VM.MainFiber.PopValue();

        Assert.That(result.Number, Is.EqualTo(10000.0));

        var disasm = InstructionDecoder.Decode(testChunk);
        var getProp = disasm.Instructions.FirstOrDefault(i => i.OpCode is OpCode.GETPROP_B or OpCode.GETPROP);
        Assert.That(getProp, Is.Not.Null);

        ref var ic = ref testChunk.GetPropertyInlineCache(getProp!.Offset);
        Assert.That(ic.Shape0, Is.Not.Null);
        Assert.That(ic.Slot0, Is.EqualTo(0));
    }

    [Test]
    public void Table_InlineCache_BimorphicHit_FastPath()
    {
        var script = """
            fn readX(t) -> t.x;

            fn test() {
                val p1 = { x: 5, y: 10 };
                val p2 = { x: 15, z: 20 };
                rw val sum = 0;
                for (rw val i = 0; i < 500; i++) {
                    sum += readX(p1);
                    sum += readX(p2);
                }
                ret sum;
            }
            """;

        var chunk = Compiler.Compile(script, "ic_bimorphic_test");
        var readXChunk = chunk["readX"]!;
        var testChunk = chunk["test"]!;

        VM.MainFiber.Schedule(chunk);
        VM.MainFiber.Schedule(testChunk, false);
        VM.MainFiber.Resume();
        VM.MainFiber.BlockUntilFinished();
        var result = VM.MainFiber.PopValue();

        // (5 + 15) * 500 = 10000
        Assert.That(result.Number, Is.EqualTo(10000.0));

        var disasm = InstructionDecoder.Decode(readXChunk);
        var getProp = disasm.Instructions.FirstOrDefault(i => i.OpCode is OpCode.GETPROP_B or OpCode.GETPROP);
        Assert.That(getProp, Is.Not.Null);

        ref var ic = ref readXChunk.GetPropertyInlineCache(getProp!.Offset);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(ic.Shape0, Is.Not.Null);
            Assert.That(ic.Shape1, Is.Not.Null);
            Assert.That(ic.Shape0, Is.Not.EqualTo(ic.Shape1));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(ic.Slot0, Is.EqualTo(0));
            Assert.That(ic.Slot1, Is.EqualTo(0));
        }
    }

    [Test]
    public void Table_InlineCache_SetProp_InPlaceUpdate()
    {
        var script = """
            fn test() {
                val pt = { x: 1, y: 2 };
                for (rw val i = 0; i < 500; i++) {
                    pt.x = i;
                }
                ret pt.x;
            }
            """;

        var result = EvilTestResult(script);
        Assert.That(result.Number, Is.EqualTo(499.0));
    }

    [Test]
    public void Table_InlineCache_DemotionOnNilOrNonString()
    {
        var script = """
            fn test() {
                val pt = { x: 10, y: 20 };
                pt.x = nil;
                ret { hasX: pt.x != nil, y: pt.y, len: #pt };
            }
            """;

        var result = EvilTestResult(script);
        Assert.That(result.Type, Is.EqualTo(DynamicValueType.Table));
        
        var tbl = result.Table!;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(tbl["hasX"].Boolean, Is.False);
            Assert.That(tbl["y"].Number, Is.EqualTo(20.0));
            Assert.That(tbl["len"].Number, Is.EqualTo(1.0));
        }
    }

    [Test]
    public void Table_InlineCache_FrozenTable_RejectsSetProp()
    {
        var script = """
            fn test(pt) {
                pt.x = 999;
                ret pt.x;
            }
            """;

        var pt = new Table { { "x", 10 }, { "y", 20 } };
        pt.Freeze();

        var result = EvilTestResult(script, pt);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.Number, Is.EqualTo(10.0));
            Assert.That(pt["x"].Number, Is.EqualTo(10.0));
        }
    }

    [Test]
    public void Table_StructEnumerator_TraversesAllPhases_ArrayShapeAndHash()
    {
        var t = new Table
        {
            [0] = 100,
            [1] = 200,
            ["x"] = 10,
            ["y"] = 20
        };

        var visited = new List<(DynamicValue Key, DynamicValue Value)>();
        foreach (var kvp in t)
        {
            visited.Add((kvp.Key, kvp.Value));
        }

        Assert.That(visited, Has.Count.EqualTo(4));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(visited[0], Is.EqualTo(((DynamicValue)0.0, (DynamicValue)100.0)));
            Assert.That(visited[1], Is.EqualTo(((DynamicValue)1.0, (DynamicValue)200.0)));
            Assert.That(visited[2].Key.String, Is.EqualTo("x"));
            Assert.That(visited[2].Value.Number, Is.EqualTo(10.0));
            Assert.That(visited[3].Key.String, Is.EqualTo("y"));
            Assert.That(visited[3].Value.Number, Is.EqualTo(20.0));
        }
    }

    [Test]
    public void Table_StructEnumerator_DirectForeach_ZeroAllocations()
    {
        var t = new Table { { "a", 1 }, { "b", 2 }, { "c", 3 }, { "d", 4 } };

        var a = GC.GetAllocatedBytesForCurrentThread();
        var e = t.GetEnumerator();
        var b = GC.GetAllocatedBytesForCurrentThread();
        while (e.MoveNext())
        {
            var k = e.CurrentKey;
            var v = e.CurrentValue;
        }
        var c = GC.GetAllocatedBytesForCurrentThread();
        e.Dispose();
        var d = GC.GetAllocatedBytesForCurrentThread();

        Assert.That(b - a, Is.EqualTo(0), "GetEnumerator allocated");
        Assert.That(c - b, Is.EqualTo(0), "MoveNext allocated");
        Assert.That(d - c, Is.EqualTo(0), "Dispose allocated");

        var dummy = 0.0;
        foreach (var kvp in t)
        {
            dummy += kvp.Value.Number;
        }

        var startAlloc = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 10000; i++)
        {
            foreach (var kvp in t)
            {
                dummy += kvp.Value.Number;
            }
        }
        var endAlloc = GC.GetAllocatedBytesForCurrentThread();

        Assert.That(endAlloc - startAlloc, Is.EqualTo(0), $"foreach allocated {endAlloc - startAlloc}");
        Assert.That(dummy, Is.GreaterThan(0));
    }

    [Test]
    public void Table_EachLoop_ScriptExecution_NestedLoopsAndBreak()
    {
        var script = """
            fn test() {
                val t1 = { a: 1, b: 2 };
                val t2 = { x: 10, y: 20 };
                rw val sum = 0;
                each (rw val k1, v1 : t1) {
                    each (rw val k2, v2 : t2) {
                        sum += v1 * v2;
                        if (k2 == "x") {
                            break;
                        }
                    }
                }
                ret sum;
            }
            """;

        var result = EvilTestResult(script);
        // (1 * 10) + (2 * 10) = 30
        Assert.That(result.Number, Is.EqualTo(30.0));
    }

    [Test]
    public void Table_EachLoop_ScriptExecution_ArrayIteration()
    {
        var script = """
            fn test() {
                val arr = array { 10, 20, 30, 40 };
                rw val sum = 0;
                each (rw val idx, v : arr) {
                    sum += idx + v;
                }
                ret sum;
            }
            """;

        var result = EvilTestResult(script);
        // (0 + 10) + (1 + 20) + (2 + 30) + (3 + 40) = 106
        Assert.That(result.Number, Is.EqualTo(106.0));
    }

    [Test]
    public void Table_EachLoop_ScriptExecution_ZeroGCCollections()
    {
        var script = """
            fn test(t) {
                rw val sum = 0;
                for (rw val i = 0; i < 10000; i++) {
                    each (rw val k, v : t) {
                        sum += v;
                    }
                }
                ret sum;
            }
            """;

        var chunk = Compiler.Compile(script, "zero_gc_test");
        var testChunk = chunk["test"]!;
        var t = new Table { { "a", 1 }, { "b", 2 }, { "c", 3 }, { "d", 4 } };

        VM.MainFiber.Schedule(chunk);
        VM.MainFiber.Schedule(testChunk, false, t);
        VM.MainFiber.Resume();
        VM.MainFiber.BlockUntilFinished();
        var warmupResult = VM.MainFiber.PopValue();
        Assert.That(warmupResult.Number, Is.EqualTo(100000.0));

        var allocBefore = GC.GetAllocatedBytesForCurrentThread();

        VM.MainFiber.Schedule(chunk);
        VM.MainFiber.Schedule(testChunk, false, t);
        VM.MainFiber.Resume();
        VM.MainFiber.BlockUntilFinished();
        var result = VM.MainFiber.PopValue();

        var allocAfter = GC.GetAllocatedBytesForCurrentThread();

        Assert.That(result.Number, Is.EqualTo(100000.0));
        Assert.That(allocAfter - allocBefore, Is.LessThan(1024));
    }
}
