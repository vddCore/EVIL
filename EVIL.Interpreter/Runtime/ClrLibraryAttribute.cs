using System;

namespace EVIL.Interpreter.Runtime
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