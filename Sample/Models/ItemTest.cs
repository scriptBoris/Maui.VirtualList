using PropertyChanged;
using Sample.Utils;
using System.Diagnostics;

namespace Sample.Models;

[DebuggerDisplay("{Number} {Text}")]
public class ItemTest : BaseNotify
{
    public required int Number { get; set; }
    public required string Text {  get; set; }
    
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
        return new ItemTest
        {
            Number = number,
            Text = $"{RandomNameGenerator.GenerateRandomMaleFirstName()} {RandomNameGenerator.GenerateRandomLastName()}",
        };
    }
}