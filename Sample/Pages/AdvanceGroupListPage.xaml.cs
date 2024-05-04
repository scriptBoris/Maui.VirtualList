using Sample.Models;
using System.Collections.ObjectModel;

namespace Sample.Pages;

public partial class AdvanceGroupListPage : ContentPage
{
    public AdvanceGroupListPage()
    {
        InitializeComponent();

        var src = new ObservableCollection<ServiceGroup>
        {
            new("Haircuts")
            {
                new ServiceItem
                {
                    Name = "Men's haircut",
                    Price = 20,
                    DurationOnMinutes = 30,
                },
                new ServiceItem
                {
                    Name = "Women's haircut",
                    Price = 30,
                    DurationOnMinutes = 45,
                },
                new ServiceItem
                {
                    Name = "Children's haircut",
                    Price = 15,
                    DurationOnMinutes = 20,
                },
            },
            new("Hair Styling")
            {
                new ServiceItem
                {
                    Name = "Blowouts",
                    Price = 25,
                    DurationOnMinutes = 45,
                },
                new ServiceItem
                {
                    Name = "Styling for special occasions (weddings, proms, etc.)",
                    Price = 40,
                    DurationOnMinutes = 60,
                },
                new ServiceItem
                {
                    Name = "Updos",
                    Price = 50,
                    DurationOnMinutes = 75,
                },
            },
            new("Hair Treatments")
            {
                new ServiceItem
                {
                    Name = "Deep conditioning treatments",
                    Price = 35,
                    DurationOnMinutes = 45,
                },
                new ServiceItem
                {
                    Name = "Scalp treatments (for dandruff, dry scalp, etc.)",
                    Price = 40,
                    DurationOnMinutes = 60,
                },
                new ServiceItem
                {
                    Name = "Keratin treatments",
                    Price = 200,
                    DurationOnMinutes = 120,
                },
            },
            new("Hair Coloring & Services")
            {
                new ServiceItem
                {
                    Name = "Full color",
                    Price = 75,
                    DurationOnMinutes = 90,
                },
                new ServiceItem
                {
                    Name = "Highlights",
                    Price = 112,
                    DurationOnMinutes = 120,
                },
                new ServiceItem
                {
                    Name = "Balayage/ombre",
                    Price = 150,
                    DurationOnMinutes = 150
                },
                new ServiceItem
                {
                    Name = "Perms",
                    Price = 90,
                    DurationOnMinutes = 120,
                },
                new ServiceItem
                {
                    Name = "Relaxers",
                    Price = 150,
                    DurationOnMinutes = 90,
                },
                new ServiceItem
                {
                    Name = "Texturizing",
                    Price = 100,
                    DurationOnMinutes = 60,
                },
            },
            new("Hair Extensions")
            {
                new ServiceItem
                {
                    Name = "Installation",
                    Price = 400,
                    DurationOnMinutes = 180,
                },
                new ServiceItem
                {
                    Name = "Maintenance",
                    Price = 140,
                    DurationOnMinutes = 180,
                },
                new ServiceItem
                {
                    Name = "Removal",
                    Price = 85,
                    DurationOnMinutes = 60
                },
            },
            new("Facial Grooming")
            {
                new ServiceItem
                {
                    Name = "Beard trims",
                    Price = 10,
                    DurationOnMinutes = 15,
                },
                new ServiceItem
                {
                    Name = "Eyebrow shaping",
                    Price = 15,
                    DurationOnMinutes = 20,
                },
                new ServiceItem
                {
                    Name = "Mustache trims",
                    Price = 5,
                    DurationOnMinutes = 10,
                },
            },
            new("Specialty Services")
            {
                new ServiceItem
                {
                    Name = "Hair loss treatments",
                    Price = 300,
                    DurationOnMinutes = 70,
                },
            },
            new("Sauna Sessions")
            {
                new ServiceItem
                {
                    Name = "Standard session",
                    Price = 25,
                    DurationOnMinutes = 60,
                },
                new ServiceItem
                {
                    Name = "VIP session with additional amenities",
                    Price = 50,
                    DurationOnMinutes = 90,
                },
            },
            new("Massage")
            {
                new ServiceItem
                {
                    Name = "Classic massage",
                    Price = 60,
                    DurationOnMinutes = 60,
                },
                new ServiceItem
                {
                    Name = "Relaxation massage with aromatherapy",
                    Price = 80,
                    DurationOnMinutes = 90,
                },
                new ServiceItem
                {
                    Name = "Swedish massage",
                    Price = 70,
                    DurationOnMinutes = 75,
                },
            },
            new("Thermal Procedures")
            {
                new ServiceItem
                {
                    Name = "Steam room",
                    Price = 40,
                    DurationOnMinutes = 30,
                },
                new ServiceItem
                {
                    Name = "Thermal pool",
                    Price = 30,
                    DurationOnMinutes = 45,
                },
                new ServiceItem
                {
                    Name = "Infrared sauna",
                    Price = 35,
                    DurationOnMinutes = 60,
                },
            },
            new("Exfoliation and Wraps")
            {
                new ServiceItem
                {
                    Name = "Body scrub",
                    Price = 50,
                    DurationOnMinutes = 45,
                },
                new ServiceItem
                {
                    Name = "Aloe vera wrap",
                    Price = 70,
                    DurationOnMinutes = 60,
                },
                new ServiceItem
                {
                    Name = "Anti-cellulite wrap",
                    Price = 80,
                    DurationOnMinutes = 75,
                },
            },
            new("Additional Services")
            {
                new ServiceItem
                {
                    Name = "Relaxation room with tea and fruits",
                    Price = 20,
                    DurationOnMinutes = 30,
                },
                new ServiceItem
                {
                    Name = "Game room with billiards and board games",
                    Price = 15,
                    DurationOnMinutes = 60,
                },
                new ServiceItem
                {
                    Name = "Fitness room with a trainer",
                    Price = 10,
                    DurationOnMinutes = 60,
                },
            },
        };

        list.ItemsSource = src;
    }
}