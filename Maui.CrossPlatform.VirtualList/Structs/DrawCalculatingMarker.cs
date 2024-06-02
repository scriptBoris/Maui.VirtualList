namespace Maui.CrossPlatform.VirtualList.Structs;

internal class DrawCalculatingMarker : IDisposable
{
    public DrawCalculatingMarker(bool isContinueCalc)
    {
        IsContinueCalculating = isContinueCalc;
    }

    public bool IsContinueCalculating { get; private set; }

    public void Dispose()
    {
        IsContinueCalculating = false;
    }
}
