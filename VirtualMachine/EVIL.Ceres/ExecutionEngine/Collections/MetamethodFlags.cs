namespace EVIL.Ceres.ExecutionEngine.Collections;

using System;

[Flags]
public enum MetamethodFlags : uint
{
    None   = 0,
    Add    = 1u << 0,
    Sub    = 1u << 1,
    Mul    = 1u << 2,
    Div    = 1u << 3,
    Mod    = 1u << 4,
    Shl    = 1u << 5,
    Shr    = 1u << 6,
    Aneg   = 1u << 7,
    Lnot   = 1u << 8,
    Lor    = 1u << 9,
    Land   = 1u << 10,
    Bor    = 1u << 11,
    Bxor   = 1u << 12,
    Band   = 1u << 13,
    Bnot   = 1u << 14,
    Deq    = 1u << 15,
    Dne    = 1u << 16,
    Eq     = 1u << 17,
    Ne     = 1u << 18,
    Gt     = 1u << 19,
    Gte    = 1u << 20,
    Lt     = 1u << 21,
    Lte    = 1u << 22,
    Invoke = 1u << 23,
    Len    = 1u << 24,
    ToStr  = 1u << 25,
    ToNum  = 1u << 26,
    Exists = 1u << 27,
    Inc    = 1u << 28,
    Dec    = 1u << 29,
    Set    = 1u << 30,
    Get    = 1u << 31
}
