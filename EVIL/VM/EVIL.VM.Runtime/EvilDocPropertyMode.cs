namespace EVIL.VirtualMachine.Runtime;

using System;

[Flags]
public enum EvilDocPropertyMode : byte
{
    Get = 1 << 0,
    Set = 1 << 1
}