using System.Collections.Generic;
using System.IO;
using System.Text;
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
                    var (chunkTableOffset, globalListOffset) 
                        = ReadHeader(br);
                    
                    ReadGlobalList(br, exe, globalListOffset);
                    var chunkTableEntries = ReadChunkTable(br, chunkTableOffset);
                    ReadChunks(br, chunkTableEntries, exe);
                }
                return exe;
            }
            finally
            {
                br.Dispose();
            }
        }

        private static (int, int) ReadHeader(BinaryReader br)
        {
            var data = br.ReadBytes(3);

            if (data[0] != 'E'
                || data[1] != 'V'
                || data[2] != 'X')
            {
                throw new InvalidDataException("Magic number is invalid.");
            }

            var version = br.ReadByte();
            var fnv1a64 = br.ReadInt64();
            var stamp = br.ReadInt64();
            var linkerId = br.ReadBytes(4);
            var linkerData = br.ReadBytes(32);

            var chunkTableOffset = br.ReadInt32();
            var globalListOffset = br.ReadInt32();

            return (chunkTableOffset, globalListOffset);
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

        private static void ReadGlobalList(BinaryReader br, Executable exe, int globalListOffset)
        {
            br.BaseStream.Seek(globalListOffset, SeekOrigin.Begin);
            var globalCount = br.ReadInt32();

            for (var i = 0; i < globalCount; i++)
            {
                var nameText = ReadString(br);
                exe.Globals.Add(nameText);
            }
        }

        private static List<int> ReadChunkTable(BinaryReader br, int chunkTableOffset)
        {
            var ret = new List<int>();
            
            br.BaseStream.Seek(chunkTableOffset, SeekOrigin.Begin);
            var chunkCount = br.ReadInt32();

            for (var i = 0; i < chunkCount; i++)
            {
                ret.Add(br.ReadInt32());
            }

            return ret;
        }

        private static void ReadChunks(BinaryReader br, List<int> chunkOffsets, Executable exe)
        {
            for (var i = 0; i < chunkOffsets.Count; i++)
            {
                br.BaseStream.Seek(
                    chunkOffsets[i],
                    SeekOrigin.Begin
                );
                
                var chunk = ReadChunk(br);
                exe.Chunks.Add(chunk);
            }
        }

        private static Chunk ReadChunk(BinaryReader br)
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
            
            return chunk;
        }

        private static ExternInfo ReadExtern(BinaryReader br)
        {
            var name = ReadString(br);
            var owner = ReadString(br);
            var symId = br.ReadInt32();
            var isParam = br.ReadBoolean();

            return new ExternInfo(name, owner, symId, isParam);
        }

        private static string ReadString(BinaryReader br)
        {
            var nameLen = br.ReadInt32();
            var nameData = br.ReadBytes(nameLen);
            return Encoding.UTF8.GetString(nameData);
        }
    }
}