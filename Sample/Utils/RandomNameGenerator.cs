namespace Sample.Utils;

public static class RandomNameGenerator
{
    private static readonly Random _random = new(519);
    private static readonly Dictionary<char, string[]> MaleFirstNamesSorted = [];
    private static readonly Dictionary<char, string[]> FemaleFirstNamesSorted = [];

    private static readonly string[] MaleFirstNames =
    {
        "Adam", "Alex", "Alexandr", "Alan", "Atham",
        "Benjamin", "Brian", "Boris", "Brandon", "Bradley",
        "Charles", "Chris", "Christopher", "Connor", "Caleb",
        "Daniel", "David", "Dylan", "Dominic", "Derek",
        "Edward", "Eric", "Ethan", "Evgeniy", "Edgar",
        "Frank", "Fred", "Frederick", "Finn", "Felix",
        "George", "Greg", "Gabriel", "Gavin", "Grant",
        "Henry", "Harry", "Hector", "Harold", "Hayden",
        "Isaac", "Ian", "Ivan", "Isaiah", "Israel",
        "James", "John", "Jason", "Joseph", "Joshua",
        "Kevin", "Kyle", "Kenneth", "Keith", "Kaleb",
        "Louis", "Larry", "Liam", "Lucas", "Logan",
        "Michael", "Matthew", "Mason", "Max", "Marcus",
        "Nathan", "Neil", "Noah", "Nicholas", "Nolan",
        "Oliver", "Oscar", "Owen", "Omar", "Orion",
        "Patrick", "Peter", "Paul", "Philip", "Preston",
        "Quentin", "Quincy", "Quan", "Quirino", "Quade",
        "Robert", "Roger", "Richard", "Ryan", "Raymond",
        "Samuel", "Scott", "Sean", "Steven", "Simon",
        "Thomas", "Timothy", "Thomas", "Tyler", "Tristan",
        "Ulysses", "Uriel", "Ugo", "Umar", "Umut",
        "Victor", "Vincent", "Vaughn", "Vance", "Valentino",
        "William", "Walter", "Wyatt", "Wesley", "Warren",
        "Xavier", "Xander", "Xiang", "Xin", "Xerxes",
        "Yuri", "Youssef", "Yannick", "Yasin",
        "Zachary", "Zander", "Zane", "Ziad", "Zeki",
    };

    private static readonly string[] FemaleFirstNames =
    {
        "Abigail", "Alice", "Ava", "Amelia", "Aria",
        "Bella", "Brooklyn", "Brielle", "Bailey", "Beatrice",
        "Charlotte", "Chloe", "Clara", "Cora", "Camila",
        "Danielle", "Delilah", "Daisy", "Diana", "Daphne",
        "Emma", "Emily", "Ella", "Evelyn", "Elena",
        "Fiona", "Faith", "Felicity", "Freya", "Francesca",
        "Grace", "Gabriella", "Gianna", "Gemma", "Genevieve",
        "Hannah", "Harper", "Hazel", "Hope", "Hailey",
        "Isabella", "Isabelle", "Ivy", "Isla", "Imogen",
        "Jasmine", "Josephine", "Jade", "Julia", "Juliette",
        "Katherine", "Kayla", "Kylie", "Kennedy", "Kira",
        "Lily", "Luna", "Lucy", "Lila", "Leah",
        "Mia", "Mila", "Madison", "Molly", "Maya",
        "Natalie", "Nora", "Naomi", "Nevaeh", "Nina",
        "Olivia", "Olga", "Ophelia", "Octavia", "Odette",
        "Penelope", "Piper", "Phoebe", "Paige", "Poppy",
        "Quinn", "Queena", "Queenie", "Quincy", "Qiana",
        "Rose", "Ruby", "Rebecca", "Rachel", "Rosalie",
        "Sophia", "Scarlett", "Stella", "Sofia", "Serena",
        "Taylor", "Tessa", "Thea", "Talia", "Trinity",
        "Ursula", "Uma", "Unity", "Ulla", "Ulyana",
        "Victoria", "Violet", "Valentina", "Vivian", "Vanessa",
        "Willow", "Willa", "Winnie", "Winter", "Wendy",
        "Ximena", "Xanthe", "Xena", "Xiomara", "Xavia",
        "Yasmine", "Yara", "Yvette", "Yvonne", "Yara",
        "Zoe", "Zara", "Zelda", "Zara", "Zinnia"
    };

    static RandomNameGenerator()
    {
        MaleFirstNamesSorted = Fill(MaleFirstNames);
        FemaleFirstNamesSorted = Fill(FemaleFirstNames);
    }

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

    public static string GenerateRandomFirstName()
    {
        if (_random.Next(0, 1) == 0)
            return GenerateRandomMaleFirstName();
        else
            return GenerateRandomFemaleFirstName();
    }

    public static string GenerateRandomFirstName(char letter)
    {
        Dictionary<char, string[]> dic;
        if (_random.Next(0, 1) == 0)
            dic = MaleFirstNamesSorted;
        else
            dic = FemaleFirstNamesSorted;

        var names = dic[letter];
        int randNameId = _random.Next(0, names.Length - 1);
        return names[randNameId];
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
            1 => "Dedicated worker with a passion for problem-solving and teamwork.",
            2 => "Experienced professional with a track record of innovation, collaboration, and leadership, driving impactful results.",
            3 => "Esteemed professional renowned for exceptional leadership, innovative thinking, collaborative prowess, and a proven track record of transformative achievements across diverse domains.",
            4 => "Distinguished professional widely acclaimed for exemplary leadership, innovative strategies, collaborative prowess, and a consistent track record of delivering transformative results that resonate across multifaceted landscapes.",
            _ => "No bio.",
        };
    }

    private static Dictionary<char, string[]> Fill(string[] names)
    {
        var tempDic = new Dictionary<char, List<string>>();
        foreach (var name in names)
        {
            char nameStart = name[0];

            if (tempDic.TryGetValue(nameStart, out var item))
            {
                item.Add(name);
            }
            else
            {
                tempDic.Add(nameStart, [ name ]);
            }
        }

        var result = new Dictionary<char, string[]>();
        foreach(var item in tempDic)
        {
            string[] arr = item.Value.ToArray();
            result.Add(item.Key, arr);
        }
        return result;
    }
}
