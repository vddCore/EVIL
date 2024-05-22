using System;

namespace EVIL.Ceres.TranslationEngine.Diagnostics
{
    public sealed class CompilerMessageEmitEventArgs : EventArgs
    {
        public CompilerMessage Message { get; }

        public CompilerMessageEmitEventArgs(CompilerMessage message)
        {
            Message = message;
        }
    }
}