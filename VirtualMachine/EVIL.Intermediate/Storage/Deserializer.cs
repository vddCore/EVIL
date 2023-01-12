using System.IO;
using System.Text;
using EVIL.Intermediate.CodeGeneration;

namespace EVIL.Intermediate.Storage
{
    public static class Deserializer
    {
        public static string ReadString(BinaryReader br)
        {
            var nameLen = br.ReadInt32();
            var nameData = br.ReadBytes(nameLen);
            return Encoding.UTF8.GetString(nameData);
        }
        
        public static Chunk ReadChunk(BinaryReader br)
        {
            var name = ReadString(br);
            var isPublic = br.ReadBoolean();
            
            var chunk = new Chunk(name, isPublic);

            ReadConstPool(br, chunk);
            
            var labelCount = br.ReadInt32();
            for (var i = 0; i < labelCount; i++)
            {
                chunk.Labels.Add(br.ReadInt32());
            }

            var paramCount = br.ReadInt32();
            for (var i = 0; i < paramCount; i++)
            {
                chunk.Parameters.Add(ReadString(br));
            }

            var localCount = br.ReadInt32();
            for (var i = 0; i < localCount; i++)
            {
                chunk.Locals.Add(ReadString(br));
            }

            var externCount = br.ReadInt32();
            for (var i = 0; i < externCount; i++)
            {
                chunk.Externs.Add(ReadExtern(br));
            }

            var subChunkCount = br.ReadInt32();
            for (var i = 0; i < subChunkCount; i++)
            {
                chunk.SubChunks.Add(ReadChunk(br));
            }

            var insnCount = br.ReadInt32();
            chunk.Instructions.AddRange(br.ReadBytes(insnCount));

            var debugEntryCount = br.ReadInt32();
            for (var i = 0; i < debugEntryCount; i++)
            {
                var line = br.ReadInt32();
                var column = br.ReadInt32();
                var ip = br.ReadInt32();

                chunk.DebugInfo.Add(new(line, column, ip));
            }
            return chunk;
        }
        
        private static void ReadConstPool(BinaryReader br, Chunk chunk)
        {
            var constCount = br.ReadInt32();

            for (var i = 0; i < constCount; i++)
            {
                var type = br.ReadByte();

                if (type == 0)
                {
                    var strText = ReadString(br);
                    chunk.Constants.FetchOrAddConstant(strText);
                }
                else if (type == 1)
                {
                    var dbl = br.ReadDouble();
                    chunk.Constants.FetchOrAddConstant(dbl);
                }
                else
                {
                    throw new InvalidDataException("Invalid const type.");
                }
            }
        }
        
        private static ExternInfo ReadExtern(BinaryReader br)
        {
            var name = ReadString(br);
            var owner = ReadString(br);
            var symId = br.ReadInt32();
            var type = (ExternInfo.ExternType)br.ReadByte();

            return new ExternInfo(name, owner, symId, type);
        }
    }
}