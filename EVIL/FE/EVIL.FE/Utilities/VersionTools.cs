namespace EVIL.FE.Utilities;

using System.Text;
using EVIL.Core.Analysis;
using EVIL.Core.Diagnostics;
using EVIL.Core.Grammar;
using EVIL.Core.Lexical;
using EVIL.VM.Debugger;
using EVIL.VM.ExecutionEngine;
using EVIL.VM.Runtime;

internal class VersionTools
{
    public static string BuildVersionBanner()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Integrated EVIL front-end environment / REPL.");
        sb.AppendLine("This component is distributed under GPLv3 license.");
        sb.AppendLine();
        sb.AppendLine("--[component versions]--------");
        sb.AppendLine($"              EVIL.FE: v{GetAssemblyVersionFromType<EvmFrontEnd>()}");
        sb.AppendLine($"    EVIL.Core.Lexical: v{GetAssemblyVersionFromType<Lexer>()}");
        sb.AppendLine($"    EVIL.Core.Grammar: v{GetAssemblyVersionFromType<Parser>()}");
        sb.AppendLine($"   EVIL.Core.Analysis: v{GetAssemblyVersionFromType<AnalyzerDriver>()}");
        sb.AppendLine($"EVIL.Core.Diagnostics: v{GetAssemblyVersionFromType<Diagnostic>()}");
        sb.AppendLine($"              EVIL.VM: v{GetAssemblyVersionFromType<VirtualMachine>()}");
        sb.AppendLine($"     EVIL.VM.Debugger: v{GetAssemblyVersionFromType<DebuggerSession>()}");
        sb.AppendLine($"      EVIL.VM.Runtime: v{GetAssemblyVersionFromType<RuntimeModule>()}");

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
}