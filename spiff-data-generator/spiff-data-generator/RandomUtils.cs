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

    public static string FixedDigitsNonZero(Random rng, int digits)
    {
        if (digits <= 0) return "0";
        char first = (char)('1' + rng.Next(0, 9));
        if (digits == 1) return first.ToString();
        return first + FixedDigits(rng, digits - 1);
    }

    public static string RandomDecimal(Random rng, int minLeft, int maxLeft, int decimals)
    {
        int leftDigits = rng.Next(minLeft, maxLeft + 1);
        string left = FixedDigitsNonZero(rng, leftDigits);
        string right = FixedDigits(rng, decimals);
        return $"{left}.{right}";
    }

    public static string GenerateSIN(Random rng)
    {
        int[] d = new int[9];
        int sum = 0;
        for (int i = 0; i < 8; i++)
            d[i] = rng.Next(0, 10);

        for (int i = 0; i < 8; i++)
        {
            if (i % 2 == 1)
            {
                int v = d[i] * 2;
                sum += (v / 10) + (v % 10);
            }
            else sum += d[i];
        }
        d[8] = (30 - (sum % 10)) % 10;
        return string.Concat(d.Select(x => x.ToString(CultureInfo.InvariantCulture)));
    }

    public static string GenerateCanadianPostalCode(Random rng, string province)
    {
        string allowed = AllowedFirstLetters.TryGetValue(province, out var s)
            ? s : "ABCEGHJKLMNPRSTVWXYZ";

        char L1 = allowed[rng.Next(allowed.Length)];
        int D1 = rng.Next(maxValue: 10);
        const string letters = "ABCEGHJKLMNPRSTVWXYZ";
        char L2 = letters[rng.Next(letters.Length)];
        int D2 = rng.Next(maxValue: 10);
        char L3 = letters[rng.Next(letters.Length)];
        int D3 = rng.Next(maxValue: 10);

        return $"{L1}{D1}{L2}{D2}{L3}{D3}";
    }

    public static string GenerateNEQ(Random rng, int genre)
    {
        string prefix = Constants.NEQ_ENTITES_JURIDIQUES[genre.ToString(CultureInfo.InvariantCulture)];
        string block = FixedDigits(rng, length: 7);

        int sum = 0;
        for (int i = 0; i < block.Length; i++)
        {
            int d = block[i] - '0';
            if (i % 2 == 0)
            {
                int dd = d * 2;
                while (dd > 0) { sum += dd % 10; dd /= 10; }
            }
            else sum += d;
        }
        int check = (30 - (sum % 10)) % 10;
        return $"{prefix}{block}{check}";
    }

    public static string GenerateAccount(Random rng)
    {
        string block = FixedDigits(rng, length: 9);
        int[] f = { 4, 3, 2, 7, 6, 5, 4, 3, 2 };
        int sum = 0;
        for (int i = 0; i < 9; i++)
            sum += (block[i] - '0') * f[i];
        int r = sum % 11;
        int check = (r == 0) ? 0 : (r == 1) ? 0 : 11 - r;
        return block + check.ToString(CultureInfo.InvariantCulture);
    }
}
