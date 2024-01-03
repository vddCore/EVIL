using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Ceres.ExecutionEngine.Concurrency;
using Ceres.ExecutionEngine.Diagnostics;
using Ceres.ExecutionEngine.TypeSystem;
using EVIL.CommonTypes.TypeSystem;

namespace Ceres.ExecutionEngine.Collections.Serialization
{
    internal static class DynamicValueSerializer
    {
        public static void Serialize(DynamicValue dynamicValue, Stream stream, bool throwOnUnsupported = false)
        {
            if (dynamicValue.Type == DynamicValueType.Fiber)
            {
                if (throwOnUnsupported)
                {
                    throw new SerializationException(
                        $"Serialization of values of type '{dynamicValue.Type}' is not supported."
                    );
                }

                return;
            }

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

                    case DynamicValueType.TypeCode:
                        bw.Write((byte)dynamicValue.TypeCode);
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

                    case DynamicValueType.NativeFunction:
                    {
                        var nativeFunc = dynamicValue.NativeFunction!;

                        if (!nativeFunc.Method.IsStatic)
                        {
                            throw new SerializationException(
                                "Serialization of non-static native functions is not supported."
                            );
                        }

                        if (nativeFunc.Method.DeclaringType == null)
                        {
                            throw new SerializationException(
                                "A serialized native function must have a declaring type."
                            );
                        }

                        var fullTypeName = nativeFunc.Method.DeclaringType!.AssemblyQualifiedName;
                        var methodName = nativeFunc.Method.Name;

                        bw.Write(fullTypeName!);
                        bw.Write(methodName);

                        break;
                    }

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
                }
            }
        }

        public static DynamicValue Deserialize(Stream stream, bool throwOnErrors = false)
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

                    case DynamicValueType.TypeCode:
                        return new DynamicValue((DynamicValueType)br.ReadByte());

                    case DynamicValueType.Chunk:
                        return DeserializeChunk(br);

                    case DynamicValueType.NativeFunction:
                        return DeserializeNativeFunction(br);

                    case DynamicValueType.NativeObject:
                        return DeserializeNativeObject(br);
                }

                if (throwOnErrors)
                {
                    throw new SerializationException(
                        $"Deserialization of dynamic values of type '{type}' is not supported."
                    );
                }
                else return DynamicValue.Nil;
            }
        }

        private static DynamicValue DeserializeChunk(BinaryReader br)
        {
            try
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
            }
            catch (Exception e)
            {
                throw new SerializationException(
                    $"Failed to deserialize a function.",
                    e
                );
            }
        }

        private static DynamicValue DeserializeNativeFunction(BinaryReader br)
        {
            var typeName = br.ReadString();
            var methodName = br.ReadString();

            var nativeType = Type.GetType(typeName);
            
            if (nativeType == null)
            {
                throw new SerializationException(
                    $"Failed to resolve native type '{typeName}'."
                );
            }

            var method = nativeType.GetMethod(
                methodName,
                BindingFlags.Static
                | BindingFlags.Public
                | BindingFlags.NonPublic
            );

            if (method == null)
            {
                throw new SerializationException(
                    $"Failed to resolve method '{methodName}' in native type '{typeName}."
                );
            }

            try
            {
                return new DynamicValue(
                    (NativeFunction)Delegate.CreateDelegate(typeof(NativeFunction), method)
                );
            }
            catch
            {
                throw new SerializationException(
                    $"Failed to bind the method '{methodName}' in native type '{typeName}'."
                );
            }
        }

        private static DynamicValue DeserializeNativeObject(BinaryReader br)
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
}