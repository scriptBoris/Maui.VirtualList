using PropertyChanged;
using Sample.Utils;

namespace Sample.Models;

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
}