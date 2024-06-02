using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace VirtualList.Maui.Platforms.Android;

public class ScrollViewOutHandler : ScrollViewHandler
{
    protected override MauiScrollView CreatePlatformView()
    {
        var ctxt = new ContextThemeWrapper(MauiContext!.Context, Microsoft.Maui.Resource.Style.scrollViewTheme);
        return new ExtScrollView(ctxt, null!, Microsoft.Maui.Resource.Attribute.scrollViewStyle);
    }

    public override void SetVirtualView(IView view)
    {
        base.SetVirtualView(view);
        PlatformView.OverScrollMode = OverScrollMode.Never;
    }
}

public class ExtScrollView : MauiScrollView
{
    public ExtScrollView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
    {
    }
}