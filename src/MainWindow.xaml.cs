using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace UtilityWinUI;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title = "Utility";
        Navigation.SelectedItem = Navigation.MenuItems[0];
        ContentFrame.Navigate(typeof(UtilityPage), "Home");
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
