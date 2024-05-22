using System;
using System.IO;
using System.Text;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.CommonTypes.TypeSystem;

namespace EVIL.Ceres.ExecutionEngine.Collections.Serialization
{
    internal static class ArraySerializer
    {
        public static void Serialize(Array array, Stream stream)
        {
            using (var bw = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                bw.Write(new[] { (byte)'E', (byte)'V', (byte)'A' });
                bw.Write(array.Length);

                for (var i = 0; i < array.Length; i++)
                {
                    var value = array[i];

                    if (value.Type == DynamicValueType.Array && ReferenceEquals(value.Array!, array))
                    {
                        throw new SerializationException("Circular reference in serialized array.");
                    }

                    array[i].Serialize(stream);
                }
            }
        }

        public static Array Deserialize(Stream stream)
        {
            var offset = stream.Position;

            using (var br = new BinaryReader(stream, Encoding.UTF8, true))
            {
                var header = br.ReadBytes(3);

                if (header[0] != 'E'
                    || header[1] != 'V'
                    || header[2] != 'A')
                {
                    throw new SerializationException("Invalid array header.");
                }

                try
                {
                    var length = br.ReadInt32();
                    var array = new Array(length);

                    for (var i = 0; i < length; i++)
                    {
                        array[i] = DynamicValue.Deserialize(stream);
                    }

                    return array;
                }
                catch (Exception e)
                {
                    throw new SerializationException(
                        $"Failed to deserialize the array at stream offset '{offset}'. Data may be corrupt.",
                        e
                    );
                }
            }
        }
    }
}