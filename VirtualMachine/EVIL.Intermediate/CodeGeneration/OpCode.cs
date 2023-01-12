﻿namespace EVIL.Intermediate.CodeGeneration
{
    public enum OpCode : byte
    {
        // ---------- [ GENERIC
        NOP   = 0x00, // [N]o [OP]eration
        POP   = 0x01, // [POP] from evaluation stack
        HLT   = 0x02, // [H]a[LT] execution
        DUP   = 0x03, // [DUP]licate the top of evaluation stack
        
        // ---------- [ ARITHMETIC
        ADD   = 0x10, // [ADD]
        SUB   = 0x11, // [SUB]tract
        MUL   = 0x12, // [MUL]tiply
        DIV   = 0x13, // [DIV]ivide
        MOD   = 0x14, // [MOD]ulo
        LEN   = 0x15, // [LEN]gth
        UNM   = 0x16, // [UN]ary [M]inus
        SHR   = 0x17, // [SH]ift [R]ight
        SHL   = 0x18, // [SH]ift [L]eft
        
        // ---------- [ LOGICAL
        AND   = 0x20, // Bitwise [AND]
        NOT   = 0x21, // Bitwise [NOT]
        OR    = 0x22, // Bitwise [OR]
        XOR   = 0x23, // Bitwise [XOR]
        CEQ   = 0x24, // [C]ompare ([EQ]ual)
        CNE   = 0x25, // [C]ompare ([N]ot [E]qual)
        CGT   = 0x26, // [C]ompare ([G]reater [T]han)
        CLT   = 0x27, // [C]ompare ([L]ess [T]han)
        CGE   = 0x28, // [C]ompare ([G]reater or [E]qual)
        CLE   = 0x29, // [C]ompare ([L]ess or [E]qual)
        LOR   = 0x2A, // [L]ogical [OR]
        LAND  = 0x2B, // [L]ogical [AND]
        LNOT  = 0x2C, // [L]ogical [NOT]
        XIST  = 0x2D, // Check if A e[XIST]s in B 
        
        // ---------- [ BRANCHING
        JUMP  = 0x30, // Unconditionally [JUMP]
        TJMP  = 0x31, // If [T]rue, [J]u[MP]
        FJMP  = 0x32, // If [F]alse, [J]u[MP]
        CALL  = 0x33, // [CALL] a chunk
        TCALL = 0x34, // [T]ail [CALL] a chunk
        RETN  = 0x35, // [RET]ur[N] from a chunk
        
        // ---------- [ STORAGE: STORE
        STL = 0x41,   // [S]e[T] [L]ocal
        STG = 0x42,   // [S]e[T] [G]lobal
        STA = 0x43,   // [S]e[T] [A]rgument
        STE = 0x44,   // [S]et [T]able [E]ntry
        STX = 0x45,   // [S]e[T] e[X]ternal local
        
        // ---------- [ STORAGE: LOAD
        LDCN = 0x50,  // [L]oa[D] [C]onstant [N]umber
        LDCS = 0x51,  // [L]oa[D] [C]onstant [S]tring
        LDG = 0x52,   // [L]oa[D] [G]lobal
        LDL = 0x53,   // [L]oa[D] [L]ocal
        LDA = 0x54,   // [L]oa[D] [A]rgument
        LDF = 0x55,   // [L]oa[D] [F]unction
        LDX = 0x56,   // [L]oa[D] e[X]ternal local
        
        // ---------- [ STORAGE: REMOVE
        RTE = 0x60,   // [R]emove [T]able [E]ntry
        RGL = 0x61,   // [R]emove [GL]obal
        
        // ---------- [ MISCELLANEOUS
        NEWTB = 0x70, // Create a [NEW] [T]a[B]le
        INDEX = 0x71, // Attempt to [INDEX] a type
        TONUM = 0x72, // Convert [TO] [NUM]ber
        TOSTR = 0x73, // Convert [TO] [STR]ing
        XARGS = 0x74, // Load e[X]tra [ARG]ument[S]
        GNAME = 0x75, // [G]lobal [NAME]
        LNAME = 0x76, // [L]ocal [NAME]
        XNAME = 0x77, // e[X]tern [NAME]
        PNAME = 0x78, // [P]arameter [NAME]
        EACH = 0x79,  // Begin [EACH] loop
        ITER = 0x7A,  // [ITER]ate over a table during each-loop
        ENDE = 0x7B,  // [END] [E]ach loop
    }
}