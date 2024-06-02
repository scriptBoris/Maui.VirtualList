# VirtualList.Maui
The .NET7 & .NET8 library for create not-native VirtualList (CollectionView).
This allows you to avoid different errors on different platforms.
VirtualList will (try to) behave the same and predictably across platforms.

## Installation
Use extension method `.UseButtonSam()` in static method MauiProgram.CreateMauiApp() 

Example
```
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseVirtualListMaui()
        ...
    }
}
```

## XAML Sample
```
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:list="clr-namespace:VirtualList.Maui;assembly=VirtualList.Maui"
             ...>
    <list:VirtualList ItemsSource="{Binding Items}">
        <list:VirtualList.ItemTemplate>
            <DataTemplate>
                <Label Text="{Binding .}"/>
            </DataTemplate>
        </list:VirtualList.ItemTemplate>
    </list:VirtualList>
</ContentPage>
```
