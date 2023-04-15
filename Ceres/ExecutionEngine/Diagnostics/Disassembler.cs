using System.IO;
using System.Linq;
using System.Text;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine.Diagnostics
{
    public static class Disassembler
    {
        public static void Disassemble(Chunk chunk, TextWriter output)
        {
            output.Write(".CHUNK ");

            if (!chunk.IsAnonymous)
            {
                output.Write(chunk.Name);
                output.Write(" ");
            }

            output.WriteLine("{");

            output.WriteLine($"  .LOCALS {chunk.LocalCount}");
            output.WriteLine($"  .PARAMS {chunk.ParameterCount}");
            output.WriteLine();
            foreach (var attribute in chunk.Attributes)
            {
                output.Write($"  .ATTR {attribute.Name}");

                if (attribute.Values.Count > 0)
                {
                    output.Write("(");
                    output.Write(
                        string.Join(
                            ", ",
                            attribute.Values.Select(x => x.ConvertToString().String)
                        )
                    );
                    output.WriteLine(")");
                }
            }
            output.WriteLine("  .TEXT {");

            using (var reader = chunk.SpawnCodeReader())
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var ip = reader.BaseStream.Position;
                    var opCode = (OpCode)reader.ReadByte();

                    output.Write($"    {ip:X8}: ");
                    switch (opCode)
                    {
                        case OpCode.INVOKE:
                        case OpCode.SETLOCAL:
                        case OpCode.GETLOCAL:
                        case OpCode.SETARG:
                        case OpCode.GETARG:
                            output.Write(opCode);
                            output.Write(" ");
                            output.WriteLine(reader.ReadInt32());
                            break;
                        
                        case OpCode.FJMP:
                        case OpCode.TJMP:
                        case OpCode.JUMP:
                            output.Write(opCode);
                            output.Write(" ");
                            
                            var labelid = reader.ReadInt32();
                            output.Write(labelid);

                            output.Write(" ; ");
                            output.WriteLine($"{chunk.Labels[(int)labelid]:X8}");
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
                            output.Write(" ; ");

                            if (str != null)
                            {
                                output.WriteLine($"\"{str}\"");
                            }
                            else
                            {
                                output.WriteLine("<???>");
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
    }
}