namespace EVIL.VirtualMachine.TranslationEngine.Diagnostics;

using System;

public sealed class CompilerMessageEmitEventArgs : EventArgs
{
    public CompilerMessage Message { get; }

    public CompilerMessageEmitEventArgs(CompilerMessage message)
    {
        Message = message;
    }
}