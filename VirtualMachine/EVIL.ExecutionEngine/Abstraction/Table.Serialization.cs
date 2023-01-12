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
        
        public void Serialize(Stream outStream)
        {
            using (var bw = new BinaryWriter(outStream))
            {
                bw.Write(Magic);
                bw.Write(Entries.Count);

                foreach (var kvp in Entries)
                {
                    var (key, val) = kvp;
                    WriteDynamicValue(bw, key);
                    WriteDynamicValue(bw, val);
                }
            }
        }

        private static void WriteDynamicValue(BinaryWriter bw, DynamicValue value)
        {
            switch (value.Type)
            {
                case DynamicValueType.Number:
                    WriteNumber(bw, value.Number);
                    break;
                
                case DynamicValueType.String:
                    WriteString(bw, value.String);
                    break;
                
                case DynamicValueType.Function:
                    WriteFunction(bw, value.Function);
                    break;
                
                case DynamicValueType.ClrFunction:
                    WriteClrFunction(bw, value.ClrFunction);
                    break;
                
                case DynamicValueType.Table:
                    WriteTable(bw, value.Table);
                    break;
            }
        }

        private static void WriteNumber(BinaryWriter bw, double number)
        {
            bw.Write((byte)DynamicValueType.Number);
            bw.Write(number);
        }

        private static void WriteString(BinaryWriter bw, string str)
        {
            bw.Write((byte)DynamicValueType.String);

            var bytes = Encoding.UTF8.GetBytes(str);
            bw.Write(bytes.Length);
            bw.Write(bytes);
        }

        private static void WriteFunction(BinaryWriter bw, Chunk chunk)
        {
            bw.Write((byte)DynamicValueType.Function);
            Serializer.WriteChunk(bw, chunk);
        }

        private static void WriteClrFunction(BinaryWriter bw, ClrFunction clrFunction)
        {
            bw.Write((byte)DynamicValueType.ClrFunction);
            var declaringTypeName = clrFunction.Method.DeclaringType!.FullName;
            var methodName = clrFunction.Method.Name;
            
            WriteString(bw, declaringTypeName);
            WriteString(bw, methodName);
        }

        private static void WriteTable(BinaryWriter bw, Table table)
        {
            bw.Write((byte)DynamicValueType.Table);
            using (var ms = new MemoryStream())
            {
                table.Serialize(ms);

                var data = ms.ToArray();
                bw.Write(data.Length);
                bw.Write(data);
            }
        }
    }
}