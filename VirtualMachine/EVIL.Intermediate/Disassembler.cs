using System.Runtime.CompilerServices;
using System.Text;

namespace EVIL.Intermediate
{
    public class Disassembler
    {
        private StringBuilder _disasm = new();

        private int IP { get; set; }
        private Chunk CurrentChunk { get; set; }

        public string Disassemble(Executable executable)
        {
            _disasm.Clear();

            foreach (var chunk in executable.Chunks)
            {
                IP = 0;
                CurrentChunk = chunk;
                _disasm.Append($"{chunk.Name} [{chunk.LocalCount} local(s)] [{chunk.ParameterCount} parameter(s)]");
                _disasm.AppendLine(":");

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
                            DecodeJump(op);
                            break;

                        case OpCode.LDC:
                        case OpCode.LDG:
                        case OpCode.STG:
                            DecodeLdConst(op, executable.ConstPool);
                            break;

                        case OpCode.LDL:
                        case OpCode.STL:
                        case OpCode.STA:
                        case OpCode.LDA:
                        case OpCode.CALL:
                            DecodeParametrizedLoad(op);
                            break;
                    }
                }
            }

            return _disasm.ToString();
        }

        private void AppendCurrentIP()
        {
            _disasm.Append($"  {IP:X8}");
        }

        private void Decode(OpCode opCode)
        {
            _disasm.Append($" {((int)opCode):X2} {opCode}");
        }

        private void DecodeLine(OpCode opCode)
        {
            Decode(opCode);
            _disasm.AppendLine();
        }

        private void DecodeJump(OpCode opCode)
        {
            Decode(opCode);
            _disasm.AppendLine(" ");
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

            _disasm.AppendLine($" {index:X8} ; {dereferenced}");
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