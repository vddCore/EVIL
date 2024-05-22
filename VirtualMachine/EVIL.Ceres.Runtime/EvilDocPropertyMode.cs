using System;

namespace EVIL.Ceres.Runtime
{
    [Flags]
    public enum EvilDocPropertyMode : byte
    {
        Get = 1 << 0,
        Set = 1 << 1
    }
}