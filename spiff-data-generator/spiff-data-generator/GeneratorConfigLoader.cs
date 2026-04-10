using Bogus;
using Microsoft.Extensions.Configuration;
using spiff_data_generator.Common.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace spiff_data_generator;
public static class GeneratorConfigLoader
{
    public static GeneratorConfig Load(string typeFeuillet)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(Common.Constants.ConfigFileName, false)
            .Build();

        var section = typeFeuillet switch
        {
            Common.Constants.T5 => "T5RL3",
            Common.Constants.NR4 => "NR4",
            Common.Constants.RRSP => "RRSP",
            Common.Constants.T4RIFRL2 => "T4RIFRL2",
            Common.Constants.T4RSPRL2 => "T4RSPRL2",
            Common.Constants.T5008RL18 => "T5008RL18",
            Common.Constants.T4FHSARL32 => "T4FHSARL32",
            Common.Constants.T4ARCRL1 => "T4ARCRL1",
            _ => throw new ArgumentException($"Type de feuillet inconnu: {typeFeuillet}")
        };

        var config = configuration.GetSection(section).Get<GeneratorConfig>() ?? new GeneratorConfig();

        Randomizer.Seed = new Random(config.Seed);
        return config;
    }
}
