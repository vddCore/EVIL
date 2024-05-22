using System;
using System.IO;
using System.Text;
using EVIL.Ceres.ExecutionEngine.Diagnostics;
using EVIL.Ceres.ExecutionEngine.TypeSystem;
using EVIL.CommonTypes.TypeSystem;

namespace EVIL.Ceres.ExecutionEngine.Collections.Serialization
{
    internal static class TableSerializer
    {
        public static void Serialize(Table table, Stream stream)
        {
            using (var bw = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                bw.Write(new[] { (byte)'E', (byte)'V', (byte)'T' });
                bw.Write(table.Length);

                foreach (var (key, value) in table)
                {
                    if (key.Type == DynamicValueType.Table && ReferenceEquals(key.Table, table))
                    {
                        throw new SerializationException("Circular reference in table key.");
                    }

                    if (value.Type == DynamicValueType.Table && ReferenceEquals(value.Table, table))
                    {
                        throw new SerializationException("Circular reference in table value.");
                    }

                    key.Serialize(stream);
                    value.Serialize(stream);
                }

                bw.Write(table.HasMetaTable);
                if (table.HasMetaTable)
                {
                    table.MetaTable!.Serialize(stream);
                }
            }
        }

        public static Table Deserialize(Stream stream)
        {
            var offset = stream.Position;
            
            using (var br = new BinaryReader(stream, Encoding.UTF8, true))
            {
                var header = br.ReadBytes(3);

                if (header[0] != 'E'
                    || header[1] != 'V'
                    || header[2] != 'T')
                {
                    throw new SerializationException("Invalid table header.");
                }

                try
                {
                    var valueCount = br.ReadInt32();
                    var table = new Table();

                    for (var i = 0; i < valueCount; i++)
                    {
                        var key = DynamicValue.Deserialize(stream);
                        var value = DynamicValue.Deserialize(stream);

                        table[key] = value;
                    }

                    var hasMetaTable = br.ReadBoolean();
                    if (hasMetaTable)
                    {
                        var metaTable = Table.Deserialize(stream);
                        table.MetaTable = metaTable;
                    }

                    return table;
                }
                catch (Exception e)
                {
                    throw new SerializationException(
                        $"Failed to deserialize the table at stream offset '{offset}'. Data may be corrupt.",
                        e
                    );
                }
            }
        }
    }
}