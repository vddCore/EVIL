using System;
using System.Collections.Generic;
using System.IO;
using EVIL.Intermediate.CodeGeneration;

namespace EVIL.Intermediate.Storage
{
    public static class EvxLinker
    {
        private static byte[] _linkerData = new byte[32];

        public static readonly byte[] MagicNumber = new byte[] { 0x45, 0x56, 0x58 }; // EVX
        public static readonly byte[] LinkerID = new byte[] { 0x43, 0x56, 0x49, 0x4C }; // CVIL
        public const byte FormatVersion = 1;

        public static void Link(Executable executable, string filePath)
        {
            Link(executable, new FileStream(filePath, FileMode.Create));
        }

        public static void Link(Executable executable, Stream outStream)
        {
            if (executable == null)
                throw new ArgumentNullException(nameof(executable));
                    
            var bw = new BinaryWriter(outStream);
            
            try
            {
                WriteHeader(bw);
                WriteChunks(bw, executable.Chunks);

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
        }

        private static void WriteChunks(BinaryWriter bw, List<Chunk> chunks)
        {
            bw.Write(chunks.Count);
            for (var i = 0; i < chunks.Count; i++)
            {
                Serializer.WriteChunk(bw, chunks[i]);
            }
        }

        private static void WriteChecksum(BinaryWriter bw, Stream outStream)
        {
            outStream.Seek(EvxHeaderOffsets.TimeStamp, SeekOrigin.Begin);

            var hash = Hash.FNV1A64(outStream);

            outStream.Seek(
                EvxHeaderOffsets.Checksum,
                SeekOrigin.Begin
            );

            bw.Write(hash);
        }
    }
}