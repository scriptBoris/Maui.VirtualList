using Sample.Utils;

namespace Sample.Models;

public class User : BaseNotify
{
    public int Id { get; set; }
    public int Number { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhotoUrl { get; set; }
    public string ShortBio { get; set; }
    public bool ShowBio { get; set; } = true;

    public override string ToString()
    {
        return $"[id:{Id}] #{Number} {FirstName} {LastName}";
    }
}
