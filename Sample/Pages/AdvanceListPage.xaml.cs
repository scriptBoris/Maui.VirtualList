using Sample.Models;
using Sample.Utils;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Sample.Pages;

public partial class AdvanceListPage : ContentPage
{
    public AdvanceListPage()
    {
        InitializeComponent();
        CommandShowHideBio = new Command<User>(ActionShowHideBio);

        var users = new ObservableCollection<User>();
        for (int i = 0; i < 50; i++)
        {
            var user = BuildUser(true, i);
            users.Add(user);
        }

        for (int i = 0; i < 50; i++)
        {
            var user = BuildUser(false, i);
            users.Add(user);
        }
        users.Shuffle();

        for (int i = 0; i < users.Count; i++)
        {
            users[i].Id = i;
            users[i].Number = i + 1;
        }

        list.ItemsSource = users;
    }

    public ICommand CommandShowHideBio { get; }
    private void ActionShowHideBio(User user)
    {
        user.ShowBio = !user.ShowBio;
    }

    public static User BuildUser(bool isMan, int count)
    {
        string firstName;
        string photo;
        if (isMan)
        {
            firstName = RandomNameGenerator.GenerateRandomMaleFirstName();
            photo = $"https://randomuser.me/api/portraits/men/{count}.jpg";
        }
        else
        {
            firstName = RandomNameGenerator.GenerateRandomFemaleFirstName();
            photo = $"https://randomuser.me/api/portraits/women/{count}.jpg";
        }
        var user = new User
        {
            FirstName = firstName,
            LastName = RandomNameGenerator.GenerateRandomLastName(),
            PhotoUrl = photo,
            ShortBio = RandomNameGenerator.GenerateRandomBio(),
        };
        return user;
    }
}