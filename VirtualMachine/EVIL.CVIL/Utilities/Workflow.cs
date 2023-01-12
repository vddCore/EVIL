using System;
using System.IO;
using System.Reflection;
using Mono.Options;

namespace EVIL.CVIL.Utilities
{
    public static class Workflow
    {
        public static string InvocationVerb => Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
        public static Version ProgramVersion => Assembly.GetExecutingAssembly().GetName().Version;

        public static void ExitWithMessage(string message, int exitCode = 0)
        {
            if (exitCode != 0)
            {
                Console.WriteLine($"{InvocationVerb}: fatal error: {message}");
                Console.WriteLine("compilation terminated.");
            }
            else
            {
                Console.WriteLine(message);
            }
            
            Environment.Exit(exitCode);
        }
        
        public static void ExitWithHelp(OptionSet options)
        {
            var v = ProgramVersion;
            
            Console.WriteLine($"EVIL compiler front-end {v.Major}.{v.Minor}.{v.Build}.\n\nUsage:");
            options.WriteOptionDescriptions(Console.Out);
            Environment.Exit(0);
        }
    }
}