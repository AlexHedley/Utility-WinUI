using System.Text.Json;
using System.Xml;
using Utility.Core;

namespace Utility.Core.Tests;

public class UtilityOperationsTests
{
    [Theory]
    [InlineData("HTML encoder", "Encode", "<p>&</p>", "&lt;p&gt;&amp;&lt;/p&gt;")]
    [InlineData("HTML encoder", "Decode", "&lt;p&gt;&amp;&lt;/p&gt;", "<p>&</p>")]
    [InlineData("URL encoder", "Encode", "hello world", "hello%20world")]
    [InlineData("URL encoder", "Decode", "hello%20world", "hello world")]
    public void Run_encodes_and_decodes_text(string tool, string action, string input, string expected)
    {
        Assert.Equal(expected, UtilityOperations.Run(tool, action, input));
    }

    [Theory]
    [InlineData("#ABC", "rgb(170, 187, 204)")]
    [InlineData("12abEF", "rgb(18, 171, 239)")]
    [InlineData(" #000000 ", "rgb(0, 0, 0)")]
    public void Run_converts_hex_colors(string input, string expected)
    {
        Assert.Equal(expected, UtilityOperations.Run("HEX to RGB", "Convert", input));
    }

    [Theory]
    [InlineData("#12")]
    [InlineData("#GGGGGG")]
    public void Run_rejects_invalid_hex_colors(string input)
    {
        Assert.Throws<FormatException>(() => UtilityOperations.Run("HEX to RGB", "Convert", input));
    }

    [Theory]
    [InlineData("1,O'Reilly,3", "IN ('1', 'O''Reilly', '3')")]
    [InlineData("alpha\nbeta", "IN ('alpha', 'beta')")]
    [InlineData("\"one\",'two'", "IN ('one', 'two')")]
    public void Run_builds_escaped_sql_in_clauses(string input, string expected)
    {
        Assert.Equal(expected, UtilityOperations.Run("SQL IN builder", "Build", input));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" , \n")]
    public void Run_rejects_empty_sql_in_values(string input)
    {
        Assert.Throws<FormatException>(() => UtilityOperations.Run("SQL IN builder", "Build", input));
    }

    [Fact]
    public void Run_generates_a_parseable_guid()
    {
        var result = UtilityOperations.Run("GUID generator", "Generate", string.Empty);

        Assert.True(Guid.TryParse(result, out _));
    }

    [Fact]
    public void Run_formats_and_minifies_json()
    {
        var formatted = UtilityOperations.Run("JSON formatter", "Format", "{\"name\":\"Utility\",\"count\":2}");
        var minified = UtilityOperations.Run("JSON formatter", "Minify", "{ \"name\": \"Utility\", \"count\": 2 }");

        Assert.Contains(Environment.NewLine, formatted);
        Assert.Equal("{\"name\":\"Utility\",\"count\":2}", minified);
    }

    [Fact]
    public void Run_rejects_invalid_json()
    {
        Assert.Throws<JsonException>(() => UtilityOperations.Run("JSON formatter", "Format", "{invalid}"));
    }

    [Fact]
    public void Run_formats_and_minifies_xml()
    {
        var formatted = UtilityOperations.Run("XML formatter", "Format", "<root><item>one</item></root>");
        var minified = UtilityOperations.Run("XML formatter", "Minify", "<root>\n  <item>one</item>\n</root>");

        Assert.Contains(Environment.NewLine, formatted);
        Assert.Equal("<root><item>one</item></root>", minified);
    }

    [Fact]
    public void Run_rejects_invalid_xml()
    {
        Assert.Throws<XmlException>(() => UtilityOperations.Run("XML formatter", "Format", "<root>"));
    }

    [Fact]
    public void Run_formats_sql_clauses_without_changing_quoted_text()
    {
        var result = UtilityOperations.Run("SQL formatter", "Format", "select 'from text' from users where active = 1");

        Assert.Equal($"SELECT 'from text'{Environment.NewLine}FROM users{Environment.NewLine}WHERE active = 1", result);
    }

    [Fact]
    public void Run_rejects_unknown_actions()
    {
        Assert.Throws<InvalidOperationException>(() => UtilityOperations.Run("Unknown", "Run", string.Empty));
    }
}
