using System.IO;

namespace Ceres.LanguageTests
{
    public record TestRunnerOptions(
        bool FailOnCompilerErrors,
        bool FailOnTestErrors,
        string[] TestDirectories,
        TextWriter? TestOutput = null
    );
}