using Maui.CrossPlatform.VirtualList.Controls;

namespace Maui.CrossPlatform.VirtualList;

public static class Init
{
    public static MauiAppBuilder UseMauiVirtualList(this MauiAppBuilder app)
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
