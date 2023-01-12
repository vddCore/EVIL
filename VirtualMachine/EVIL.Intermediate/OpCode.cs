namespace EVIL.Intermediate
{
    public enum OpCode : byte
    {
        // ---------- [ GENERIC
        NOP   = 0x00,
        POP   = 0x01,
        HLT   = 0x02,
        DUP   = 0x03,
        
        // ---------- [ ARITHMETIC
        ADD   = 0x10,  //
        SUB   = 0x11,  // [SUB]tract
        MUL   = 0x12,  // [MUL]tiply
        DIV   = 0x13,  // [DIV]ivide
        MOD   = 0x14,  // [MOD]ulo
        LEN   = 0x15,  // [LEN]gth
        UNM   = 0x16,  // [UN]ary [M]inus
        SHR   = 0x17,  // [SH]ift [R]ight
        SHL   = 0x18,  // [SH]ift [L]eft
        
        // ---------- [ LOGICAL
        AND   = 0x20,
        NOT   = 0x21,
        OR    = 0x22,
        XOR   = 0x23,
        CEQ   = 0x24,  // [C]ompare ([EQ]ual)
        CNE   = 0x25,  // [C]ompare ([N]ot [E]qual)
        CGT   = 0x26,  // [C]ompare ([G]reater [T]han)
        CLT   = 0x27,  // [C]ompare ([L]ess [T]han)
        CGE   = 0x28,  // [C]ompare ([G]reater or [E]qual)
        CLE   = 0x29,  // [C]ompare ([L]ess or [E]qual)
        LOR   = 0x2A,
        LAND  = 0x2B,
        LNOT  = 0x2C,
        XINT  = 0x2D,
        
        // ---------- [ BRANCHING
        JUMP  = 0x30, // Unconditionally [JUMP]
        TJMP  = 0x31, // If [T]rue, [J]u[MP]
        FJMP  = 0x32, // If [F]alse, [J]u[MP]
        CALL  = 0x33, // [CALL] a chunk
        RETN  = 0x34, // [RET]ur[N] from a chunk
        
        // ---------- [ STORAGE: STORE
        STL = 0x41,   // [S]e[T] [L]ocal
        STG = 0x42,   // [S]e[T] [G]lobal
        STA = 0x43,   // [S]e[T] [A]rgument
        STE = 0x44,   // [S]et [T]able [E]ntry
        
        // ---------- [ STORAGE: LOAD
        LDC = 0x50,   // [L]oa[D] [C]onstant
        LDG = 0x51,   // [L]oa[D] [G]lobal
        LDL = 0x52,   // [L]oa[D] [L]ocal
        LDA = 0x53,   // [L]oa[D] [A]rgument
        LTE = 0x54,   // [L]oad [T]able [E]ntry
        
        // ---------- [ MISCELLANEOUS
        NEWTB = 0x60, // Create a [NEW] [T]a[B]le
        TONUM = 0x61, // 
        TOSTR = 0x62,
        XARGS = 0x63,
    }
}