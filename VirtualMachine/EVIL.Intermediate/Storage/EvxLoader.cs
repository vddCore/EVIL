using System.IO;
using EVIL.Intermediate.CodeGeneration;

namespace EVIL.Intermediate.Storage
{
    public static class EvxLoader
    {
        public static Executable Load(string filePath)
        {
            return Load(
                new FileStream(
                    filePath,
                    FileMode.Open
                )
            );
        }
        
        public static Executable Load(Stream stream)
        {
            var br = new BinaryReader(stream);
            
            try
            {
                var exe = new Executable();
                {
                    ReadHeader(br);
                    ReadChunks(br, exe);
                }
                return exe;
            }
            finally
            {
                br.Dispose();
            }
        }

        private static void ReadHeader(BinaryReader br)
        {
            var data = br.ReadBytes(4);

            if (data[0] != 0xC
                || data[1] != 'E'
                || data[2] != 'V'
                || data[3] != 'X')
            {
                throw new InvalidDataException("Magic number is invalid.");
            }

            var version = br.ReadByte();
            var fnv1a64 = br.ReadInt64();
            var stamp = br.ReadInt64();
            var linkerId = br.ReadBytes(4);
            var linkerData = br.ReadBytes(32);
        }

        private static void ReadChunks(BinaryReader br, Executable executable)
        {
            var chunkCount = br.ReadInt32();
            for (var i = 0; i < chunkCount; i++)
            {
                executable.Chunks.Add(
                    Deserializer.ReadChunk(br)
                );
            }
        }
    }
}