using System;
using System.IO;
using System.Text;
using EVIL.Intermediate.CodeGeneration;

namespace EVIL.Intermediate.Storage
{
    public class Linker : IDisposable
    {
        private Executable _exe;
        private Stream _outStream;
        private BinaryWriter _bw;

        private long _timeStamp;
        private byte[] _linkerData = new byte[32];
        private byte[] _reserved = new byte[20];

        private int _chunkTableOffset;
        
        public readonly byte[] MagicNumber = new byte[] { 0x45, 0x56, 0x58 }; // EVX
        public readonly byte[] LinkerID = new byte[] { 0x4E, 0x56, 0x49, 0x4C }; // CVIL
        public const byte FormatVersion = 1;

        public Linker(Executable exe)
        {
            _exe = exe;
        }

        public void Link(Stream outStream)
        {
            _outStream = outStream;
            _timeStamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            _bw = new BinaryWriter(outStream);
            
            WriteHeader();
            WriteConstTable();
            WriteGlobalList();
            WriteChunkTable();

            for (var i = 0; i < _exe.Chunks.Count; i++)
            {
                var offset = _bw.BaseStream.Position;

                WriteChunkData(_exe.Chunks[i]);

                var endOffset = _bw.BaseStream.Position;

                _bw.BaseStream.Seek(
                    _chunkTableOffset
                    + 4 // sizeof(chunkCount)
                    + (4 * i), // sizeof(int) * chunkId
                    SeekOrigin.Begin
                );

                _bw.Write((int)offset);
                _bw.BaseStream.Seek(endOffset, SeekOrigin.Begin);
            }
        }

        private void WriteHeader()
        {
            _bw.Write(MagicNumber);
            _bw.Write(FormatVersion);
            _bw.Write(_timeStamp);
            _bw.Write(LinkerID);
            _bw.Write(_linkerData);
            _bw.Write(0);
            _bw.Write(0);
            _bw.Write(0);
            _bw.Write(_reserved);
        }

        private void WriteChunkTable()
        {
            _chunkTableOffset = (int)_bw.BaseStream.Position;
            
            _bw.Write(_exe.Chunks.Count);
            for (var i = 0; i < _exe.Chunks.Count; i++)
            {
                _bw.Write(0);
            }

            var endOffset = _bw.BaseStream.Position;

            _bw.BaseStream.Seek(EvxKnownFileOffsets.Header.ChunkTableOffset, SeekOrigin.Begin);
            _bw.Write(_chunkTableOffset);
            _bw.BaseStream.Seek(endOffset, SeekOrigin.Begin);
        }
        
        private void WriteChunkData(Chunk c)
        {
            WriteString(c.Name);

            _bw.Write(c.Labels.Count);
            for (var i = 0; i < c.Labels.Count; i++)
            {
                _bw.Write(c.Labels[i]);
            }

            _bw.Write(c.Parameters.Count);
            for (var i = 0; i < c.Parameters.Count; i++)
            {
                WriteString(c.Parameters[i]);
            }

            _bw.Write(c.Locals.Count);
            for (var i = 0; i < c.Locals.Count; i++)
            {
                WriteString(c.Locals[i]);
            }

            _bw.Write(c.Externs.Count);
            for (var i = 0; i < c.Externs.Count; i++)
            {
                WriteExtern(c.Externs[i]);
            }

            _bw.Write(c.Instructions.Count);
            for (var i = 0; i < c.Instructions.Count; i++)
            {
                _bw.Write(c.Instructions[i]);
            }
        }

        private void WriteExtern(ExternInfo externInfo)
        {
            WriteString(externInfo.Name);
            WriteString(externInfo.OwnerChunkName);
            _bw.Write(externInfo.SymbolId);
            _bw.Write(externInfo.IsParameter);
        }

        private void WriteGlobalList()
        {
            var offset = _bw.BaseStream.Position;

            _bw.Write(_exe.Globals.Count);
            for (var i = 0; i < _exe.Globals.Count; i++)
            {
                WriteString(_exe.Globals[i]);
            }

            var endOffset = _bw.BaseStream.Position;

            _bw.BaseStream.Seek(EvxKnownFileOffsets.Header.GlobalListOffset, SeekOrigin.Begin);
            _bw.Write((int)offset);
            _bw.BaseStream.Seek(endOffset, SeekOrigin.Begin);
        }

        private void WriteConstTable()
        {
            var offset = _bw.BaseStream.Position;

            _bw.Write(_exe.ConstPool.Count);
            for (var i = 0; i < _exe.ConstPool.Count; i++)
            {
                var num = _exe.ConstPool.GetNumberConstant(i);
                
                if (num != null)
                {
                    WriteConst(num.Value);
                }
                else // must be string
                {
                    WriteConst(_exe.ConstPool.GetStringConstant(i));
                }
            }

            var endOffset = _bw.BaseStream.Position;

            _bw.BaseStream.Seek(EvxKnownFileOffsets.Header.ConstPoolOffset, SeekOrigin.Begin);
            _bw.Write((int)offset);
            _bw.BaseStream.Seek(endOffset, SeekOrigin.Begin);
        }

        private void WriteConst(double c)
        {
            _bw.Write((byte)1);
            _bw.Write(c);
        }

        private void WriteConst(string c)
        {
            _bw.Write((byte)0);
            WriteString(c);
        }
        
        private void WriteString(string s)
        {
            var stringData = Encoding.UTF8.GetBytes(s);
            _bw.Write(stringData.Length);
            _bw.Write(stringData);
        }

        public void Dispose()
        {
            _outStream?.Dispose();
            _bw?.Dispose();
        }
    }
}