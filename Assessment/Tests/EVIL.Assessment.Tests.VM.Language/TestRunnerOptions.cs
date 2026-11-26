namespace EVIL.Assessment.Tests.VM.Language;

using System.IO;

public record TestRunnerOptions(
    bool FailOnCompilerErrors,
    bool FailOnTestErrors,
    bool OptimizeCodeGeneration,
    string[] TestDirectories,
    TextWriter? TestOutput = null
);