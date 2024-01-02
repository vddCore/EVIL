using Ceres.ExecutionEngine.Collections;
using Ceres.ExecutionEngine.Collections.Serialization;
using Ceres.ExecutionEngine.TypeSystem;
using Ceres.TranslationEngine;
using EVIL.CommonTypes.TypeSystem;
using NUnit.Framework;
using Shouldly;
using Array = Ceres.ExecutionEngine.Collections.Array;

namespace Ceres.SerializationTests
{
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
                    .ShouldBeEquivalentTo(stringValue);;
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
            var chunk = compiler.Compile("fn test() -> 1234;", "unit.test/TestFileName.vil");
            var chunkValue = new DynamicValue(chunk);

            using (var ms = new MemoryStream())
            {
                chunkValue.Serialize(ms);
                ms.Seek(0, SeekOrigin.Begin);

                DynamicValue.Deserialize(ms).ShouldBeEquivalentTo(chunkValue);
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
        public void NativeFunctionSerializeShouldThrow()
        {
            Should.Throw<SerializationException>(() =>
            {
                var nativeFunctionValue = new DynamicValue((f, args) =>
                {
                    return DynamicValue.Nil;
                });

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
    }
}