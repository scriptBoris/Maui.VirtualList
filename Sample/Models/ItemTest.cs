using PropertyChanged;
using Sample.Utils;
using System.Diagnostics;

namespace Sample.Models;

[DebuggerDisplay("{Number} {Text}")]
public class ItemTest : BaseNotify
{
    public ItemTest(int number, string text)
    {
        Number = number;
        Text = text;
    }

    public int Number { get; set; }
    public string Text {  get; set; }
    
    [DependsOn(nameof(Number))]
    public bool IsEven => Number % 2 == 0;

    [DependsOn(nameof(Number))]
    public Color Color => IsEven ? Colors.Black : Colors.DarkGray;

    public override string ToString()
    {
        return $"{Text} {Number}";
    }

    public static ItemTest Gen(int number)
    {
        return new ItemTest(number, $"{RandomNameGenerator.GenerateRandomMaleFirstName()} {RandomNameGenerator.GenerateRandomLastName()}");
    }

    public static ItemTest Gen(int number, char letter)
    {
        string firstName = RandomNameGenerator.GenerateRandomFirstName(letter);
        string lastName = RandomNameGenerator.GenerateRandomLastName();
        string name = $"{firstName} {lastName}";
        return new ItemTest(number, name);
    }
}