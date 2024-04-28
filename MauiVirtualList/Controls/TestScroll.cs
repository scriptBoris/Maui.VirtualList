namespace MauiVirtualList.Controls;

public class TestScroll : ContentView
{
    private double _scrollY;
    private double lastY;

    public event EventHandler<ScrolledEventArgs>? Scrolled;

    public TestScroll()
    {
        var pan = new PanGestureRecognizer
        {
        };
        pan.PanUpdated += Pan_PanUpdated;
        this.GestureRecognizers.Add(pan);
    }

    public double ContentHeight => ((Body)Content).EstimatedHeight;
    public Size ContentSize => new Size(Content.Width, ContentHeight);

    public double ScrollY 
    { 
        get => -_scrollY;
        private set
        {
            if (_scrollY != value)
            {
                _scrollY = value;
                Content.TranslationY = value;
                Scrolled?.Invoke(this, new ScrolledEventArgs(0, -value));
            }
        }
    }

    private void Pan_PanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                break;
            case GestureStatus.Running:
                var intent = lastY + e.TotalY;
                if (intent > 0)
                    intent = 0;

                if (intent < -ContentHeight)
                    intent = -ContentHeight;

                ScrollY = intent;
                break;
            case GestureStatus.Completed:
                lastY = _scrollY;
                break;
            case GestureStatus.Canceled:
                lastY = _scrollY;
                break;
            default:
                break;
        }
    }

    public Task ScrollToAsync(double x, double y, bool animated)
    {
        double fixedY = y;
        if (y < 0)
            fixedY = 0;

        if (y >= ContentHeight - Height)
            fixedY = ContentHeight - Height;

        ScrollY = -fixedY;
        lastY = -fixedY;
        return Task.CompletedTask;
    }
}
