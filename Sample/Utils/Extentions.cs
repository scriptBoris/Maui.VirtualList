using System.Text.RegularExpressions;

namespace Sample.Utils;

public static class Extensions
{
    private static readonly Random random = new Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static async Task<int?> DisplayNumberPrompt(this Page page, 
        string title, 
        string description,
        string placeholder = "",
        int? initialValue = null)
    {
        var res = await page.DisplayPromptAsync(title, description, 
            keyboard: Keyboard.Numeric,
            placeholder: placeholder,
            initialValue: initialValue?.ToString());
        if (res == null)
            return null;

        int parse = -1;

        if (!string.IsNullOrEmpty(res))
        {
            if (!int.TryParse(res, out parse))
            {
                await page.DisplayAlert("Error", "Bad input data", "OK");
                return null;
            }
        }

        return parse;
    }
}
