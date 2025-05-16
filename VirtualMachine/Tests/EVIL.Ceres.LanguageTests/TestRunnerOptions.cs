namespace EVIL.Ceres.LanguageTests;

using System.IO;

public record TestRunnerOptions(
    bool FailOnCompilerErrors,
    bool FailOnTestErrors,
    bool OptimizeCodeGeneration,
    string[] TestDirectories,
    TextWriter? TestOutput = null
);