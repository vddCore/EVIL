using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace EVIL.Intermediate
{
    public class Disassembler
    {
        private int _longestOpCodeLength;
        private StringBuilder _disasm = new();

        private int IP { get; set; }
        private Chunk CurrentChunk { get; set; }

        public DisassemblerOptions Options { get; }

        public Disassembler(DisassemblerOptions options = null)
        {
            Options = options ?? new DisassemblerOptions();

            _longestOpCodeLength = Enum.GetNames<OpCode>()
                .Max(x => x.Length);
        }

        public string Disassemble(Executable executable)
        {
            _disasm.Clear();

            for(var ci = 0; ci < executable.Chunks.Count; ci++)
            {
                var chunk = executable.Chunks[ci];
                
                IP = 0;
                CurrentChunk = chunk;
                _disasm.Append($"FUNC__{ci}");

                if (Options.EmitFunctionNames)
                {
                    _disasm.Append($" {chunk.Name}");
                }
                
                if (Options.EmitFunctionParameters)
                {
                    _disasm.Append("(");
                    for (var i = 0; i < chunk.Parameters.Count; i++)
                    {
                        _disasm.Append(chunk.Parameters[i]);

                        if (i + 1 < chunk.Parameters.Count)
                            _disasm.Append(", ");
                    }
                    _disasm.Append(")");
                }
                _disasm.AppendLine(" {");

                if (Options.EmitLocalTable)
                {
                    for (var i = 0; i < chunk.Locals.Count; i++)
                    {
                        _disasm.AppendLine($" .LOCAL {i} ; {chunk.Locals[i]}");
                    }
                }

                if (Options.EmitExternTable)
                {
                    for (var i = 0; i < chunk.Externs.Count; i++)
                    {
                        var e = chunk.Externs[i];
                        
                        _disasm.AppendLine(
                            $" .EXTERN {i} ; {e.Name} ({(e.IsParameter ? "parameter" : "local")} {e.SymbolId} from {e.OwnerChunkName})");
                    }
                }

                if (Options.EmitParamTable)
                {
                    for (var i = 0; i < chunk.Parameters.Count; i++)
                    {
                        _disasm.AppendLine($" .PARAM {i} ; {chunk.Parameters[i]}");
                    }
                }

                _disasm.AppendLine(" .TEXT {");
                while (IP < chunk.Instructions.Count)
                {
                    AppendCurrentIP();

                    var op = (OpCode)FetchByte();

                    switch (op)
                    {
                        default:
                            DecodeLine(op);
                            break;

                        case OpCode.JUMP:
                        case OpCode.TJMP:
                        case OpCode.FJMP:
                            DecodeJump(op, chunk.Labels);
                            break;

                        case OpCode.LDC:
                        case OpCode.LDG:
                        case OpCode.STG:
                        case OpCode.GNAME:
                        case OpCode.RGL:
                            DecodeLdConst(op, executable.ConstPool);
                            break;

                        case OpCode.LDL:
                        case OpCode.STL:
                        case OpCode.LNAME:
                            DecodeLocalOp(op, chunk.Locals);
                            break;

                        case OpCode.LDA:
                        case OpCode.STA:
                        case OpCode.PNAME:
                            DecodeLocalOp(op, chunk.Parameters);
                            break;

                        case OpCode.LDX:
                        case OpCode.STX:
                        case OpCode.XNAME:
                            DecodeExternOp(op, chunk.Externs);
                            break;

                        case OpCode.LDF:
                            DecodeLoadFunc(op, executable.Chunks);
                            break;

                        case OpCode.STE:
                        case OpCode.CALL:
                            DecodeParametrizedLoad(op);
                            break;
                    }
                }

                _disasm.AppendLine("  }");
                _disasm.AppendLine("}");
            }

            return _disasm.ToString();
        }

        private void AppendCurrentIP()
        {
            _disasm.Append($"    {IP:X8}");
        }

        private void Decode(OpCode opCode)
        {
            _disasm.Append($" {(int)opCode:X2} {opCode.ToString().PadRight(_longestOpCodeLength, ' ')}");
        }

        private void DecodeLine(OpCode opCode)
        {
            Decode(opCode);
            _disasm.AppendLine();
        }

        private void DecodeJump(OpCode opCode, List<int> labels)
        {
            Decode(opCode);
            var labelId = FetchInt32();
            var labelAddr = labels[labelId];

            _disasm.AppendLine($" {labelId:X8} ; {CurrentChunk.Name}+{labelAddr:X8}");
        }

        private void DecodeLdConst(OpCode opCode, ConstPool constPool)
        {
            Decode(opCode);

            var index = FetchInt32();

            string dereferenced;
            string str;
            double? num;

            str = constPool.GetStringConstant(index);
            if (str != null)
            {
                var constString = constPool.GetStringConstant(index);
                EscapeString(ref constString);

                dereferenced = $"\"{constString}\"";
            }
            else
            {
                num = constPool.GetNumberConstant(index);
                dereferenced = $"{num}";
            }

            _disasm.Append($" {index:X8}");
            if (Options.EmitGlobalHints)
            {
                _disasm.Append($" ; {dereferenced}");
            }
            _disasm.AppendLine();
        }

        private void DecodeLocalOp(OpCode opCode, List<string> locals)
        {
            Decode(opCode);
            var index = FetchInt32();
            _disasm.Append($" {index:X8}");

            if (Options.EmitLocalHints)
            {
                _disasm.Append($" ; {locals[index]}");
            }

            _disasm.AppendLine();
        }

        private void DecodeExternOp(OpCode opCode, List<ExternInfo> externs)
        {
            Decode(opCode);
            var index = FetchInt32();
            var e = externs[index];

            _disasm.Append($" {index:X8}");

            if (Options.EmitExternHints)
            {
                _disasm.Append($" ; {e.Name} ({(e.IsParameter ? "parameter" : "local")} {e.SymbolId} in {e.OwnerChunkName})");
            }

            _disasm.AppendLine();
        }

        private void DecodeLoadFunc(OpCode opCode, List<Chunk> chunks)
        {
            Decode(opCode);
            var index = FetchInt32();
            _disasm.Append($" {index:X8}");

            if (Options.EmitFunctionHints)
            {
                _disasm.Append($" ; {chunks[index].Name}");
            }

            _disasm.AppendLine();
        }

        private void DecodeParametrizedLoad(OpCode opCode)
        {
            Decode(opCode);
            _disasm.AppendLine($" {FetchInt32():X8}");
        }

        private double FetchNumber()
        {
            var data = FetchBytes(8);

            unsafe
            {
                fixed (byte* num = &data[0])
                    return *((double*)num);
            }
        }

        private int FetchInt32()
        {
            var data = FetchBytes(4);

            unsafe
            {
                fixed (byte* num = &data[0])
                    return *((int*)num);
            }
        }

        private uint FetchUInt32()
        {
            var data = FetchBytes(4);

            unsafe
            {
                fixed (byte* num = &data[0])
                    return *((uint*)num);
            }
        }

        private byte[] FetchBytes(int len)
        {
            var data = new byte[len];

            for (var i = 0; i < len; i++)
                data[i] = FetchByte();

            return data;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte FetchByte()
            => CurrentChunk.Instructions[IP++];

        private void EscapeString(ref string str)
        {
            str = str.Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t")
                .Replace("\b", "\\b");
        }
    }
}