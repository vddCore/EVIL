namespace EVIL.Ceres.RuntimeTests;

using EVIL.Ceres.TranslationEngine;
using NUnit.Framework;
using Shouldly;

[TestFixture]
public class CompilerDiagnosticTests
{
    [Test]
    public void Compiler_WithMultipleSyntaxErrors_CollectsAllErrorsInLog()
    {
        var compiler = new Compiler();
        var source = "val = 1;\nval b = 2;\nval = 3;\nval c = 4;";
        
        Should.Throw<CompilerException>(() => compiler.Compile(source, "test.vil"));

        compiler.Log.HasErrors.ShouldBeTrue();
        compiler.Log.ErrorCount.ShouldBe(2);
        compiler.Log.Messages.Count.ShouldBe(2);
    }
}
