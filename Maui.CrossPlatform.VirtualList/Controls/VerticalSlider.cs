using Maui.CrossPlatform.VirtualList.Utils;
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Animations;

namespace Maui.CrossPlatform.VirtualList.Controls;

internal class VerticalSlider : ContentView
{
    private readonly double _boxWidth;
    private readonly View _box;
    private double _boxHeight = 20;
    private double _progress = 0;

    public VerticalSlider(double width)
    {
        _boxWidth = width;
        _box = new BoxView()
        {
            VerticalOptions = LayoutOptions.Start,
            HorizontalOptions = LayoutOptions.Start,
            BackgroundColor = Colors.White,
            Color = Colors.White,
            WidthRequest = _boxWidth,
            HeightRequest = _boxHeight,
        };

        Content = _box;
        BackgroundColor = Colors.Gray;
    }

    public double Progress
    {
        get => _progress;
        set
        {
            _progress = value;
            Draw();
        }
    }

    private void Draw()
    {
        double limTop = 0;
        double limBottom = Height - _boxHeight;

        double y = limTop.Lerp(limBottom, _progress);
        _box.TranslationY = y;
    }
}

