using System;
using System.IO;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace UtilityWinUI;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title = "Utility";
        var iconPath = Path.Combine(AppContext.BaseDirectory, "assets", "utility-icon.ico");
        if (File.Exists(iconPath))
        {
            AppWindow.SetIcon(iconPath);
        }

        ContentFrame.Navigated += ContentFrame_Navigated;
        Navigation.SelectedItem = Navigation.MenuItems[0];
        ContentFrame.Navigate(typeof(UtilityPage), "Home");
    }

    private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
    {
        if (e.Content is UtilityPage page && e.Parameter as string == "Home")
        {
            page.ToolInvoked += SelectTool;
        }
    }

    private void SelectTool(string tool)
    {
        var item = Navigation.MenuItems
            .OfType<NavigationViewItem>()
            .FirstOrDefault(menuItem => string.Equals(menuItem.Tag as string, tool, StringComparison.Ordinal));
        if (item is not null)
        {
            Navigation.SelectedItem = item;
        }
    }

    private void Navigation_SelectionChanged(
        NavigationView sender,
        NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem item && item.Tag is string tool)
        {
            ContentFrame.Navigate(typeof(UtilityPage), tool);
        }
    }
}
