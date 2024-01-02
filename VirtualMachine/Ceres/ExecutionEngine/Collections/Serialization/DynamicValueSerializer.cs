using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using EVIL.CommonTypes.TypeSystem;

namespace Ceres.ExecutionEngine.Collections.Serialization
{
    internal static class DynamicValueSerializer
    {
        public static void Serialize(DynamicValue dynamicValue, Stream stream)
        {
            using (var bw = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                bw.Write((byte)dynamicValue.Type);

                switch (dynamicValue.Type)
                {
                    case DynamicValueType.Nil:
                        break;

                    case DynamicValueType.Number:
                        bw.Write(dynamicValue.Number);
                        break;

                    case DynamicValueType.String:
                        bw.Write(dynamicValue.String!);
                        break;

                    case DynamicValueType.Boolean:
                        bw.Write(dynamicValue.Boolean);
                        break;

                    case DynamicValueType.Table:
                        TableSerializer.Serialize(dynamicValue.Table!, stream);
                        break;

                    case DynamicValueType.Array:
                        ArraySerializer.Serialize(dynamicValue.Array!, stream);
                        break;

                    case DynamicValueType.Chunk:
                    {
                        using (var ms = new MemoryStream())
                        {
                            dynamicValue.Chunk!.Serialize(ms);
                            bw.Write((int)ms.Length);
                            bw.Write(ms.ToArray());
                        }
                        
                        break;
                    }

                    case DynamicValueType.TypeCode:
                        bw.Write((byte)dynamicValue.TypeCode);
                        break;

                    case DynamicValueType.NativeObject:
                    {
                        try
                        {
                            var bf = new BinaryFormatter();

                            using (var ms = new MemoryStream())
                            {
                                bf.Serialize(ms, dynamicValue.NativeObject!);
                                bw.Write((int)ms.Length);
                                bw.Write(ms.ToArray());
                            }
                            
                            break;
                        }
                        catch (Exception e)
                        {
                            throw new SerializationException(
                                $"Failed to serialize the native object '{dynamicValue.NativeObject!.GetType().FullName}",
                                e
                            );
                        }
                    }

                    case DynamicValueType.NativeFunction:
                        throw new SerializationException("Serialization of native functions is not supported.");

                    case DynamicValueType.Fiber:
                        throw new SerializationException("Serialization of fibers is not supported.");
                }
            }
        }

        public static DynamicValue Deserialize(Stream stream)
        {
            using (var br = new BinaryReader(stream, Encoding.UTF8, true))
            {
                var type = (DynamicValueType)br.ReadByte();

                switch (type)
                {
                    case DynamicValueType.Nil:
                        return DynamicValue.Nil;

                    case DynamicValueType.Number:
                        return new DynamicValue(br.ReadDouble());

                    case DynamicValueType.String:
                        return new DynamicValue(br.ReadString());

                    case DynamicValueType.Boolean:
                        return new DynamicValue(br.ReadBoolean());

                    case DynamicValueType.Table:
                        return TableSerializer.Deserialize(stream);

                    case DynamicValueType.Array:
                        return ArraySerializer.Deserialize(stream);

                    case DynamicValueType.Chunk:
                    {
                        var length = br.ReadInt32();
                        var data = br.ReadBytes(length);

                        using (var ms = new MemoryStream(data))
                        {
                            ms.Seek(0, SeekOrigin.Begin);
                            
                            return new DynamicValue(
                                Chunk.Deserialize(ms, out _, out _)
                            );
                        }

                        break;
                    }

                    case DynamicValueType.TypeCode:
                        return new DynamicValue((DynamicValueType)br.ReadByte());

                    case DynamicValueType.NativeObject:
                    {
                        try
                        {
                            var length = br.ReadInt32();
                            var data = br.ReadBytes(length);

                            using (var ms = new MemoryStream(data))
                            {
                                ms.Seek(0, SeekOrigin.Begin);
                                var bf = new BinaryFormatter();
                                var nativeObject = bf.Deserialize(ms);

                                return new DynamicValue(nativeObject);
                            }
                        }
                        catch (Exception e)
                        {
                            throw new SerializationException(
                                "Failed to deserialize the native object.", 
                                e
                            );
                        }
                    }
                }
                
                throw new SerializationException($"Deserialization of dynamic values of type '{type}' is not supported.");
            }
        }
    }
}