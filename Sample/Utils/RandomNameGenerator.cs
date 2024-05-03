namespace Sample.Utils;

public static class RandomNameGenerator
{
    private static readonly Random _random = new(519);

    private static readonly string[] MaleFirstNames =
    {
        "Adam", "Alex", "Benjamin", "Charles", "Daniel",
        "Edward", "Frank", "George", "Henry", "Isaac",
        "James", "John", "Kevin", "Louis", "Michael",
        "Nathan", "Oliver", "Patrick", "Robert", "Samuel",
        "Thomas", "Ulysses", "Victor", "William", "Xavier",
        "Yuri", "Zachary", "Alan", "Brian", "Chris",
        "David", "Eric", "Fred", "Greg", "Harry",
        "Ian", "Jason", "Kyle", "Larry", "Matthew",
        "Neil", "Oscar", "Peter", "Quentin", "Roger",
        "Scott", "Timothy", "Vincent", "Walter", "Xander"
    };

    private static readonly string[] FemaleFirstNames =
    {
        "Alice", "Bella", "Catherine", "Diana", "Emma",
        "Fiona", "Grace", "Hannah", "Isabella", "Jessica",
        "Kate", "Lily", "Mia", "Natalie", "Olivia",
        "Paige", "Rachel", "Sarah", "Tiffany", "Ursula",
        "Victoria", "Wendy", "Xena", "Yvonne", "Zoe",
        "Abigail", "Brooklyn", "Chloe", "Danielle", "Eva",
        "Faith", "Gabriella", "Haley", "Isabel", "Jasmine",
        "Kayla", "Leah", "Madison", "Nora", "Ophelia",
        "Penelope", "Quinn", "Rebecca", "Samantha", "Tara",
        "Uma", "Violet", "Willow", "Xiomara", "Yasmine"
    };

    private static readonly string[] LastNames =
    {
        "Smith", "Johnson", "Williams", "Brown", "Jones",
        "Miller", "Davis", "Garcia", "Rodriguez", "Martinez",
        "Hernandez", "Lopez", "Gonzalez", "Wilson", "Anderson",
        "Thomas", "Taylor", "Moore", "Jackson", "Martin",
        "Lee", "Perez", "Thompson", "White", "Harris",
        "Sanchez", "Clark", "Ramirez", "Lewis", "Robinson",
        "Walker", "Young", "Allen", "King", "Wright",
        "Scott", "Torres", "Nguyen", "Hill", "Flores",
        "Green", "Adams", "Nelson", "Baker", "Hall",
        "Rivera", "Campbell", "Mitchell", "Carter", "Roberts"
    };

    public static string GenerateRandomMaleFirstName()
    {
        int index = _random.Next(MaleFirstNames.Length);
        return MaleFirstNames[index];
    }

    public static string GenerateRandomFemaleFirstName()
    {
        int index = _random.Next(FemaleFirstNames.Length);
        return FemaleFirstNames[index];
    }

    public static string GenerateRandomLastName()
    {
        int index = _random.Next(LastNames.Length);
        return LastNames[index];
    }

    internal static string GenerateRandomBio()
    {
        int rand = _random.Next(20);
        return rand switch
        {
            1 => "Dedicated worker with a passion for problem-solving and teamwork",
            2 => "Experienced professional with a track record of innovation, collaboration, and leadership, driving impactful results",
            3 => "Esteemed professional renowned for exceptional leadership, innovative thinking, collaborative prowess, and a proven track record of transformative achievements across diverse domains.",
            4 => "Distinguished professional widely acclaimed for exemplary leadership, innovative strategies, collaborative prowess, and a consistent track record of delivering transformative results that resonate across multifaceted landscapes.",
            _ => "No bio",
        };
    }
}
