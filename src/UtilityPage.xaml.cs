using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace UtilityWinUI;

public sealed partial class UtilityPage : Page
{
    private string _tool = "Home";

    public UtilityPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        _tool = e.Parameter as string ?? "Home";
        TitleText.Text = _tool;
        StatusText.Text = string.Empty;
        OutputBox.Text = string.Empty;

        var isHome = _tool == "Home";
        HomePanel.Visibility = isHome ? Visibility.Visible : Visibility.Collapsed;
        ToolPanel.Visibility = isHome ? Visibility.Collapsed : Visibility.Visible;
        DescriptionText.Text = isHome
            ? "Utility brings useful text, data, and formatting helpers together in one place."
            : DescriptionFor(_tool);

        InputBox.Text = string.Empty;
        InputBox.Header = _tool == "HEX to RGB" ? "HEX color" : "Input";
        InputBox.PlaceholderText = _tool == "HEX to RGB" ? "#1A2B3C" : "Enter or paste text here";
        ActionsPanel.Children.Clear();

        foreach (var action in ActionsFor(_tool))
        {
            var button = new Button { Content = action.Label };
            button.Click += (_, _) => RunAction(action.Name);
            ActionsPanel.Children.Add(button);
        }

        var clearButton = new Button { Content = "Clear" };
        clearButton.Click += (_, _) =>
        {
            InputBox.Text = string.Empty;
            OutputBox.Text = string.Empty;
            StatusText.Text = string.Empty;
        };
        ActionsPanel.Children.Add(clearButton);
    }

    private void RunAction(string action)
    {
        try
        {
            OutputBox.Text = UtilityOperations.Run(_tool, action, InputBox.Text);
            StatusText.Text = string.Empty;
        }
        catch (Exception exception) when (
            exception is FormatException or InvalidOperationException or System.Xml.XmlException or System.Text.Json.JsonException)
        {
            OutputBox.Text = string.Empty;
            StatusText.Text = exception.Message;
        }
    }

    private static string DescriptionFor(string tool) => tool switch
    {
        "HTML encoder" => "Encode or decode HTML entities in a string.",
        "URL encoder" => "Encode or decode a URL component.",
        "HEX to RGB" => "Convert a 3- or 6-digit HEX color to its RGB values.",
        "SQL IN builder" => "Turn comma- or line-separated values into a safely quoted SQL IN clause.",
        "GUID generator" => "Generate a new globally unique identifier.",
        "JSON formatter" => "Validate JSON, then format it for readability or compact it.",
        "XML formatter" => "Validate XML, then format it for readability or compact it.",
        "SQL formatter" => "Format common SQL clauses onto separate lines.",
        _ => string.Empty
    };

    private static IEnumerable<(string Name, string Label)> ActionsFor(string tool) => tool switch
    {
        "HTML encoder" =>
        [
            ("Encode", "Encode"),
            ("Decode", "Decode")
        ],
        "URL encoder" =>
        [
            ("Encode", "Encode"),
            ("Decode", "Decode")
        ],
        "HEX to RGB" =>
        [
            ("Convert", "Convert")
        ],
        "SQL IN builder" =>
        [
            ("Build", "Build IN clause")
        ],
        "GUID generator" =>
        [
            ("Generate", "Generate GUID")
        ],
        "JSON formatter" =>
        [
            ("Format", "Format"),
            ("Minify", "Minify")
        ],
        "XML formatter" =>
        [
            ("Format", "Format"),
            ("Minify", "Minify")
        ],
        "SQL formatter" =>
        [
            ("Format", "Format")
        ],
        _ => []
    };
}
