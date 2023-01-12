using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace EVIL.Intermediate
{
    public class Disassembler
    {
        private StringBuilder _disasm = new();
        
        private IReadOnlyList<byte> _instructions;
        private string[] _constants;
        
        public int IP { get; set; }

        public string Disassemble(IReadOnlyList<byte> instructions, string[] constants)
        {
            _disasm.Clear();
            
            _instructions = instructions;
            _constants = constants;
            
            while (IP < _instructions.Count)
            {
                var op = (OpCode)FetchByte();

                switch (op)
                {
                    case OpCode.NOP:
                    case OpCode.POP:
                    case OpCode.HLT:
                    case OpCode.ADD:
                    case OpCode.SUB:
                    case OpCode.MUL:
                    case OpCode.DIV:
                    case OpCode.MOD:
                    case OpCode.LEN:
                    case OpCode.FLR:
                    case OpCode.AND:
                    case OpCode.NOT:
                    case OpCode.OR:
                    case OpCode.XOR:
                    case OpCode.CEQ:
                    case OpCode.CGT:
                    case OpCode.CLT:
                    case OpCode.CGE:
                    case OpCode.CLE:
                    case OpCode.SHR:
                    case OpCode.SHL:
                    case OpCode.RETN:
                    case OpCode.NETBL:
                    case OpCode.CNCAT:
                    case OpCode.INDEX:
                        DecodeLine(op);
                        break;
                    
                    case OpCode.JUMP:
                    case OpCode.TJMP:
                    case OpCode.FJMP:
                        DecodeJump(op);
                        break;
                    
                    case OpCode.CALL:
                        DecodeCall(op);
                        break;
                    
                    case OpCode.LDSTR:
                        DecodeLdStr(op);
                        break;
                    
                    case OpCode.LDNUM:
                        DecodeLdNum(op);
                        break;
                    
                    case OpCode.LDVAR:
                        DecodeLdVar(op);
                        break;
                    
                    case OpCode.STVAR:
                        DecodeStVar(op);
                        break;
                }
            }

            return _disasm.ToString();
        }
        
        private void Decode(OpCode opCode)
        {
            _disasm.Append($"{IP:X8} {opCode:X2} {opCode}");
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

        private void DecodeCall(OpCode opCode)
        {
            Decode(opCode);
            _disasm.AppendLine(" !NIY!");
        }

        private void DecodeLdStr(OpCode opCode)
        {
            Decode(opCode);
            
            var index = FetchInt32();
            _disasm.AppendLine($" {index:X8} ; \"{_constants[index]}\"");
        }

        private void DecodeLdNum(OpCode opCode)
        {
            Decode(opCode);
            _disasm.AppendLine($" {FetchNumber():X8}");
        }

        private void DecodeLdVar(OpCode opCode)
        {
            Decode(opCode);
            _disasm.AppendLine($" {FetchInt32():X8}");
        }

        private void DecodeStVar(OpCode opCode)
        {
            Decode(opCode);
            _disasm.AppendLine($" {FetchInt32():X8}");
        }
        
        private double FetchNumber()
        {
            var data = FetchBytes(8);
            
            unsafe
            {
                fixed (double* dbl = &data[0])
                    return *dbl;
            }
        }

        private int FetchInt32()
        {
            var data = FetchBytes(4);

            unsafe
            {
                fixed (int* num = &data[0])
                    return *num;
            }
        }

        private uint FetchUInt32()
        {
            var data = FetchBytes(4);

            unsafe
            {
                fixed (uint* num = &data[0])
                    return *num;
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
            => _instructions[IP++];
    }
}