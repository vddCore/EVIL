using System.IO;
using System.Linq;
using System.Text;
using EVIL.Intermediate.CodeGeneration;

namespace EVIL.Intermediate.Storage
{
    public static class Serializer
    {
        public static void WriteString(BinaryWriter bw, string s)
        {
            var stringData = Encoding.UTF8.GetBytes(s);
            bw.Write(stringData.Length);
            bw.Write(stringData);
        }
        
        public static void WriteChunk(BinaryWriter bw, Chunk c)
        {
            WriteString(bw, c.Name);

            bw.Write(c.IsPublic);
            
            WriteConstTable(bw, c.Constants);
            
            bw.Write(c.Labels.Count);
            for (var i = 0; i < c.Labels.Count; i++)
            {
                bw.Write(c.Labels[i]);
            }

            bw.Write(c.Parameters.Count);
            for (var i = 0; i < c.Parameters.Count; i++)
            {
                WriteString(bw, c.Parameters[i]);
            }

            bw.Write(c.Locals.Count);
            for (var i = 0; i < c.Locals.Count; i++)
            {
                WriteString(bw, c.Locals[i]);
            }

            bw.Write(c.Externs.Count);
            for (var i = 0; i < c.Externs.Count; i++)
            {
                WriteExtern(bw, c.Externs[i]);
            }

            bw.Write(c.SubChunks.Count);
            for (var i = 0; i < c.SubChunks.Count; i++)
            {
                WriteChunk(bw, c.SubChunks[i]);
            }

            bw.Write(c.Instructions.Count);
            for (var i = 0; i < c.Instructions.Count; i++)
            {
                bw.Write(c.Instructions[i]);
            }
            
            bw.Write(c.DebugInfo.Count);
            for (var i = 0; i < c.DebugInfo.Count; i++)
            {
                WriteDebugEntry(bw, c.DebugInfo.ElementAt(i));
            }
        }
        
        private static void WriteDebugEntry(BinaryWriter bw, DebugEntry de)
        {
            bw.Write(de.Line);
            bw.Write(de.Column);
            bw.Write(de.IP);
        }
        
        private static void WriteConst(BinaryWriter bw, double c)
        {
            bw.Write((byte)1);
            bw.Write(c);
        }
        
        private static void WriteConst(BinaryWriter bw, string c)
        {
            bw.Write((byte)0);
            WriteString(bw, c);
        }
        
        private static void WriteConstTable(BinaryWriter bw, ConstPool constPool)
        {
            bw.Write(constPool.Count);
            for (var i = 0; i < constPool.Count; i++)
            {
                var str = constPool.GetStringConstant(i);

                if (str != null)
                {
                    WriteConst(bw, str);
                }
                else
                {
                    WriteConst(bw, constPool.GetNumberConstant(i));
                }
            }
        }
        
        private static void WriteExtern(BinaryWriter bw, ExternInfo externInfo)
        {
            WriteString(bw, externInfo.Name);
            WriteString(bw, externInfo.OwnerChunkName);
            bw.Write(externInfo.SymbolId);
            bw.Write((byte)externInfo.Type);
        }
    }
}