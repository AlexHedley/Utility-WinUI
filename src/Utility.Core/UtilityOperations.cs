using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Utility.Core;

public static class UtilityOperations
{
    public static string Run(string tool, string action, string input) => (tool, action) switch
    {
        ("HTML encoder", "Encode") => WebUtility.HtmlEncode(input),
        ("HTML encoder", "Decode") => WebUtility.HtmlDecode(input),
        ("URL encoder", "Encode") => Uri.EscapeDataString(input),
        ("URL encoder", "Decode") => Uri.UnescapeDataString(input),
        ("HEX to RGB", "Convert") => HexToRgb(input),
        ("SQL IN builder", "Build") => BuildInClause(input),
        ("GUID generator", "Generate") => Guid.NewGuid().ToString(),
        ("JSON formatter", "Format") => FormatJson(input, indented: true),
        ("JSON formatter", "Minify") => FormatJson(input, indented: false),
        ("XML formatter", "Format") => FormatXml(input, indented: true),
        ("XML formatter", "Minify") => FormatXml(input, indented: false),
        ("SQL formatter", "Format") => FormatSql(input),
        _ => throw new InvalidOperationException("This action is not available.")
    };

    private static string HexToRgb(string input)
    {
        var hex = input.Trim();
        if (hex.StartsWith('#'))
        {
            hex = hex[1..];
        }

        if (hex.Length == 3)
        {
            hex = string.Concat(hex.Select(character => new string(character, 2)));
        }

        if (hex.Length != 6 ||
            !int.TryParse(hex[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var red) ||
            !int.TryParse(hex[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var green) ||
            !int.TryParse(hex[4..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var blue))
        {
            throw new FormatException("Enter a valid 3- or 6-digit HEX color, such as #ABC or #12ABEF.");
        }

        return $"rgb({red}, {green}, {blue})";
    }

    private static string BuildInClause(string input)
    {
        var values = input
            .Split([',', '\r', '\n'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(value => value.Trim('\'', '"'))
            .Where(value => value.Length > 0)
            .Select(value => $"'{value.Replace("'", "''", StringComparison.Ordinal)}'")
            .ToArray();

        if (values.Length == 0)
        {
            throw new FormatException("Enter at least one value.");
        }

        return $"IN ({string.Join(", ", values)})";
    }

    private static string FormatJson(string input, bool indented)
    {
        using var document = JsonDocument.Parse(input);
        return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions
        {
            WriteIndented = indented
        });
    }

    private static string FormatXml(string input, bool indented)
    {
        var document = XDocument.Parse(input);
        return document.ToString(indented ? SaveOptions.None : SaveOptions.DisableFormatting);
    }

    private static string FormatSql(string input)
    {
        var sql = new StringBuilder();
        var word = new StringBuilder();
        var quote = '\0';
        var inLineComment = false;

        void AppendWord()
        {
            if (word.Length == 0)
            {
                return;
            }

            var value = word.ToString();
            var upper = value.ToUpperInvariant();
            if (SqlLineBreakKeywords.Contains(upper))
            {
                if (sql.Length > 0 && sql[^1] != '\n')
                {
                    sql.AppendLine();
                }
                sql.Append(upper);
            }
            else
            {
                sql.Append(SqlKeywords.Contains(upper) ? upper : value);
            }

            word.Clear();
        }

        for (var index = 0; index < input.Length; index++)
        {
            var character = input[index];
            if (inLineComment)
            {
                sql.Append(character);
                if (character is '\r' or '\n')
                {
                    inLineComment = false;
                }
                continue;
            }

            if (quote != '\0')
            {
                sql.Append(character);
                if (character == quote)
                {
                    if (index + 1 < input.Length && input[index + 1] == quote)
                    {
                        sql.Append(input[++index]);
                    }
                    else
                    {
                        quote = '\0';
                    }
                }
                continue;
            }

            if (character == '-' && index + 1 < input.Length && input[index + 1] == '-')
            {
                AppendWord();
                sql.Append("--");
                index++;
                inLineComment = true;
                continue;
            }

            if (character is '\'' or '"' or '`')
            {
                AppendWord();
                quote = character;
                sql.Append(character);
            }
            else if (char.IsLetter(character))
            {
                word.Append(character);
            }
            else
            {
                AppendWord();
                sql.Append(character);
            }
        }

        AppendWord();
        return Regex.Replace(sql.ToString().Trim(), @"[ \t]+\r?\n", Environment.NewLine);
    }

    private static readonly HashSet<string> SqlLineBreakKeywords =
    [
        "SELECT", "FROM", "WHERE", "JOIN", "INNER", "LEFT", "RIGHT", "FULL",
        "CROSS", "ON", "GROUP", "ORDER", "HAVING", "LIMIT", "UNION", "VALUES",
        "SET", "INSERT", "UPDATE", "DELETE"
    ];

    private static readonly HashSet<string> SqlKeywords = new(SqlLineBreakKeywords)
    {
        "AS", "AND", "OR", "NOT", "IN", "IS", "NULL", "LIKE", "DISTINCT",
        "BY", "ASC", "DESC", "INTO", "CREATE", "TABLE", "CASE", "WHEN",
        "THEN", "ELSE", "END"
    };
}
