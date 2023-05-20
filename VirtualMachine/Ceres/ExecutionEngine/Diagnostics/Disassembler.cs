using System.IO;
using System.Linq;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine.Diagnostics
{
    public static class Disassembler
    {
        public class DisassemblyOptions
        {
            public bool WriteLineNumbersWhenAvailable { get; set; } = true;
            public bool WriteLocalNames { get; set; } = true;
            public bool WriteParameterNames { get; set; } = true;
            
            public bool WriteStringConstants { get; set; } = true;
        }
        public static void Disassemble(Chunk chunk, TextWriter output)
        {
            output.Write(".CHUNK ");

            if (chunk.IsAnonymous)
            {
                output.Write("[anonymous] ");
            }
            else
            {
                output.Write(chunk.Name);
                output.Write(" ");
            }

            if (chunk.HasDebugInfo)
            {
                if (chunk.DebugDatabase.DefinedOnLine > 0)
                {
                    output.Write(
                        $"(def. on line {chunk.DebugDatabase.DefinedOnLine}) "
                    );
                }
            }
            
            output.WriteLine("{");

            output.WriteLine($"  .LOCALS {chunk.LocalCount}");
            output.WriteLine($"  .PARAMS {chunk.ParameterCount}");

            for (var i = 0; i < chunk.ParameterCount; i++)
            {
                if (chunk.ParameterInitializers.TryGetValue(i, out var value))
                {
                    output.WriteLine($"    .PARAM DO_INIT({i}): {value.ConvertToString().String}");
                }
                else
                {
                    output.WriteLine($"    .PARAM NO_INIT({i})");
                }
            }

            if (chunk.Attributes.Any())
            {
                output.WriteLine();
                DumpAttributes(chunk, output);
            }
            
            output.WriteLine();
            output.WriteLine("  .TEXT {");

            using (var reader = chunk.SpawnCodeReader())
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var ip = reader.BaseStream.Position;
                    var opCode = (OpCode)reader.ReadByte();

                    if (chunk.HasDebugInfo)
                    {
                        var line = chunk.DebugDatabase.GetLineForIP((int)ip);
                        if (line > 0)
                            output.WriteLine($"   line {line}:");
                    }
                    
                    output.Write($"    {ip:X8}: ");
                    switch (opCode)
                    {
                        case OpCode.INVOKE:
                        case OpCode.YIELD:
                            output.Write(opCode);
                            output.Write(" ");
                            output.WriteLine(reader.ReadInt32());
                            break;
                        
                        case OpCode.SETLOCAL:
                        case OpCode.GETLOCAL:
                        case OpCode.SETARG:
                        case OpCode.GETARG:
                            output.Write(opCode);
                            output.Write(" ");

                            var localId = reader.ReadInt32();
                            output.Write(localId);

                            if (chunk.HasDebugInfo)
                            {
                                if (opCode == OpCode.SETLOCAL || opCode == OpCode.GETLOCAL)
                                {
                                    if (chunk.DebugDatabase.TryGetLocalName(localId, out var name))
                                    {
                                        output.Write($" ({name})");
                                    }
                                }
                                else
                                {
                                    if (chunk.DebugDatabase.TryGetParameterName(localId, out var name))
                                    {
                                        output.Write($" ({name})");
                                    }
                                }
                            }

                            output.WriteLine();
                            break;
                        
                        case OpCode.FJMP:
                        case OpCode.TJMP:
                        case OpCode.JUMP:
                            output.Write(opCode);
                            output.Write(" ");
                            
                            var labelid = reader.ReadInt32();
                            output.Write(labelid);

                            output.WriteLine($" ({chunk.Labels[(int)labelid]:X8})");
                            break;

                        case OpCode.LDNUM:
                            output.Write(opCode);
                            output.Write(" ");
                            output.WriteLine(reader.ReadDouble());
                            break;

                        case OpCode.LDSTR:
                        {
                            output.Write(opCode);
                            output.Write(" ");

                            var strid = reader.ReadInt32();
                            output.Write(strid);

                            var str = chunk.StringPool[strid];

                            if (str != null)
                            {
                                output.WriteLine($" (\"{str}\")");
                            }
                            else
                            {
                                output.WriteLine(" (<???>)");
                            }

                            break;
                        }
                        
                        default:
                            output.WriteLine(opCode);
                            break;
                    }
                }
            }

            output.WriteLine("  }");
            output.WriteLine("}");
        }

        private static void DumpAttributes(Chunk chunk, TextWriter output)
        {
            foreach (var attribute in chunk.Attributes)
            {
                output.Write($"  .ATTR {attribute.Name}");

                if (attribute.Values.Count > 0)
                {
                    output.Write("(");
                    output.Write(
                        string.Join(
                            ", ",
                            attribute.Values.Select(StringifyDynamicValue)
                        )
                    );
                    output.Write(")");
                }
                
                output.WriteLine();
            }
        }

        private static string StringifyDynamicValue(DynamicValue value)
        {
            if (value.Type == DynamicValue.DynamicValueType.String)
            {
                return $"\"{value.String}\"";
            }
                                
            return value.ConvertToString().String!;
        }
    }
}