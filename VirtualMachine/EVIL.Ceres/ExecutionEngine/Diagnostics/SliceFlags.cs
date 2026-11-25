namespace EVIL.Ceres.ExecutionEngine.Diagnostics;

using System;

[Flags]
internal enum SliceFlags : byte
{
    StartPresent = 1 << 0,
    EndPresent = 1 << 1,
    StartFromEnd = 1 << 2,
    EndFromEnd = 1 << 3,
    ScalarResult = 1 << 4
}