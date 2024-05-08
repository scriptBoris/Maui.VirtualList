using MauiVirtualList.Utils;
using Microsoft.Maui.Layouts;

namespace MauiVirtualList.Controls;

public class ScrollViewTest : Layout, IScroller, ILayoutManager
{
    /// <summary>
    /// Реверсивный (ниже нуля)
    /// </summary>
    private double _scrollY;
    private double _lastY;
    private BodyGroup _content = null!;
    private VerticalSlider _progressLine;
    private const double _progressWidth = 15;

    public event EventHandler<ScrolledEventArgs>? Scrolled;

    public ScrollViewTest()
    {
        var pan = new PanGestureRecognizer();
        pan.PanUpdated += Pan_PanUpdated;
        GestureRecognizers.Add(pan);

        _progressLine = new VerticalSlider(_progressWidth);
        Children.Add(_progressLine);
    }

    #region props
    public View Content
    {
        get => _content;
        set
        {
            _content = (BodyGroup)value;
            Children.Add(value);
        }
    }

    public double ContentHeight => _content.EstimatedHeight;
    public Size ContentSize => new Size(Content.Width, _content.EstimatedHeight);

    public double ScrollY
    {
        get => -_scrollY;
        set
        {
            if (_scrollY != value)
            {
                _scrollY = value;
                this.BatchBegin();
                Scrolled?.Invoke(this, new ScrolledEventArgs(0, -value));
                Content.TranslationY = value;
                DrawProgress();
                this.BatchCommit();
            }
        }
    }

    public double ScrollerWidth => _progressWidth;
    public double ViewPortWidth { get; set; } = 200;
    public double ViewPortHeight { get; set; } = 200;
    #endregion props

    void IScroller.SetScrollY(double setupScrollY)
    {
        _lastY = -setupScrollY;
        _scrollY = -setupScrollY;
        Content.TranslationY = -setupScrollY;
        DrawProgress();
    }

    private void Pan_PanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                break;
            case GestureStatus.Running:
                var intent = _lastY + e.TotalY;
                if (intent > 0)
                    intent = 0;

                if (intent < -(ContentHeight - Height))
                    intent = -(ContentHeight - Height);

                ScrollY = intent;
                break;
            case GestureStatus.Completed:
                _lastY = _scrollY;
                break;
            case GestureStatus.Canceled:
                _lastY = _scrollY;
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
        _lastY = -fixedY;
        DrawProgress();
        return Task.CompletedTask;
    }

    private void DrawProgress()
    {
        double dif = ScrollY / (ContentHeight - Height);
        _progressLine.Progress = dif;
    }

    public Size Measure(double widthConstraint, double heightConstraint)
    {
        _progressLine.HardMeasure(_progressWidth, heightConstraint);

        double contentWidthConstraint = widthConstraint - _progressWidth;
        double height;

        var res = Content.HardMeasure(contentWidthConstraint, heightConstraint);
        if (res.Height > heightConstraint)
        {
            height = heightConstraint;
        }
        else
        {
            height = res.Height;
        }

        return new Size(widthConstraint, height);
    }

    public Size ArrangeChildren(Rect bounds)
    {
        double widthBody = bounds.Width - _progressWidth;

        var rect1 = new Rect(0, 0, widthBody, bounds.Height);
        Content.HardArrange(rect1);

        var rect2 = new Rect(widthBody, 0, _progressWidth, bounds.Height);
        _progressLine.HardArrange(rect2);

        return bounds.Size;
    }

    protected override ILayoutManager CreateLayoutManager()
    {
        return this;
    }
}
