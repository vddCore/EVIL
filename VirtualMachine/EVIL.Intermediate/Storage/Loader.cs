using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EVIL.Intermediate.CodeGeneration;

namespace EVIL.Intermediate.Storage
{
    public class Loader : IDisposable
    {
        private Stream _stream;
        private BinaryReader _br;

        private int _constPoolOffset;
        private int _globalListOffset;
        private int _chunkTableOffset;

        private List<int> _chunkOffsets = new();
        
        public Loader(Stream stream)
        {
            _stream = stream;
            _br = new BinaryReader(_stream);
        }

        public Executable Load()
        {
            var exe  = new Executable(false);
            ReadHeader();
            ReadConstPool(exe);
            ReadGlobalList(exe);
            ReadChunkTable();
            ReadChunks(exe);

            return exe;
        }

        private void ReadHeader()
        {
            var data = _br.ReadBytes(3);

            if (data[0] != 'E'
                || data[1] != 'V'
                || data[2] != 'X')
            {
                throw new InvalidDataException("Magic number is invalid.");
            }

            var version = _br.ReadByte();
            var stamp = _br.ReadInt64();
            var linkerId = _br.ReadBytes(4);
            var linkerData = _br.ReadBytes(32);

            _chunkTableOffset = _br.ReadInt32();
            _globalListOffset = _br.ReadInt32();
            _constPoolOffset = _br.ReadInt32();
            
            var reserved = _br.ReadBytes(20);
        }

        private void ReadConstPool(Executable exe)
        {
            _stream.Seek(_constPoolOffset, SeekOrigin.Begin);
            var constCount = _br.ReadInt32();

            for (var i = 0; i < constCount; i++)
            {
                var type = _br.ReadByte();

                if (type == 0)
                {
                    var strText = ReadString();
                    exe.ConstPool.FetchOrAddConstant(strText);
                }
                else if (type == 1)
                {
                    var dbl = _br.ReadDouble();
                    exe.ConstPool.FetchOrAddConstant(dbl);
                }
                else
                {
                    throw new InvalidDataException("Invalid const type.");
                }
            }
        }

        private void ReadGlobalList(Executable exe)
        {
            _stream.Seek(_globalListOffset, SeekOrigin.Begin);
            var globalCount = _br.ReadInt32();

            for (var i = 0; i < globalCount; i++)
            {
                var nameText = ReadString();
                exe.Globals.Add(nameText);
            }
        }

        private void ReadChunkTable()
        {
            _stream.Seek(_chunkTableOffset, SeekOrigin.Begin);
            var chunkCount = _br.ReadInt32();

            for (var i = 0; i < chunkCount; i++)
            {
                _chunkOffsets.Add(_br.ReadInt32());
            }
        }

        private void ReadChunks(Executable exe)
        {
            for (var i = 0; i < _chunkOffsets.Count; i++)
            {
                _stream.Seek(_chunkOffsets[i], SeekOrigin.Begin);
                var chunk = ReadChunk();
                exe.Chunks.Add(chunk);
            }
        }

        private Chunk ReadChunk()
        {
            var name = ReadString();
            var chunk = new Chunk(name);

            var labelCount = _br.ReadInt32();
            for (var i = 0; i < labelCount; i++)
            {
                chunk.Labels.Add(_br.ReadInt32());
            }

            var paramCount = _br.ReadInt32();
            for (var i = 0; i < paramCount; i++)
            {
                chunk.Parameters.Add(ReadString());
            }

            var localCount = _br.ReadInt32();
            for (var i = 0; i < localCount; i++)
            {
                chunk.Locals.Add(ReadString());
            }

            var externCount = _br.ReadInt32();
            for (var i = 0; i < externCount; i++)
            {
                chunk.Externs.Add(ReadExtern());
            }

            var insnCount = _br.ReadInt32();
            chunk.Instructions.AddRange(_br.ReadBytes(insnCount));
            
            return chunk;
        }

        private ExternInfo ReadExtern()
        {
            var name = ReadString();
            var owner = ReadString();
            var symId = _br.ReadInt32();
            var isParam = _br.ReadBoolean();

            return new ExternInfo(name, owner, symId, isParam);
        }

        private string ReadString()
        {
            var nameLen = _br.ReadInt32();
            var nameData = _br.ReadBytes(nameLen);
            return Encoding.UTF8.GetString(nameData);
        }

        public void Dispose()
        {
            _stream?.Dispose();
            _br?.Dispose();
        }
    }
}