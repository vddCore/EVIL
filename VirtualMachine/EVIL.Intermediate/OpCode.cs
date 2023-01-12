namespace EVIL.Intermediate
{
    public enum OpCode : byte
    {
        // ---------- [ GENERIC
        NOP   = 0x00,
        POP   = 0x01,
        HLT   = 0x02,
        
        // ---------- [ ARITHMETIC
        ADD   = 0x10,
        SUB   = 0x11,
        MUL   = 0x12,
        DIV   = 0x13,
        MOD   = 0x14,
        LEN   = 0x15,
        UNM   = 0x16,
        
        // ---------- [ LOGICAL
        AND   = 0x20,
        NOT   = 0x21,
        OR    = 0x22,
        XOR   = 0x23,
        CEQ   = 0x24,
        CNE   = 0x25,
        CGT   = 0x26,
        CLT   = 0x27,
        CGE   = 0x28,
        CLE   = 0x29,
        SHR   = 0x2A,
        SHL   = 0x2B,
        LOR   = 0x2C,
        LAND  = 0x2D,
        LNOT  = 0x2E,
        XINT  = 0x2F,
        
        // ---------- [ BRANCHING
        JUMP  = 0x30,
        TJMP  = 0x31,
        CALL  = 0x32,
        RETN  = 0x33,
        
        // ---------- [ STORAGE
        LDCONST = 0x40,
        LDCHUNK = 0x41,
        LDGLOBAL = 0x42,
        STGLOBAL = 0x43,
        LDLOCAL = 0x44,
        STLOCAL = 0x45,
        LDARG = 0x46,
        STARG = 0x47,
        NEWTABLE = 0x48,
        CLOSE = 0x49,
        LDCLS = 0x4A,
        STCLS = 0x4B,
        
        // ---------- [ DATA MANIPULATION
        INDEX = 0x50,
        TONUM = 0x51,
        TOSTR = 0x52
    }
}