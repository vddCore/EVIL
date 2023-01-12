namespace EVIL
{
    public interface IMemory
    {
        byte[] Array { get; }

        void Poke(int addr, short value);
        void Poke(int addr, int value);
        void Poke(int addr, byte value);

        byte Peek8(int addr);
        short Peek16(int addr);
        int Peek32(int addr);
    }
}
