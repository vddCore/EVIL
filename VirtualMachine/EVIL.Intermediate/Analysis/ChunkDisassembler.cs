using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using EVIL.Intermediate.CodeGeneration;

namespace EVIL.Intermediate.Analysis
{
    public class ChunkDisassembler
    {
        private static int _longestOpCodeLength;

        private int _indent;
        private StringBuilder _disasm = new();

        private Chunk Chunk { get; }
        private DisassemblerOptions Options { get; }

        private int IP { get; set; }

        public ChunkDisassembler(Chunk chunk, DisassemblerOptions options, bool isSubChunk, int indent = 0)
        {
            if (_longestOpCodeLength == 0)
            {
                _longestOpCodeLength = Enum.GetNames<OpCode>()
                    .Max(x => x.Length);
            }

            _indent = indent;

            Chunk = chunk;
            Options = options;

            if (isSubChunk)
            {
                Indent();
                _disasm.Append($"SUBFUNC");
            }
            else
            {
                _disasm.Append($"FUNC");
            }

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
                    Indent();
                    _disasm.AppendLine($" .LOCAL {i} ; {chunk.Locals[i]}");
                }
            }

            DumpConstPool(chunk.Constants);

            if (Options.EmitExternTable)
            {
                for (var i = 0; i < chunk.Externs.Count; i++)
                {
                    var e = chunk.Externs[i];

                    Indent();
                    _disasm.AppendLine(
                        $" .EXTERN {i} ; {e.Name} ({(e.IsParameter ? "parameter" : "local")} {e.SymbolId} from {e.OwnerChunkName})");
                }
            }

            if (Options.EmitParamTable)
            {
                for (var i = 0; i < chunk.Parameters.Count; i++)
                {
                    Indent();
                    _disasm.AppendLine($" .PARAM {i} ; {chunk.Parameters[i]}");
                }
            }

            Indent();
            _disasm.AppendLine(" .TEXT {");
            while (IP < chunk.Instructions.Count)
            {
                Indent();
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

                    case OpCode.LDCN:
                    case OpCode.LDCS:
                    case OpCode.LDG:
                    case OpCode.STG:
                    case OpCode.GNAME:
                    case OpCode.RGL:
                        DecodeLdConst(op, chunk.Constants);
                        break;

                    case OpCode.LDL:
                    case OpCode.STL:
                    case OpCode.LNAME:
                        DecodeLocalOp(op, chunk.Locals);
                        break;

                    case OpCode.LDA:
                    case OpCode.STA:
                    case OpCode.PNAME:
                        DecodeParameterOp(op, chunk.Parameters);
                        break;

                    case OpCode.LDX:
                    case OpCode.STX:
                    case OpCode.XNAME:
                        DecodeExternOp(op, chunk.Externs);
                        break;

                    case OpCode.LDF:
                        DecodeLoadFunc(op, chunk.SubChunks);
                        break;

                    case OpCode.STE:
                    case OpCode.CALL:
                    case OpCode.ITER:
                        DecodeParametrizedLoad(op);
                        break;
                }
            }
            Indent();
            _disasm.AppendLine("  }");

            if (chunk.SubChunks.Count > 0)
            {
                Indent();
                _disasm.AppendLine("  .SUBCHUNKS {");
                foreach (var subChunk in chunk.SubChunks)
                {
                    _disasm.Append(new ChunkDisassembler(subChunk, options, true, _indent + 4));
                }
                
                Indent();
                _disasm.AppendLine("  }");
            }
            Indent();
            _disasm.AppendLine("}");
        }

        public override string ToString() => _disasm.ToString();

        private void Indent()
        {
            _disasm.Append("".PadLeft(_indent, ' '));
        }

        private void DumpConstPool(ConstPool constPool)
        {
            for (var i = 0; i < constPool.Count; i++)
            {
                var str = constPool.GetStringConstant(i);

                Indent();
                if (str != null)
                {
                    EscapeString(ref str);
                    _disasm.AppendLine($" .CONST {i} STR \"{str}\"");
                }
                else
                {
                    _disasm.AppendLine($" .CONST {i} NUM {constPool.GetNumberConstant(i)}");
                }
            }
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

            _disasm.AppendLine($" {labelId:X8} ; {Chunk.Name}+{labelAddr:X8}");
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

        private void DecodeExternOp(OpCode opCode, List<ExternInfo> externs)
        {
            Decode(opCode);
            var index = FetchInt32();
            var e = externs[index];

            _disasm.Append($" {index:X8}");

            if (Options.EmitExternHints)
            {
                _disasm.Append(
                    $" ; {e.Name} ({(e.IsParameter ? "parameter" : "local")} {e.SymbolId} in {e.OwnerChunkName})");
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
            _disasm.AppendLine($" {FetchByte():X2}");
        }

        private void DecodeLocalOp(OpCode opCode, List<string> locals)
        {
            Decode(opCode);
            var index = FetchByte();
            _disasm.Append($" {index:X2}");

            if (Options.EmitLocalHints)
            {
                _disasm.Append($" ; {locals[index]}");
            }

            _disasm.AppendLine();
        }

        private void DecodeParameterOp(OpCode opCode, List<string> parameters)
        {
            Decode(opCode);
            var index = FetchByte();
            _disasm.Append($" {index:X2}");

            if (Options.EmitParameterHints)
            {
                _disasm.Append($" ; {parameters[index]}");
            }

            _disasm.AppendLine();
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
            => Chunk.Instructions[IP++];


        private void EscapeString(ref string str)
        {
            str = str.Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t")
                .Replace("\b", "\\b");
        }
    }
}