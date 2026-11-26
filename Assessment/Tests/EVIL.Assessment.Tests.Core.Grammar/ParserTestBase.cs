namespace EVIL.Assessment.Tests.Core.Grammar;

using EVIL.Core.Grammar;
using EVIL.Core.Grammar.AST.Miscellaneous;
using NUnit.Framework;

public abstract class ParserTestBase
{
    protected Parser _parser = null!;

    [SetUp]
    public virtual void Setup()
    {
        _parser = new Parser();
    }

    [TearDown]
    public virtual void Teardown()
    {
        _parser = null!;
    }

    protected ProgramNode BuildProgramTree(string source)
        => _parser.Parse(source);
}