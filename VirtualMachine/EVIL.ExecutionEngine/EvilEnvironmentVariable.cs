namespace EVIL.ExecutionEngine
{
    public class EvilEnvironmentVariable
    {
        public string Name { get; }

        /// <summary>
        /// _ARGS: [table] list of arguments with which the executable was invoked
        /// </summary>
        public static readonly EvilEnvironmentVariable ExecutableArguments = new("_ARGS");

        /// <summary>
        /// _HOME: [string] the directory in which the launched script resides
        /// </summary>
        public static readonly EvilEnvironmentVariable ScriptHomeDirectory = new("_HOME");

        /// <summary>
        /// _PATH: [string] semicolon separated list of lua-like patterns
        /// </summary>
        public static readonly EvilEnvironmentVariable LibraryLookupPaths = new("_PATH");

        /// <summary>
        /// _CWD: [string] location from which the interpreter was launched
        /// </summary>
        public static readonly EvilEnvironmentVariable CurrentWorkingDirectory = new("_CWD");
        
        private EvilEnvironmentVariable(string name)
        {
            Name = name;
        }
    }
}