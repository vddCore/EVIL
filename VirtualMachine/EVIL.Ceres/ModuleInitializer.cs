namespace EVIL.Ceres;

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal static class ModuleInitializer
{
    [DllImport("ntdll")]
    private static extern int NtSetTimerResolution(
        ulong desiredResolution,
        bool setResolution,
        ref ulong currentResolution
    );
        
    [ModuleInitializer]
    public static void Initialize()
    {           
        if (OperatingSystem.IsWindows())
        {
            ulong res = 0;
            NtSetTimerResolution(1, true, ref res);
        }    
    }
}