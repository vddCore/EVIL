namespace EVIL.FE.Utilities;

using System;

internal class Diagnostics
{
    public static void Message(string msg)
    {
        Console.WriteLine(msg);
    }

    public static void Terminate(string? msg = null, Action? postBannerMessage = null, ExitCode exitCode = ExitCode.OK)
    {
        if (msg != null)
        {
            Message(msg);
        }

        if (postBannerMessage != null)
        {
            postBannerMessage();
        }

        Console.Out.Flush();
        Environment.Exit((int)exitCode);
    }
}