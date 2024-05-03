using System.Collections.ObjectModel;

namespace Sample.Pages;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void Button_ClickedTestList50(object sender, EventArgs e)
    {
        Navigation.PushAsync(new PageList(50));
    }

    private void Button_Clicked_1(object sender, EventArgs e)
    {
        Navigation.PushAsync(new PageSimpleList());
    }

    private void Button_Clicked_2(object sender, EventArgs e)
    {
        Navigation.PushAsync(new AdvanceListPage());
    }

    private void Button_Clicked_3(object sender, EventArgs e)
    {
        Navigation.PushAsync(new CollectionViewPage());
    }

    private void Button_Clicked_4(object sender, EventArgs e)
    {
        Navigation.PushAsync(new PageListMaui());
    }

    private void Button_ClickedTestList2(object sender, EventArgs e)
    {
        Navigation.PushAsync(new PageList(2));
    }

    private void Button_ClickedTestList0(object sender, EventArgs e)
    {
        Navigation.PushAsync(new PageList(0));
    }
}
