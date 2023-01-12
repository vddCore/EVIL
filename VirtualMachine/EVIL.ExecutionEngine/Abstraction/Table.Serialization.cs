using System.IO;
using System.Text;
using EVIL.ExecutionEngine.Interop;
using EVIL.Intermediate.CodeGeneration;
using EVIL.Intermediate.Storage;

namespace EVIL.ExecutionEngine.Abstraction
{
    public partial class Table
    {
        private static readonly byte[] Magic = { 0x45, 0x54, 0x42, 0x46 };

        public void Serialize(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                Serialize(fs);
            }
        }
        
        public void Serialize(Stream outStream)
        {
            lock (_lock)
            {
                using (var bw = new BinaryWriter(outStream))
                {
                    bw.Write(Magic);
                    bw.Write(_entries.Count);

                    foreach (var kvp in _entries)
                    {
                        var (key, val) = kvp;
                        WriteDynamicValue(bw, key);
                        WriteDynamicValue(bw, val);
                    }
                }
            }
        }

        private static void WriteDynamicValue(BinaryWriter bw, DynamicValue value)
        {
            switch (value.Type)
            {
                case DynamicValueType.Number:
                    WriteMember(bw, value.Number);
                    break;

                case DynamicValueType.String:
                    WriteMember(bw, value.String);
                    break;

                case DynamicValueType.Function:
                    WriteMember(bw, value.Function);
                    break;

                case DynamicValueType.ClrFunction:
                    WriteMember(bw, value.ClrFunction);
                    break;

                case DynamicValueType.Table:
                    WriteMember(bw, value.Table);
                    break;
                
                case DynamicValueType.Null:
                    WriteMember(bw);
                    break;
            }
        }

        private static void WriteMember(BinaryWriter bw, double number)
        {
            bw.Write((byte)DynamicValueType.Number);
            bw.Write(number);
        }

        private static void WriteMember(BinaryWriter bw, string str)
        {
            bw.Write((byte)DynamicValueType.String);
            Serializer.WriteString(bw, str);
        }

        private static void WriteMember(BinaryWriter bw)
        {
            bw.Write((byte)DynamicValueType.Null);
        }

        private static void WriteMember(BinaryWriter bw, Chunk chunk)
        {
            bw.Write((byte)DynamicValueType.Function);
            Serializer.WriteChunk(bw, chunk);
        }

        private static void WriteMember(BinaryWriter bw, ClrFunction clrFunction)
        {
            bw.Write((byte)DynamicValueType.ClrFunction);
            var declaringTypeName = clrFunction.Method.DeclaringType!.FullName;
            var methodName = clrFunction.Method.Name;

            Serializer.WriteString(bw, declaringTypeName);
            Serializer.WriteString(bw, methodName);
        }

        private static void WriteMember(BinaryWriter bw, Table table)
        {
            bw.Write((byte)DynamicValueType.Table);
            table.Serialize(bw.BaseStream);
        }
    }
}