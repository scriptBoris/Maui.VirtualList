using Sample.Models;
using Sample.Utils;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Sample.Pages;

public partial class CollectionViewPage : ContentPage
{
	public CollectionViewPage()
	{
		InitializeComponent();
        CommandShowHideBio = new Command<User>(ActionShowHideBio);

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

    public ICommand CommandShowHideBio { get; }
    private void ActionShowHideBio(User user)
    {
        user.ShowBio = !user.ShowBio;
    }

    private void Border_MeasureInvalidated(object sender, EventArgs e)
    {

    }
}