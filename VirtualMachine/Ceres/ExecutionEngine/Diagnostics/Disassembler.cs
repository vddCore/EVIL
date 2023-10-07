using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ceres.ExecutionEngine.TypeSystem;

namespace Ceres.ExecutionEngine.Diagnostics
{
    public static class Disassembler
    {
        public class DisassemblyOptions
        {
            public bool WriteLineNumbersWhenAvailable { get; set; } = false;
            public bool WriteLocalNames { get; set; } = true;
            public bool WriteParameterNames { get; set; } = true;
            public bool WriteClosureInfo { get; set; } = true;
            public bool WriteSubChunkNames { get; set; } = true;
            public bool WriteLabelAddresses { get; set; } = true;
            public bool WriteNumericConstants { get; set; } = true;

            public bool WriteStringConstants { get; set; } = true;
        }

        public static void Disassemble(Chunk chunk, TextWriter output, DisassemblyOptions? options = null, int indentLevel = 0)
        {
            options ??= new DisassemblyOptions();
            var indent = new string(' ', indentLevel);

            output.Write($"{indent}.CHUNK ");

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

            output.WriteLine($"{indent}  .LOCALS {chunk.LocalCount}");
            output.WriteLine($"{indent}  .CLOSRS {chunk.ClosureCount}");
            output.WriteLine($"{indent}  .PARAMS {chunk.ParameterCount}");

            for (var i = 0; i < chunk.ParameterCount; i++)
            {
                if (chunk.ParameterInitializers.TryGetValue(i, out var value))
                {
                    output.WriteLine($"{indent}    .PARAM DO_INIT({i}): {value.ConvertToString().String}");
                }
                else
                {
                    output.WriteLine($"{indent}    .PARAM NO_INIT({i})");
                }
            }

            if (chunk.Attributes.Any())
            {
                output.WriteLine();
                DumpAttributes(chunk, output);
            }

            output.WriteLine();
            output.WriteLine($"{indent}  .TEXT {{");

            var prevLine = -1;
            using (var reader = chunk.SpawnCodeReader())
            {
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var ip = reader.BaseStream.Position;
                    var opCode = (OpCode)reader.ReadByte();

                    if (chunk.HasDebugInfo && options.WriteLineNumbersWhenAvailable)
                    {
                        var line = chunk.DebugDatabase.GetLineForIP((int)ip);

                        if (line > 0)
                        {
                            if (prevLine != line)
                            {
                                output.WriteLine($"{indent}   line {line}:");
                                prevLine = line;
                            }
                        }
                    }

                    output.Write($"{indent}    {ip:X8}: ");
                    switch (opCode)
                    {
                        case OpCode.INVOKE:
                        case OpCode.YIELD:
                        case OpCode.NEXT:
                            output.Write(opCode);
                            output.Write(" ");
                            output.WriteLine(reader.ReadInt32());
                            break;

                        case OpCode.GETCLOSURE:
                        case OpCode.SETCLOSURE:
                        {
                            output.Write(opCode);
                            output.Write(" ");

                            var closureId = reader.ReadInt32();
                            output.Write(closureId);

                            if (options.WriteClosureInfo)
                            {
                                var closure = chunk.Closures[closureId];
                                var closureType = "local";
                                if (closure.IsParameter)
                                {
                                    closureType = "parameter";
                                }
                                else if (closure.IsClosure)
                                {
                                    closureType = "closure";
                                }

                                var names = new List<string>();
                                var closureFrom = chunk;
                                var nesting = closure.NestingLevel;
                                
                                while (closureFrom?.Parent != null)
                                {
                                    closureFrom = closureFrom?.Parent;
                                    names.Add($"'{closureFrom?.Name ?? "???"}'");
                                }

                                for (var i = 0; i < nesting - 1; i++)
                                {
                                    names.RemoveAt(0);
                                }

                                names.Reverse();
                                
                                output.Write($" ({closureType} {closure.EnclosedId} in {string.Join(" -> ", names)})");
                            }

                            output.WriteLine();
                            break;
                        }

                        case OpCode.LDCNK:
                        {
                            output.Write(opCode);
                            output.Write(" ");

                            var chunkId = reader.ReadInt32();
                            output.Write(chunkId);

                            if (options.WriteSubChunkNames)
                            {
                                var subChunk = chunk.SubChunks[chunkId];
                                output.Write($" (subchunk {chunkId} '{subChunk.Name ?? "<no name>"}')");
                            }

                            output.WriteLine();
                            break;
                        }

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
                                    if (options.WriteLocalNames)
                                    {
                                        if (chunk.DebugDatabase.TryGetLocalName(localId, out var name))
                                        {
                                            output.Write($" ({name})");
                                        }
                                    }
                                }
                                else
                                {
                                    if (options.WriteParameterNames)
                                    {
                                        if (chunk.DebugDatabase.TryGetParameterName(localId, out var name))
                                        {
                                            output.Write($" ({name})");
                                        }
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

                            if (options.WriteLabelAddresses)
                            {
                                output.Write($" ({chunk.Labels[(int)labelid]:X8})");
                            }

                            output.WriteLine();
                            break;

                        case OpCode.LDNUM:
                            output.Write(opCode);
                            output.Write(" ");

                            if (options.WriteNumericConstants)
                            {
                                output.Write(reader.ReadDouble());
                            }

                            output.WriteLine();
                            break;

                        case OpCode.LDSTR:
                        {
                            output.Write(opCode);
                            output.Write(" ");

                            var strid = reader.ReadInt32();
                            output.Write(strid);

                            if (options.WriteStringConstants)
                            {
                                var str = chunk.StringPool[strid];

                                if (str != null)
                                {
                                    output.Write($" (\"{str}\")");
                                }
                                else
                                {
                                    output.Write($" (<???>)");
                                }
                            }

                            output.WriteLine();
                            break;
                        }

                        default:
                            output.WriteLine(opCode);
                            break;
                    }
                }
            }

            output.WriteLine($"{indent}  }}");

            if (chunk.SubChunks.Count > 0)
            {
                output.WriteLine();
                output.WriteLine($"{indent}  .SUBCHUNKS {{");
                for (var i = 0; i < chunk.SubChunkCount; i++)
                {
                    Disassemble(chunk.SubChunks[i], output, options, indentLevel + 4);
                }

                output.WriteLine($"{indent}  }}");
            }

            output.WriteLine($"{indent}}}");
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