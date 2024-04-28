namespace Sample.Models;

public class ItemTest
{
    public required Color Color { get; set; }
    public required int Number { get; set; }

    public override string ToString()
    {
        return $"HELLO WORLD {Number}";
    }
}