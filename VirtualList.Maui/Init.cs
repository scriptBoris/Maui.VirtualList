using VirtualList.Maui.Controls;

namespace VirtualList.Maui;

public static class Init
{
    public static MauiAppBuilder UseVirtualListMaui(this MauiAppBuilder app)
    {
        app.ConfigureMauiHandlers(x =>
        {
#if ANDROID
            x.AddHandler(typeof(ScrollViewOut), typeof(Platforms.Android.ScrollViewOutHandler));
#endif
        });
        return app;
    }
}
