using System;

namespace EVIL.ExecutionEngine.Interop
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ClrLibraryAttribute : Attribute
    {
        public string LibraryName { get; }

        public ClrLibraryAttribute(string libraryName)
        {
            LibraryName = libraryName;
        }
    }
}