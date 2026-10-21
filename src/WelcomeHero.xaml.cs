using Microsoft.UI.Composition;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace UtilityWinUI;

public sealed partial class WelcomeHero : UserControl
{
    private static readonly ToolDefinition[] Tools =
    [
        new("HTML encoder", Symbol.Character, 0, 0),
        new("URL encoder", Symbol.Link, 0, 1),
        new("HEX to RGB", Symbol.FontColor, 0, 2),
        new("SQL IN builder", Symbol.Bullets, 1, 0),
        new("GUID generator", Symbol.Tag, 1, 2),
        new("JSON formatter", Symbol.OpenFile, 2, 0),
        new("XML formatter", Symbol.Page, 2, 1),
        new("SQL formatter", Symbol.Edit, 2, 2)
    ];

    private static bool _introPlayed;
    private readonly List<Button> _toolButtons = [];
    private readonly List<StarParticle> _stars = [];
    private readonly List<StreakParticle> _streaks = [];
    private readonly UISettings _uiSettings = new();
    private readonly AccessibilitySettings _accessibilitySettings = new();
    private Button? _logoButton;
    private Visual? _sceneVisual;
    private Vector3 _sceneBaseOffset;
    private Compositor? _compositor;
    private bool _initialized;

    public WelcomeHero()
    {
        InitializeComponent();
        HeroRoot.ActualThemeChanged += (_, _) => UpdateStarPalette();
        CreateToolButtons();
    }

    public event Action<string>? ToolInvoked;

    private void CreateToolButtons()
    {
        foreach (var tool in Tools)
        {
            var icon = new SymbolIcon
            {
                Symbol = tool.Symbol,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    tool.Row == 0 ? Color.FromArgb(255, 56, 189, 248) : Color.FromArgb(255, 167, 139, 250)),
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            var label = new TextBlock
            {
                Text = tool.Name,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            var content = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Children = { icon, label }
            };
            var button = new Button
            {
                Content = content,
                Tag = tool.Name,
                Style = (Style)Resources["ToolTileStyle"],
                Opacity = 0
            };
            AutomationProperties.SetName(button, tool.Name);
            Grid.SetRow(button, tool.Row);
            Grid.SetColumn(button, tool.Column);
            button.Click += (_, _) => ToolInvoked?.Invoke(tool.Name);
            button.PointerEntered += (_, _) =>
            {
                HoverLabel.Text = tool.Name;
                AnimateScale(button, 1.06f, 160);
            };
            button.PointerExited += (_, _) =>
            {
                HoverLabel.Text = "Select a tool, or select the Utility mark to replay";
                AnimateScale(button, 1f, 160);
            };
            ToolGrid.Children.Add(button);
            _toolButtons.Add(button);
        }

        var logo = new Grid
        {
            Width = 74,
            Height = 74,
            Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Color.FromArgb(255, 15, 23, 42)),
            Children =
            {
                new TextBlock
                {
                    Text = "U",
                    FontSize = 44,
                    FontWeight = FontWeights.Bold,
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Color.FromArgb(255, 56, 189, 248)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, -5, 0, 0)
                },
                new Ellipse
                {
                    Width = 7,
                    Height = 7,
                    Fill = new Microsoft.UI.Xaml.Media.SolidColorBrush(Color.FromArgb(255, 167, 139, 250)),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(17, 12, 0, 0)
                },
                new Ellipse
                {
                    Width = 7,
                    Height = 7,
                    Fill = new Microsoft.UI.Xaml.Media.SolidColorBrush(Color.FromArgb(255, 167, 139, 250)),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 12, 17, 0)
                }
            }
        };
        _logoButton = new Button
        {
            Content = logo,
            Width = 86,
            Height = 86,
            Padding = new Thickness(0),
            CornerRadius = new CornerRadius(43),
            Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Color.FromArgb(255, 15, 23, 42)),
            BorderBrush = new Microsoft.UI.Xaml.Media.SolidColorBrush(Color.FromArgb(255, 56, 189, 248)),
            BorderThickness = new Thickness(2),
            Opacity = 0
        };
        AutomationProperties.SetName(_logoButton, "Replay the welcome animation");
        _logoButton.Click += (_, _) => PlayIntro();
        Grid.SetRow(_logoButton, 1);
        Grid.SetColumn(_logoButton, 1);
        ToolGrid.Children.Add(_logoButton);
    }

    private void WelcomeHero_Loaded(object sender, RoutedEventArgs e)
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        _compositor = ElementCompositionPreview.GetElementVisual(HeroRoot).Compositor;
        _sceneVisual = ElementCompositionPreview.GetElementVisual(ToolGrid);
        _sceneBaseOffset = _sceneVisual.Offset;
        CreateStars();
        CreateStreaks();

        if (!AnimationsAllowed)
        {
            ShowFinalState();
        }
        else if (!_introPlayed)
        {
            _introPlayed = true;
            PlayIntro();
        }
        else
        {
            PlaySettle();
        }
    }

    private void HeroRoot_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (HeroRoot.ActualWidth <= 0 || HeroRoot.ActualHeight <= 0)
        {
            return;
        }

        if (_stars.Count > 0)
        {
            foreach (var star in _stars)
            {
                Canvas.SetLeft(star.Element, star.X * HeroRoot.ActualWidth);
                Canvas.SetTop(star.Element, star.Y * HeroRoot.ActualHeight);
            }
        }

        foreach (var streak in _streaks)
        {
            PositionStreak(streak);
        }
    }

    private void PositionStreak(StreakParticle streak)
    {
        var centerX = HeroRoot.ActualWidth / 2;
        var centerY = HeroRoot.ActualHeight / 2;
        var radians = streak.Angle * Math.PI / 180;
        var directionX = Math.Cos(radians);
        var directionY = Math.Sin(radians);
        var distanceX = Math.Abs(directionX) < 0.001 ? double.PositiveInfinity : centerX / Math.Abs(directionX);
        var distanceY = Math.Abs(directionY) < 0.001 ? double.PositiveInfinity : centerY / Math.Abs(directionY);
        streak.Element.Width = Math.Min(distanceX, distanceY) * streak.LengthFactor;
        streak.Element.RenderTransform = new RotateTransform
        {
            Angle = streak.Angle,
            CenterX = 0,
            CenterY = 1
        };
        Canvas.SetLeft(streak.Element, centerX);
        Canvas.SetTop(streak.Element, centerY - 1);
    }

    private void CreateStars()
    {
        var random = new Random(50873);
        var color = HeroRoot.ActualTheme == ElementTheme.Light
            ? Color.FromArgb(150, 71, 85, 105)
            : Color.FromArgb(190, 226, 232, 240);

        for (var index = 0; index < 52; index++)
        {
            var size = (index % 9 == 0) ? 3.5 : 1.5 + random.NextDouble() * 1.5;
            var star = new Ellipse
            {
                Width = size,
                Height = size,
                Opacity = 0,
                Fill = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                    index % 7 == 0
                        ? Color.FromArgb(190, 56, 189, 248)
                        : color),
                IsHitTestVisible = false
            };
            var x = random.NextDouble();
            var y = random.NextDouble();
            Canvas.SetLeft(star, x * HeroRoot.ActualWidth);
            Canvas.SetTop(star, y * HeroRoot.ActualHeight);
            StarField.Children.Add(star);
            _stars.Add(new StarParticle(star, x, y));
        }
    }

    private void UpdateStarPalette()
    {
        var neutral = HeroRoot.ActualTheme == ElementTheme.Light
            ? Color.FromArgb(150, 71, 85, 105)
            : Color.FromArgb(190, 226, 232, 240);
        for (var index = 0; index < _stars.Count; index++)
        {
            _stars[index].Element.Fill = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                index % 7 == 0 ? Color.FromArgb(190, 56, 189, 248) : neutral);
        }
    }

    private void CreateStreaks()
    {
        if (_accessibilitySettings.HighContrast)
        {
            return;
        }

        var colors = new[]
        {
            Color.FromArgb(255, 56, 189, 248),
            Color.FromArgb(255, 167, 139, 250)
        };
        for (var index = 0; index < 18; index++)
        {
            var streak = new Border
            {
                Height = 2,
                Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(colors[index % colors.Length]),
                Opacity = 0,
                IsHitTestVisible = false
            };
            var particle = new StreakParticle(streak, index * 20, 0.68 + (index % 4) * 0.08);
            PositionStreak(particle);
            StreakField.Children.Add(streak);
            _streaks.Add(particle);
        }
    }

    private void PlayIntro()
    {
        if (_compositor is null || !AnimationsAllowed)
        {
            ShowFinalState();
            return;
        }

        StarField.Visibility = Visibility.Visible;
        StreakField.Visibility = Visibility.Visible;
        foreach (var button in _toolButtons)
        {
            var visual = ElementCompositionPreview.GetElementVisual(button);
            visual.StopAnimation(nameof(Visual.Offset));
            visual.StopAnimation(nameof(Visual.Opacity));
            visual.StopAnimation(nameof(Visual.Scale));
        }

        foreach (var star in _stars)
        {
            var visual = ElementCompositionPreview.GetElementVisual(star.Element);
            visual.StopAnimation(nameof(Visual.Offset));
            visual.StopAnimation(nameof(Visual.Opacity));
        }

        foreach (var streak in _streaks)
        {
            var visual = ElementCompositionPreview.GetElementVisual(streak.Element);
            visual.StopAnimation(nameof(Visual.Opacity));
            visual.StopAnimation(nameof(Visual.Scale));
        }

        var logoVisual = ElementCompositionPreview.GetElementVisual(_logoButton!);
        logoVisual.StopAnimation(nameof(Visual.Offset));
        logoVisual.StopAnimation(nameof(Visual.Opacity));
        logoVisual.StopAnimation(nameof(Visual.Scale));
        var center = new Point(HeroRoot.ActualWidth / 2, HeroRoot.ActualHeight / 2);
        foreach (var star in _stars)
        {
            var visual = ElementCompositionPreview.GetElementVisual(star.Element);
            var target = star.Element.TransformToVisual(HeroRoot).TransformPoint(
                new Point(star.Element.ActualWidth / 2, star.Element.ActualHeight / 2));
            var targetOffset = visual.Offset;
            visual.Offset = new Vector3(
                targetOffset.X + (float)(center.X - target.X),
                targetOffset.Y + (float)(center.Y - target.Y),
                targetOffset.Z);
            visual.Opacity = 0;

            var flight = _compositor.CreateVector3KeyFrameAnimation();
            flight.InsertKeyFrame(1f, targetOffset);
            flight.Duration = TimeSpan.FromMilliseconds(1050);
            flight.DelayTime = TimeSpan.FromMilliseconds((int)(star.X * 220));
            visual.StartAnimation(nameof(Visual.Offset), flight);

            var twinkle = _compositor.CreateScalarKeyFrameAnimation();
            twinkle.InsertKeyFrame(0.08f, 0f);
            twinkle.InsertKeyFrame(0.42f, 0.8f);
            twinkle.InsertKeyFrame(1f, 0f);
            twinkle.Duration = flight.Duration;
            twinkle.DelayTime = flight.DelayTime;
            visual.StartAnimation(nameof(Visual.Opacity), twinkle);
        }

        for (var index = 0; index < _streaks.Count; index++)
        {
            AnimateStreak(_streaks[index], index * 22);
        }

        for (var index = 0; index < _toolButtons.Count; index++)
        {
            AnimateTile(_toolButtons[index], index * 65, 820);
        }

        AnimateLogo(650);
        AnimateImpactFlash(1020);
    }

    private void PlaySettle()
    {
        if (_compositor is null || !AnimationsAllowed)
        {
            ShowFinalState();
            return;
        }

        StarField.Visibility = Visibility.Collapsed;
        StreakField.Visibility = Visibility.Collapsed;
        for (var index = 0; index < _toolButtons.Count; index++)
        {
            var button = _toolButtons[index];
            var visual = ElementCompositionPreview.GetElementVisual(button);
            visual.Opacity = 0;
            var targetOffset = visual.Offset;
            visual.Offset = targetOffset + new Vector3(0, 12, 0);
            var move = _compositor.CreateVector3KeyFrameAnimation();
            move.InsertKeyFrame(1f, targetOffset);
            move.Duration = TimeSpan.FromMilliseconds(450);
            move.DelayTime = TimeSpan.FromMilliseconds(index * 28);
            visual.StartAnimation(nameof(Visual.Offset), move);
            AnimateOpacity(visual, move.Duration, move.DelayTime);
        }

        var logoVisual = ElementCompositionPreview.GetElementVisual(_logoButton!);
        logoVisual.Opacity = 0;
        logoVisual.Scale = new Vector3(0.8f);
        AnimateOpacity(logoVisual, TimeSpan.FromMilliseconds(450), TimeSpan.FromMilliseconds(220));
        AnimateScale(_logoButton!, 1f, 450, 220);
    }

    private void AnimateTile(Button button, int delayMilliseconds, int durationMilliseconds)
    {
        var visual = ElementCompositionPreview.GetElementVisual(button);
        var target = button.TransformToVisual(HeroRoot).TransformPoint(
            new Point(button.ActualWidth / 2, button.ActualHeight / 2));
        var center = new Point(HeroRoot.ActualWidth / 2, HeroRoot.ActualHeight / 2);
        visual.CenterPoint = new Vector3((float)(button.ActualWidth / 2), (float)(button.ActualHeight / 2), 0);
        var targetOffset = visual.Offset;
        visual.Offset = new Vector3(
            targetOffset.X + (float)(center.X - target.X),
            targetOffset.Y + (float)(center.Y - target.Y),
            targetOffset.Z);
        visual.Scale = new Vector3(0.08f);
        visual.Opacity = 0;

        var flight = _compositor!.CreateVector3KeyFrameAnimation();
        flight.InsertKeyFrame(1f, targetOffset);
        flight.Duration = TimeSpan.FromMilliseconds(durationMilliseconds);
        flight.DelayTime = TimeSpan.FromMilliseconds(delayMilliseconds);
        visual.StartAnimation(nameof(Visual.Offset), flight);

        var scale = _compositor.CreateVector3KeyFrameAnimation();
        scale.InsertKeyFrame(0.82f, new Vector3(1.08f));
        scale.InsertKeyFrame(1f, Vector3.One);
        scale.Duration = flight.Duration;
        scale.DelayTime = flight.DelayTime;
        visual.StartAnimation(nameof(Visual.Scale), scale);
        AnimateOpacity(visual, flight.Duration, flight.DelayTime);
    }

    private void AnimateStreak(StreakParticle streak, int delayMilliseconds)
    {
        var visual = ElementCompositionPreview.GetElementVisual(streak.Element);
        visual.CenterPoint = new Vector3(0, 1, 0);
        visual.Scale = new Vector3(0.03f, 1, 1);
        visual.Opacity = 0;

        var scale = _compositor!.CreateVector3KeyFrameAnimation();
        scale.InsertKeyFrame(1f, Vector3.One);
        scale.Duration = TimeSpan.FromMilliseconds(760);
        scale.DelayTime = TimeSpan.FromMilliseconds(delayMilliseconds);
        visual.StartAnimation(nameof(Visual.Scale), scale);

        var opacity = _compositor.CreateScalarKeyFrameAnimation();
        opacity.InsertKeyFrame(0.36f, 0.38f);
        opacity.InsertKeyFrame(1f, 0f);
        opacity.Duration = scale.Duration;
        opacity.DelayTime = scale.DelayTime;
        visual.StartAnimation(nameof(Visual.Opacity), opacity);
    }

    private void AnimateLogo(int delayMilliseconds)
    {
        var visual = ElementCompositionPreview.GetElementVisual(_logoButton!);
        visual.CenterPoint = new Vector3((float)(_logoButton!.ActualWidth / 2), (float)(_logoButton.ActualHeight / 2), 0);
        var targetOffset = visual.Offset;
        visual.Offset = targetOffset + new Vector3(0, -55, 0);
        visual.Scale = new Vector3(2.6f);
        visual.Opacity = 0;

        var slam = _compositor!.CreateVector3KeyFrameAnimation();
        slam.InsertKeyFrame(0.72f, new Vector3(0.86f));
        slam.InsertKeyFrame(0.86f, new Vector3(1.12f));
        slam.InsertKeyFrame(1f, Vector3.One);
        slam.Duration = TimeSpan.FromMilliseconds(700);
        slam.DelayTime = TimeSpan.FromMilliseconds(delayMilliseconds);
        visual.StartAnimation(nameof(Visual.Scale), slam);

        var drop = _compositor.CreateVector3KeyFrameAnimation();
        drop.InsertKeyFrame(1f, targetOffset);
        drop.Duration = slam.Duration;
        drop.DelayTime = slam.DelayTime;
        visual.StartAnimation(nameof(Visual.Offset), drop);
        AnimateOpacity(visual, slam.Duration, slam.DelayTime);
    }

    private void AnimateImpactFlash(int delayMilliseconds)
    {
        var visual = ElementCompositionPreview.GetElementVisual(ImpactFlash);
        visual.StopAnimation(nameof(Visual.Opacity));
        visual.Opacity = 0;
        var flash = _compositor!.CreateScalarKeyFrameAnimation();
        flash.InsertKeyFrame(0.35f, 0.82f);
        flash.InsertKeyFrame(1f, 0f);
        flash.Duration = TimeSpan.FromMilliseconds(360);
        flash.DelayTime = TimeSpan.FromMilliseconds(delayMilliseconds);
        visual.StartAnimation(nameof(Visual.Opacity), flash);
    }

    private void AnimateScale(Button button, float target, int durationMilliseconds, int delayMilliseconds = 0)
    {
        if (_compositor is null || !AnimationsAllowed)
        {
            return;
        }

        var visual = ElementCompositionPreview.GetElementVisual(button);
        visual.CenterPoint = new Vector3((float)(button.ActualWidth / 2), (float)(button.ActualHeight / 2), 0);
        var animation = _compositor.CreateVector3KeyFrameAnimation();
        animation.InsertKeyFrame(1f, new Vector3(target));
        animation.Duration = TimeSpan.FromMilliseconds(durationMilliseconds);
        animation.DelayTime = TimeSpan.FromMilliseconds(delayMilliseconds);
        visual.StartAnimation(nameof(Visual.Scale), animation);
    }

    private void AnimateOpacity(Visual visual, TimeSpan duration, TimeSpan delay)
    {
        var animation = _compositor!.CreateScalarKeyFrameAnimation();
        animation.InsertKeyFrame(1f, 1f);
        animation.Duration = duration;
        animation.DelayTime = delay;
        visual.StartAnimation(nameof(Visual.Opacity), animation);
    }

    private void ShowFinalState()
    {
        _introPlayed = true;
        StarField.Visibility = Visibility.Collapsed;
        StreakField.Visibility = Visibility.Collapsed;
        ImpactFlash.Opacity = 0;
        foreach (var button in _toolButtons)
        {
            button.Opacity = 1;
            ElementCompositionPreview.GetElementVisual(button).Scale = Vector3.One;
        }

        if (_logoButton is not null)
        {
            _logoButton.Opacity = 1;
            ElementCompositionPreview.GetElementVisual(_logoButton).Scale = Vector3.One;
        }
    }

    private void HeroRoot_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_sceneVisual is null || !AnimationsAllowed)
        {
            return;
        }

        var point = e.GetCurrentPoint(HeroRoot).Position;
        var x = (float)((point.X / HeroRoot.ActualWidth - 0.5) * 8);
        var y = (float)((point.Y / HeroRoot.ActualHeight - 0.5) * 8);
        _sceneVisual.Offset = _sceneBaseOffset + new Vector3(x, y, 0);
    }

    private void HeroRoot_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (_sceneVisual is null || !AnimationsAllowed)
        {
            return;
        }

        var settle = _compositor!.CreateVector3KeyFrameAnimation();
        settle.InsertKeyFrame(1f, _sceneBaseOffset);
        settle.Duration = TimeSpan.FromMilliseconds(220);
        _sceneVisual.StartAnimation(nameof(Visual.Offset), settle);
    }

    private bool AnimationsAllowed => _uiSettings.AnimationsEnabled && !_accessibilitySettings.HighContrast;

    private sealed record ToolDefinition(string Name, Symbol Symbol, int Row, int Column);

    private sealed record StarParticle(Ellipse Element, double X, double Y);

    private sealed record StreakParticle(Border Element, double Angle, double LengthFactor);
}
