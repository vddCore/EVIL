namespace Ceres.ExecutionEngine.Diagnostics
{
    public enum OpCode : byte
    {
        NOOP,
        DUP,
        ADD,
        SUB,
        MUL,
        MOD,
        DIV,
        SHL,
        SHR,
        BOR,
        BXOR,
        BAND,
        BNOT,
        LDNIL,
        LDTRUE,
        LDFALSE,
        LDZERO,
        LDONE,
        LDNUM,
        LDSTR,
        ANEG,
        POP,
        RET,
        INC,
        DEC,
        INVOKE,
        TAILINVOKE,
        SETGLOBAL,
        GETGLOBAL,
        SETLOCAL,
        GETLOCAL,
        SETARG,
        GETARG,
        LENGTH,
        EXISTS,
        TONUMBER,
        TOSTRING,
        TYPE,
        LNOT,
        LAND,
        LOR,
        DEQ,
        DNE,
        CEQ,
        CNE,
        CGT,
        CLT,
        CGE,
        CLE,
        FJMP,
        TJMP,
        JUMP,
        TABNEW,
        TABINIT,
        TABSET,
        INDEX,
        YIELD,
        YRET,
        XARGS,
        EACH,
        NEXT,
        EEND
    }
}