namespace EVIL.VM.ExecutionEngine.Serialization;

using System;
using System.Buffers.Binary;
using System.Text;
using EVIL.VM.ExecutionEngine;

public sealed class BinaryChunkReader
{
    private readonly ReadOnlyMemory<byte> _data;
    private int _position;

    public int Remaining => _data.Length - _position;

    public BinaryChunkReader(ReadOnlyMemory<byte> data)
    {
        _data = data;
    }

    public byte ReadByte()
    {
        EnsureRemaining(1);
        return _data.Span[_position++];
    }

    public bool ReadBoolean()
    {
        var value = ReadByte();
        return value switch
        {
            0 => false,
            1 => true,
            _ => throw new ChunkDeserializationException($"Invalid boolean value '{value}'.")
        };
    }

    public ushort ReadUInt16()
    {
        EnsureRemaining(sizeof(ushort));
        
        var value = BinaryPrimitives.ReadUInt16LittleEndian(
            _data.Span.Slice(
                _position,
                sizeof(ushort)
            )
        );
        _position += sizeof(ushort);
        
        return value;
    }

    public int ReadInt32()
    {
        EnsureRemaining(sizeof(int));
        
        var value = BinaryPrimitives.ReadInt32LittleEndian(
            _data.Span.Slice(
                _position,
                sizeof(int)
            )
        );
        _position += sizeof(int);
        
        return value;
    }

    public uint ReadUInt32()
    {
        EnsureRemaining(sizeof(uint));
        
        var value = BinaryPrimitives.ReadUInt32LittleEndian(
            _data.Span.Slice(
                _position,
                sizeof(uint)
            )
        );
        _position += sizeof(uint);

        return value;
    }

    public long ReadInt64()
    {
        EnsureRemaining(sizeof(long));
        
        var value = BinaryPrimitives.ReadInt64LittleEndian(
            _data.Span.Slice(
                _position,
                sizeof(long)
            )
        );
        _position += sizeof(long);
        
        return value;
    }

    public double ReadDouble()
        => BitConverter.Int64BitsToDouble(ReadInt64());

    public Guid ReadGuid()
        => new(ReadMemory(16).Span, bigEndian: true);

    public int ReadCount(int minimumElementSize = 0)
    {
        var count = ReadUInt32();
        if (count > BinaryChunkLimits.MaximumItemCount)
        {
            throw new ChunkDeserializationException(
                $"Chunk item count '{count}' exceeds the v6 limit."
            );
        }

        if (minimumElementSize < 0 || minimumElementSize > 0
            && count > (uint)(Remaining / minimumElementSize))
        {
            throw new ChunkDeserializationException(
                "Chunk item count exceeds the remaining section data."
            );
        }

        return (int)count;
    }

    public string ReadString()
    {
        var length = ReadUInt32();
        if (length > BinaryChunkLimits.MaximumStringSize 
            || length > (uint)Remaining)
        {
            throw new ChunkDeserializationException(
                $"Chunk string length '{length}' is invalid or exceeds the v6 limit."
            );
        }

        try
        {
            var value = BinaryChunkLimits.UTF8.GetString(
                _data.Span.Slice(
                    _position, 
                    (int)length
                )
            );
            _position += (int)length;
            
            return value;
        }
        catch (DecoderFallbackException)
        {
            throw new ChunkDeserializationException(
                "Chunk string contains invalid UTF-8."
            );
        }
    }

    public ReadOnlyMemory<byte> ReadMemory(int length)
    {
        EnsureRemaining(length);
        
        var result = _data.Slice(_position, length);
        _position += length;
        
        return result;
    }

    public uint PeekUInt32(int offset)
    {
        if (offset < 0 || offset > Remaining - sizeof(uint))
        {
            throw new ChunkDeserializationException(
                "Truncated value in chunk section."
            );
        }

        return BinaryPrimitives.ReadUInt32LittleEndian(
            _data.Span.Slice(
                _position + offset,
                sizeof(uint)
            )
        );
    }

    public void EnsureFullyConsumed(string sectionName)
    {
        if (Remaining != 0)
        {
            throw new ChunkDeserializationException(
                $"Chunk {sectionName} section contains '{Remaining}' trailing byte(s)."
            );
        }
    }

    private void EnsureRemaining(int length)
    {
        if (length < 0 || length > Remaining)
        {
            throw new ChunkDeserializationException(
                "Truncated value in chunk section."
            );
        }
    }
}