namespace EVIL.Assessment.Tests.Core.Grammar;

using EVIL.Core.Grammar;
using NUnit.Framework;
using Shouldly;

[TestFixture]
public class IncompleteInputParserTests : ParserTestBase
{
    [TestCase("val value = (1 + 2")]
    [TestCase("val values = [1, 2")]
    [TestCase("fn calculate() {\n  val value = 1;")]
    public void Parse_MarksUnexpectedEndOfInputAsIncomplete(string source)
    {
        var exception = Should.Throw<ParserException>(() => BuildProgramTree(source));

        exception.IsIncomplete.ShouldBeTrue();
    }

    [TestCase("val = 1;")]
    [TestCase("val value = 1 + * 2;")]
    public void Parse_DoesNotMarkInvalidCompleteInputAsIncomplete(string source)
    {
        var exception = Should.Throw<ParserException>(() => BuildProgramTree(source));

        exception.IsIncomplete.ShouldBeFalse();
    }
}