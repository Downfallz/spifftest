using spiff_data_generator.T5Rl3.Config;
using System.Globalization;
using Bogus;

namespace spiff_data_generator.Common.RandomGen;

public sealed class RandomService : IRandomService
{
    private static readonly Dictionary<string, string> PostalCodePrefixes = new()
    {
        ["QC"] = "GHIJ", ["ON"] = "KLMNP", ["AB"] = "T", ["BC"] = "V",
        ["MB"] = "R", ["NB"] = "E", ["NS"] = "B", ["PE"] = "C",
        ["SK"] = "S", ["NL"] = "A", ["NT"] = "X", ["NU"] = "X", ["YT"] = "Y",
    };

    private const string PostalLetters = "ABCEGHJKLMNPRSTVWXYZ";

    private readonly Random _rng;
    private readonly Faker _faker;

    public RandomService(T5Rl3Config config)
    {
        _rng = new Random(config.Seed);
        _faker = new Faker("en_CA");
    }

    // ── Generic random ──────────────────────────────────────────

    public T RandomChoice<T>(IReadOnlyList<T> vals) => vals[_rng.Next(vals.Count)];

    public T WeightedChoice<T>(IReadOnlyList<T> vals, IReadOnlyList<int> weights)
    {
        if (vals.Count != weights.Count)
            throw new ArgumentException("values/weights length mismatch");

        int total = weights.Sum();
        int pick = _rng.Next(total);
        int cumulative = 0;
        for (int i = 0; i < vals.Count; i++)
        {
            cumulative += weights[i];
            if (pick < cumulative) return vals[i];
        }
        return vals[^1];
    }

    public string FixedDigits(int length)
    {
        var chars = new char[length];
        for (int i = 0; i < length; i++)
            chars[i] = (char)('0' + _rng.Next(0, 10));
        return new string(chars);
    }

    public string RandomDecimal(int minLeft, int maxLeft, int decimals)
    {
        int leftDigits = _rng.Next(minLeft, maxLeft + 1);
        string left = FixedDigitsNonZero(leftDigits);
        string right = FixedDigits(decimals);
        return $"{left}.{right}";
    }

    // ── Identifiants ────────────────────────────────────────────

    public string GenerateSIN()
    {
        var d = new int[9];
        int sum = 0;
        for (int i = 0; i < 8; i++)
            d[i] = _rng.Next(0, 10);

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

    public string GenerateNEQ(OrganisationType genre)
    {
        string prefix = Constants.NeqPrefixes[genre];
        string block = FixedDigits(7);

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

    public string GenerateNI()
    {
        string block = FixedDigits(9);
        int[] f = [4, 3, 2, 7, 6, 5, 4, 3, 2];
        int sum = 0;
        for (int i = 0; i < 9; i++)
            sum += (block[i] - '0') * f[i];
        int r = sum % 11;
        int check = (r == 0) ? 1 : (r == 1) ? 0 : 11 - r;
        return block + check.ToString(CultureInfo.InvariantCulture);
    }

    public string GenerateAccount()
    {
        string block = FixedDigits(9);
        int[] f = [4, 3, 2, 7, 6, 5, 4, 3, 2];
        int sum = 0;
        for (int i = 0; i < 9; i++)
            sum += (block[i] - '0') * f[i];
        int r = sum % 11;
        int check = (r == 0) ? 0 : (r == 1) ? 0 : 11 - r;
        return block + check.ToString(CultureInfo.InvariantCulture);
    }

    public string GenerateCanadianPostalCode(string province)
    {
        string allowed = PostalCodePrefixes.TryGetValue(province, out var s)
            ? s : PostalLetters;

        char l1 = allowed[_rng.Next(allowed.Length)];
        int d1 = _rng.Next(10);
        char l2 = PostalLetters[_rng.Next(PostalLetters.Length)];
        int d2 = _rng.Next(10);
        char l3 = PostalLetters[_rng.Next(PostalLetters.Length)];
        int d3 = _rng.Next(10);

        return $"{l1}{d1}{l2}{d2}{l3}{d3}";
    }

    // ── Bogus wrappers ──────────────────────────────────────────

    public string FirstName() => _faker.Name.FirstName().ToUpperInvariant();
    public string LastName() => _faker.Name.LastName().ToUpperInvariant();
    public string CompanyName() => _faker.Company.CompanyName().ToUpperInvariant().Replace(",", "");
    public string CompanySuffix() => _faker.Company.CompanySuffix().ToUpperInvariant().Replace(",", "");
    public string StreetName() => _faker.Address.StreetName().ToUpperInvariant();
    public string City() => _faker.Address.City().ToUpperInvariant();
    public string BuildingNumber() => _faker.Address.BuildingNumber();
    public string SecondaryAddress() => _faker.Address.SecondaryAddress().ToUpperInvariant();

    // ── Private ─────────────────────────────────────────────────

    private string FixedDigitsNonZero(int digits)
    {
        if (digits <= 0) return "0";
        char first = (char)('1' + _rng.Next(0, 9));
        if (digits == 1) return first.ToString();
        return first + FixedDigits(digits - 1);
    }
}
