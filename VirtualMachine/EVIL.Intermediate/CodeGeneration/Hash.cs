using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EVIL.Intermediate.CodeGeneration
{
    internal class Hash
    {
        public static ulong FNV1A64(List<byte> bytes)
        {
            var hash = 0xCBF29CE484222325;

            for (var i = 0; i < bytes.Count; i++)
            {
                hash ^= bytes[i];
                hash *= 0x00000100000001B3;
            }

            return hash;
        }

        public static ulong FNV1A64(Stream stream)
        {
            var hash = 0xCBF29CE484222325;
            using (var br = new BinaryReader(stream, Encoding.UTF8, true))
            {
                while (stream.Position < stream.Length)
                {
                    hash ^= br.ReadByte();
                    hash *= 0x00000100000001B3;
                }
            }

            return hash;
        }
    }
}