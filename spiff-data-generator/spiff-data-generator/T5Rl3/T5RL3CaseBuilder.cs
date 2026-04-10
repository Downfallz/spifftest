namespace spiff_data_generator.T5Rl3;

public static class T5RL3CaseBuilder
{
    public static List<object> Build(T5RL3SlipContext context)
    {
        var cases = new List<object>
        {
            new Dictionary<string, object> { ["case"] = "13", ["valeur"] = context.Case13 },
            new Dictionary<string, object> { ["case"] = "28", ["valeur"] = context.NumTransit },
            new Dictionary<string, object> { ["case"] = "29", ["valeur"] = context.NumCompte },
        };

        if (context.IsQc)
        {
            cases.Add(new Dictionary<string, object> { ["case"] = "D", ["valeur"] = context.CaseD });
            cases.Add(new Dictionary<string, object> { ["case"] = "Succ", ["valeur"] = context.NumTransit });
        }

        return cases;
    }
}
