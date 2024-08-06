namespace EVIL.Ceres.SerializationTests;

using Array = EVIL.Ceres.ExecutionEngine.Collections.Array;

using System;
using System.IO;
using EVIL.Ceres.ExecutionEngine;
using EVIL.Ceres.ExecutionEngine.Collections;
using EVIL.Ceres.ExecutionEngine.Collections.Serialization;
using EVIL.Ceres.ExecutionEngine.Concurrency;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.Ceres.TranslationEngine;
using EVIL.CommonTypes.TypeSystem;
using NUnit.Framework;
using Shouldly;

public class DynamicValueSerializerTests
{
    [Test]
    public void NilSerializeDeserialize()
    {
        var nilValue = DynamicValue.Nil;

        using (var ms = new MemoryStream())
        {
            nilValue.Serialize(ms);
            ms.Seek(0, SeekOrigin.Begin);

            DynamicValue.Deserialize(ms)
                .ShouldBeEquivalentTo(nilValue);
        }
    }

    [Test]
    public void NumberSerializeDeserialize()
    {
        var numberValue = new DynamicValue(2137);

        using (var ms = new MemoryStream())
        {
            numberValue.Serialize(ms);
            ms.Seek(0, SeekOrigin.Begin);

            DynamicValue.Deserialize(ms)
                .ShouldBeEquivalentTo(numberValue);
        }
    }

    [Test]
    public void StringSerializeDeserialize()
    {
        var stringValue = new DynamicValue("this is a test string 12342137");

        using (var ms = new MemoryStream())
        {
            stringValue.Serialize(ms);
            ms.Seek(0, SeekOrigin.Begin);

            DynamicValue.Deserialize(ms)
                .ShouldBeEquivalentTo(stringValue);
        }
    }

    [Test]
    public void BooleanSerializeDeserialize()
    {
        var booleanValue = new DynamicValue(true);

        using (var ms = new MemoryStream())
        {
            booleanValue.Serialize(ms);
            ms.Seek(0, SeekOrigin.Begin);

            DynamicValue.Deserialize(ms)
                .ShouldBeEquivalentTo(booleanValue);
        }
    }

    [Test]
    public void TableSerializeDeserialize()
    {
        var tableValue = new DynamicValue(new Table()
        {
            ["key"] = "a string value",
            [2137] = "2137 + meow = uwu",
            [true] = false,
            ["a table"] = new Table()
            {
                ["inside a table"] = "inside a table",
                [2.137] = "666"
            }
        });
            
        using (var ms = new MemoryStream())
        {
            tableValue.Serialize(ms);
            ms.Seek(0, SeekOrigin.Begin);
            
            DynamicValue.Deserialize(ms)
                .IsDeeplyEqualTo(tableValue)
                .ShouldBe(true);
        }
    }

    [Test]
    public void ArraySerializeDeserialize()
    {
        var arr = new Array(5);
            
        arr[0] = 1;
        arr[1] = new Table() { ["aaaa"] = 333, ["bbbb"] = "ąśćę" };
        arr[2] = "ąćżęłóń";
        arr[3] = -1234;
        arr[4] = true;
            
        var arrayValue = new DynamicValue(arr);
            
        using (var ms = new MemoryStream())
        {
            arrayValue.Serialize(ms);
            ms.Seek(0, SeekOrigin.Begin);
                
            DynamicValue.Deserialize(ms)
                .IsDeeplyEqualTo(arrayValue)
                .ShouldBe(true);
        }
    }
        
    [Test]
    public void ChunkSerializeDeserialize()
    {
        var compiler = new Compiler();
        var chunk = compiler.Compile("ret 1234;", "unit.test/TestFileName.vil");
        var chunkValue = new DynamicValue(chunk);

        using (var ms = new MemoryStream())
        {
            chunkValue.Serialize(ms);
            ms.Seek(0, SeekOrigin.Begin);

            var deserialized = DynamicValue.Deserialize(ms);

            using (var vm = new CeresVM())
            {
                vm.Start();
                vm.MainFiber.Schedule(deserialized.Chunk!);
                vm.MainFiber.BlockUntilFinished();
                var ret = vm.MainFiber.PopValue();
                vm.Stop();
                ret.ShouldBe(1234);
            }
        }
    }

    [Test]
    public void TypeCodeSerializeDeserialize()
    {
        var typeCodeValue = new DynamicValue(DynamicValueType.Table);

        using (var ms = new MemoryStream())
        {
            typeCodeValue.Serialize(ms);
            ms.Seek(0, SeekOrigin.Begin);

            DynamicValue.Deserialize(ms)
                .ShouldBeEquivalentTo(typeCodeValue);
        }
    }
        
    [Test]
    public void NativeFunctionSerializeDeserialize()
    {
        var nativeFunctionValue = new DynamicValue(TestNativeFunction);
            
        using (var ms = new MemoryStream())
        {
            nativeFunctionValue.Serialize(ms);
            ms.Seek(0, SeekOrigin.Begin);

            var deserialized = DynamicValue.Deserialize(ms);
            deserialized.ShouldBeEquivalentTo(new DynamicValue(TestNativeFunction));

            using (var vm = new CeresVM())
            {
                var compiler = new Compiler();
                var chunk = compiler.Compile("ret testfunc();", "unit.test/TestFileName.vil");
                    
                vm.Start();
                vm.Global["testfunc"] = deserialized;
                vm.MainFiber.Schedule(chunk);
                vm.MainFiber.BlockUntilFinished();
                var ret = vm.MainFiber.PopValue();
                ret.ShouldBe(2137);
            }
        }
    }

    [Test]
    public void NativeFunctionSerializeShouldThrowOnInstanceMethod()
    {
        var nativeFunctionValue = new DynamicValue((_, _) => 2137);

        Should.Throw<SerializationException>(() =>
        {
            using (var ms = new MemoryStream())
            {
                nativeFunctionValue.Serialize(ms);
            }
        });
    }
        
    [Test]
    public void NativeObjectSerializeDeserialize()
    {
        var nativeObjectValue = new DynamicValue(new FormatException());

        using (var ms = new MemoryStream())
        {
            nativeObjectValue.Serialize(ms);
            ms.Seek(0, SeekOrigin.Begin);

            DynamicValue.Deserialize(ms)
                .ShouldBeEquivalentTo(nativeObjectValue);
        }
    }
        
    public static DynamicValue TestNativeFunction(Fiber fiber, params DynamicValue[] args)
    {
        return 2137;
    }
}