namespace EVIL.Assessment.Tests.Core.Grammar;

using EVIL.Core.Grammar;
using EVIL.Core.Grammar.AST.Expressions;
using EVIL.Core.Grammar.AST.Statements;
using NUnit.Framework;
using Shouldly;

[TestFixture]
public class ValDeclarationPatternParserTests : ParserTestBase
{
    [Test]
    public void ArrayAndTablePatterns_PreserveBindingsAliasesAndMutability()
    {
        var program = BuildProgramTree(
            "val ordinary = 1, [first, rw second] <- source, " +
            "{name, points: score, ['total points']: rw total} <- source;"
        );
        var statement = program.Statements[0].ShouldBeOfType<ValStatement>();

        statement.Declarators.Count.ShouldBe(3);
        statement.Declarators[0].ShouldBeOfType<IdentifierValDeclarator>()
            .Identifier.Name.ShouldBe("ordinary");

        var array = statement.Declarators[1].ShouldBeOfType<ArrayPatternValDeclarator>();
        array.Bindings.Count.ShouldBe(2);
        array.Bindings[0].Identifier.Name.ShouldBe("first");
        array.Bindings[0].ReadWrite.ShouldBeFalse();
        array.Bindings[1].Identifier.Name.ShouldBe("second");
        array.Bindings[1].ReadWrite.ShouldBeTrue();

        var table = statement.Declarators[2].ShouldBeOfType<TablePatternValDeclarator>();
        table.Fields.Count.ShouldBe(3);
        table.Fields[0].Key.ShouldBe("name");
        table.Fields[0].Binding.Identifier.Name.ShouldBe("name");
        table.Fields[1].Key.ShouldBe("points");
        table.Fields[1].Binding.Identifier.Name.ShouldBe("score");
        table.Fields[1].Binding.ReadWrite.ShouldBeFalse();
        table.Fields[2].Key.ShouldBe("total points");
        table.Fields[2].Binding.Identifier.Name.ShouldBe("total");
        table.Fields[2].Binding.ReadWrite.ShouldBeTrue();
    }

    [TestCase("val [] <- source;")]
    [TestCase("val {} <- source;")]
    [TestCase("val [item] = source;")]
    [TestCase("val [item] <- ;")]
    [TestCase("val [[item]] <- source;")]
    [TestCase("val [...rest] <- source;")]
    [TestCase("val [item = 1] <- source;")]
    [TestCase("val [item, item] <- source;")]
    [TestCase("val {name: first, name: second} <- source;")]
    [TestCase("val {left: value, right: value} <- source;")]
    [TestCase("val {[key]: item} <- source;")]
    public void UnsupportedOrInvalidPatterns_ThrowParserException(string source)
    {
        Should.Throw<ParserException>(() => BuildProgramTree(source));
    }

    [Test]
    public void LeftArrowInRelationalExpression_PreservesLessThanNegativeParsing()
    {
        var program = BuildProgramTree("left<-right * 2 + 3;");
        
        var statement = program.Statements[0]
            .ShouldBeOfType<ExpressionStatement>();
        
        var comparison = statement.Expression
            .ShouldBeOfType<BinaryExpression>();
        
        var product = comparison
            .Right.ShouldBeOfType<BinaryExpression>()
            .Left.ShouldBeOfType<BinaryExpression>();
        
        var negative = product.Left.ShouldBeOfType<UnaryExpression>();

        negative.Type.ShouldBe(UnaryOperationType.Minus);
    }

    [Test]
    public void LeftArrowFollowedByMinusInExpression_PreservesPreDecrement()
    {
        var program = BuildProgramTree("left<--right;");
        var statement = program.Statements[0].ShouldBeOfType<ExpressionStatement>();
        var comparison = statement.Expression.ShouldBeOfType<BinaryExpression>();

        comparison.Right.ShouldBeOfType<DecrementationExpression>();
    }
}