using System;
using System.IO;
using System.Reflection;
using System.Text;
using EVIL.ExecutionEngine.Interop;
using EVIL.Intermediate.CodeGeneration;
using EVIL.Intermediate.Storage;

namespace EVIL.ExecutionEngine.Abstraction
{
    public partial class Table
    {
        public static Table Deserialize(Stream inStream)
            => Deserialize(inStream, false);

        private static Table Deserialize(Stream inStream, bool leaveOpen)
        {
            var table = new Table();

            using (var br = new BinaryReader(inStream, Encoding.UTF8, leaveOpen))
            {
                var magic = br.ReadBytes(4);

                if (magic[0] != 'E'
                    || magic[1] != 'T'
                    || magic[2] != 'B'
                    || magic[3] != 'F')
                {
                    throw new InvalidDataException("Invalid table magic number.");
                }

                var entryCount = br.ReadInt32();

                for (var i = 0; i < entryCount; i++)
                {
                    var key = ReadValue(br);
                    var value = ReadValue(br);

                    table.Set(key, value);
                }
            }

            return table;
        }

        private static DynamicValue ReadValue(BinaryReader br)
        {
            var type = (DynamicValueType)br.ReadByte();

            switch (type)
            {
                case DynamicValueType.Number:
                    return new(ReadNumber(br));
                
                case DynamicValueType.String:
                    return new(ReadString(br));
                
                case DynamicValueType.Function:
                    return new(ReadFunction(br));
                
                case DynamicValueType.ClrFunction:
                    return new(ReadClrFunction(br));
                
                case DynamicValueType.Table:
                    return new(ReadTable(br));
                
                default: 
                    throw new InvalidDataException($"Unknown type '{type}'.");
            }
        }

        private static double ReadNumber(BinaryReader br)
        {
            return br.ReadDouble();
        }

        private static string ReadString(BinaryReader br)
        {
            return Deserializer.ReadString(br);
        }

        private static Chunk ReadFunction(BinaryReader br)
        {
            return Deserializer.ReadChunk(br);
        }

        private static ClrFunction ReadClrFunction(BinaryReader br)
        {
            var declaringTypeName = ReadString(br);
            var methodName = ReadString(br);

            var type = Type.GetType(declaringTypeName);

            if (type == null)
                return null;

            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);

            if (method == null)
                return null;

            if (!method.ReturnType.IsAssignableTo(typeof(DynamicValue)))
                return null;

            var funcAttr = method.GetCustomAttribute<ClrFunctionAttribute>();

            if (funcAttr == null)
                return null;

            return method.CreateDelegate<ClrFunction>();
        }

        private static Table ReadTable(BinaryReader br)
        {
            return Deserialize(br.BaseStream, true);
        }
    }
}