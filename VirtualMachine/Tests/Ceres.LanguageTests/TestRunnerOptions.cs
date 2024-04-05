using System.IO;

namespace Ceres.LanguageTests
{
    public record TestRunnerOptions(
        bool FailOnCompilationErrors,
        bool FailOnTestErrors,
        string TestDirectoryRoot,
        TextWriter? TestOutput = null
    );
}