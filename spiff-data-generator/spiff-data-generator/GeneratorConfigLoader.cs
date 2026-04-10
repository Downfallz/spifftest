using Bogus;
using Microsoft.Extensions.Configuration;
using spiff_data_generator.Common.Config;

namespace spiff_data_generator;

public static class GeneratorConfigLoader
{
    public static GeneratorConfig Load(string typeFeuillet)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(Common.Constants.ConfigFileName, false)
            .Build();

        var section = ResolveSection(typeFeuillet);
        var config = new GeneratorConfig();

        // 1. Bind defaults
        configuration.GetSection("_defaults").Bind(config);

        // 2. Overlay type-specific overrides
        var typeSection = configuration.GetSection(section);
        if (typeSection.Exists())
            typeSection.Bind(config);

        // 3. Auto-derive OutputDir if not explicitly set in type section
        if (typeSection.GetValue<string>("OutputDir") is null)
            config.OutputDir = $"out/{typeFeuillet}";

        Randomizer.Seed = new Random(config.Seed);
        return config;
    }

    private static string ResolveSection(string typeFeuillet) =>
        typeFeuillet switch
        {
            Common.Constants.T5 => "T5Rl3",
            Common.Constants.NR4 => "NR4",
            Common.Constants.RRSP => "RRSP",
            Common.Constants.T4RIFRL2 => "T4RIFRL2",
            Common.Constants.T4RSPRL2 => "T4RSPRL2",
            Common.Constants.T5008RL18 => "T5008RL18",
            Common.Constants.T4FHSARL32 => "T4FHSARL32",
            Common.Constants.T4ARCRL1 => "T4ARCRL1",
            _ => throw new ArgumentException($"Type de feuillet inconnu: {typeFeuillet}")
        };
}
