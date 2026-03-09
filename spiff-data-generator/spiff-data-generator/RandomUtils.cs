using System.Globalization;
using spiff_data_generator;

public static class RandomUtils
{
    private static readonly Dictionary<string, string> AllowedFirstLetters = new()
    {
        ["QC"] = "GHIJ",
        ["ON"] = "KLMNP",
        ["AB"] = "T",
        ["BC"] = "V",
        ["MB"] = "R",
        ["NB"] = "E",
        ["NS"] = "B",
        ["PE"] = "C",
        ["SK"] = "S",
        ["NL"] = "A",
        ["NT"] = "X",
        ["NU"] = "X",
        ["YT"] = "Y",
    };

    public static T RandomChoice<T>(Random rng, IReadOnlyList<T> vals)
        => vals[rng.Next(vals.Count)];

    public static T WeightedChoice<T>(Random rng, IReadOnlyList<T> vals, IReadOnlyList<int> weights)
    {
        if (vals.Count != weights.Count) throw new ArgumentException("values/weights length mismatch");
        int total = weights.Sum();
        int pick = rng.Next(total);
        int cum = 0;
        for (int i = 0; i < vals.Count; i++)
        {
            cum += weights[i];
            if (pick < cum) return vals[i];
        }
        return vals[^1];
    }

    public static string FixedDigits(Random rng, int length)
    {
        var chars = new char[length];
        for (int i = 0; i < length; i++)
            chars[i] = (char)('0' + rng.Next(0, 10));
        return new string(chars);
    }
}
