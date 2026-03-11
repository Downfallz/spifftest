namespace spiff_data_generator;

public static class CaseBuilder
{
    public static List<object> Build(SlipContext context)
    {
        var cases = new List<object>
        {
            new Dictionary<string, object> { ["case"] = "13", ["valeur"] = context.Case13 },
            new Dictionary<string, object> { ["case"] = "2B", ["valeur"] = context.NumTransit },
            new Dictionary<string, object> { ["case"] = "28", ["valeur"] = context.NumCompte },
        };

        if (context.IsQc)
        {
            cases.Add(new Dictionary<string, object> { ["case"] = "D", ["valeur"] = context.CaseD });
            cases.Add(new Dictionary<string, object> { ["case"] = "Succ", ["valeur"] = context.NumTransit });
        }

        return cases;
    }
}
