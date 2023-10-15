using System;
using System.Text;
using Ceres.ExecutionEngine;
using Ceres.Runtime;
using EVIL.Grammar.Parsing;
using EVIL.Lexical;

namespace EVIL.evil
{
    public partial class EvmFrontEnd
    {
        private static string BuildVersionBanner()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Integrated EVIL front-end environment.");
            sb.AppendLine("This project is distributed under GPLv3 license.");
            sb.AppendLine();
            sb.AppendLine("--[Component versions]--------");
            sb.AppendLine($"  EVM front-end: v{GetAssemblyVersionFromType<EvmFrontEnd>()}");
            sb.AppendLine($"     EVIL lexer: v{GetAssemblyVersionFromType<Lexer>()}");
            sb.AppendLine($"    EVIL parser: v{GetAssemblyVersionFromType<Parser>()}");
            sb.AppendLine($"       Ceres VM: v{GetAssemblyVersionFromType<CeresVM>()}");
            sb.AppendLine($"  Ceres.Runtime: v{GetAssemblyVersionFromType<RuntimeModule>()}");

            return sb.ToString();
        }
        
        private static string GetAssemblyVersionFromType<T>()
        {
            var type = typeof(T);
            var asm = type.Assembly;
            var name = asm.GetName();
            var version = name.Version!;

            return $"{version.Major}.{version.Minor}.{version.Build}";
        }
        
        private static void Message(string msg)
        {
            Console.WriteLine(msg);
        }

        private static void Terminate(string? msg = null, bool writeHelp = false, ExitCode exitCode = ExitCode.OK)
        {
            if (msg != null)
            {
                Message(msg);
            }

            if (writeHelp)
            {
                _options.WriteOptionDescriptions(
                    Console.Out
                );
            }

            Environment.Exit((int)exitCode);
        }
    }
}