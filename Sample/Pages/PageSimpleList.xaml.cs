namespace Sample.Pages;

public partial class PageSimpleList : ContentPage
{
	public PageSimpleList()
	{
		InitializeComponent();
        var items = new List<string>();
        for (int i = 0; i < 30; i++)
        {
            items.Add(i.ToString());
        }
        list.ItemsSource = items;
        //BindableLayout.SetItemsSource(list, items);
    }
}