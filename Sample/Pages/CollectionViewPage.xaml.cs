using Sample.Models;
using Sample.Utils;
using System.Collections.ObjectModel;

namespace Sample.Pages;

public partial class CollectionViewPage : ContentPage
{
	public CollectionViewPage()
	{
		InitializeComponent();

        var users = new ObservableCollection<User>();
        for (int i = 0; i < 50; i++)
        {
            var user = AdvanceListPage.BuildUser(true, i);
            users.Add(user);
        }

        for (int i = 0; i < 50; i++)
        {
            var user = AdvanceListPage.BuildUser(false, i);
            users.Add(user);
        }
        users.Shuffle();

        for (int i = 0; i < users.Count; i++)
        {
            users[i].Id = i;
            users[i].Number = i + 1;
        }

        collectionView.ItemsSource = users;
    }
}