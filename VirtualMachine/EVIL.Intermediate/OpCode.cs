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
        FLR   = 0x16,
        
        // ---------- [ LOGICAL
       
        AND   = 0x20,
        NOT   = 0x21,
        OR    = 0x22,
        XOR   = 0x23,
        CEQ   = 0x24,
        CGT   = 0x25,
        CLT   = 0x26,
        CGE   = 0x27,
        CLE   = 0x28,
        SHR   = 0x29,
        SHL   = 0x2A,
        
        // ---------- [ BRANCHING
        JUMP  = 0x31,
        FJMP  = 0x32,
        TJMP  = 0x33,
        CALL  = 0x34,
        RETN  = 0x35,
        
        // ---------- [ STORAGE

        LDSTR = 0x40,
        LDNUM = 0x41,
        LDVAR = 0x42,
        STVAR = 0x43,
        LDLOC = 0x44,
        STLOC = 0x45,
        NETBL = 0x46,
        
        // ---------- [ DATA MANIPULATION
        
        CNCAT = 0x50,
        INDEX = 0x51,
    }
}