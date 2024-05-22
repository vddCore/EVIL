using System.IO;

namespace EVIL.Ceres.LanguageTests
{
    public record TestRunnerOptions(
        bool FailOnCompilerErrors,
        bool FailOnTestErrors,
        string[] TestDirectories,
        TextWriter? TestOutput = null
    );
}