namespace EVIL.CoreTests;

using System;
using System.Text.Json;
using EVIL.Diagnostics;
using EVIL.Diagnostics.Formatters;
using NUnit.Framework;
using Shouldly;

[TestFixture]
public class DiagnosticFormatterTests
{
    private static Diagnostic CreateSampleDiagnostic(
        string id = "EV8003",
        string message = "The local variable 'x' is declared but its value is never used.",
        DiagnosticSeverity severity = DiagnosticSeverity.Warning,
        string? filePath = "test.vil",
        int line = 12,
        int column = 5,
        int length = 1)
    {
        var descriptor = new DiagnosticDescriptor(
            id,
            "Unused local variable",
            message,
            DiagnosticCategory.Inspection,
            severity
        );

        var location = new SourceLocation(filePath, line, column, Length: length);
        return new Diagnostic(
            descriptor,
            location,
            severity,
            message,
            null,
            [new RelatedLocation(new SourceLocation("other.vil", 3, 1), "Previously declared here")],
            [new CodeFix("Remove unused variable", string.Empty, location)]
        );
    }

    [Test]
    public void StandardFormatter_FormatsWithLocationAndRelatedLocations()
    {
        var diag = CreateSampleDiagnostic();
        var formatted = DiagnosticFormatter.Standard.Format(diag);

        formatted.ShouldContain("test.vil(12,5): warning EV8003: The local variable 'x' is declared but its value is never used.");
        formatted.ShouldContain("other.vil(3,1): info: Previously declared here");
    }

    [Test]
    public void StandardFormatter_FormatsWithoutLocation()
    {
        var descriptor = new DiagnosticDescriptor("EV0001", "Generic", "General error", DiagnosticCategory.CodeGen, DiagnosticSeverity.Error);
        var diag = new Diagnostic(descriptor, SourceLocation.None, DiagnosticSeverity.Error, "General error");
        var formatted = DiagnosticFormatter.Standard.Format(diag);

        formatted.ShouldBe("error EV0001: General error");
    }

    [Test]
    public void RichFormatter_FormatsWithGutterAndCaret()
    {
        var rich = new RichDiagnosticFormatter
        {
            UseColor = false,
            SourceProvider = _ => "val a = 1;\nval x = 10;\nret a;"
        };

        var diag = CreateSampleDiagnostic(line: 2, column: 5, length: 1);
        var formatted = rich.Format(diag);

        formatted.ShouldContain("warning[EV8003]: The local variable 'x' is declared but its value is never used.");
        formatted.ShouldContain("--> test.vil:2:5");
        formatted.ShouldContain("2 | val x = 10;");
        formatted.ShouldContain("|     ^");
        formatted.ShouldContain("::: other.vil:3:1: Previously declared here");
        formatted.ShouldContain("help: Remove unused variable");
    }

    [Test]
    public void RichFormatter_FormatAll_SeparatesWithDoubleNewline()
    {
        var rich = new RichDiagnosticFormatter { UseColor = false };
        var diag1 = CreateSampleDiagnostic(id: "EV8003");
        var diag2 = CreateSampleDiagnostic(id: "EV8004", message: "Unreachable code detected.");

        var all = rich.FormatAll([diag1, diag2]);
        all.ShouldContain("EV8003");
        all.ShouldContain("EV8004");
        all.ShouldContain(Environment.NewLine + Environment.NewLine);
    }

    [Test]
    public void JsonFormatter_FormatsValidJsonStructure()
    {
        var diag = CreateSampleDiagnostic();
        var json = DiagnosticFormatter.Json.Format(diag);

        json.ShouldNotBeNullOrWhiteSpace();

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        root.GetProperty("id").GetString().ShouldBe("EV8003");
        root.GetProperty("severity").GetString().ShouldBe("Warning");
        root.GetProperty("category").GetString().ShouldBe("Inspection");
        root.GetProperty("location").GetProperty("line").GetInt32().ShouldBe(12);
        root.GetProperty("location").GetProperty("column").GetInt32().ShouldBe(5);

        var related = root.GetProperty("relatedLocations");
        related.GetArrayLength().ShouldBe(1);
        related[0].GetProperty("message").GetString().ShouldBe("Previously declared here");

        var fixes = root.GetProperty("codeFixes");
        fixes.GetArrayLength().ShouldBe(1);
        fixes[0].GetProperty("description").GetString().ShouldBe("Remove unused variable");
    }

    [Test]
    public void JsonFormatter_FormatAll_ReturnsJsonArray()
    {
        var diag1 = CreateSampleDiagnostic(id: "EV8001");
        var diag2 = CreateSampleDiagnostic(id: "EV8002");

        var json = DiagnosticFormatter.Json.FormatAll([diag1, diag2]);
        using var doc = JsonDocument.Parse(json);
        doc.RootElement.ValueKind.ShouldBe(JsonValueKind.Array);
        doc.RootElement.GetArrayLength().ShouldBe(2);
    }

    [Test]
    public void DiagnosticFormatter_GetByName_ResolvesKnownFormatters()
    {
        DiagnosticFormatter.GetByName("standard").ShouldBeOfType<StandardDiagnosticFormatter>();
        DiagnosticFormatter.GetByName("STANDARD").ShouldBeOfType<StandardDiagnosticFormatter>();
        DiagnosticFormatter.GetByName("rich").ShouldBeOfType<RichDiagnosticFormatter>();
        DiagnosticFormatter.GetByName("json").ShouldBeOfType<JsonDiagnosticFormatter>();
        DiagnosticFormatter.GetByName(null).ShouldBeOfType<StandardDiagnosticFormatter>();
        DiagnosticFormatter.GetByName("").ShouldBeOfType<StandardDiagnosticFormatter>();

        Should.Throw<ArgumentException>(() => DiagnosticFormatter.GetByName("unknown_format"));
    }
}
