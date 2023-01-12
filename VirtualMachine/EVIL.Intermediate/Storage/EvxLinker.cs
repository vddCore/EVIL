using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EVIL.Intermediate.CodeGeneration;

namespace EVIL.Intermediate.Storage
{
    public static class EvxLinker
    {
        private static byte[] _linkerData = new byte[32];

        public static readonly byte[] MagicNumber = new byte[] { 0x45, 0x56, 0x58 }; // EVX
        public static readonly byte[] LinkerID = new byte[] { 0x43, 0x56, 0x49, 0x4C }; // CVIL
        public const byte FormatVersion = 1;

        public static void Link(Executable exe, string filePath)
        {
            Link(exe, new FileStream(filePath, FileMode.Create));
        }

        public static void Link(Executable exe, Stream outStream)
        {
            var bw = new BinaryWriter(outStream);
            
            try
            {

                WriteHeader(bw);
                WriteGlobalList(bw, exe.Globals);

                var chunkTableOffset = WriteChunkTable(
                    bw,
                    exe.Chunks
                );

                WriteChunkDataBlock(
                    bw,
                    chunkTableOffset,
                    exe.Chunks
                );

                WriteChecksum(bw, outStream);
            }
            finally
            {
                bw.Dispose();
            }
        }

        private static void WriteHeader(BinaryWriter bw)
        {
            bw.Write(MagicNumber);
            bw.Write(FormatVersion);
            bw.Write(0L);
            bw.Write(DateTimeOffset.Now.ToUnixTimeSeconds());
            bw.Write(LinkerID);
            bw.Write(_linkerData);
            bw.Write(0);
            bw.Write(0);
        }

        private static void WriteChunkDataBlock(BinaryWriter bw, int chunkTableOffset, List<Chunk> chunks)
        {
            for (var i = 0; i < chunks.Count; i++)
            {
                var offset = bw.BaseStream.Position;

                WriteChunkData(bw, chunks[i]);

                var endOffset = bw.BaseStream.Position;

                bw.BaseStream.Seek(
                    chunkTableOffset
                    + 4 // sizeof(chunkCount)
                    + (4 * i), // sizeof(int) * chunkId
                    SeekOrigin.Begin
                );

                bw.Write((int)offset);
                bw.BaseStream.Seek(endOffset, SeekOrigin.Begin);
            }
        }

        private static int WriteChunkTable(BinaryWriter bw, List<Chunk> chunks)
        {
            var chunkTableOffset = (int)bw.BaseStream.Position;

            bw.Write(chunks.Count);
            for (var i = 0; i < chunks.Count; i++)
            {
                bw.Write(0);
            }

            var endOffset = bw.BaseStream.Position;

            bw.BaseStream.Seek(
                EvxHeaderOffsets.ChunkTableOffset,
                SeekOrigin.Begin
            );
            bw.Write(chunkTableOffset);
            bw.BaseStream.Seek(endOffset, SeekOrigin.Begin);

            return chunkTableOffset;
        }

        private static void WriteChunkData(BinaryWriter bw, Chunk c)
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
                WriteChunkData(bw, c.SubChunks[i]);
            }

            bw.Write(c.Instructions.Count);
            for (var i = 0; i < c.Instructions.Count; i++)
            {
                bw.Write(c.Instructions[i]);
            }
        }

        private static void WriteExtern(BinaryWriter bw, ExternInfo externInfo)
        {
            WriteString(bw, externInfo.Name);
            WriteString(bw, externInfo.OwnerChunkName);
            bw.Write(externInfo.SymbolId);
            bw.Write((byte)externInfo.Type);
        }

        private static void WriteGlobalList(BinaryWriter bw, List<string> globals)
        {
            var offset = bw.BaseStream.Position;

            bw.Write(globals.Count);
            for (var i = 0; i < globals.Count; i++)
            {
                WriteString(bw, globals[i]);
            }

            var endOffset = bw.BaseStream.Position;

            bw.BaseStream.Seek(EvxHeaderOffsets.GlobalListOffset, SeekOrigin.Begin);
            bw.Write((int)offset);
            bw.BaseStream.Seek(endOffset, SeekOrigin.Begin);
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

        private static void WriteString(BinaryWriter bw, string s)
        {
            var stringData = Encoding.UTF8.GetBytes(s);
            bw.Write(stringData.Length);
            bw.Write(stringData);
        }

        private static void WriteChecksum(BinaryWriter bw, Stream outStream)
        {
            outStream.Seek(EvxHeaderOffsets.TimeStamp, SeekOrigin.Begin);
            
            var hash = 0xCBF29CE484222325;
            using (var br = new BinaryReader(outStream, Encoding.UTF8, true))
            {
                while (outStream.Position < outStream.Length)
                {
                    hash ^= br.ReadByte();
                    hash *= 0x00000100000001B3;
                }
            }

            outStream.Seek(
                EvxHeaderOffsets.Checksum,
                SeekOrigin.Begin
            );

            bw.Write(hash);
        }
    }
}