using System;

namespace Ceres.Runtime
{
    [Flags]
    public enum EvilDocPropertyMode : byte
    {
        Get = 1 << 0,
        Set = 1 << 1
    }
}